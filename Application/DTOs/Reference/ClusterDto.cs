using Domain.Enums;

namespace Application.DTOs.Reference;

public class ClusterDto
{
    public int Id { get; set; }
    public int ClusterNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    
    public List<ClusterSubjectDto> PartASubjects { get; set; } = new();
    public List<ClusterSubjectDto> PartBSubjects { get; set; } = new();
}

public class ClusterSubjectDto
{
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string SubjectIconUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
