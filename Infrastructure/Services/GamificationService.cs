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
    private readonly INotificationService _notificationService;

    public GamificationService(
        ApplicationDbContext context, 
        IScoringService scoringService,
        INotificationService notificationService)
    {
        _context = context;
        _scoringService = scoringService;
        _notificationService = notificationService;
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
        userProfile.WeeklyXP += totalXp;

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
        var leagues = await _context.Leagues.OrderBy(l => l.Id).ToListAsync();
        var diamondLeague = leagues.FirstOrDefault(l => l.Name.Contains("Diamond", StringComparison.OrdinalIgnoreCase));

        // Special Diamond League processing
        if (diamondLeague != null)
        {
            var diamondUsers = await _context.UserProfiles
                .Where(p => p.CurrentLeagueId == diamondLeague.Id)
                .OrderByDescending(p => p.WeeklyXP)
                .ToListAsync();

            if (diamondUsers.Count > 0)
            {
                var winner = diamondUsers[0];
                
                winner.DiamondWinStreak++;
                
                // Notify the winner
                await _notificationService.SendToUserAsync(
                    winner.UserId, 
                    "ТАБРИК! 🏆", 
                    $"Шумо ғолиби Лигаи Алмос шуدید! WeeklyXP: {winner.WeeklyXP}");

                if (winner.DiamondWinStreak >= 3)
                {
                    // Grand Prize Trigger
                    await _notificationService.CreateSystemAlertAsync(
                        "SUPER WINNER!", 
                        $"Корбар {winner.UserId} 3 бор пайиҳам дар Лигаи Алмос ғолиб шуд!");

                    await _notificationService.SendToUserAsync(
                        winner.UserId,
                        "🏆 GRAND PRIZE 🏆", 
                        "Шумо 3 бор Чемпиони Лигаи Алмос шудед! Барои гирифтани туҳфа мо бо шумо тамос мегирем.");

                    winner.DiamondWinStreak = 0;
                }

                // Reset streak for all other Diamond users
                foreach (var user in diamondUsers.Skip(1))
                {
                    user.DiamondWinStreak = 0;
                }
            }
        }

        // Standard league processing (Bronze to Platinum)
        foreach (var league in leagues.Where(l => l.Id != diamondLeague?.Id))
        {
            var usersInLeague = await _context.UserProfiles
                .Where(p => p.CurrentLeagueId == league.Id)
                .OrderByDescending(p => p.WeeklyXP)
                .ToListAsync();

            if (usersInLeague.Count == 0)
                continue;

            int totalUsers = usersInLeague.Count;
            int promoteCount = (int)(totalUsers * league.PromotionThreshold);
            int relegateCount = (int)(totalUsers * league.RelegationThreshold);

            var higherLeague = leagues.FirstOrDefault(l => l.Id > league.Id);
            var lowerLeague = leagues.LastOrDefault(l => l.Id < league.Id);

            if (higherLeague != null)
            {
                for (int i = 0; i < Math.Min(promoteCount, usersInLeague.Count); i++)
                {
                    usersInLeague[i].CurrentLeagueId = higherLeague.Id;
                    await _notificationService.SendToUserAsync(
                        usersInLeague[i].UserId, 
                        "ПЕШРАВӢ! 🔝", 
                        $"Шумо ба лигаи нав гузаштед: {higherLeague.Name}!");
                }
            }

            if (lowerLeague != null)
            {
                for (int i = 0; i < Math.Min(relegateCount, usersInLeague.Count); i++)
                {
                    int index = totalUsers - 1 - i;
                    if (index >= 0 && index < usersInLeague.Count)
                    {
                        usersInLeague[index].CurrentLeagueId = lowerLeague.Id;
                    }
                }
            }
        }

        // Global WeeklyXP reset for ALL users
        var allProfiles = await _context.UserProfiles.ToListAsync();
        foreach (var profile in allProfiles)
        {
            profile.WeeklyXP = 0;
        }

        await _context.SaveChangesAsync();
    }
}
