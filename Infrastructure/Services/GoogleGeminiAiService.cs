using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Diagnostics;
using System.Text;

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
        _apiKey = configuration["GoogleGemini:ApiKey"]!;
        _model = configuration["GoogleGemini:Model"] ?? "gemini-2.0-flash";
        _baseUrl = configuration["GoogleGemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta/models/";
        
        // Configure timeout
        _httpClient.Timeout = TimeSpan.FromSeconds(120);
    }

    public async Task<string> GetExplanationAsync(string question, string correctAnswer, string chosenAnswer)
    {
        var prompt = $@"
Шумо як донишманди тоҷик ва омӯзгори меҳрубон ҳастед. Корбар ба саволи зерин ҷавоби нодуруст дод.
Савол: ""{question}""
Ҷавоби интихобшуда: ""{chosenAnswer}""
Ҷавоби дуруст: ""{correctAnswer}""

Вазифаи шумо:
1. Фаҳмонед, ки чаро ҷавоби интихобшуда хато аст.
2. Маъно ва моҳияти ҷавоби дурустро пурра шарҳ диҳед.
3. Дар бораи ҷавоби дуруст як факти таърихӣ, илмӣ ё ҷолибе илова кунед, ки дониши корбарро васеъ кунад (ин қисм бояд хеле ҷолиб бошад).
4. Танҳо ба забони тоҷикӣ, бо лаҳни ҳавасмандкунанда ва ҷолиб нависед.
5. Матни ниҳоӣ бояд диққатҷалбкунанда ва илмӣ бошад.
";
        var result = await GetAiResponseAsync(prompt);
        return result ?? "Мутаассифона, AI шарҳ дода натавонист. Аммо кӯшиши шумо шоистаи таҳсин аст! Давом диҳед.";
    }

    public async Task<string> GetMotivationAsync(string question, string answer)
    {
        var prompt = $@"
Шумо як мураббии муваффақият ҳастед. Корбар ба саволи зерин дуруст ҷавоб дод:
Савол: ""{question}""
Ҷавоб: ""{answer}""

Вазифаи шумо:
1. Корбарро барои дониши баландаш самимона ва бо шавқ табрик кунед.
2. Дар бораи ин ҷавоб ё мавзӯъ як ҷумлаи иловагии ҷолиб (Deep Fact) бигӯед, то ӯро боз ҳам бештар ба шавқ оред.
3. Танҳо ба забони тоҷикӣ ва хеле ҷолиб нависед.
";
        var result = await GetAiResponseAsync(prompt);
        return result ?? "Офарин! Ҷавоби шумо дуруст аст. Кӯшишро идома диҳед!";
    }

    public async Task<string> AnalyzeTestResultAsync(int totalScore, int totalQuestions, List<(string Question, bool IsCorrect)> summary)
    {
        // Limit summary to prevent large requests
        var limitedSummary = summary.Take(20).ToList();
        var resultsText = string.Join("\n", limitedSummary.Select(s => 
            $"- Савол: {(s.Question.Length > 100 ? s.Question.Substring(0, 100) + "..." : s.Question)} | Натиҷа: {(s.IsCorrect ? "Дуруст" : "Хато")}"));
        
        var prompt = $@"
Шумо як таҳлилгари соҳаи маориф ва равоншиноси таълимӣ ҳастед. Корбар тестро анҷом дод.
Натиҷа: {totalScore} аз {totalQuestions} дуруст.

Маълумоти сессия:
{resultsText}

Вазифаи шумо:
1. Таҳлили амиқи натиҷаҳоро анҷом диҳед.
2. Муайян кунед, ки корбар дар кадом қисматҳо қавӣ аст ва дар куҷо заиф.
3. Тавсияҳои конкретӣ ва илмӣ диҳед, ки чӣ гуна дониши худро дар ин мавзӯъҳо мукаммал кунад.
4. Лаҳни шумо бояд касбӣ, ҳавасмандкунанда ва диққатҷалбкунанда бошад.
5. Танҳо ба забони тоҷикӣ нависед.
";
        var result = await GetAiResponseAsync(prompt);
        return result ?? "Таҳлили AI муваққатан дастнорас аст. Аммо шумо тестро тамом кардед, ки ин аллакай дастовард аст!";
    }

    private async Task<string?> GetAiResponseAsync(string prompt)
    {
        var sw = Stopwatch.StartNew();
        int retryDelay = InitialRetryDelayMs;
        var url = $"{_model}:generateContent?key={_apiKey}";

        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation($"Google Gemini request attempt {attempt + 1}/{MaxRetries}, prompt length: {prompt.Length}");

                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = prompt } } }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        maxOutputTokens = 1500
                    }
                };

                var response = await _httpClient.PostAsJsonAsync(url, requestBody);
                sw.Stop();

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Google Gemini Error ({response.StatusCode}) after {sw.ElapsedMilliseconds}ms: {errorContent}");
                    
                    // Rate limit error (429)
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        _logger.LogError("Rate limit exceeded for Google Gemini.");
                        if (attempt < MaxRetries - 1)
                        {
                            await Task.Delay(retryDelay * 2);
                            retryDelay *= 2;
                            sw.Restart();
                            continue;
                        }
                        return null;
                    }
                    
                    // Don't retry on other 4xx errors
                    if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                    {
                        return null;
                    }
                    
                    // Retry on 5xx errors
                    if (attempt < MaxRetries - 1)
                    {
                        await Task.Delay(retryDelay);
                        retryDelay *= 2;
                        sw.Restart();
                        continue;
                    }
                    
                    return null;
                }

                var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
                
                // Navigate through candidates[0].content.parts[0].text
                if (jsonResponse.TryGetProperty("candidates", out var candidates) && 
                    candidates.GetArrayLength() > 0 &&
                    candidates[0].TryGetProperty("content", out var contentNode) &&
                    contentNode.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0)
                {
                    var content = parts[0].GetProperty("text").GetString();
                    _logger.LogInformation($"Google Gemini request successful after {sw.ElapsedMilliseconds}ms");
                    return content;
                }

                _logger.LogWarning("Google Gemini returned success but no content found in response.");
                return null;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError($"Unexpected error on attempt {attempt + 1}/{MaxRetries} after {sw.ElapsedMilliseconds}ms: {ex.GetType().Name} - {ex.Message}");
                
                if (attempt < MaxRetries - 1)
                {
                    await Task.Delay(retryDelay);
                    retryDelay *= 2;
                    sw.Restart();
                }
                else
                {
                    return null;
                }
            }
        }

        return null;
    }
}
