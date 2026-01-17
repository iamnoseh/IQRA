namespace Application.DTOs.Reference;

public class FacultyDto
{
    public int Id { get; set; }
    public int UniversityId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CreateFacultyRequest
{
    public int UniversityId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class UpdateFacultyRequest : CreateFacultyRequest
{
}
