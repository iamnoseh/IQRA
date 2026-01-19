using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Diagnostics;

namespace Infrastructure.Services;

public class GoogleGeminiAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleGeminiAiService> _logger;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _baseUrl;
    private const int MaxRetries = 3;
    private const int InitialRetryDelayMs = 1000;

    public GoogleGeminiAiService(
        HttpClient httpClient, 
        IConfiguration configuration,
        ILogger<GoogleGeminiAiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        _apiKey = configuration["GoogleGemini:ApiKey"] 
                  ?? throw new ArgumentNullException("GoogleGemini:ApiKey is missing");
        
        _model = configuration["GoogleGemini:Model"] ?? "gemini-1.5-flash";
        
        _baseUrl = configuration["GoogleGemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta/";

        // Танзими HttpClient
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }
        _httpClient.Timeout = TimeSpan.FromSeconds(120);
    }

    public async Task<string> GetExplanationAsync(string question, string correctAnswer, string chosenAnswer)
    {
        var prompt = $@"
Вопрос: ""{question}""
Выбранный ответ (Неверный): ""{chosenAnswer}""
Правильный ответ: ""{correctAnswer}""

Задача:
Кратко объясните, почему выбранный ответ неверен и в чем суть правильного ответа.
- Без приветствий.
- Максимум 3 предложения.
- Только на русском языке.";

        return await GetAiResponseAsync(prompt) 
               ?? "К сожалению, не удалось получить объяснение.";
    }

    public async Task<string> GetMotivationAsync(string question, string answer)
    {
        var prompt = $@"Напишите одну короткую мотивирующую фразу за правильный ответ на вопрос: ""{question}"" (без приветствий, на русском).";
        return await GetAiResponseAsync(prompt) ?? "Молодец! Так держать.";
    }

    public async Task<string> GetDashboardMotivationAsync()
    {
        var prompt = @"Напишите одну вдохновляющую цитату или мотивирующее высказывание известного ученого или успешного человека (современного или классика).
- На русском языке.
- Без вступлений, только цитата и автор.
- Чтобы вдохновляло студентов на учебу.";
        
        return await GetAiResponseAsync(prompt) ?? "Знание — сила.";
    }

    public async Task<string> AnalyzeTestResultAsync(int totalScore, int totalQuestions, List<(string Question, bool IsCorrect)> summary)
    {
        var resultsText = string.Join("\n", summary.Take(10).Select(s => $"- {s.Question}: {(s.IsCorrect ? "Верно" : "Ошибка")}"));
        var prompt = $"Краткий анализ результатов ({totalScore} из {totalQuestions}). 3 важных совета. Без приветствий. Только на русском.";
        
        return await GetAiResponseAsync(prompt) ?? "Анализ недоступен.";
    }

    private async Task<string?> GetAiResponseAsync(string prompt)
    {
        var sw = Stopwatch.StartNew();
        int retryDelay = InitialRetryDelayMs;

        var url = $"models/{_model}:generateContent?key={_apiKey}";

        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = prompt } } }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        maxOutputTokens = 2000
                    }
                };

                var response = await _httpClient.PostAsJsonAsync(url, requestBody);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Gemini Error ({response.StatusCode}): {errorContent}");

                    if ((int)response.StatusCode == 429 || (int)response.StatusCode >= 500)
                    {
                        if (attempt < MaxRetries - 1)
                        {
                            await Task.Delay(retryDelay);
                            retryDelay *= 2;
                            continue;
                        }
                    }
                    return null;
                }

                var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
                return ExtractText(jsonResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Attempt {attempt + 1} failed: {ex.Message}");
                if (attempt == MaxRetries - 1) return null;
                await Task.Delay(retryDelay);
                retryDelay *= 2;
            }
        }
        return null;
    }

    private string? ExtractText(JsonElement jsonResponse)
    {
        try 
        {
            return jsonResponse
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
        }
        catch { return null; }
    }
}