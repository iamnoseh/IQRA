using Application.Interfaces;
using Domain.Entities.Users;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Hangfire;
using Hangfire.PostgreSql;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebApp.Extensions;
using WebApp.Middleware;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddIdentityConfiguration();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddCorsConfiguration();

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerConfiguration();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:3001",
                "http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();

    try
    {
        await context.Database.MigrateAsync();
        await SeedData.SeedAsync(context, userManager);
        var schoolScoreService = services.GetRequiredService<ISchoolScoreService>();
        await schoolScoreService.SyncAllSchoolsStatsAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка при инициализации базы данных (seeding).");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "IQRA API v1");
        options.RoutePrefix = "swagger"; 
    });
}

app.UseCors("AllowAll");

app.UseMiddleware<ExceptionHandlingMiddleware>();



app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard();

// Weekly League Reset (Every Sunday at 23:59)
RecurringJob.AddOrUpdate<ILeagueService>(
    "weekly-league-processing",
    service => service.ProcessWeeklyLeaguesAsync(),
    Cron.Weekly(DayOfWeek.Sunday, 23, 59)
);

// Daily Rank Snapshot (Every Day at 00:00)
RecurringJob.AddOrUpdate<ILeagueService>(
    "daily-rank-snapshot",
    service => service.SnapshotDailyRanksAsync(),
    Cron.Daily(0, 0)
);

app.MapControllers();
app.MapHub<Infrastructure.Hubs.DuelHub>("/hubs/duel");
app.Run();