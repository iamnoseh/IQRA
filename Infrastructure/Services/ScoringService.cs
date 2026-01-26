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

        var userPairs = userAnswer.TextResponse
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(p => p.Split(':'))
            .Where(parts => parts.Length == 2)
            .Select(parts => new { Left = parts[0].Trim().ToLowerInvariant(), Right = parts[1].Trim().ToLowerInvariant() })
            .ToList();

        int correctCount = 0;
        var leftOptions = question.Answers.Where(a => !string.IsNullOrWhiteSpace(a.MatchPairText)).ToList();

        foreach (var leftOption in leftOptions)
        {
            var userMatch = userPairs.FirstOrDefault(up => 
                up.Left == leftOption.Id.ToString() || 
                up.Left == leftOption.Text.Trim().ToLowerInvariant());
            
            if (userMatch != null && userMatch.Right == leftOption.MatchPairText!.Trim().ToLowerInvariant())
            {
                correctCount++;
            }
        }
        
        return Math.Min(correctCount * ScoringConstants.ScoreMatchingPair, ScoringConstants.ScoreMatchingMax);
    }
}
