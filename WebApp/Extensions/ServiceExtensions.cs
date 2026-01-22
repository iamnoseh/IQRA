using Application.Interfaces;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Hangfire;
using Hangfire.PostgreSql;

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
        services.AddScoped<IReferenceService, ReferenceService>();
        services.AddScoped<IAiService, GoogleGeminiAiService>();
        services.AddScoped<IRedListService, RedListService>();
        
        services.AddScoped<IScoringService, ScoringService>();
        services.AddScoped<IGamificationService, GamificationService>();
        services.AddScoped<ILeagueService, LeagueService>();
        services.AddScoped<INotificationService, NotificationService>();
        
        services.AddHttpClient<IAiService, GoogleGeminiAiService>(client =>
        {
            client.BaseAddress = new Uri(configuration["GoogleGemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta/");
        });
        
        services.AddHangfire(x => x.UsePostgreSqlStorage(configuration.GetConnectionString("DefaultConnection")));
        services.AddHangfireServer();

        return services;
    }
}
