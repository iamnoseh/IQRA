using Application.DTOs.Testing;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class QuestionService(ApplicationDbContext context) : IQuestionService
{
    public async Task<List<QuestionWithAnswersDto>> GetRandomQuestionsAsync(int subjectId, int count)
    {
        return await context.Questions
            .Where(q => q.SubjectId == subjectId)
            .OrderBy(_ => Guid.NewGuid())
            .Take(count)
            .Include(q => q.Subject)
            .Select(q => new QuestionWithAnswersDto
            {
                Id = q.Id,
                Content = q.Content,
                ImageUrl = q.ImageUrl,
                SubjectId = q.SubjectId,
                SubjectName = q.Subject.Name
            })
            .ToListAsync();
    }

    public async Task<List<QuestionWithAnswersDto>> GetTestQuestionsAsync(Guid userId, int subjectId, int count, QuestionType? questionType = null)
    {
        var redListQuestions = await context.RedListQuestions
            .Include(rl => rl.Question)
                .ThenInclude(q => q.Subject)
            .Where(rl => rl.UserId == userId && rl.Question.SubjectId == subjectId)
            .Select(rl => new
            {
                rl.Question,
                rl.ConsecutiveCorrectCount
            })
            .Take(3)
            .ToListAsync();

        if (questionType.HasValue)
        {
            redListQuestions = redListQuestions
                .Where(x => x.Question.Type == questionType.Value)
                .ToList();
        }

        var questionsRef = redListQuestions.Select(x => new QuestionWithAnswersDto
        {
            Id = x.Question.Id,
            Content = x.Question.Content,
            ImageUrl = x.Question.ImageUrl,
            SubjectId = x.Question.SubjectId,
            SubjectName = x.Question.Subject.Name,
            IsInRedList = true,
            RedListCorrectCount = x.ConsecutiveCorrectCount
        }).ToList();

        var redListIds = redListQuestions.Select(x => x.Question.Id).ToList();
        int remainingCount = count - redListIds.Count;

        if (remainingCount > 0)
        {
            var query = context.Questions
                .Where(q => q.SubjectId == subjectId && !redListIds.Contains(q.Id));

            if (questionType.HasValue)
            {
                query = query.Where(q => q.Type == questionType.Value);
            }

            var randomQuestions = await query
                .OrderBy(_ => Guid.NewGuid())
                .Take(remainingCount)
                .Include(q => q.Subject)
                .Select(q => new QuestionWithAnswersDto
                {
                    Id = q.Id,
                    Content = q.Content,
                    ImageUrl = q.ImageUrl,
                    SubjectId = q.SubjectId,
                    SubjectName = q.Subject.Name,
                    IsInRedList = false,
                    RedListCorrectCount = 0
                })
                .ToListAsync();

            questionsRef.AddRange(randomQuestions);
        }

        return questionsRef.OrderBy(_ => Guid.NewGuid()).ToList();
    }

    public async Task<QuestionWithAnswersDto?> GetQuestionByIdAsync(long id)
    {
        return await context.Questions
            .Where(q => q.Id == id)
            .Include(q => q.Subject)
            .Select(q => new QuestionWithAnswersDto
            {
                Id = q.Id,
                Content = q.Content,
                ImageUrl = q.ImageUrl,
                SubjectId = q.SubjectId,
                SubjectName = q.Subject.Name
            })
            .FirstOrDefaultAsync();
    }
}
