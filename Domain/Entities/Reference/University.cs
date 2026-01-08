namespace Domain.Entities.Reference;

public class University
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    
    public ICollection<Faculty> Faculties { get; set; } = new List<Faculty>();
}
