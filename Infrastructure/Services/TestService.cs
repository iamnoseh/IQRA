using Application.DTOs.Testing;
using Application.Interfaces;
using Application.Responses;
using Domain.Entities.Testing;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace Infrastructure.Services;

public class TestService(ApplicationDbContext context, IQuestionService questionService) : ITestService
{
    public async Task<Response<Guid>> StartTestAsync(Guid userId, StartTestRequest request)
    {
        var template = await context.TestTemplates
            .FirstOrDefaultAsync(t => t.ClusterNumber == request.ClusterNumber);
        
        if (template == null)
            return new Response<Guid>(HttpStatusCode.BadRequest, "Template барои ин кластер ёфт нашуд");

        var distribution = JsonSerializer.Deserialize<Dictionary<string, int>>(template.SubjectDistributionJson);
        var questionIds = new List<long>();
        
        foreach (var (subjectIdStr, count) in distribution!)
        {
            var subjectId = int.Parse(subjectIdStr);
            var questions = await questionService.GetRandomQuestionsAsync(subjectId, count);
            questionIds.AddRange(questions.Select(q => q.Id));
        }

        if (questionIds.Count == 0)
            return new Response<Guid>(HttpStatusCode.BadRequest, "Саволҳо барои ин кластер ёфт нашуданд");

        var session = new TestSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Mode = request.Mode,
            ClusterNumber = request.ClusterNumber,
            TestTemplateId = template.Id,
            QuestionIdsJson = JsonSerializer.Serialize(questionIds),
            StartedAt = DateTime.UtcNow,
            IsCompleted = false
        };

        context.TestSessions.Add(session);
        await context.SaveChangesAsync();

        return new Response<Guid>(session.Id);
    }

    public async Task<Response<List<QuestionWithAnswersDto>>> GetTestQuestionsAsync(Guid testSessionId)
    {
        var session = await context.TestSessions.FindAsync(testSessionId);
        if (session == null)
            return new Response<List<QuestionWithAnswersDto>>(HttpStatusCode.NotFound, "Тест ёфт нашуд");

        var questionIds = JsonSerializer.Deserialize<List<long>>(session.QuestionIdsJson);
        
        var questions = await context.Questions
            .Include(q => q.Subject)
            .Include(q => q.Answers)
            .Where(q => questionIds!.Contains(q.Id))
            .Select(q => new QuestionWithAnswersDto
            {
                Id = q.Id,
                Content = q.Content,
                ImageUrl = q.ImageUrl,
                SubjectId = q.SubjectId,
                SubjectName = q.Subject.Name,
                Answers = q.Answers.Select(a => new AnswerOptionDto 
                { 
                    Id = a.Id, 
                    Text = a.Text 
                }).ToList()
            })
            .ToListAsync();

        foreach (var question in questions)
        {
            question.Answers = question.Answers.OrderBy(_ => Guid.NewGuid()).ToList();
        }

        return new Response<List<QuestionWithAnswersDto>>(questions);
    }

    public async Task<Response<bool>> SubmitAnswerAsync(Guid userId, SubmitAnswerRequest request)
    {
        var session = await context.TestSessions.FindAsync(request.TestSessionId);
        if (session == null || session.UserId != userId)
            return new Response<bool>(HttpStatusCode.NotFound, "Тест ёфт нашуд");

        if (session.IsCompleted)
            return new Response<bool>(HttpStatusCode.BadRequest, "Тест аллакай тамом шудааст");

        var existingAnswer = await context.UserAnswers
            .FirstOrDefaultAsync(ua => ua.TestSessionId == request.TestSessionId 
                && ua.QuestionId == request.QuestionId);

        if (existingAnswer != null)
            return new Response<bool>(HttpStatusCode.BadRequest, "Шумо аллакай ба ин савол ҷавоб додаед");

        var correctAnswer = await context.AnswerOptions
            .FirstOrDefaultAsync(a => a.QuestionId == request.QuestionId && a.IsCorrect);

        var isCorrect = correctAnswer?.Id == request.ChosenAnswerId;

        var userAnswer = new UserAnswer
        {
            TestSessionId = request.TestSessionId,
            QuestionId = request.QuestionId,
            ChosenAnswerId = request.ChosenAnswerId,
            IsCorrect = isCorrect,
            TimeSpentSeconds = request.TimeSpentSeconds
        };

        context.UserAnswers.Add(userAnswer);
        await context.SaveChangesAsync();

        return new Response<bool>(true);
    }

    public async Task<Response<TestResultDto>> FinishTestAsync(Guid userId, Guid testSessionId)
    {
        var session = await context.TestSessions
            .Include(t => t.Answers)
            .FirstOrDefaultAsync(t => t.Id == testSessionId && t.UserId == userId);

        if (session == null)
            return new Response<TestResultDto>(HttpStatusCode.NotFound, "Тест ёфт нашуд");

        if (session.IsCompleted)
            return new Response<TestResultDto>(HttpStatusCode.BadRequest, "Тест аллакай тамом шудааст");

        session.FinishedAt = DateTime.UtcNow;
        session.IsCompleted = true;
        session.TotalScore = session.Answers.Count(a => a.IsCorrect);

        await context.SaveChangesAsync();

        var totalQuestions = session.Answers.Count;
        var percentage = totalQuestions > 0 ? (double)session.TotalScore / totalQuestions * 100 : 0;

        var result = new TestResultDto
        {
            TestSessionId = testSessionId,
            TotalScore = session.TotalScore,
            CorrectAnswers = session.Answers.Count(a => a.IsCorrect),
            TotalQuestions = totalQuestions,
            Percentage = percentage,
            IsPassed = percentage >= 60,
            XPEarned = session.TotalScore * 10,
            Results = session.Answers.Select(a => new QuestionResultDto
            {
                QuestionId = a.QuestionId,
                IsCorrect = a.IsCorrect,
                UserAnswerId = a.ChosenAnswerId,
                CorrectAnswerId = context.AnswerOptions
                    .Where(ao => ao.QuestionId == a.QuestionId && ao.IsCorrect)
                    .Select(ao => ao.Id)
                    .FirstOrDefault()
            }).ToList()
        };

        return new Response<TestResultDto>(result);
    }

    public async Task<Response<List<TestSessionDto>>> GetUserTestHistoryAsync(Guid userId, int page, int pageSize)
    {
        var sessions = await context.TestSessions
            .Where(t => t.UserId == userId && t.IsCompleted)
            .OrderByDescending(t => t.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TestSessionDto
            {
                Id = t.Id,
                Mode = t.Mode,
                ClusterNumber = t.ClusterNumber,
                TotalScore = t.TotalScore,
                StartedAt = t.StartedAt,
                FinishedAt = t.FinishedAt
            })
            .ToListAsync();

        return new Response<List<TestSessionDto>>(sessions);
    }
}
