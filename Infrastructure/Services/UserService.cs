using System.Net;
using Application.DTOs.Users;
using Application.Interfaces;
using Application.Responses;
using Domain.Entities.Users;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public partial class UserService(
    ApplicationDbContext context,
    UserManager<AppUser> userManager,
    IFileStorageService fileStorageService,
    ISchoolScoreService schoolScoreService) : IUserService
{
    public async Task<Response<UserProfileDto>> GetProfileAsync(Guid userId)
    {
        var profile = await context.UserProfiles
            .Include(p => p.School)
            .Include(p => p.TargetMajor)
                .ThenInclude(m => m!.Faculty)
                .ThenInclude(f => f.University)
            .Include(p => p.CurrentLeague)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
            return new Response<UserProfileDto>(HttpStatusCode.NotFound, "Профиль не найден");

        if (profile.LastTestDate == null)
        {
            var latestSession = await context.TestSessions
                .Where(s => s.UserId == profile.UserId && s.IsCompleted)
                .OrderByDescending(s => s.FinishedAt)
                .FirstOrDefaultAsync();
            
            if (latestSession != null)
            {
                profile.LastTestDate = latestSession.FinishedAt;
                await context.SaveChangesAsync();
            }
        }

        var dto = MapToDto(profile);

        var lastSessions = await context.TestSessions
            .Include(s => s.Subject)
            .Where(s => s.UserId == profile.UserId && s.IsCompleted)
            .OrderByDescending(s => s.FinishedAt)
            .Take(5)
            .Select(s => new Application.DTOs.Testing.TestSessionDto
            {
                Id = s.Id,
                Mode = s.Mode,
                ClusterId = s.ClusterId,
                ComponentType = s.ComponentType,
                ClusterNumber = s.ClusterNumber,
                TotalScore = s.TotalScore,
                SubjectId = s.SubjectId,
                SubjectName = s.Subject != null ? s.Subject.Name : null,
                StartedAt = s.StartedAt,
                FinishedAt = s.FinishedAt
            })
            .ToListAsync();

        dto.LastTestResults = lastSessions;
        return new Response<UserProfileDto>(dto);
    }

    public async Task<Response<bool>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var profile = await context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        
        if (profile == null)
            return new Response<bool>(HttpStatusCode.NotFound, "Профиль не найден");

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

        if (request.SchoolId.HasValue && profile.SchoolId != request.SchoolId)
        {
            if (profile.SchoolId.HasValue)
            {
                await schoolScoreService.UpdateStudentCountAsync(profile.SchoolId.Value, -1);
            }
            
            await schoolScoreService.UpdateStudentCountAsync(request.SchoolId.Value, 1);
            profile.SchoolId = request.SchoolId;
        }

        if (request.Grade.HasValue)
            profile.Grade = request.Grade;

        if (request.ClusterId.HasValue)
            profile.ClusterId = request.ClusterId;

        if (!string.IsNullOrWhiteSpace(request.TargetUniversity))
            profile.TargetUniversity = request.TargetUniversity;

        if (!string.IsNullOrWhiteSpace(request.TargetFaculty))
            profile.TargetFaculty = request.TargetFaculty;

        if (request.TargetUniversityId.HasValue)
        {
            var university = await context.Universities.FindAsync(request.TargetUniversityId.Value);
            if (university != null)
                profile.TargetUniversity = university.Name;
        }

        if (request.TargetFacultyId.HasValue)
        {
            var faculty = await context.Faculties.FindAsync(request.TargetFacultyId.Value);
            if (faculty != null)
                profile.TargetFaculty = faculty.Name;
        }

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
        
        return new Response<bool>(true) { Message = "Профиль успешно обновлен" };
    }

    public async Task<Response<UserProfileDto>> GetProfileByUsernameAsync(string username)
    {
        var user = await userManager.FindByNameAsync(username);
        if (user == null)
            return new Response<UserProfileDto>(HttpStatusCode.NotFound, "Пользователь не найден");

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
            LastTestDate = profile.LastTestDate,
            RegistrationDate = profile.User?.CreatedAt ?? DateTime.MinValue
        };
    }
}
