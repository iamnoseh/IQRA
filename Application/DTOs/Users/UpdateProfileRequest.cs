using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Users;

public class UpdateProfileRequest
{
    [StringLength(50, MinimumLength = 2)]
    public string? FirstName { get; set; }

    [StringLength(50, MinimumLength = 2)]
    public string? LastName { get; set; }

    public Gender? Gender { get; set; }

    [StringLength(50)]
    public string? Province { get; set; }

    [StringLength(50)]
    public string? District { get; set; }

    public int? SchoolId { get; set; }

    [Range(1, 11)]
    public int? Grade { get; set; }

    [Range(1, 5)]
    public int? ClusterId { get; set; }

    [StringLength(100)]
    public string? TargetUniversity { get; set; }

    [StringLength(100)]
    public string? TargetFaculty { get; set; }

    public int? TargetMajorId { get; set; }
    
    public IFormFile? Avatar { get; set; }
}
