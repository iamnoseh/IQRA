namespace Application.DTOs.Gamification;

public class DuelDto
{
    public Guid Id { get; set; }
    public Guid Player1Id { get; set; }
    public string Player1Name { get; set; } = string.Empty;
    public Guid? Player2Id { get; set; }
    public string? Player2Name { get; set; }
    public int Player1Score { get; set; }
    public int Player2Score { get; set; }
    public Guid? WinnerId { get; set; }
    public string? WinnerName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
}
