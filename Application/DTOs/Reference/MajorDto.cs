namespace Application.DTOs.Reference;

public class MajorDto
{
    public int Id { get; set; }
    public int FacultyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MinScore2024 { get; set; }
    public int MinScore2025 { get; set; }
}

public class CreateMajorRequest
{
    public int FacultyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MinScore2024 { get; set; }
    public int MinScore2025 { get; set; }
}

public class UpdateMajorRequest : CreateMajorRequest
{
}
