using Application.Interfaces;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace WebApp.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<IOsonSmsService, OsonSmsService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITestService, TestService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IQuestionManagementService, QuestionManagementService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IAiService, OpenRouterAiService>();
        services.AddHttpClient<IAiService, OpenRouterAiService>(client =>
        {
            client.BaseAddress = new Uri(configuration["OpenRouter:BaseUrl"] ?? "https://openrouter.ai/api/v1/");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {configuration["OpenRouter:ApiKey"]}");
            client.DefaultRequestHeaders.Add("HTTP-Referer", "https://iqra.tj");
            client.DefaultRequestHeaders.Add("X-Title", "IQRA Education");
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        });
        
        return services;
    }
}
