using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Testing;

public class StartTestRequest
{
    [Required]
    public TestMode Mode { get; set; }

    public int? ClusterId { get; set; }
    public Domain.Enums.ComponentType? ComponentType { get; set; }

    public int? SubjectId { get; set; }
}
