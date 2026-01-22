using Application.DTOs.Users;
using Application.DTOs.User;
using Application.Responses;

namespace Application.Interfaces;

public interface IUserService
{
    Task<Response<UserProfileDto>> GetProfileAsync(Guid userId);
    Task<Response<bool>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task<Response<UserProfileDto>> GetProfileByUsernameAsync(string username);
    Task<Response<UserActivityDto>> GetUserActivityAsync(Guid userId);
    Task RecordLoginActivityAsync(Guid userId);
    Task<Response<TestActivityStatsDto>> GetTestActivityAsync(Guid userId, int days = 30);
}
