namespace Application.DTOs.Testing;

public class PairValidationDto
{
    public string LeftSide { get; set; } = string.Empty;
    public string RightSide { get; set; } = string.Empty;
    public string CorrectRightSide { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
