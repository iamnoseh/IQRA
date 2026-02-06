using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Reference;

public class SubjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
}

public class CreateSubjectRequest
{
    public string Name { get; set; } = string.Empty;
    public IFormFile? Icon { get; set; }
}

public class UpdateSubjectRequest
{
    public string Name { get; set; } = string.Empty;
    public IFormFile? Icon { get; set; }
}
