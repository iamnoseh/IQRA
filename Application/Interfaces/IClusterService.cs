using Application.DTOs.Admin;
using Application.Responses;

namespace Application.Interfaces;

public interface IClusterService
{
    Task<Response<List<ClusterDto>>> GetAllClustersAsync();
    Task<Response<ClusterDto>> GetClusterByIdAsync(int id);
    Task<Response<ClusterDto>> CreateClusterAsync(CreateClusterRequest request);
    Task<Response<ClusterDto>> UpdateClusterAsync(int id, UpdateClusterRequest request);
    Task<Response<bool>> DeleteClusterAsync(int id);
    Task<Response<bool>> AddSubjectToClusterAsync(int clusterId, AddSubjectToClusterRequest request);
    Task<Response<bool>> RemoveSubjectFromClusterAsync(int clusterId, int subjectId, Domain.Enums.ComponentType componentType);
    Task<Response<bool>> ReorderSubjectsAsync(int clusterId, ReorderSubjectsRequest request);
}
