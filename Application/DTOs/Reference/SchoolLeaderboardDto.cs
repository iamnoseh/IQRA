namespace Application.DTOs.Reference;

public class SchoolLeaderboardDto
{
    public int Rank { get; set; }
    public int SchoolId { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public long TotalXP { get; set; }
    public int StudentCount { get; set; }
    public double AverageXP { get; set; }
}

public class SchoolLeaderboardResponse
{
    public List<SchoolLeaderboardDto> Leaderboard { get; set; } = new();
    public SchoolLeaderboardDto? UserSchool { get; set; }
}
