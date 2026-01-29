using Application.DTOs.Testing;
using Domain.Enums;

namespace Application.DTOs.Duel;


public class MatchFoundEvent
{
    public string SessionId { get; set; } = string.Empty;
    public PlayerInfo Player1 { get; set; } = new();
    public PlayerInfo Player2 { get; set; } = new();
    public DuelStatus Status { get; set; }
}

public class QuestionStartEvent
{
    public QuestionWithAnswersDto Question { get; set; } = new();
    public int QuestionIndex { get; set; }
    public int TimeLimitSeconds { get; set; } = 30;
}

public class RoundResultEvent
{
    public bool IsCorrect { get; set; }
    public long? CorrectAnswerId { get; set; }
    public string? CorrectAnswerText { get; set; }
    public List<string> CorrectPairIds { get; set; } = new();
    public int MyTotalScore { get; set; }
    public int OpponentTotalScore { get; set; }
    public int PointsEarned { get; set; }
    public bool IsRoundOver { get; set; }
    public bool IsDuelFinished { get; set; }

}


public class DuelFinishedEvent
{
    public string SessionId { get; set; } = string.Empty;
    public PlayerInfo Player1 { get; set; } = new();
    public PlayerInfo Player2 { get; set; } = new();
    public int Player1TotalScore { get; set; }
    public int Player2TotalScore { get; set; }
    public string? WinnerId { get; set; }
    public DuelStatus Status { get; set; }
}
