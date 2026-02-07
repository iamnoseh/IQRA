using Application.DTOs.Reference;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClustersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ClustersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllClusters()
    {
        var clusters = await _context.Clusters
            .Include(c => c.ClusterSubjects)
                .ThenInclude(cs => cs.Subject)
            .Where(c => c.IsActive)
            .OrderBy(c => c.ClusterNumber)
            .ToListAsync();

        var clusterDtos = clusters.Select(c => new ClusterDto
        {
            Id = c.Id,
            ClusterNumber = c.ClusterNumber,
            Name = c.Name,
            Description = c.Description,
            ImageUrl = c.ImageUrl,
            PartASubjects = c.ClusterSubjects
                .Where(cs => cs.ComponentType == Domain.Enums.ComponentType.PartA)
                .OrderBy(cs => cs.DisplayOrder)
                .Select(cs => new ClusterSubjectDto
                {
                    SubjectId = cs.SubjectId,
                    SubjectName = cs.Subject?.Name ?? string.Empty,
                    SubjectIconUrl = cs.Subject?.IconUrl ?? string.Empty,
                    DisplayOrder = cs.DisplayOrder
                })
                .ToList(),
            PartBSubjects = c.ClusterSubjects
                .Where(cs => cs.ComponentType == Domain.Enums.ComponentType.PartB)
                .OrderBy(cs => cs.DisplayOrder)
                .Select(cs => new ClusterSubjectDto
                {
                    SubjectId = cs.SubjectId,
                    SubjectName = cs.Subject?.Name ?? string.Empty,
                    SubjectIconUrl = cs.Subject?.IconUrl ?? string.Empty,
                    DisplayOrder = cs.DisplayOrder
                })
                .ToList()
        }).ToList();

        return Ok(clusterDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetClusterById(int id)
    {
        var cluster = await _context.Clusters
            .Include(c => c.ClusterSubjects)
                .ThenInclude(cs => cs.Subject)
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

        if (cluster == null)
            return NotFound("Кластер ёфт нашуд");

        var clusterDto = new ClusterDto
        {
            Id = cluster.Id,
            ClusterNumber = cluster.ClusterNumber,
            Name = cluster.Name,
            Description = cluster.Description,
            ImageUrl = cluster.ImageUrl,
            PartASubjects = cluster.ClusterSubjects
                .Where(cs => cs.ComponentType == Domain.Enums.ComponentType.PartA)
                .OrderBy(cs => cs.DisplayOrder)
                .Select(cs => new ClusterSubjectDto
                {
                    SubjectId = cs.SubjectId,
                    SubjectName = cs.Subject?.Name ?? string.Empty,
                    SubjectIconUrl = cs.Subject?.IconUrl ?? string.Empty,
                    DisplayOrder = cs.DisplayOrder
                })
                .ToList(),
            PartBSubjects = cluster.ClusterSubjects
                .Where(cs => cs.ComponentType == Domain.Enums.ComponentType.PartB)
                .OrderBy(cs => cs.DisplayOrder)
                .Select(cs => new ClusterSubjectDto
                {
                    SubjectId = cs.SubjectId,
                    SubjectName = cs.Subject?.Name ?? string.Empty,
                    SubjectIconUrl = cs.Subject?.IconUrl ?? string.Empty,
                    DisplayOrder = cs.DisplayOrder
                })
                .ToList()
        };

        return Ok(clusterDto);
    }
}
