namespace Application.DTOs.Gamification;

public class LeaderboardDto
{
    public List<LeaderboardEntryDto> Entries { get; set; } = new();
    public int TotalCount { get; set; }
}

public class LeaderboardEntryDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int XP { get; set; }
    public int Rank { get; set; }
    public string LeagueName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}
