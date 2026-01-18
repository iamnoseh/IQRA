using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace Infrastructure.Services;

public class OpenRouterAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public OpenRouterAiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenRouter:ApiKey"]!;
        _model = configuration["OpenRouter:Model"] ?? "google/learnlm-1.5-pro-experimental:free";
    }

    public async Task<string> GetExplanationAsync(string question, string correctAnswer, string chosenAnswer)
    {
        var prompt = $"Савол: {question}\nҶавоби дуруст: {correctAnswer}\nҶавоби интихобшуда: {chosenAnswer}\n\nЛутфан як шарҳи кутоҳ ва фаҳмо ба забони тоҷикӣ диҳед, ки чаро ин ҷавоб хато аст ва чаро ҷавоби дигар дуруст мебошад.";
        return await GetAiResponseAsync(prompt);
    }

    public async Task<string> GetMotivationAsync(string question, string answer)
    {
        var prompt = $"Корбар ба саволи зерин ҷавоби дуруст дод: \"{question}\" (Ҷавоб: {answer})\n\nЛутфан як ҷумлаи кутоҳи мотиватсионӣ ва табрикотӣ ба забони тоҷикӣ нависед, то ӯро рӯҳбаланд кунед.";
        return await GetAiResponseAsync(prompt);
    }

    private async Task<string> GetAiResponseAsync(string prompt)
    {
        try
        {
            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("chat/completions", requestBody);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"OpenRouter Error: {response.StatusCode} - {errorContent}");
                return "Офарин! Шумо дар роҳи дуруст ҳастед.";
            }

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "Офарин!";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AI Service Exception: {ex.Message}");
            return "Шумо тавонистед! Давом диҳед.";
        }
    }
}
