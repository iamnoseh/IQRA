using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Gamification;

public class CreateDuelRequest
{
    public Guid? OpponentId { get; set; }
    
    [Required]
    public int SubjectId { get; set; }
}
