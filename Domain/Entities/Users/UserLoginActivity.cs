namespace Domain.Entities.Users;

public class UserLoginActivity
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime LoginDate { get; set; }
    
    public AppUser User { get; set; } = null!;
}
