using Domain.Enums;

namespace Application.DTOs.Admin;

public class ClusterDto
{
    public int Id { get; set; }
    public int ClusterNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    
    public List<ClusterSubjectDto> Subjects { get; set; } = new();
}

public class ClusterSubjectDto
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string SubjectIconUrl { get; set; } = string.Empty;
    public ComponentType ComponentType { get; set; }
    public int DisplayOrder { get; set; }
}
