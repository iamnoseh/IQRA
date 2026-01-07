namespace Domain.Entities;

public class TeamMember
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsActive { get; set; }
}
