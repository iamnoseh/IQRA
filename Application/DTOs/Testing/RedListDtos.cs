using Application.DTOs.Testing;

namespace Application.DTOs.Testing;

public class RedListQuestionDto
{
    public long Id { get; set; }
    public long QuestionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public int ConsecutiveCorrectCount { get; set; }
    public List<AnswerOptionDto> Answers { get; set; } = new();
}

public class SubmitRedListAnswerRequest
{
    public long RedListQuestionId { get; set; }
    public long? ChosenAnswerId { get; set; }
    public string? TextResponse { get; set; }
}

public class RedListPracticeFeedbackDto
{
    public bool IsCorrect { get; set; }
    public int ConsecutiveCorrectCount { get; set; }
    public bool IsRemoved { get; set; }
    public int? XPEarned { get; set; }
    public string? CorrectAnswerText { get; set; }
}
