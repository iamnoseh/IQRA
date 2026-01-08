namespace Domain.Entities.Reference;

public class Faculty
{
    public int Id { get; set; }
    public int UniversityId { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public University University { get; set; } = null!;
    public ICollection<Major> Majors { get; set; } = new List<Major>();
}
