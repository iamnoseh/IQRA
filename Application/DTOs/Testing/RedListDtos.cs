using Application.DTOs.Testing;

namespace Application.DTOs.Testing;

public class RedListQuestionDto
{
    public long Id { get; set; }
    public long QuestionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string? Topic { get; set; }
    public DateTime AddedAt { get; set; }
    public int ConsecutiveCorrectCount { get; set; }
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

public class RedListDashboardDto
{
    public RedListStatsDto Stats { get; set; } = new();
    public List<RedListChartPointDto> ChartData { get; set; } = new();
    public List<RedListQuestionDto> ActiveQuestions { get; set; } = new();
}

public class RedListStatsDto
{
    public int TotalQuestions { get; set; }
    public int NewQuestionsToday { get; set; }
    
    public int XPToday { get; set; }
    public int XPIncreasePercent { get; set; } // e.g. 15%
    
    public int ReadyToRemoveCount { get; set; } // consecutive == 2
    public int RemovedTodayCount { get; set; }
}

public class RedListChartPointDto
{
    public string DateLabel { get; set; } = string.Empty; // e.g. "1 June"
    public int Value { get; set; }
}
