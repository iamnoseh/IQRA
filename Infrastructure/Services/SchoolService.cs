using Application.DTOs.Reference;
using Application.Interfaces;
using Application.Responses;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class SchoolService(ApplicationDbContext context) : ISchoolService
{
    public async Task<Response<SchoolLeaderboardResponse>> GetLeaderboardAsync(Guid? userId)
    {
        int? userSchoolId = null;

        if (userId.HasValue)
        {
            userSchoolId = await context.UserProfiles
                .Where(up => up.UserId == userId.Value)
                .Select(up => up.SchoolId)
                .FirstOrDefaultAsync();
        }

        var rankedSchools = await context.Schools
            .AsNoTracking()
            .Where(s => s.StudentCount > 0)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Province,
                s.District,
                s.TotalXP,
                s.StudentCount,
                AverageXP = (double)s.TotalXP / s.StudentCount
            })
            .OrderByDescending(s => s.AverageXP)
            .ToListAsync();

        var rankedList = rankedSchools.Select((s, index) => new SchoolLeaderboardDto
        {
            Rank = index + 1,
            SchoolId = s.Id,
            SchoolName = s.Name,
            Province = s.Province,
            District = s.District,
            TotalXP = s.TotalXP,
            StudentCount = s.StudentCount,
            AverageXP = Math.Round(s.AverageXP, 2)
        }).ToList();

        var topSchools = rankedList.Take(20).ToList();

        SchoolLeaderboardDto? userSchool = null;
        if (userSchoolId.HasValue)
        {
            userSchool = rankedList.FirstOrDefault(s => s.SchoolId == userSchoolId.Value);
        }

        var response = new SchoolLeaderboardResponse
        {
            Leaderboard = topSchools,
            UserSchool = userSchool
        };

        return new Response<SchoolLeaderboardResponse>(response);
    }
}
