using Application.Interfaces;
using Infrastructure.Services;

namespace WebApp.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<IOsonSmsService, OsonSmsService>();
        
        return services;
    }
}
