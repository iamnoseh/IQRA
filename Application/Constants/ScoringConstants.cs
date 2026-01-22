namespace Application.Constants;

public static class ScoringConstants
{
    public const int ScoreSingleChoice = 2;
    public const int ScoreClosedAnswer = 4;
    public const int ScoreMatchingPair = 1;
    public const int ScoreMatchingMax = 4;

    public const int XpEasy = 10;
    public const int XpMedium = 20;
    public const int XpHard = 40;
    public const int XpTestCompletionBonus = 100;
    public const int XpConsolation = 2;

    public const int XpPerLevel = 500;

    public const int DuelBaseScore = 600;
    public const int DuelMaxTimeBonus = 400;

    public const int EloWin = 25;
    public const int EloLoss = 15;

    public const double LeaguePromotionPercent = 0.15;
    public const double LeagueRelegationPercent = 0.15;
}
