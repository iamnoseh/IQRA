using Domain.Enums;

namespace Domain.Entities.Gamification;

public class DuelMatch
{
    public Guid Id { get; set; }
    public Guid Player1Id { get; set; }
    public Guid? Player2Id { get; set; }
    public Guid? WinnerId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    
    public int Player1Score { get; set; }
    public int Player2Score { get; set; }
    
    public string QuestionIdsJson { get; set; } = string.Empty;
    public int TimeLimit { get; set; } = 60;
    public string? Player1AnswersJson { get; set; }
    public string? Player2AnswersJson { get; set; }
    public DuelStatus Status { get; set; } = DuelStatus.Waiting;
    
    public Users.AppUser Player1 { get; set; } = null!;
    public Users.AppUser? Player2 { get; set; }
    public Users.AppUser? Winner { get; set; }
}
