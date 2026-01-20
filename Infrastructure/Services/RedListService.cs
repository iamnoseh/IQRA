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
                ConsecutiveCorrectCount = rl.ConsecutiveCorrectCount,
                Answers = rl.Question.Answers.Select(a => new AnswerOptionDto
                {
                    Id = a.Id,
                    Text = a.Text,
                    MatchPairText = a.MatchPairText
                }).ToList()
            })
            .ToListAsync();

        foreach (var item in redList)
        {
            item.Answers = item.Answers.OrderBy(_ => Guid.NewGuid()).ToList();
        }

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
                feedback.XPEarned = 50; // Bonus for clearing from Red List

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

    public async Task<Response<int>> GetRedListCountAsync(Guid userId)
    {
        var count = await context.RedListQuestions.CountAsync(rl => rl.UserId == userId);
        return new Response<int>(count);
    }
}
