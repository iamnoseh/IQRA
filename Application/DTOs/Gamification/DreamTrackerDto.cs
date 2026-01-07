namespace Application.DTOs.Gamification;

public class DreamTrackerDto
{
    public string TargetUniversity { get; set; } = string.Empty;
    public int TargetPassingScore { get; set; }
    public double CurrentAverageScore { get; set; }
    public double SuccessProbability { get; set; }
    public string Advice { get; set; } = string.Empty;
    public Dictionary<string, double> SubjectScores { get; set; } = new();
}
