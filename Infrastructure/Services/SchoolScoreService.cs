using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class SchoolScoreService(ApplicationDbContext context, ILogger<SchoolScoreService> logger) : ISchoolScoreService
{
    public async Task AddSchoolPointsAsync(int schoolId, int points)
    {
        if (points == 0) return;

        try
        {
            await context.Database.ExecuteSqlRawAsync(
                "UPDATE Schools SET TotalXP = TotalXP + {0} WHERE Id = {1}",
                points, schoolId);
            
            logger.LogInformation("Added {Points} XP to School ID {SchoolId}", points, schoolId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding points to school {SchoolId}", schoolId);
        }
    }

    public async Task UpdateStudentCountAsync(int schoolId, int delta)
    {
        if (delta == 0) return;

        try
        {
            await context.Database.ExecuteSqlRawAsync(
                "UPDATE Schools SET StudentCount = StudentCount + {0} WHERE Id = {1}",
                delta, schoolId);
            
            logger.LogInformation("Updated StudentCount for School ID {SchoolId} by {Delta}", schoolId, delta);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating student count for school {SchoolId}", schoolId);
        }
    }

    public async Task SyncAllSchoolsStatsAsync()
    {
        try
        {
            logger.LogInformation("Starting full synchronization of school statistics...");
            
           
            await context.Database.ExecuteSqlRawAsync(
                @"UPDATE ""Schools"" s 
                  SET ""StudentCount"" = (
                      SELECT COUNT(*) 
                      FROM ""UserProfiles"" u 
                      WHERE u.""SchoolId"" = s.""Id""
                  )");

            logger.LogInformation("School statistics synchronization completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during school statistics synchronization");
        }
    }
}
