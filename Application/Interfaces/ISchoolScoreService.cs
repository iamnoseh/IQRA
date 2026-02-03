namespace Application.Interfaces;

public interface ISchoolScoreService
{
    Task AddSchoolPointsAsync(int schoolId, int points);
    Task UpdateStudentCountAsync(int schoolId, int delta);
    Task SyncAllSchoolsStatsAsync();
}
