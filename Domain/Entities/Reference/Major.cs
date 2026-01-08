namespace Domain.Entities.Reference;

public class Major
{
    public int Id { get; set; }
    public int FacultyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MinScore2024 { get; set; }
    public int MinScore2025 { get; set; }
    
    public Faculty Faculty { get; set; } = null!;
    public ICollection<Users.UserProfile> TargetedByUsers { get; set; } = new List<Users.UserProfile>();
}
