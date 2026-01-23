namespace Application.DTOs.Dashboard;

public class DashboardStatsDto
{
    public List<DailyActivityDto> DailyActivity { get; set; } = new();
    public List<SubjectPerformanceDto> SubjectPerformance { get; set; } = new();
    public int TodoRedListCount { get; set; }
    public List<UniversityProbabilityDto> UniversityProbability { get; set; } = new();
}

public class DailyActivityDto
{
    public string Date { get; set; } = string.Empty;
    public int TestsCount { get; set; }
}

public class SubjectPerformanceDto
{
    public string Subject { get; set; } = string.Empty;
    public int Score { get; set; }
}

public class UniversityProbabilityDto
{
    public string Name { get; set; } = string.Empty;
    public int Percent { get; set; }
}
