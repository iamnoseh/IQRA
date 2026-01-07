using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Users;

public class UpdateProfileRequest
{
    [StringLength(50, MinimumLength = 2)]
    public string? FirstName { get; set; }

    [StringLength(50, MinimumLength = 2)]
    public string? LastName { get; set; }

    [StringLength(100)]
    public string? SchoolName { get; set; }

    [StringLength(50)]
    public string? City { get; set; }

    [Range(1, 5)]
    public int? ClusterId { get; set; }

    [StringLength(100)]
    public string? TargetUniversity { get; set; }

    [StringLength(100)]
    public string? TargetFaculty { get; set; }

    [Range(100, 600)]
    public int? TargetPassingScore { get; set; }
}
