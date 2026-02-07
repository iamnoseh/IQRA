using Application.DTOs.Admin;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class ClustersController(IClusterService clusterService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllClusters()
    {
        var response = await clusterService.GetAllClustersAsync();
        return response.Success 
            ? Ok(response.Data) 
            : StatusCode(response.StatusCode, response.Message);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetClusterById(int id)
    {
        var response = await clusterService.GetClusterByIdAsync(id);
        return response.Success 
            ? Ok(response.Data) 
            : StatusCode(response.StatusCode, response.Message);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCluster([FromForm] CreateClusterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await clusterService.CreateClusterAsync(request);
        return response.Success 
            ? Ok(response.Data) 
            : StatusCode(response.StatusCode, response.Message);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCluster(int id, [FromForm] UpdateClusterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await clusterService.UpdateClusterAsync(id, request);
        return response.Success 
            ? Ok(response.Data) 
            : StatusCode(response.StatusCode, response.Message);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCluster(int id)
    {
        var response = await clusterService.DeleteClusterAsync(id);
        return response.Success 
            ? Ok(new { message = response.Message }) 
            : StatusCode(response.StatusCode, response.Message);
    }

    [HttpPost("{id}/subjects")]
    public async Task<IActionResult> AddSubjectToCluster(int id, [FromBody] AddSubjectToClusterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await clusterService.AddSubjectToClusterAsync(id, request);
        return response.Success 
            ? Ok(new { message = response.Message }) 
            : StatusCode(response.StatusCode, response.Message);
    }

    [HttpDelete("{id}/subjects/{subjectId}/{componentType}")]
    public async Task<IActionResult> RemoveSubjectFromCluster(
        int id, 
        int subjectId, 
        Domain.Enums.ComponentType componentType)
    {
        var response = await clusterService.RemoveSubjectFromClusterAsync(id, subjectId, componentType);
        return response.Success 
            ? Ok(new { message = response.Message }) 
            : StatusCode(response.StatusCode, response.Message);
    }

    [HttpPut("{id}/subjects/reorder")]
    public async Task<IActionResult> ReorderSubjects(int id, [FromBody] ReorderSubjectsRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await clusterService.ReorderSubjectsAsync(id, request);
        return response.Success 
            ? Ok(new { message = response.Message }) 
            : StatusCode(response.StatusCode, response.Message);
    }
}
