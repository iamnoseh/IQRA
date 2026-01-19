namespace Application.Interfaces;

public interface IAiService
{
    Task<string> GetExplanationAsync(string question, string correctAnswer, string chosenAnswer);
    Task<string> GetMotivationAsync(string question, string answer);
    Task<string> AnalyzeTestResultAsync(int totalScore, int totalQuestions, List<(string Question, bool IsCorrect)> summary);
    Task<string> GetDashboardMotivationAsync();
}
