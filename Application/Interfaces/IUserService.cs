using Application.DTOs.Users;
using Application.Responses;

namespace Application.Interfaces;

public interface IUserService
{
    Task<Response<UserProfileDto>> GetProfileAsync(Guid userId);
    Task<Response<bool>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task<Response<UserProfileDto>> GetProfileByUsernameAsync(string username);
}
