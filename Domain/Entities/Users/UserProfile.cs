using Domain.Enums;

namespace Domain.Entities.Users;

public class UserProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Gender? Gender { get; set; }
    
    public string? Province { get; set; }
    public string? District { get; set; }
    public int? SchoolId { get; set; }
    public int? Grade { get; set; }
    
    public int? ClusterId { get; set; }
    public string? TargetUniversity { get; set; }
    public string? TargetFaculty { get; set; }
    public int? TargetMajorId { get; set; }
    public int? TargetPassingScore { get; set; }
    
    public int XP { get; set; }
    public string? AvatarUrl { get; set; }
    public int EloRating { get; set; } = 1000;
    public int? CurrentLeagueId { get; set; }
    public DateTime? LastTestDate { get; set; }
    
    public AppUser User { get; set; } = null!;
    public Reference.School? School { get; set; }
    public Reference.Major? TargetMajor { get; set; }
    public Gamification.League? CurrentLeague { get; set; }
}
