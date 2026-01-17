using Application.Responses;

namespace Application.DTOs.Reference;

public class SchoolDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
}

public class CreateSchoolRequest
{
    public string Name { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
}

public class UpdateSchoolRequest : CreateSchoolRequest
{
}

public class SchoolSearchRequest : PaginationRequest
{
    public string? SearchTerm { get; set; }
    public string? Province { get; set; }
    public string? District { get; set; }
}
