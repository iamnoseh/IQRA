using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Testing;

public class StartTestRequest
{
    [Required]
    public TestMode Mode { get; set; }

    [Required]
    public int ClusterNumber { get; set; }

    public int? SubjectId { get; set; }
}
