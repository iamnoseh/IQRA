using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class RegisterRequest
{
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 7)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string SchoolName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string City { get; set; } = string.Empty;

    [Required]
    [Range(1, 5)]
    public int ClusterId { get; set; }

    [Required]
    [StringLength(100)]
    public string TargetUniversity { get; set; } = string.Empty;

    [StringLength(100)]
    public string? TargetFaculty { get; set; }

    [Required]
    [Range(100, 600)]
    public int TargetPassingScore { get; set; }
}
