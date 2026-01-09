namespace Application.DTOs.Testing.Management;

public class QuestionImportResultDto
{
    public int TotalQuestions { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<ImportError> Errors { get; set; } = new();
    public List<ImportWarning> Warnings { get; set; } = new();
}

public class ImportError
{
    public int Index { get; set; }
    public string QuestionPreview { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
}

public class ImportWarning
{
    public int Index { get; set; }
    public string Message { get; set; } = string.Empty;
}
