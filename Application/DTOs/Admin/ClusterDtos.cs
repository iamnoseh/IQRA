using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Admin;

public class CreateClusterRequest
{
    [Required(ErrorMessage = "Рақами кластер зарур аст")]
    [Range(1, 100, ErrorMessage = "Рақами кластер бояд аз 1 то 100 бошад")]
    public int ClusterNumber { get; set; }

    [Required(ErrorMessage = "Номи кластер зарур аст")]
    [StringLength(200, ErrorMessage = "Ном бояд то 200 аломат бошад")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Тавсиф бояд то 500 аломат бошад")]
    public string Description { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
}

public class UpdateClusterRequest
{
    [StringLength(200, ErrorMessage = "Ном бояд то 200 аломат бошад")]
    public string? Name { get; set; }

    [StringLength(500, ErrorMessage = "Тавсиф бояд то 500 аломат бошад")]
    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public bool? IsActive { get; set; }
}

public class AddSubjectToClusterRequest
{
    [Required(ErrorMessage = "ID фан зарур аст")]
    public int SubjectId { get; set; }

    [Required(ErrorMessage = "Навъи қисм зарур аст")]
    public ComponentType ComponentType { get; set; }

    [Range(0, 100, ErrorMessage = "Тартиби намоиш бояд аз 0 то 100 бошад")]
    public int DisplayOrder { get; set; } = 0;
}

public class ReorderSubjectsRequest
{
    [Required(ErrorMessage = "Рӯйхати тартиб зарур аст")]
    public List<SubjectOrderItem> SubjectOrders { get; set; } = new();
}

public class SubjectOrderItem
{
    [Required]
    public int SubjectId { get; set; }
    
    [Required]
    public ComponentType ComponentType { get; set; }
    
    [Required]
    public int DisplayOrder { get; set; }
}
