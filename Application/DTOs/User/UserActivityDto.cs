namespace Application.DTOs.User;

public class UserActivityDto
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public List<ActivityDayDto> ActivityDays { get; set; } = new();
}

public class ActivityDayDto
{
    public int Day { get; set; }
    public bool IsActive { get; set; }
    public bool IsToday { get; set; }
}
