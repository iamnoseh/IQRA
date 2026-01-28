using System;
using System.Collections.Generic;

namespace Application.DTOs.Duel;

public enum DuelStatus
{
    Waiting,
    Starting,
    InProgress,
    Finished,
    Canceled
}

public class DuelQuestionDto
{
    public long Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string? Topic { get; set; }
    public string? ImageUrl { get; set; }
    public int Type { get; set; } // 1=Single, 2=Matching, 3=Closed
    public int TimeLimitSeconds { get; set; } = 30;
    
    public List<DuelAnswerOptionDto> Answers { get; set; } = new();
    public List<string> MatchOptions { get; set; } = new();
}

public class DuelAnswerOptionDto
{
    public long Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? MatchPairText { get; set; }
}

public class PlayerInfo
{
    public string ConnectionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; }
    public int Score { get; set; }
    public bool IsReady { get; set; }
}

public class DuelSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public PlayerInfo Player1 { get; set; } = new();
    public PlayerInfo Player2 { get; set; } = new();
    public List<DuelQuestionDto> Questions { get; set; } = new();
    public bool QuestionsReady { get; set; }
    public int CurrentQuestionIndex { get; set; } = -1;
    public DuelStatus Status { get; set; } = DuelStatus.Waiting;
    
    public int AnsweredCount { get; set; } 
    public bool Player1Answered { get; set; }
    public bool Player2Answered { get; set; }
}
