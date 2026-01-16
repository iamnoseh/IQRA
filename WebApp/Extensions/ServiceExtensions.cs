using Application.Interfaces;
using Infrastructure.Services;

namespace WebApp.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
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
        
        return services;
    }
}
