using System.Security.Claims;
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

namespace Infrastructure.Services;

public class AuthService(
    UserManager<AppUser> userManager,
    ApplicationDbContext context,
    IJwtService jwtService,
    ISmsService smsService,
    IHttpContextAccessor httpContextAccessor,
    ILogger<AuthService> logger) : IAuthService
{
    private const int OtpExpirationMinutes = 3;
    private const int ResetTokenExpirationMinutes = 10;

    public async Task<AuthResponse> LoginAsync(LoginDto loginDto)
    {
        try
        {
            var user = await userManager.FindByNameAsync(loginDto.Username);
            if (user == null)
                return CreateErrorResponse(Messages.Auth.InvalidCredentials);

            var isPasswordValid = await userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordValid)
                return CreateErrorResponse(Messages.Auth.InvalidCredentials);

            var token = jwtService.GenerateToken(user);
            
            logger.LogInformation("User {Username} logged in successfully", loginDto.Username);
            
            return new AuthResponse
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Role = user.Role.ToString(),
                IsNewUser = false
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in LoginAsync for {Username}", loginDto.Username);
            return CreateErrorResponse(Messages.Auth.InvalidCredentials);
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var normalizedPhone = PhoneNumberHelper.NormalizePhoneNumber(request.PhoneNumber);
            
            if (!PhoneNumberHelper.IsValidTajikPhoneNumber(normalizedPhone))
            {
                logger.LogWarning("Invalid phone number format: {PhoneNumber}", request.PhoneNumber);
                return CreateErrorResponse("Рақами телефон нодуруст аст. Формат: +992XXXXXXXXX ё 9XXXXXXXX");
            }

            var existingUser = await userManager.Users
                .FirstOrDefaultAsync(u => u.UserName == normalizedPhone || u.PhoneNumber == normalizedPhone);

            if (existingUser != null)
            {
                logger.LogWarning("User with phone {PhoneNumber} already exists", normalizedPhone);
                return CreateErrorResponse("Корбар бо ин рақам аллакай вуҷуд дорад");
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
                return CreateErrorResponse("Хатогӣ ҳангоми бақайдгирӣ");
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

            var message = $"Салом! Шумо дар IQRA ба қайд гирифта шудед.\n\n РАМЗ: {password}\n\nИН РАМЗРО МАХФӢ НИГОҲ ДОРЕД!\nБарои даромад рақами телефони худро истифода баред.\n\nIQRA.tj";
            await smsService.SendSmsAsync(normalizedPhone, message);

            var token = jwtService.GenerateToken(user);

            logger.LogInformation("User {PhoneNumber} registered successfully", request.PhoneNumber);

            return new AuthResponse
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                IsNewUser = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in RegisterAsync for {PhoneNumber}", request.PhoneNumber);
            return CreateErrorResponse("Хатогӣ ҳангоми бақайдгирӣ");
        }
    }

    public async Task<string> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
    {
        try
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in ChangePasswordAsync");
            return string.Format(Messages.Auth.PasswordChangeError, ex.Message);
        }
    }

    public async Task<string> SendOtpAsync(SendOtpDto sendOtpDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sendOtpDto.Username))
                return Messages.Auth.UsernameRequired;

            var user = await userManager.FindByNameAsync(sendOtpDto.Username);
            if (user == null)
                return Messages.Auth.UserNotFoundByUsername;

            var otpCode = new Random().Next(1000, 9999).ToString();

            user.Code = otpCode;
            user.CodeDate = DateTime.UtcNow;

            var result = await context.SaveChangesAsync();
            if (result <= 0)
                return Messages.Auth.OtpCreationError;

            if (string.IsNullOrWhiteSpace(user.PhoneNumber)) return Messages.Auth.OtpSent;
            var smsMessage = $"IQRA - Барқароркунии парол\n\nРАМЗИ ТАСДИҚ: {otpCode}\n\nМуҳлат: 3 дақиқа\nАгар шумо дархост накардед, ин паёмро рад кунед.\n\nIQRA.tj";
            await smsService.SendSmsAsync(user.PhoneNumber, smsMessage);

            return Messages.Auth.OtpSent;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in SendOtpAsync for {Username}", sendOtpDto.Username);
            return string.Format(Messages.Auth.OtpSendError, ex.Message);
        }
    }

    public async Task<VerifyOtpResponseDto> VerifyOtpAsync(VerifyOtpDto verifyOtpDto)
    {
        try
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

            var resetToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + "_" + user.Id;
            user.Code = $"VERIFIED_{resetToken}";
            user.CodeDate = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return new VerifyOtpResponseDto
            {
                ResetToken = resetToken,
                Message = Messages.Auth.OtpVerified
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in VerifyOtpAsync for {Username}", verifyOtpDto.Username);
            return new VerifyOtpResponseDto { Message = string.Format(Messages.Auth.OtpVerifyError, ex.Message) };
        }
    }

    public async Task<string> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(resetPasswordDto.ResetToken) ||
                string.IsNullOrWhiteSpace(resetPasswordDto.NewPassword) ||
                string.IsNullOrWhiteSpace(resetPasswordDto.ConfirmPassword))
                return Messages.Auth.TokenAndPasswordRequired;

            if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
                return Messages.Auth.PasswordsNotMatch;

            var tokenParts = resetPasswordDto.ResetToken.Split('_');
            if (tokenParts.Length != 2 || !Guid.TryParse(tokenParts[1], out var userId))
                return Messages.Auth.TokenInvalid;

            var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return Messages.Auth.UserNotFound;

            var expectedCode = $"VERIFIED_{resetPasswordDto.ResetToken}";
            if (user.Code != expectedCode)
                return Messages.Auth.TokenUsedOrInvalid;

            if (user.CodeDate == null)
                return Messages.Auth.TokenInvalid;

            var timeElapsed = DateTime.UtcNow - user.CodeDate.Value;
            if (timeElapsed.TotalMinutes > ResetTokenExpirationMinutes)
                return Messages.Auth.TokenExpired;

            var passwordResetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await userManager.ResetPasswordAsync(user, passwordResetToken, resetPasswordDto.NewPassword);
            if (!resetResult.Succeeded)
                return IdentityHelper.FormatIdentityErrors(resetResult);

            user.Code = null;
            user.CodeDate = null;
            await context.SaveChangesAsync();

            return Messages.Auth.PasswordReset;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in ResetPasswordAsync");
            return string.Format(Messages.Auth.PasswordResetError, ex.Message);
        }
    }

    private static AuthResponse CreateErrorResponse(string message) => new()
    {
        Token = string.Empty,
        ExpiresAt = DateTime.UtcNow,
        UserId = Guid.Empty,
        PhoneNumber = string.Empty,
        Role = string.Empty,
        IsNewUser = false
    };
}
