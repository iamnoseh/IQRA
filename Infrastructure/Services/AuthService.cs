using System.Security.Claims;
using System.Security.Cryptography;
using Application.Constants;
using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities.Users;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class AuthService(
    UserManager<AppUser> userManager,
    ApplicationDbContext context,
    IJwtService jwtService,
    ISmsService smsService,
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration,
    ILogger<AuthService> logger,
    IUserService userService) : IAuthService
{
    private const int OtpExpirationMinutes = 3;
    private const int ResetTokenExpirationMinutes = 10;
    private readonly int _tokenExpiresMinutes = int.Parse(configuration["Jwt:ExpiresMinutes"] ?? "60");

    public async Task<AuthResponse> LoginAsync(LoginDto loginDto)
    {
        var normalizedUsername = PhoneNumberHelper.NormalizePhoneNumber(loginDto.Username);
        
        var user = await userManager.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.UserName == normalizedUsername || u.UserName == loginDto.Username);
        
        if (user == null)
            return CreateErrorResponse(Messages.Auth.InvalidCredentials);

        var isPasswordValid = await userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!isPasswordValid)
            return CreateErrorResponse(Messages.Auth.InvalidCredentials);

        var token = jwtService.GenerateToken(user);

        await userService.RecordLoginActivityAsync(user.Id);

        logger.LogInformation("User {Username} logged in successfully", loginDto.Username);

        return new AuthResponse
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_tokenExpiresMinutes),
            UserId = user.Id,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Role = user.Role.ToString(),
            IsNewUser = false
        };
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var normalizedPhone = PhoneNumberHelper.NormalizePhoneNumber(request.PhoneNumber);

        if (!PhoneNumberHelper.IsValidTajikPhoneNumber(normalizedPhone))
        {
            logger.LogWarning("Invalid phone number format: {PhoneNumber}", request.PhoneNumber);
            return CreateErrorResponse(Messages.Auth.InvalidPhoneFormat);
        }

        var existingUser = await userManager.Users
            .FirstOrDefaultAsync(u => u.UserName == normalizedPhone || u.PhoneNumber == normalizedPhone);

        if (existingUser != null)
        {
            logger.LogWarning("User with phone {PhoneNumber} already exists", normalizedPhone);
            return CreateErrorResponse(Messages.Auth.UserAlreadyExists);
        }

        var password = PasswordGenerator.Generate();

        var user = new AppUser
        {
            UserName = normalizedPhone,
            PhoneNumber = normalizedPhone,
            PhoneNumberConfirmed = true,
            Role = UserRole.Student,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            logger.LogError("Failed to create user {PhoneNumber}: {Errors}",
                request.PhoneNumber,
                IdentityHelper.FormatIdentityErrors(result));
            return CreateErrorResponse(Messages.Auth.RegistrationError);
        }

        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            XP = 0
        };

        context.UserProfiles.Add(profile);
        await context.SaveChangesAsync();

        var message = string.Format(Messages.Auth.RegistrationSuccessSms, password);
        await smsService.SendSmsAsync(normalizedPhone, message);

        var token = jwtService.GenerateToken(user);

        await userService.RecordLoginActivityAsync(user.Id);

        logger.LogInformation("User {PhoneNumber} registered successfully", request.PhoneNumber);

        return new AuthResponse
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_tokenExpiresMinutes),
            UserId = user.Id,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString(),
            IsNewUser = true
        };
    }

    public async Task<string> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value
                          ?? httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Messages.Auth.UserNotAuthenticated;

        var user = await userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return Messages.Auth.UserNotFound;

        var changeResult = await userManager.ChangePasswordAsync(user, changePasswordDto.OldPassword, changePasswordDto.Password);
        if (!changeResult.Succeeded)
            return IdentityHelper.FormatIdentityErrors(changeResult);

        return Messages.Auth.PasswordChanged;
    }

    public async Task<string> SendOtpAsync(SendOtpDto sendOtpDto)
    {
        if (string.IsNullOrWhiteSpace(sendOtpDto.Username))
            return Messages.Auth.UsernameRequired;

        var user = await userManager.FindByNameAsync(sendOtpDto.Username);
        if (user == null)
            return Messages.Auth.UserNotFoundByUsername;

        var otpCode = RandomNumberGenerator.GetInt32(1000, 10000).ToString();

        user.Code = otpCode;
        user.CodeDate = DateTime.UtcNow;

        var result = await context.SaveChangesAsync();
        if (result <= 0)
            return Messages.Auth.OtpCreationError;

        if (string.IsNullOrWhiteSpace(user.PhoneNumber)) return Messages.Auth.OtpSent;
        var smsMessage = string.Format(Messages.Auth.PasswordResetSms, otpCode);
        await smsService.SendSmsAsync(user.PhoneNumber, smsMessage);

        return Messages.Auth.OtpSent;
    }

    public async Task<VerifyOtpResponseDto> VerifyOtpAsync(VerifyOtpDto verifyOtpDto)
    {
        if (string.IsNullOrWhiteSpace(verifyOtpDto.Username) || string.IsNullOrWhiteSpace(verifyOtpDto.OtpCode))
            return new VerifyOtpResponseDto { Message = Messages.Auth.UsernameAndOtpRequired };

        var user = await userManager.FindByNameAsync(verifyOtpDto.Username);
        if (user == null)
            return new VerifyOtpResponseDto { Message = Messages.Auth.UserNotFoundByUsername };

        if (user.Code != verifyOtpDto.OtpCode)
            return new VerifyOtpResponseDto { Message = Messages.Auth.OtpInvalid };

        if (user.CodeDate == null)
            return new VerifyOtpResponseDto { Message = Messages.Auth.OtpInvalid };

        var timeElapsed = DateTime.UtcNow - user.CodeDate.Value;
        if (timeElapsed.TotalMinutes > OtpExpirationMinutes)
            return new VerifyOtpResponseDto { Message = Messages.Auth.OtpExpired };

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);

        user.Code = null; 
        user.CodeDate = null;
        await context.SaveChangesAsync();

        return new VerifyOtpResponseDto
        {
            ResetToken = resetToken,
            Message = Messages.Auth.OtpVerified
        };
    }

    public async Task<string> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        if (string.IsNullOrWhiteSpace(resetPasswordDto.PhoneNumber) ||
            string.IsNullOrWhiteSpace(resetPasswordDto.ResetToken) ||
            string.IsNullOrWhiteSpace(resetPasswordDto.NewPassword))
            return Messages.Auth.TokenAndPasswordRequired;

        if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
            return Messages.Auth.PasswordsNotMatch;

        var normalizedPhone = PhoneNumberHelper.NormalizePhoneNumber(resetPasswordDto.PhoneNumber);
        var user = await userManager.FindByNameAsync(normalizedPhone) ?? await userManager.FindByEmailAsync(normalizedPhone);

        if (user == null)
            return Messages.Auth.UserNotFound;

        var resetResult = await userManager.ResetPasswordAsync(user, resetPasswordDto.ResetToken, resetPasswordDto.NewPassword);
        if (!resetResult.Succeeded)
            return IdentityHelper.FormatIdentityErrors(resetResult);

        return Messages.Auth.PasswordReset;
    }

    private static AuthResponse CreateErrorResponse(string message) => new()
    {
        Token = string.Empty,
        ExpiresAt = DateTime.UtcNow,
        UserId = Guid.Empty,
        PhoneNumber = string.Empty,
        Role = string.Empty,
        IsNewUser = false,
        ErrorMessage = message
    };
}
