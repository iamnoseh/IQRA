namespace Application.DTOs.Reference;

public class SchoolLeaderboardDto
{
    public int Rank { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public long TotalXP { get; set; }
    public int StudentCount { get; set; }
}
