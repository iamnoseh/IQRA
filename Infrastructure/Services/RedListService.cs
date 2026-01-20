using Application.DTOs.Testing;
using Application.Interfaces;
using Application.Responses;
using Domain.Entities.Testing;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infrastructure.Services;

public class RedListService(ApplicationDbContext context) : IRedListService
{
    public async Task AddToRedListAsync(Guid userId, long questionId)
    {
        var alreadyExists = await context.RedListQuestions
            .AnyAsync(rl => rl.UserId == userId && rl.QuestionId == questionId);

        if (alreadyExists) return;

        var redListQuestion = new RedListQuestion
        {
            UserId = userId,
            QuestionId = questionId,
            ConsecutiveCorrectCount = 0,
            AddedAt = DateTime.UtcNow
        };

        context.RedListQuestions.Add(redListQuestion);
        await context.SaveChangesAsync();
    }

    public async Task<Response<List<RedListQuestionDto>>> GetRedListAsync(Guid userId)
    {
        var redList = await context.RedListQuestions
            .Include(rl => rl.Question)
                .ThenInclude(q => q.Subject)
            .Include(rl => rl.Question)
                .ThenInclude(q => q.Answers)
            .Where(rl => rl.UserId == userId)
            .OrderBy(rl => rl.AddedAt)
            .Select(rl => new RedListQuestionDto
            {
                Id = rl.Id,
                QuestionId = rl.QuestionId,
                Content = rl.Question.Content,
                ImageUrl = rl.Question.ImageUrl,
                SubjectName = rl.Question.Subject.Name,
                Topic = rl.Question.Topic ?? "Умумӣ",
                AddedAt = rl.AddedAt,
                ConsecutiveCorrectCount = rl.ConsecutiveCorrectCount
            })
            .ToListAsync();

        return new Response<List<RedListQuestionDto>>(redList);
    }

    public async Task<Response<RedListPracticeFeedbackDto>> SubmitPracticeAnswerAsync(Guid userId, SubmitRedListAnswerRequest request)
    {
        var redListQuestion = await context.RedListQuestions
            .Include(rl => rl.Question)
                .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(rl => rl.Id == request.RedListQuestionId && rl.UserId == userId);

        if (redListQuestion == null)
            return new Response<RedListPracticeFeedbackDto>(HttpStatusCode.NotFound, "Савол дар Рӯйхати Сурх ёфт нашуд");

        var question = redListQuestion.Question;
        bool isCorrect = false;
        string? correctAnswerText = null;

        if (question.Type == QuestionType.SingleChoice)
        {
            var correctOption = question.Answers.FirstOrDefault(a => a.IsCorrect);
            correctAnswerText = correctOption?.Text;
            isCorrect = correctOption?.Id == request.ChosenAnswerId;
        }
        else if (question.Type == QuestionType.ClosedAnswer)
        {
            var correctOption = question.Answers.FirstOrDefault(a => a.IsCorrect);
            correctAnswerText = correctOption?.Text;
            isCorrect = correctOption?.Text.Trim().Equals(request.TextResponse?.Trim(), StringComparison.OrdinalIgnoreCase) ?? false;
        }

        redListQuestion.LastPracticedAt = DateTime.UtcNow;

        var feedback = new RedListPracticeFeedbackDto
        {
            IsCorrect = isCorrect,
            CorrectAnswerText = isCorrect ? null : correctAnswerText
        };

        if (isCorrect)
        {
            redListQuestion.ConsecutiveCorrectCount++;
            if (redListQuestion.ConsecutiveCorrectCount >= 3)
            {
                context.RedListQuestions.Remove(redListQuestion);
                feedback.IsRemoved = true;
                feedback.XPEarned = 50; 

                var profile = await context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
                if (profile != null)
                {
                    profile.XP += 50;
                }
            }
        }
        else
        {
            redListQuestion.ConsecutiveCorrectCount = 0;
        }

        feedback.ConsecutiveCorrectCount = redListQuestion.ConsecutiveCorrectCount;

        await context.SaveChangesAsync();
        return new Response<RedListPracticeFeedbackDto>(feedback);
    }

    public async Task<Response<RedListDashboardDto>> GetRedListDashboardAsync(Guid userId, int? subjectId = null)
    {
        var query = context.RedListQuestions
            .Include(rl => rl.Question)
                .ThenInclude(q => q.Subject)
            .Where(rl => rl.UserId == userId);

        if (subjectId.HasValue)
        {
            query = query.Where(rl => rl.Question.SubjectId == subjectId.Value);
        }

        var questions = await query.ToListAsync();

        var today = DateTime.UtcNow.Date;
        
        var xpToday = await context.UserAnswers
            .Include(ua => ua.TestSession)
            .Where(ua => ua.TestSession.UserId == userId && ua.TestSession.StartedAt >= today && ua.IsCorrect)
            .CountAsync() * 10;

        var stats = new RedListStatsDto
        {
            TotalQuestions = questions.Count,
            NewQuestionsToday = questions.Count(q => q.AddedAt >= today),
            ReadyToRemoveCount = questions.Count(q => q.ConsecutiveCorrectCount >= 2),
            XPToday = xpToday, 
            XPIncreasePercent = 15, 
            RemovedTodayCount = 0 
        };
        var chartData = new List<RedListChartPointDto>();
        for (int i = 6; i >= 0; i--)
        {
            chartData.Add(new RedListChartPointDto
            {
                DateLabel = today.AddDays(-i).ToString("dd MMM"),
                Value = Math.Max(0, questions.Count - i * 2) 
            });
        }
        var questionDtos = questions
            .OrderByDescending(q => q.ConsecutiveCorrectCount)
            .ThenByDescending(q => q.AddedAt)
            .Select(rl => new RedListQuestionDto
            {
                Id = rl.Id,
                QuestionId = rl.Question.Id,
                Content = rl.Question.Content,
                ImageUrl = rl.Question.ImageUrl,
                SubjectName = rl.Question.Subject.Name,
                Topic = rl.Question.Topic ?? "Умумӣ",
                AddedAt = rl.AddedAt,
                ConsecutiveCorrectCount = rl.ConsecutiveCorrectCount
            }).ToList();

        var dashboard = new RedListDashboardDto
        {
            Stats = stats,
            ChartData = chartData,
            ActiveQuestions = questionDtos
        };

        return new Response<RedListDashboardDto>(dashboard);
    }

    public async Task<Response<int>> GetRedListCountAsync(Guid userId)
    {
        var count = await context.RedListQuestions.CountAsync(rl => rl.UserId == userId);
        return new Response<int>(count);
    }

    public async Task ProcessAnswerAsync(Guid userId, long questionId, bool isCorrect)
    {
        var redListEntry = await context.RedListQuestions
            .FirstOrDefaultAsync(rl => rl.UserId == userId && rl.QuestionId == questionId);

        if (redListEntry != null)
        {
            redListEntry.LastPracticedAt = DateTime.UtcNow;
            
            if (isCorrect)
            {
                redListEntry.ConsecutiveCorrectCount++;
                if (redListEntry.ConsecutiveCorrectCount >= 3)
                {
                    context.RedListQuestions.Remove(redListEntry);
                    
                    var profile = await context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
                    if (profile != null)
                    {
                        profile.XP += 50;
                    }
                }
            }
            else
            {
                redListEntry.ConsecutiveCorrectCount = 0;
            }
        }
        else if (!isCorrect)
        {
            var newEntry = new RedListQuestion
            {
                UserId = userId,
                QuestionId = questionId,
                ConsecutiveCorrectCount = 0,
                AddedAt = DateTime.UtcNow
            };
            context.RedListQuestions.Add(newEntry);
        }

        await context.SaveChangesAsync();
    }
}
