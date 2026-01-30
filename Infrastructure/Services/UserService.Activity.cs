using Application.DTOs.User;
using Application.Interfaces;
using Application.Responses;
using Domain.Entities.Users;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infrastructure.Services;

public partial class UserService
{
    public async Task<Response<UserActivityDto>> GetUserActivityAsync(Guid userId)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null)
            return new Response<UserActivityDto>(HttpStatusCode.NotFound, "Корбар ёфт нашуд");

        var today = DateTime.UtcNow.Date;

        // 1. Get all login dates for Streak Calculation
        var allLoginDates = await context.UserLoginActivities
            .Where(a => a.UserId == userId)
            .Select(a => a.LoginDate.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();

        var currentStreak = CalculateCurrentStreak(allLoginDates, today);
        var longestStreak = CalculateLongestStreak(allLoginDates);

        // 2. Get data for Current Month Calendar
        var startOfMonth = new DateTime(today.Year, today.Month, 1);
        var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
        
        // Filter from in-memory list avoids 2nd DB call, efficient enough for now
        var monthLogins = allLoginDates
            .Where(d => d >= startOfMonth && d < startOfMonth.AddMonths(1))
            .ToHashSet();

        var activityDays = new List<ActivityDayDto>();
        for (int i = 1; i <= daysInMonth; i++)
        {
            var date = new DateTime(today.Year, today.Month, i);
            
            activityDays.Add(new ActivityDayDto
            {
                Day = i,
                IsActive = monthLogins.Contains(date),
                IsToday = date == today
            });
        }

        var result = new UserActivityDto
        {
            CurrentStreak = currentStreak,
            LongestStreak = longestStreak,
            ActivityDays = activityDays
        };

        return new Response<UserActivityDto>(result);
    }

    public async Task RecordLoginActivityAsync(Guid userId)
    {
        var today = DateTime.UtcNow.Date;
        
        var exists = await context.UserLoginActivities
            .AnyAsync(a => a.UserId == userId && a.LoginDate.Date == today);

        if (!exists)
        {
            var activity = new UserLoginActivity
            {
                UserId = userId,
                LoginDate = DateTime.UtcNow
            };
            
            context.UserLoginActivities.Add(activity);
            await context.SaveChangesAsync();
        }
    }

    private int CalculateCurrentStreak(List<DateTime> loginDates, DateTime today)
    {
        if (loginDates.Count == 0)
            return 0;

        if (!loginDates.Contains(today) && !loginDates.Contains(today.AddDays(-1)))
            return 0;

        int streak = 0;
        var checkDate = today;

        if (!loginDates.Contains(today))
            checkDate = today.AddDays(-1);

        while (loginDates.Contains(checkDate))
        {
            streak++;
            checkDate = checkDate.AddDays(-1);
        }

        return streak;
    }

    private int CalculateLongestStreak(List<DateTime> loginDates)
    {
        if (loginDates.Count == 0)
            return 0;

        int longestStreak = 1;
        int currentStreakCount = 1;

        for (int i = 1; i < loginDates.Count; i++)
        {
            if ((loginDates[i] - loginDates[i - 1]).Days == 1)
            {
                currentStreakCount++;
                longestStreak = Math.Max(longestStreak, currentStreakCount);
            }
            else
            {
                currentStreakCount = 1;
            }
        }

        return longestStreak;
    }

    public async Task<Response<TestActivityStatsDto>> GetTestActivityAsync(Guid userId, int days = 30)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null)
            return new Response<TestActivityStatsDto>(HttpStatusCode.NotFound, "Корбар ёфт нашуд");

        if (days != 30 && days != 60 && days != 90)
            days = 30;

        var endDate = DateTime.UtcNow.Date;
        var startDate = endDate.AddDays(-(days - 1));

        var sessions = await context.TestSessions
            .Include(s => s.Answers)
            .Where(s => s.UserId == userId && s.StartedAt >= startDate && s.StartedAt <= endDate.AddDays(1))
            .ToListAsync();

        // Get duel statistics
        var duels = await context.DuelMatches
            .Where(d => (d.Player1Id == userId || d.Player2Id == userId) 
                     && d.FinishedAt != null 
                     && d.FinishedAt >= startDate 
                     && d.FinishedAt <= endDate.AddDays(1))
            .ToListAsync();

        var totalDuels = duels.Count;
        var totalDuelWins = duels.Count(d => d.WinnerId == userId);

        var dailyStats = new List<DailyTestCountDto>();
        var totalCorrectAnswers = 0;
        var totalAnswers = 0;

        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            
            var daysSessions = sessions.Where(s => s.StartedAt.Date == date).ToList();
            
            var dayCorrect = daysSessions.Sum(s => s.Answers.Count(a => a.IsCorrect));
            var dayTotal = daysSessions.Sum(s => s.Answers.Count);
            
            totalCorrectAnswers += dayCorrect;
            totalAnswers += dayTotal;

            dailyStats.Add(new DailyTestCountDto
            {
                Date = date,
                TotalAnswers = dayTotal,
                CorrectAnswers = dayCorrect,
                IncorrectAnswers = dayTotal - dayCorrect
            });
        }

        var result = new TestActivityStatsDto
        {
            TotalTests = sessions.Count,
            TotalDuels = totalDuels,
            TotalDuelWins = totalDuelWins,
            OverallCorrectPercentage = totalAnswers > 0 ? Math.Round((double)totalCorrectAnswers / totalAnswers * 100, 1) : 0,
            DailyStats = dailyStats
        };

        return new Response<TestActivityStatsDto>(result);
    }
}
