namespace Domain.Entities.Reference;

public class ClusterDefinition
{
    public int Id { get; set; }
    public int ClusterNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public string SubjectIdsJson { get; set; } = string.Empty;
}
