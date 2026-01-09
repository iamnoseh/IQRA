using Application.DTOs.Testing;
using Application.Interfaces;
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
