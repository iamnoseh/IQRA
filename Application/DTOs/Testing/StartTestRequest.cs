using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Testing;

public class StartTestRequest
{
    [Required]
    public int SubjectId { get; set; }

    [Required]
    public string Mode { get; set; } = string.Empty;
}
