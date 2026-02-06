using Application.DTOs.Reference;
using Application.Responses;

namespace Application.Interfaces;

public interface ISchoolService
{
    Task<Response<SchoolLeaderboardResponse>> GetLeaderboardAsync(Guid? userId);
}
