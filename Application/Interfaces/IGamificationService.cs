using Domain.Entities.Education;
using Domain.Entities.Testing;

namespace Application.Interfaces;

public interface IGamificationService
{
    Task<int> CalculateXpAsync(Question question, bool isCorrect);
    Task<int> ProcessTestSessionEndAsync(Guid userId, TestSession session);
    Task ProcessDuelResultAsync(Guid winnerUserId, Guid loserUserId);
    Task ProcessWeeklyLeagueAsync();
}
