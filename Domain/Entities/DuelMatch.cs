namespace Domain.Entities;

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
    
    public AppUser Player1 { get; set; } = null!;
    public AppUser? Player2 { get; set; }
    public AppUser? Winner { get; set; }
}
