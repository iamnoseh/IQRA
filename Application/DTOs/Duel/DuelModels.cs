using Application.DTOs.Testing;
using Domain.Enums;

namespace Application.DTOs.Duel;


public class PlayerInfo
{
    public string ConnectionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; }
    public bool IsReady { get; set; }
}


public class DuelSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public PlayerInfo Player1 { get; set; } = new();
    public PlayerInfo Player2 { get; set; } = new();
    
    public List<QuestionWithAnswersDto> Questions { get; set; } = new();
    public bool QuestionsReady { get; set; }
    public int CurrentQuestionIndex { get; set; } = -1;
    
    public int Player1TotalScore { get; set; }
    public int Player2TotalScore { get; set; }
    
    public DuelStatus Status { get; set; } = DuelStatus.Waiting;
    public DateTime? MatchFoundAt { get; set; }
    
    public int AnsweredCount { get; set; }
    public bool Player1Answered { get; set; }
    public bool Player2Answered { get; set; }
    
    public DuelSubmissionResult? Player1LastResult { get; set; }
    public DuelSubmissionResult? Player2LastResult { get; set; }
    
    public List<QuestionAnswerRecord> Player1AnswerHistory { get; set; } = new();
    public List<QuestionAnswerRecord> Player2AnswerHistory { get; set; } = new();
    
    public DateTime? CurrentQuestionStartedAt { get; set; }
    public CancellationTokenSource? QuestionTimerCts { get; set; }
    
    public string? DisconnectedPlayerId { get; set; }
    public string? TimedOutPlayerId { get; set; }
}

public class DuelSubmissionResult
{
    public bool Success { get; set; }
    public int CurrentScore { get; set; }
    public int OpponentScore { get; set; }
    public int AddedScore { get; set; }
    public bool IsCorrect { get; set; }
    public List<string> CorrectPairIds { get; set; } = new();
    public long? CorrectAnswerId { get; set; }
    public bool BothAnswered { get; set; }
    public bool IsDuelFinished { get; set; }
}

public class QuestionAnswerRecord
{
    public long QuestionId { get; set; }
    public bool IsCorrect { get; set; }
    public string? UserAnswer { get; set; }
    public int TimeSpentSeconds { get; set; }
}