using Domain.Enums;

namespace Application.DTOs.Testing.Management;

public class QuestionStatsDto
{
    public int TotalQuestions { get; set; }
    public Dictionary<int, int> BySubject { get; set; } = new();
    public Dictionary<QuestionType, int> ByType { get; set; } = new();
    public Dictionary<DifficultyLevel, int> ByDifficulty { get; set; } = new();
}
