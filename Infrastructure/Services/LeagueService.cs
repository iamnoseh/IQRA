using Application.Interfaces;
using Application.DTOs.Gamification;
using Application.Responses;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class LeagueService(ApplicationDbContext context, IGamificationService gamificationService) : ILeagueService
{
    public async Task ProcessWeeklyLeaguesAsync()
    {
        await gamificationService.ProcessWeeklyLeagueAsync();
    }

    public async Task<Response<List<LeagueDto>>> GetLeaguesAsync()
    {
        var leagues = await context.Leagues
            .OrderBy(l => l.Id)
            .Select(l => new LeagueDto
            {
                Id = l.Id,
                Name = l.Name,
                MinXP = l.MinXP,
                Color = l.Color,
                PromotionThreshold = l.PromotionThreshold,
                RelegationThreshold = l.RelegationThreshold
            })
            .ToListAsync();

        return new Response<List<LeagueDto>>(leagues);
    }

    public async Task<Response<List<LeagueStandingDto>>> GetStandingsAsync(Guid userId, int leagueId)
    {
        var users = await context.UserProfiles
            .Where(p => p.CurrentLeagueId == leagueId)
            .OrderByDescending(p => p.WeeklyXP)
            .ToListAsync();

        var standings = users.Select((p, index) => new LeagueStandingDto
        {
            UserId = p.UserId,
            UserFullName = $"{p.FirstName} {p.LastName}",
            AvatarUrl = p.AvatarUrl,
            WeeklyXP = p.WeeklyXP,
            Rank = index + 1,
            IsCurrentUser = p.UserId == userId
        }).ToList();

        return new Response<List<LeagueStandingDto>>(standings);
    }
}
