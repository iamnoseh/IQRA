using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Testing.Management;

public class QuestionImportDto
{
    public int SubjectId { get; set; }
    public string? Topic { get; set; }
    
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Explanation { get; set; } = string.Empty;
    
    public DifficultyLevel Difficulty { get; set; }
    public QuestionType Type { get; set; }
    
    public List<AnswerImportDto>? Answers { get; set; }
    public string? CorrectAnswer { get; set; }
}

public class CreateQuestionRequest
{
    public int SubjectId { get; set; }
    public string? Topic { get; set; }
    
    public string Content { get; set; } = string.Empty;
    public IFormFile? Image { get; set; }
    public string Explanation { get; set; } = string.Empty;
    
    public DifficultyLevel Difficulty { get; set; }
    public QuestionType Type { get; set; }
    
    // Support both formats: array of objects OR JSON string
    public List<AnswerImportDto>? Answers { get; set; }
    public string? AnswersJson { get; set; }
    
    public string? CorrectAnswer { get; set; }
}

public class AnswerImportDto
{
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public string? MatchPair { get; set; }
}

public class BulkQuestionImportRequest
{
    public List<QuestionImportDto> Questions { get; set; } = new();
    public bool ValidateOnly { get; set; } = false;
}
