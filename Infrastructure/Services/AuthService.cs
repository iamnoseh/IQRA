using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities.Users;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class AuthService(
    UserManager<AppUser> userManager,
    ApplicationDbContext context,
    IJwtService jwtService,
    ISmsService smsService,
    ILogger<AuthService> logger) : IAuthService
{
    private const int PasswordExpirationMinutes = 10;
    private const int TokenExpirationMinutes = 60;

    public async Task<AuthResponse> SendPasswordAsync(SendPasswordRequest request)
    {
        try
        {
            var password = PasswordGenerator.Generate();
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);

            if (user == null)
                return await HandleNewUserPasswordRequest(request.PhoneNumber, password);

            return await HandleExistingUserPasswordReset(user, password);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in SendPasswordAsync for {PhoneNumber}", request.PhoneNumber);
            return CreateErrorResponse(request.PhoneNumber);
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var existingUser = await userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);

            if (existingUser != null)
            {
                logger.LogWarning("User with phone {PhoneNumber} already exists", request.PhoneNumber);
                return CreateErrorResponse(request.PhoneNumber);
            }

            var user = CreateNewUser(request.PhoneNumber);
            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                logger.LogError("Failed to create user {PhoneNumber}: {Errors}",
                    request.PhoneNumber,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                return CreateErrorResponse(request.PhoneNumber);
            }

            await CreateUserProfile(user.Id, request);
            var token = jwtService.GenerateToken(user);

            logger.LogInformation("User {PhoneNumber} registered successfully", request.PhoneNumber);

            return CreateSuccessResponse(user, token, true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in RegisterAsync for {PhoneNumber}", request.PhoneNumber);
            return CreateErrorResponse(request.PhoneNumber);
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);

            if (user == null)
            {
                logger.LogWarning("Login attempt for non-existent user {PhoneNumber}", request.PhoneNumber);
                return CreateErrorResponse(request.PhoneNumber);
            }

            var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);

            if (!passwordValid)
            {
                logger.LogWarning("Invalid password for user {PhoneNumber}", request.PhoneNumber);
                return CreateErrorResponse(request.PhoneNumber);
            }

            var token = jwtService.GenerateToken(user);

            logger.LogInformation("User {PhoneNumber} logged in successfully", request.PhoneNumber);

            return CreateSuccessResponse(user, token, false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in LoginAsync for {PhoneNumber}", request.PhoneNumber);
            return CreateErrorResponse(request.PhoneNumber);
        }
    }

    private async Task<AuthResponse> HandleNewUserPasswordRequest(string phoneNumber, string password)
    {
        var message = $"Раками шабакаи IQRA: Пароли шумо барои бақайдгирӣ: {password}";
        var smsSent = await smsService.SendSmsAsync(phoneNumber, message);

        if (!smsSent)
        {
            logger.LogError("Failed to send password SMS to {PhoneNumber}", phoneNumber);
            return CreatePasswordResponse(phoneNumber, string.Empty, true);
        }

        return CreatePasswordResponse(phoneNumber, password, true);
    }

    private async Task<AuthResponse> HandleExistingUserPasswordReset(AppUser user, string password)
    {
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, resetToken, password);

        if (!result.Succeeded)
        {
            logger.LogError("Failed to reset password for user {PhoneNumber}", user.PhoneNumber);
            return CreatePasswordResponse(user.PhoneNumber, string.Empty, false);
        }

        var message = $"Раками шабакаи IQRA: Пароли нави шумо: {password}";
        await smsService.SendSmsAsync(user.PhoneNumber, message);

        return CreatePasswordResponse(user.PhoneNumber, password, false);
    }

    private static AppUser CreateNewUser(string phoneNumber) => new()
    {
        UserName = phoneNumber,
        PhoneNumber = phoneNumber,
        PhoneNumberConfirmed = true,
        Role = UserRole.Student,
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };

    private async Task CreateUserProfile(Guid userId, RegisterRequest request)
    {
        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            SchoolName = request.SchoolName,
            City = request.City,
            ClusterId = request.ClusterId,
            TargetUniversity = request.TargetUniversity,
            TargetFaculty = request.TargetFaculty ?? string.Empty,
            TargetPassingScore = request.TargetPassingScore,
            XP = 0
        };

        context.UserProfiles.Add(profile);
        await context.SaveChangesAsync();
    }

    private static AuthResponse CreatePasswordResponse(string phoneNumber, string password, bool isNewUser) => new()
    {
        Token = password,
        ExpiresAt = DateTime.UtcNow.AddMinutes(PasswordExpirationMinutes),
        UserId = Guid.Empty,
        PhoneNumber = phoneNumber,
        Role = isNewUser ? UserRole.Student.ToString() : string.Empty,
        IsNewUser = isNewUser
    };

    private static AuthResponse CreateSuccessResponse(AppUser user, string token, bool isNewUser) => new()
    {
        Token = token,
        ExpiresAt = DateTime.UtcNow.AddMinutes(TokenExpirationMinutes),
        UserId = user.Id,
        PhoneNumber = user.PhoneNumber,
        Role = user.Role.ToString(),
        IsNewUser = isNewUser
    };

    private static AuthResponse CreateErrorResponse(string phoneNumber) => new()
    {
        Token = string.Empty,
        ExpiresAt = DateTime.UtcNow,
        UserId = Guid.Empty,
        PhoneNumber = phoneNumber,
        Role = string.Empty,
        IsNewUser = false
    };
}
