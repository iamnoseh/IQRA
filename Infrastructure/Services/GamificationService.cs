using Application.Constants;
using Application.Interfaces;
using Domain.Entities.Education;
using Domain.Entities.Testing;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class GamificationService : IGamificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IScoringService _scoringService;

    public GamificationService(ApplicationDbContext context, IScoringService scoringService)
    {
        _context = context;
        _scoringService = scoringService;
    }

    public Task<int> CalculateXpAsync(Question question, bool isCorrect)
    {
        if (!isCorrect)
            return Task.FromResult(ScoringConstants.XpConsolation);

        int xp = question.Difficulty switch
        {
            DifficultyLevel.Easy => ScoringConstants.XpEasy,
            DifficultyLevel.Medium => ScoringConstants.XpMedium,
            DifficultyLevel.Hard => ScoringConstants.XpHard,
            _ => ScoringConstants.XpEasy
        };

        return Task.FromResult(xp);
    }

    public async Task<int> ProcessTestSessionEndAsync(Guid userId, TestSession session)
    {
        var userProfile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (userProfile == null)
            return 0;

        var questions = await _context.Questions
            .Where(q => session.Answers.Select(a => a.QuestionId).Contains(q.Id))
            .ToListAsync();

        var questionMap = questions.ToDictionary(q => q.Id);

        int totalXp = 0;
        foreach (var answer in session.Answers)
        {
            if (questionMap.TryGetValue(answer.QuestionId, out var question))
            {
                int xp = await CalculateXpAsync(question, answer.IsCorrect);
                totalXp += xp;
            }
        }

        totalXp += ScoringConstants.XpTestCompletionBonus;

        userProfile.XP += totalXp;

        await _context.SaveChangesAsync();

        return totalXp;
    }

    public async Task ProcessDuelResultAsync(Guid winnerUserId, Guid loserUserId)
    {
        var winnerProfile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == winnerUserId);
        
        var loserProfile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == loserUserId);

        if (winnerProfile == null || loserProfile == null)
            return;

        winnerProfile.EloRating += ScoringConstants.EloWin;
        loserProfile.EloRating = Math.Max(0, loserProfile.EloRating - ScoringConstants.EloLoss);

        await _context.SaveChangesAsync();
    }

    public async Task ProcessWeeklyLeagueAsync()
    {
        var leagues = await _context.Leagues.ToListAsync();

        foreach (var league in leagues)
        {
            var usersInLeague = await _context.UserProfiles
                .Where(p => p.CurrentLeagueId == league.Id)
                .OrderByDescending(p => p.XP)
                .ToListAsync();

            if (usersInLeague.Count == 0)
                continue;

            int totalUsers = usersInLeague.Count;
            int promoteCount = (int)(totalUsers * ScoringConstants.LeaguePromotionPercent);
            int relegateCount = (int)(totalUsers * ScoringConstants.LeagueRelegationPercent);

            var higherLeague = await _context.Leagues
                .Where(l => l.Id > league.Id)
                .OrderBy(l => l.Id)
                .FirstOrDefaultAsync();

            var lowerLeague = await _context.Leagues
                .Where(l => l.Id < league.Id)
                .OrderByDescending(l => l.Id)
                .FirstOrDefaultAsync();

            if (higherLeague != null)
            {
                for (int i = 0; i < promoteCount; i++)
                {
                    usersInLeague[i].CurrentLeagueId = higherLeague.Id;
                }
            }

            if (lowerLeague != null)
            {
                for (int i = 0; i < relegateCount; i++)
                {
                    usersInLeague[totalUsers - 1 - i].CurrentLeagueId = lowerLeague.Id;
                }
            }
        }

        await _context.SaveChangesAsync();
    }
}
