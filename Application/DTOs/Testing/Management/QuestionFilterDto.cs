using Domain.Enums;

namespace Application.DTOs.Testing.Management;

public class QuestionFilterRequest
{
    public int? SubjectId { get; set; }
    public string? Topic { get; set; }
    public DifficultyLevel? Difficulty { get; set; }
    public QuestionType? Type { get; set; }
    public string? SearchTerm { get; set; }
    
    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    
    // Sorting
    public string SortBy { get; set; } = "CreatedAt"; // CreatedAt, Content, Difficulty
    public bool SortDescending { get; set; } = true;
}

public class QuestionListResponse
{
    public List<QuestionListItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class QuestionListItemDto
{
    public long Id { get; set; }
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string? Topic { get; set; }
    
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    
    public DifficultyLevel Difficulty { get; set; }
    public QuestionType Type { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
