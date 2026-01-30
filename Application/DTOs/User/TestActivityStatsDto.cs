namespace Application.DTOs.User;

public class TestActivityStatsDto
{
    public int TotalTests { get; set; }
    public int TotalDuels { get; set; }
    public int TotalDuelWins { get; set; }
    public double OverallCorrectPercentage { get; set; }
    public List<DailyTestCountDto> DailyStats { get; set; } = new();
}

public class DailyTestCountDto
{
    public DateTime Date { get; set; }
    public int TotalAnswers { get; set; }
    public int CorrectAnswers { get; set; }
    public int IncorrectAnswers { get; set; }
}
