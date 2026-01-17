using Application.Responses;

namespace Application.DTOs.Reference;

public class UniversityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

public class CreateUniversityRequest
{
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

public class UpdateUniversityRequest : CreateUniversityRequest
{
}

public class UniversitySearchRequest : PaginationRequest
{
    public string? SearchTerm { get; set; }
    public string? City { get; set; }
}
