using Application.DTOs.Gamification;
using Application.Responses;

namespace Application.Interfaces;

public interface ILeagueService
{
    Task ProcessWeeklyLeaguesAsync();
    Task<Response<List<LeagueDto>>> GetLeaguesAsync();
    Task<Response<List<LeagueStandingDto>>> GetStandingsAsync(Guid userId, int leagueId);
}
