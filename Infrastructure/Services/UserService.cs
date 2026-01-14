using System.Net;
using Application.DTOs.Users;
using Application.Interfaces;
using Application.Responses;
using Domain.Entities.Users;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class UserService(
    ApplicationDbContext context,
    UserManager<AppUser> userManager,
    IFileStorageService fileStorageService) : IUserService
{
    public async Task<Response<UserProfileDto>> GetProfileAsync(Guid userId)
    {
        var profile = await context.UserProfiles
            .Include(p => p.School)
            .Include(p => p.TargetMajor)
                .ThenInclude(m => m!.Faculty)
                .ThenInclude(f => f.University)
            .Include(p => p.CurrentLeague)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
            return new Response<UserProfileDto>(HttpStatusCode.NotFound, "Профил ёфт нашуд");

        var dto = MapToDto(profile);
        return new Response<UserProfileDto>(dto);
    }

    public async Task<Response<bool>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var profile = await context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        
        if (profile == null)
            return new Response<bool>(HttpStatusCode.NotFound, "Профил ёфт нашуд");

        if (!string.IsNullOrWhiteSpace(request.FirstName))
            profile.FirstName = request.FirstName;

        if (!string.IsNullOrWhiteSpace(request.LastName))
            profile.LastName = request.LastName;

        if (request.Gender.HasValue)
            profile.Gender = request.Gender;

        if (!string.IsNullOrWhiteSpace(request.Province))
            profile.Province = request.Province;

        if (!string.IsNullOrWhiteSpace(request.District))
            profile.District = request.District;

        if (request.SchoolId.HasValue)
            profile.SchoolId = request.SchoolId;

        if (request.Grade.HasValue)
            profile.Grade = request.Grade;

        if (request.ClusterId.HasValue)
            profile.ClusterId = request.ClusterId;

        if (!string.IsNullOrWhiteSpace(request.TargetUniversity))
            profile.TargetUniversity = request.TargetUniversity;

        if (!string.IsNullOrWhiteSpace(request.TargetFaculty))
            profile.TargetFaculty = request.TargetFaculty;

        if (request.TargetMajorId.HasValue)
        {
            var major = await context.Majors.FindAsync(request.TargetMajorId.Value);
            if (major != null)
            {
                profile.TargetMajorId = request.TargetMajorId;
                profile.TargetPassingScore = major.MinScore2025;
            }
        }

        if (request.Avatar != null)
        {
            var avatarPath = await fileStorageService.SaveFileAsync(request.Avatar, "uploads/avatars");
            profile.AvatarUrl = avatarPath;
        }

        await context.SaveChangesAsync();
        
        return new Response<bool>(true) { Message = "Профил навсозӣ шуд" };
    }

    public async Task<Response<UserProfileDto>> GetProfileByUsernameAsync(string username)
    {
        var user = await userManager.FindByNameAsync(username);
        if (user == null)
            return new Response<UserProfileDto>(HttpStatusCode.NotFound, "Корбар ёфт нашуд");

        return await GetProfileAsync(user.Id);
    }

    private static UserProfileDto MapToDto(UserProfile profile)
    {
        return new UserProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Gender = profile.Gender,
            Province = profile.Province,
            District = profile.District,
            SchoolId = profile.SchoolId,
            SchoolName = profile.School?.Name,
            Grade = profile.Grade,
            ClusterId = profile.ClusterId,
            TargetUniversity = profile.TargetUniversity,
            TargetFaculty = profile.TargetFaculty,
            TargetMajorId = profile.TargetMajorId,
            TargetMajorName = profile.TargetMajor?.Name,
            TargetPassingScore = profile.TargetPassingScore,
            XP = profile.XP,
            AvatarUrl = profile.AvatarUrl,
            EloRating = profile.EloRating,
            CurrentLeagueId = profile.CurrentLeagueId,
            CurrentLeagueName = profile.CurrentLeague?.Name,
            LastTestDate = profile.LastTestDate
        };
    }
}
