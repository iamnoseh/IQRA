namespace Domain.Entities.Users;

public class UserProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    
    public int ClusterId { get; set; }
    public string TargetUniversity { get; set; } = string.Empty;
    public string TargetFaculty { get; set; } = string.Empty;
    public int TargetPassingScore { get; set; }
    
    public int XP { get; set; }
    public string? AvatarUrl { get; set; }
    
    public AppUser User { get; set; } = null!;
}
