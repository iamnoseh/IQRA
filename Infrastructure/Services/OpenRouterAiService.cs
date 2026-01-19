using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Diagnostics;

namespace Infrastructure.Services;

public class OpenRouterAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenRouterAiService> _logger;
    private readonly string _apiKey;
    private readonly string _model;
    private const int MaxRetries = 3;
    private const int InitialRetryDelayMs = 1000;

    public OpenRouterAiService(
        HttpClient httpClient, 
        IConfiguration configuration,
        ILogger<OpenRouterAiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["OpenRouter:ApiKey"]!;
        _model = configuration["OpenRouter:Model"] ?? "google/learnlm-1.5-pro-experimental:free";
        
        // Configure timeout
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
        var result = await GetAiResponseAsync(prompt);
        return result ?? "К сожалению, AI не смог предоставить объяснение.";
    }

    public async Task<string> GetMotivationAsync(string question, string answer)
    {
        var prompt = $@"Напишите одну короткую мотивирующую фразу за правильный ответ на вопрос: ""{question}"" (без приветствий, на русском).";
        var result = await GetAiResponseAsync(prompt);
        return result ?? "Молодец! Ответ верный.";
    }

    public async Task<string> GetDashboardMotivationAsync()
    {
        var prompt = @"Напишите одну вдохновляющую цитату или мотивирующее высказывание известного ученого или успешного человека (современного или классика).
- На русском языке.
- Без вступлений, только цитата и автор.
- Чтобы вдохновляло студентов на учебу.";
        
        var result = await GetAiResponseAsync(prompt);
        return result ?? "Знание — сила.";
    }

    public async Task<string> AnalyzeTestResultAsync(int totalScore, int totalQuestions, List<(string Question, bool IsCorrect)> summary)
    {
        // Limit summary to prevent large requests
        var limitedSummary = summary.Take(20).ToList();
        var resultsText = string.Join("\n", limitedSummary.Select(s => 
            $"- Вопрос: {(s.Question.Length > 100 ? s.Question.Substring(0, 100) + "..." : s.Question)} | Результат: {(s.IsCorrect ? "Верно" : "Ошибка")}"));
        
        var prompt = $"Краткий анализ результатов ({totalScore} из {totalQuestions}). 3 важных совета. Без приветствий. Только на русском.";
        var result = await GetAiResponseAsync(prompt);
        return result ?? "Анализ AI временно недоступен.";
    }

    private async Task<string?> GetAiResponseAsync(string prompt)
    {
        var sw = Stopwatch.StartNew();
        int retryDelay = InitialRetryDelayMs;

        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation($"OpenRouter request attempt {attempt + 1}/{MaxRetries}, prompt length: {prompt.Length}");

                var requestBody = new
                {
                    model = _model,
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7,
                    max_tokens = 1500 // Limit response size
                };

                var response = await _httpClient.PostAsJsonAsync("chat/completions", requestBody);
                sw.Stop();

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"OpenRouter Error ({response.StatusCode}) after {sw.ElapsedMilliseconds}ms: {errorContent}");
                    
                    // Check rate limit headers
                    if (response.Headers.TryGetValues("X-RateLimit-Remaining", out var remainingValues))
                    {
                        var remaining = remainingValues.FirstOrDefault();
                        _logger.LogWarning($"Rate limit remaining: {remaining}");
                        
                        if (response.Headers.TryGetValues("X-RateLimit-Reset", out var resetValues))
                        {
                            var resetTimestamp = resetValues.FirstOrDefault();
                            if (long.TryParse(resetTimestamp, out var resetMs))
                            {
                                var resetTime = DateTimeOffset.FromUnixTimeMilliseconds(resetMs);
                                _logger.LogWarning($"Rate limit resets at: {resetTime.LocalDateTime}");
                            }
                        }
                    }
                    
                    // Rate limit error (429) - don't retry immediately
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        _logger.LogError("Rate limit exceeded. Please add credits or wait until reset time.");
                        return null;
                    }
                    
                    // Don't retry on other 4xx errors (client errors)
                    if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                    {
                        return null;
                    }
                    
                    // Retry on 5xx errors (server errors)
                    if (attempt < MaxRetries - 1)
                    {
                        _logger.LogWarning($"Retrying after {retryDelay}ms...");
                        await Task.Delay(retryDelay);
                        retryDelay *= 2; // Exponential backoff
                        sw.Restart();
                        continue;
                    }
                    
                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                var content = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                
                _logger.LogInformation($"OpenRouter request successful after {sw.ElapsedMilliseconds}ms");
                return content;
            }
            catch (HttpRequestException ex)
            {
                sw.Stop();
                _logger.LogError($"HTTP error on attempt {attempt + 1}/{MaxRetries} after {sw.ElapsedMilliseconds}ms: {ex.Message}");
                
                if (attempt < MaxRetries - 1)
                {
                    _logger.LogWarning($"Retrying after {retryDelay}ms due to connection error...");
                    await Task.Delay(retryDelay);
                    retryDelay *= 2; // Exponential backoff
                    sw.Restart();
                }
                else
                {
                    _logger.LogError($"All {MaxRetries} attempts failed. Last error: {ex.Message}");
                    return null;
                }
            }
            catch (TaskCanceledException ex)
            {
                sw.Stop();
                _logger.LogError($"Request timeout on attempt {attempt + 1}/{MaxRetries} after {sw.ElapsedMilliseconds}ms: {ex.Message}");
                
                if (attempt < MaxRetries - 1)
                {
                    _logger.LogWarning($"Retrying after {retryDelay}ms due to timeout...");
                    await Task.Delay(retryDelay);
                    retryDelay *= 2;
                    sw.Restart();
                }
                else
                {
                    _logger.LogError($"All {MaxRetries} attempts timed out");
                    return null;
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError($"Unexpected error on attempt {attempt + 1}/{MaxRetries} after {sw.ElapsedMilliseconds}ms: {ex.GetType().Name} - {ex.Message}");
                
                if (attempt < MaxRetries - 1)
                {
                    _logger.LogWarning($"Retrying after {retryDelay}ms due to unexpected error...");
                    await Task.Delay(retryDelay);
                    retryDelay *= 2;
                    sw.Restart();
                }
                else
                {
                    _logger.LogError($"All {MaxRetries} attempts failed with unexpected errors");
                    return null;
                }
            }
        }

        return null;
    }
}