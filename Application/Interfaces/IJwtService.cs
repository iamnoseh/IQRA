using Domain.Entities.Users;

namespace Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(AppUser user);
    Guid? ValidateToken(string token);
}
