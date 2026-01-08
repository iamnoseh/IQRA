using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        
        return services;
    }
}
