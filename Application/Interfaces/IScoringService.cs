using Domain.Entities.Education;
using Domain.Entities.Testing;

namespace Application.Interfaces;

public interface IScoringService
{
    int CalculateQuestionScore(Question question, UserAnswer userAnswer);
    int CalculateDuelScore(bool isCorrect, int timeSpent, int timeLimit);
    int GetMaxScoreForQuestion(Question question);
}
