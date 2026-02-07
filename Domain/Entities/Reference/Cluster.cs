namespace Domain.Entities.Reference;

public class Cluster
{
    public int Id { get; set; }
    public int ClusterNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    public ICollection<ClusterSubject> ClusterSubjects { get; set; } = new List<ClusterSubject>();
}
