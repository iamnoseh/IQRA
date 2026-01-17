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
            Topic = dto.Topic,
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
        List<AnswerImportDto>? answers = request.Answers;
        
        // If AnswersJson is provided, parse it
        if (!string.IsNullOrWhiteSpace(request.AnswersJson))
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(request.AnswersJson);
                answers = new List<AnswerImportDto>();
                
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    var answer = new AnswerImportDto
                    {
                        // Support both 'text' and 'answer' field names
                        Text = element.TryGetProperty("text", out var textProp) ? textProp.GetString() ?? ""
                             : element.TryGetProperty("answer", out var answerProp) ? answerProp.GetString() ?? ""
                             : "",
                        IsCorrect = element.TryGetProperty("isCorrect", out var isCorrectProp) && isCorrectProp.GetBoolean(),
                        MatchPair = element.TryGetProperty("matchPair", out var matchProp) ? matchProp.GetString() : null
                    };
                    answers.Add(answer);
                }
            }
            catch (Exception ex)
            {
                return new Response<QuestionDto>(HttpStatusCode.BadRequest, $"AnswersJson формат нодуруст: {ex.Message}");
            }
        }

        // Convert CreateQuestionRequest to QuestionImportDto
        var dto = new QuestionImportDto
        {
            SubjectId = request.SubjectId,
            Topic = request.Topic,
            Content = request.Content,
            Explanation = request.Explanation,
            Difficulty = request.Difficulty,
            Type = request.Type,
            Answers = answers,
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
            .Select(q => new QuestionDto
            {
                Id = q.Id,
                SubjectId = q.SubjectId,
                SubjectName = q.Subject.Name,
                Topic = q.Topic,
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
        question.Topic = dto.Topic;
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
            .Select(q => new QuestionDto
            {
                Id = q.Id,
                SubjectId = q.SubjectId,
                SubjectName = q.Subject.Name,
                Topic = q.Topic,
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
            .OrderBy(q => q.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(q => new QuestionDto
            {
                Id = q.Id,
                SubjectId = q.SubjectId,
                SubjectName = q.Subject.Name,
                Topic = q.Topic,
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
        // Topic is now a string field, this method is kept for backwards compatibility
        // but will return empty list since topicId doesn't make sense anymore
        return new Response<List<QuestionDto>>(new List<QuestionDto>());
    }

    public async Task<Response<QuestionDto>> GetQuestionByIdAsync(long id)
    {
        var question = await context.Questions
            .Where(q => q.Id == id)
            .Include(q => q.Subject)
            .Include(q => q.Answers)
            .Select(q => new QuestionDto
            {
                Id = q.Id,
                SubjectId = q.SubjectId,
                SubjectName = q.Subject.Name,
                Topic = q.Topic,
                Content = q.Content,
                ImageUrl = q.ImageUrl,
                Explanation = q.Explanation,
                Difficulty = q.Difficulty,
                Type = q.Type,
                Answers = q.Answers.Select(a => new AnswerOptionDto
                {
                    Id = a.Id,
                    Text = a.Text,
                    MatchPairText = a.MatchPairText,
                    IsCorrect = a.IsCorrect
                }).ToList()
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

    public async Task<Response<QuestionListResponse>> GetAllQuestionsAsync(QuestionFilterRequest filter)
    {
        var query = context.Questions
            .Include(q => q.Subject)
            .AsQueryable();

        // Apply filters
        if (filter.SubjectId.HasValue)
            query = query.Where(q => q.SubjectId == filter.SubjectId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Topic))
            query = query.Where(q => q.Topic == filter.Topic);

        if (filter.Difficulty.HasValue)
            query = query.Where(q => q.Difficulty == filter.Difficulty.Value);

        if (filter.Type.HasValue)
            query = query.Where(q => q.Type == filter.Type.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            query = query.Where(q => q.Content.Contains(filter.SearchTerm));

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = filter.SortBy.ToLower() switch
        {
            "content" => filter.SortDescending 
                ? query.OrderByDescending(q => q.Content) 
                : query.OrderBy(q => q.Content),
            "difficulty" => filter.SortDescending 
                ? query.OrderByDescending(q => q.Difficulty) 
                : query.OrderBy(q => q.Difficulty),
            _ => filter.SortDescending 
                ? query.OrderByDescending(q => q.CreatedAt) 
                : query.OrderBy(q => q.CreatedAt)
        };

        // Apply pagination
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(q => new QuestionListItemDto
            {
                Id = q.Id,
                SubjectId = q.SubjectId,
                SubjectName = q.Subject.Name,
                Topic = q.Topic,
                Content = q.Content,
                ImageUrl = q.ImageUrl,
                Difficulty = q.Difficulty,
                Type = q.Type,
                CreatedAt = q.CreatedAt
            })
            .ToListAsync();

        var response = new QuestionListResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
        };

        return new Response<QuestionListResponse>(response);
    }
}

class ValidationError(string field, string message)
{
    public string Field { get; set; } = field;
    public string Message { get; set; } = message;
}
