namespace Domain.Entities.Reference;

public class School
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public long TotalXP { get; set; }
    public int StudentCount { get; set; }
    
    public ICollection<Users.UserProfile> Students { get; set; } = new List<Users.UserProfile>();
}
