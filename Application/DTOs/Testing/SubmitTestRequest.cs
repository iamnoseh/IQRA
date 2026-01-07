using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Testing;

public class SubmitAnswerDto
{
    [Required]
    public long QuestionId { get; set; }

    public long? ChosenAnswerId { get; set; }
    
    public string? TextResponse { get; set; }

    [Range(0, int.MaxValue)]
    public int TimeSpentSeconds { get; set; }
}

public class SubmitTestRequest
{
    [Required]
    public Guid TestSessionId { get; set; }

    [Required]
    [MinLength(1)]
    public List<SubmitAnswerDto> Answers { get; set; } = new();
}
