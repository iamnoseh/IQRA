namespace Domain.Entities.Reference;

public class ClusterSubject
{
    public int Id { get; set; }
    
    public int ClusterId { get; set; }
    public Cluster Cluster { get; set; } = null!;
    
    public int SubjectId { get; set; }
    public Domain.Entities.Education.Subject Subject { get; set; } = null!;
    
    public Domain.Enums.ComponentType ComponentType { get; set; }
    
    public int DisplayOrder { get; set; } = 0;
}
