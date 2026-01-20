using Application.Constants;
using Application.Interfaces;
using Domain.Entities.Education;
using Domain.Entities.Testing;
using Domain.Enums;

namespace Infrastructure.Services;

public class ScoringService : IScoringService
{
    public int CalculateQuestionScore(Question question, UserAnswer userAnswer)
    {
        if (!userAnswer.IsCorrect)
            return 0;

        return question.Type switch
        {
            QuestionType.SingleChoice => ScoringConstants.ScoreSingleChoice,
            QuestionType.ClosedAnswer => ScoringConstants.ScoreClosedAnswer,
            QuestionType.Matching => CalculateMatchingScore(question, userAnswer),
            _ => 0
        };
    }

    public int CalculateDuelScore(bool isCorrect, int timeSpent, int timeLimit)
    {
        if (!isCorrect || timeLimit <= 0)
            return 0;

        int timeRemaining = Math.Max(0, timeLimit - timeSpent);
        double timeFraction = (double)timeRemaining / timeLimit;
        int timeBonus = (int)(timeFraction * ScoringConstants.DuelMaxTimeBonus);

        return ScoringConstants.DuelBaseScore + timeBonus;
    }

    public int GetMaxScoreForQuestion(Question question)
    {
        return question.Type switch
        {
            QuestionType.SingleChoice => ScoringConstants.ScoreSingleChoice,
            QuestionType.ClosedAnswer => ScoringConstants.ScoreClosedAnswer,
            QuestionType.Matching => ScoringConstants.ScoreMatchingMax,
            _ => 0
        };
    }

    private int CalculateMatchingScore(Question question, UserAnswer userAnswer)
    {
        if (string.IsNullOrWhiteSpace(userAnswer.TextResponse))
            return 0;

        var correctPairs = question.Answers
            .Where(a => !string.IsNullOrWhiteSpace(a.MatchPairText))
            .Select(a => $"{a.Text.Trim()}:{a.MatchPairText!.Trim()}")
            .OrderBy(p => p)
            .ToList();

        var userPairs = userAnswer.TextResponse
            .Split(',')
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .OrderBy(p => p)
            .ToList();

        int correctCount = userPairs.Count(userPair => correctPairs.Contains(userPair));
        return Math.Min(correctCount * ScoringConstants.ScoreMatchingPair, ScoringConstants.ScoreMatchingMax);
    }
}
