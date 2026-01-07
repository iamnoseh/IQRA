namespace Application.DTOs.Users;

public class UserProfileDto
{
    public Guid UserId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string SchoolName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    
    public int ClusterId { get; set; }
    public string TargetUniversity { get; set; } = string.Empty;
    public string TargetFaculty { get; set; } = string.Empty;
    public int TargetPassingScore { get; set; }
    
    public int XP { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsPremium { get; set; }
    public DateTime? PremiumExpiresAt { get; set; }
}
