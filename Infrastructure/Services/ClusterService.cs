using System.Net;
using Application.DTOs.Admin;
using Application.Interfaces;
using Application.Responses;
using Domain.Entities.Reference;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ClusterService(ApplicationDbContext context, IFileStorageService fileStorageService) : IClusterService
{
    public async Task<Response<List<ClusterDto>>> GetAllClustersAsync()
    {
        var clusters = await context.Clusters
            .Include(c => c.ClusterSubjects)
                .ThenInclude(cs => cs.Subject)
            .Where(c => c.IsActive)
            .OrderBy(c => c.ClusterNumber)
            .ToListAsync();

        var clusterDtos = clusters.Select(MapToDto).ToList();
        return new Response<List<ClusterDto>>(clusterDtos);
    }

    public async Task<Response<ClusterDto>> GetClusterByIdAsync(int id)
    {
        var cluster = await context.Clusters
            .Include(c => c.ClusterSubjects)
                .ThenInclude(cs => cs.Subject)
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

        if (cluster == null)
            return new Response<ClusterDto>(HttpStatusCode.NotFound, "Кластер ёфт нашуд");

        return new Response<ClusterDto>(MapToDto(cluster));
    }

    public async Task<Response<ClusterDto>> CreateClusterAsync(CreateClusterRequest request)
    {
        var maxClusterNumber = await context.Clusters
            .MaxAsync(c => (int?)c.ClusterNumber) ?? 0;

        string? imageUrl = null;
        if (request.Image != null)
        {
            imageUrl = await fileStorageService.SaveFileAsync(request.Image, "uploads/clusters");
        }

        var cluster = new Cluster
        {
            ClusterNumber = maxClusterNumber + 1,
            Name = request.Name,
            Description = request.Description,
            ImageUrl = imageUrl ?? string.Empty,
            IsActive = true
        };

        context.Clusters.Add(cluster);
        await context.SaveChangesAsync();

        await context.Entry(cluster)
            .Collection(c => c.ClusterSubjects)
            .LoadAsync();

        return new Response<ClusterDto>(MapToDto(cluster))
        {
            Message = "Кластер бомуваффақият сохта шуд"
        };
    }

    public async Task<Response<ClusterDto>> UpdateClusterAsync(int id, UpdateClusterRequest request)
    {
        var cluster = await context.Clusters
            .Include(c => c.ClusterSubjects)
                .ThenInclude(cs => cs.Subject)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cluster == null)
            return new Response<ClusterDto>(HttpStatusCode.NotFound, "Кластер ёфт нашуд");

        // Обновление только непустых полей
        if (!string.IsNullOrWhiteSpace(request.Name))
            cluster.Name = request.Name;

        if (!string.IsNullOrWhiteSpace(request.Description))
            cluster.Description = request.Description;

        if (request.Image != null)
        {
            var imageUrl = await fileStorageService.SaveFileAsync(request.Image, "uploads/clusters");
            cluster.ImageUrl = imageUrl;
        }

        if (request.IsActive.HasValue)
            cluster.IsActive = request.IsActive.Value;

        await context.SaveChangesAsync();

        return new Response<ClusterDto>(MapToDto(cluster))
        {
            Message = "Кластер бомуваффақият навсозӣ шуд"
        };
    }

    public async Task<Response<bool>> DeleteClusterAsync(int id)
    {
        var cluster = await context.Clusters
            .Include(c => c.ClusterSubjects)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cluster == null)
            return new Response<bool>(HttpStatusCode.NotFound, "Кластер ёфт нашуд");

        context.ClusterSubjects.RemoveRange(cluster.ClusterSubjects);
        context.Clusters.Remove(cluster);
        await context.SaveChangesAsync();

        return new Response<bool>(true)
        {
            Message = "Кластер бомуваффақият нест карда шуд"
        };
    }

    public async Task<Response<bool>> AddSubjectToClusterAsync(int clusterId, AddSubjectToClusterRequest request)
    {
        var cluster = await context.Clusters.FindAsync(clusterId);
        if (cluster == null)
            return new Response<bool>(HttpStatusCode.NotFound, "Кластер ёфт нашуд");

        var subject = await context.Subjects.FindAsync(request.SubjectId);
        if (subject == null)
            return new Response<bool>(HttpStatusCode.NotFound, "Фан ёфт нашуд");

        var exists = await context.ClusterSubjects
            .AnyAsync(cs => cs.ClusterId == clusterId 
                         && cs.SubjectId == request.SubjectId 
                         && cs.ComponentType == request.ComponentType);

        if (exists)
            return new Response<bool>(
                HttpStatusCode.BadRequest, 
                "Ин фан аллакай ба ин қисм илова шудааст");

        var clusterSubject = new ClusterSubject
        {
            ClusterId = clusterId,
            SubjectId = request.SubjectId,
            ComponentType = request.ComponentType,
            DisplayOrder = request.DisplayOrder
        };

        context.ClusterSubjects.Add(clusterSubject);
        await context.SaveChangesAsync();

        return new Response<bool>(true)
        {
            Message = "Фан ба кластер бомуваффақият илова шуд"
        };
    }

    public async Task<Response<bool>> RemoveSubjectFromClusterAsync(
        int clusterId, 
        int subjectId, 
        ComponentType componentType)
    {
        var clusterSubject = await context.ClusterSubjects
            .FirstOrDefaultAsync(cs => cs.ClusterId == clusterId 
                                    && cs.SubjectId == subjectId 
                                    && cs.ComponentType == componentType);

        if (clusterSubject == null)
            return new Response<bool>(HttpStatusCode.NotFound, "Алоқаи кластер-фан ёфт нашуд");

        context.ClusterSubjects.Remove(clusterSubject);
        await context.SaveChangesAsync();

        return new Response<bool>(true)
        {
            Message = "Фан аз кластер бомуваффақият хориҷ карда шуд"
        };
    }

    public async Task<Response<bool>> ReorderSubjectsAsync(int clusterId, ReorderSubjectsRequest request)
    {
        var cluster = await context.Clusters.FindAsync(clusterId);
        if (cluster == null)
            return new Response<bool>(HttpStatusCode.NotFound, "Кластер ёфт нашуд");

        foreach (var item in request.SubjectOrders)
        {
            var clusterSubject = await context.ClusterSubjects
                .FirstOrDefaultAsync(cs => cs.ClusterId == clusterId 
                                        && cs.SubjectId == item.SubjectId 
                                        && cs.ComponentType == item.ComponentType);

            if (clusterSubject != null)
            {
                clusterSubject.DisplayOrder = item.DisplayOrder;
            }
        }

        await context.SaveChangesAsync();

        return new Response<bool>(true)
        {
            Message = "Тартиби фанҳо бомуваффақият иваз карда шуд"
        };
    }

    private static ClusterDto MapToDto(Cluster cluster)
    {
        return new ClusterDto
        {
            Id = cluster.Id,
            ClusterNumber = cluster.ClusterNumber,
            Name = cluster.Name,
            Description = cluster.Description,
            ImageUrl = cluster.ImageUrl,
            IsActive = cluster.IsActive,
            Subjects = cluster.ClusterSubjects
                .OrderBy(cs => cs.DisplayOrder)
                .Select(cs => new ClusterSubjectDto
                {
                    Id = cs.Id,
                    SubjectId = cs.SubjectId,
                    SubjectName = cs.Subject?.Name ?? string.Empty,
                    SubjectIconUrl = cs.Subject?.IconUrl ?? string.Empty,
                    ComponentType = cs.ComponentType,
                    DisplayOrder = cs.DisplayOrder
                })
                .ToList()
        };
    }
}
