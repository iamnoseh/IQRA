using Application.DTOs.Testing.Management;
using Application.DTOs.Education;
using Application.Interfaces;
using Application.Responses;
using Domain.Entities.Education;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infrastructure.Services;

public class QuestionManagementService(ApplicationDbContext context, IFileStorageService fileStorageService) : IQuestionManagementService
{
    public async Task<Response<QuestionImportResultDto>> ImportQuestionsAsync(BulkQuestionImportRequest request)
    {
        var result = new QuestionImportResultDto { TotalQuestions = request.Questions.Count };

        if (request.ValidateOnly)
            return await ValidateImportAsync(request);

        await using var transaction = await context.Database.BeginTransactionAsync();
        
        try
        {
            for (var i = 0; i < request.Questions.Count; i++)
            {
                var dto = request.Questions[i];
                
                var validationError = await ValidateQuestionAsync(dto);
                if (validationError != null)
                {
                    result.Errors.Add(new ImportError
                    {
                        Index = i + 1,
                        QuestionPreview = dto.Content.Length > 50 
                            ? dto.Content.Substring(0, 50) + "..." 
                            : dto.Content,
                        ErrorMessage = validationError.Message,
                        Field = validationError.Field
                    });
                    result.FailedCount++;
                    continue;
                }

                await CreateQuestionInternalAsync(dto);
                result.SuccessCount++;
            }

            await transaction.CommitAsync();
            
            return new Response<QuestionImportResultDto>(result);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new Response<QuestionImportResultDto>(
                HttpStatusCode.InternalServerError, 
                $"Import хато: {ex.Message}"
            );
        }
    }

    public async Task<Response<QuestionImportResultDto>> ValidateImportAsync(BulkQuestionImportRequest request)
    {
        var result = new QuestionImportResultDto { TotalQuestions = request.Questions.Count };

        for (int i = 0; i < request.Questions.Count; i++)
        {
            var dto = request.Questions[i];
            var validationError = await ValidateQuestionAsync(dto);
            
            if (validationError != null)
            {
                result.Errors.Add(new ImportError
                {
                    Index = i + 1,
                    QuestionPreview = dto.Content.Length > 50 
                        ? dto.Content.Substring(0, 50) + "..." 
                        : dto.Content,
                    ErrorMessage = validationError.Message,
                    Field = validationError.Field
                });
                result.FailedCount++;
            }
            else
            {
                result.SuccessCount++;
            }
        }

        return new Response<QuestionImportResultDto>(result);
    }

    private async Task<ValidationError?> ValidateQuestionAsync(QuestionImportDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
            return new ValidationError("Content", "Савол холӣ аст");

        var subjectExists = await context.Subjects.AnyAsync(s => s.Id == dto.SubjectId);
        if (!subjectExists)
            return new ValidationError("SubjectId", $"Фан бо ID {dto.SubjectId} ёфт нашуд");

        if (dto.TopicId.HasValue)
        {
            var topicExists = await context.Topics.AnyAsync(t => t.Id == dto.TopicId.Value);
            if (!topicExists)
                return new ValidationError("TopicId", $"Мавзуъ бо ID {dto.TopicId} ёфт нашуд");
        }

        return dto.Type switch
        {
            QuestionType.SingleChoice => ValidateSingleChoice(dto),
            QuestionType.Matching => ValidateMatching(dto),
            QuestionType.ClosedAnswer => ValidateClosedAnswer(dto),
            _ => new ValidationError("Type", "Навъи савол нодуруст")
        };
    }

    private ValidationError? ValidateSingleChoice(QuestionImportDto dto)
    {
        if (dto.Answers == null || dto.Answers.Count < 2)
            return new ValidationError("Answers", "Ҳадди ақал 2 ҷавоб лозим");

        var correctCount = dto.Answers.Count(a => a.IsCorrect);
        if (correctCount != 1)
            return new ValidationError("Answers", $"Танҳо 1 дурустӣ ҷавоб лозим (ҳозир {correctCount})");

        if (dto.Answers.Any(a => string.IsNullOrWhiteSpace(a.Text)))
            return new ValidationError("Answers", "Ҷавоби холӣ мавҷуд");

        return null;
    }

    private ValidationError? ValidateMatching(QuestionImportDto dto)
    {
        if (dto.Answers == null || dto.Answers.Count < 3)
            return new ValidationError("Answers", "Ҳадди ақал 3 ҷуфт лозим");

        if (dto.Answers.Any(a => string.IsNullOrWhiteSpace(a.MatchPair)))
            return new ValidationError("Answers", "Ҳамаи ҷавобҳо бояд MatchPair дошта бошанд");

        return null;
    }

    private ValidationError? ValidateClosedAnswer(QuestionImportDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CorrectAnswer))
            return new ValidationError("CorrectAnswer", "Ҷавоби дуруст лозим");

        return null;
    }

    private async Task<Question> CreateQuestionInternalAsync(QuestionImportDto dto)
    {
        var question = new Question
        {
            SubjectId = dto.SubjectId,
            TopicId = dto.TopicId,
            Content = dto.Content,
            ImageUrl = dto.ImageUrl,
            Explanation = dto.Explanation,
            Difficulty = dto.Difficulty,
            Type = dto.Type
        };

        context.Questions.Add(question);
        await context.SaveChangesAsync();

        if (dto.Type == QuestionType.SingleChoice || dto.Type == QuestionType.Matching)
        {
            foreach (var answer in dto.Answers!)
            {
                context.AnswerOptions.Add(new AnswerOption
                {
                    QuestionId = question.Id,
                    Text = answer.Text,
                    IsCorrect = answer.IsCorrect
                });
            }
        }
        else if (dto.Type == QuestionType.ClosedAnswer)
        {
            context.AnswerOptions.Add(new AnswerOption
            {
                QuestionId = question.Id,
                Text = dto.CorrectAnswer!,
                IsCorrect = true
            });
        }

        await context.SaveChangesAsync();
        return question;
    }

    public async Task<Response<QuestionDto>> CreateQuestionAsync(CreateQuestionRequest request)
    {
        // Convert CreateQuestionRequest to QuestionImportDto
        var dto = new QuestionImportDto
        {
            SubjectId = request.SubjectId,
            TopicId = request.TopicId,
            Content = request.Content,
            Explanation = request.Explanation,
            Difficulty = request.Difficulty,
            Type = request.Type,
            Answers = request.Answers,
            CorrectAnswer = request.CorrectAnswer
        };

        // Upload image if provided
        if (request.Image != null)
        {
            dto.ImageUrl = await fileStorageService.SaveFileAsync(request.Image, "uploads/questions");
        }

        var validationError = await ValidateQuestionAsync(dto);
        if (validationError != null)
            return new Response<QuestionDto>(HttpStatusCode.BadRequest, validationError.Message);

        var question = await CreateQuestionInternalAsync(dto);
        
        var questionDto = await context.Questions
            .Where(q => q.Id == question.Id)
            .Include(q => q.Subject)
            .Include(q => q.Topic)
            .Select(q => new QuestionDto
            {
                Id = q.Id,
                SubjectId = q.SubjectId,
                SubjectName = q.Subject.Name,
                TopicId = q.TopicId,
                TopicName = q.Topic!.Name,
                Content = q.Content,
                ImageUrl = q.ImageUrl,
                Explanation = q.Explanation,
                Difficulty = q.Difficulty,
                Type = q.Type
            })
            .FirstAsync();

        return new Response<QuestionDto>(questionDto);
    }

    public async Task<Response<QuestionDto>> UpdateQuestionAsync(long id, QuestionImportDto dto)
    {
        var question = await context.Questions
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null)
            return new Response<QuestionDto>(HttpStatusCode.NotFound, "Савол ёфт нашуд");

        var validationError = await ValidateQuestionAsync(dto);
        if (validationError != null)
            return new Response<QuestionDto>(HttpStatusCode.BadRequest, validationError.Message);

        question.SubjectId = dto.SubjectId;
        question.TopicId = dto.TopicId;
        question.Content = dto.Content;
        question.ImageUrl = dto.ImageUrl;
        question.Explanation = dto.Explanation;
        question.Difficulty = dto.Difficulty;
        question.Type = dto.Type;

        context.AnswerOptions.RemoveRange(question.Answers);

        if (dto.Type == QuestionType.SingleChoice || dto.Type == QuestionType.Matching)
        {
            foreach (var answer in dto.Answers!)
            {
                context.AnswerOptions.Add(new AnswerOption
                {
                    QuestionId = question.Id,
                    Text = answer.Text,
                    IsCorrect = answer.IsCorrect
                });
            }
        }
        else if (dto.Type == QuestionType.ClosedAnswer)
        {
            context.AnswerOptions.Add(new AnswerOption
            {
                QuestionId = question.Id,
                Text = dto.CorrectAnswer!,
                IsCorrect = true
            });
        }

        await context.SaveChangesAsync();

        var questionDto = await context.Questions
            .Where(q => q.Id == id)
            .Include(q => q.Subject)
            .Include(q => q.Topic)
            .Select(q => new QuestionDto
            {
                Id = q.Id,
                SubjectId = q.SubjectId,
                SubjectName = q.Subject.Name,
                TopicId = q.TopicId,
                TopicName = q.Topic != null ? q.Topic.Name : null,
                Content = q.Content,
                ImageUrl = q.ImageUrl,
                Explanation = q.Explanation,
                Difficulty = q.Difficulty,
                Type = q.Type
            })
            .FirstAsync();

        return new Response<QuestionDto>(questionDto);
    }

    public async Task<Response<bool>> DeleteQuestionAsync(long id)
    {
        var question = await context.Questions.FindAsync(id);
        if (question == null)
            return new Response<bool>(HttpStatusCode.NotFound, "Савол ёфт нашуд");

        context.Questions.Remove(question);
        await context.SaveChangesAsync();

        return new Response<bool>(true);
    }

    public async Task<Response<List<QuestionDto>>> GetQuestionsBySubjectAsync(int subjectId, int page, int pageSize)
    {
        var questions = await context.Questions
            .Where(q => q.SubjectId == subjectId)
            .Include(q => q.Subject)
            .Include(q => q.Topic)
            .OrderBy(q => q.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(q => new QuestionDto
            {
                Id = q.Id,
                SubjectId = q.SubjectId,
                SubjectName = q.Subject.Name,
                TopicId = q.TopicId,
                TopicName = q.Topic != null ? q.Topic.Name : null,
                Content = q.Content,
                ImageUrl = q.ImageUrl,
                Explanation = q.Explanation,
                Difficulty = q.Difficulty,
                Type = q.Type
            })
            .ToListAsync();

        return new Response<List<QuestionDto>>(questions);
    }

    public async Task<Response<List<QuestionDto>>> GetQuestionsByTopicAsync(int topicId, int page, int pageSize)
    {
        var questions = await context.Questions
            .Where(q => q.TopicId == topicId)
            .Include(q => q.Subject)
            .Include(q => q.Topic)
            .OrderBy(q => q.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(q => new QuestionDto
            {
                Id = q.Id,
                SubjectId = q.SubjectId,
                SubjectName = q.Subject.Name,
                TopicId = q.TopicId,
                TopicName = q.Topic != null ? q.Topic.Name : null,
                Content = q.Content,
                ImageUrl = q.ImageUrl,
                Explanation = q.Explanation,
                Difficulty = q.Difficulty,
                Type = q.Type
            })
            .ToListAsync();

        return new Response<List<QuestionDto>>(questions);
    }

    public async Task<Response<QuestionDto>> GetQuestionByIdAsync(long id)
    {
        var question = await context.Questions
            .Where(q => q.Id == id)
            .Include(q => q.Subject)
            .Include(q => q.Topic)
            .Select(q => new QuestionDto
            {
                Id = q.Id,
                SubjectId = q.SubjectId,
                SubjectName = q.Subject.Name,
                TopicId = q.TopicId,
                TopicName = q.Topic != null ? q.Topic.Name : null,
                Content = q.Content,
                ImageUrl = q.ImageUrl,
                Explanation = q.Explanation,
                Difficulty = q.Difficulty,
                Type = q.Type
            })
            .FirstOrDefaultAsync();

        return question == null
            ? new Response<QuestionDto>(HttpStatusCode.NotFound, "Савол ёфт нашуд") 
            : new Response<QuestionDto>(question);
    }

    public async Task<Response<QuestionStatsDto>> GetQuestionStatsAsync()
    {
        var stats = new QuestionStatsDto
        {
            TotalQuestions = await context.Questions.CountAsync()
        };

        stats.BySubject = await context.Questions
            .GroupBy(q => q.SubjectId)
            .Select(g => new { SubjectId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SubjectId, x => x.Count);

        stats.ByType = await context.Questions
            .GroupBy(q => q.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count);

        stats.ByDifficulty = await context.Questions
            .GroupBy(q => q.Difficulty)
            .Select(g => new { Difficulty = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Difficulty, x => x.Count);

        return new Response<QuestionStatsDto>(stats);
    }
}

class ValidationError(string field, string message)
{
    public string Field { get; set; } = field;
    public string Message { get; set; } = message;
}
