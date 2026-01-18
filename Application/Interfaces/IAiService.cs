namespace Application.Interfaces;

public interface IAiService
{
    Task<string> GetExplanationAsync(string question, string correctAnswer, string chosenAnswer);
    Task<string> GetMotivationAsync(string question, string answer);
}
