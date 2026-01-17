using Application.DTOs.Reference;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/reference")]
[Authorize(Roles = "Admin")]
public class ReferenceController(IReferenceService referenceService) : ControllerBase
{
    #region Schools
    [HttpGet("schools")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSchools([FromQuery] SchoolSearchRequest request)
    {
        var result = await referenceService.GetSchoolsAsync(request);
        return Ok(result.Data);
    }

    [HttpGet("schools/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSchool(int id)
    {
        var result = await referenceService.GetSchoolByIdAsync(id);
        if (!result.Success) return NotFound(new { message = result.Message });
        return Ok(result.Data);
    }

    [HttpPost("schools")]
    public async Task<IActionResult> CreateSchool([FromBody] CreateSchoolRequest request)
    {
        var result = await referenceService.CreateSchoolAsync(request);
        return Ok(result.Data);
    }

    [HttpPut("schools/{id}")]
    public async Task<IActionResult> UpdateSchool(int id, [FromBody] UpdateSchoolRequest request)
    {
        var result = await referenceService.UpdateSchoolAsync(id, request);
        if (!result.Success) return NotFound(new { message = result.Message });
        return Ok(result.Data);
    }

    [HttpDelete("schools/{id}")]
    public async Task<IActionResult> DeleteSchool(int id)
    {
        var result = await referenceService.DeleteSchoolAsync(id);
        if (!result.Success) return NotFound(new { message = result.Message });
        return Ok(new { message = "Мактаб нест карда шуд" });
    }
    #endregion

    #region Universities
    [HttpGet("universities")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUniversities([FromQuery] UniversitySearchRequest request)
    {
        var result = await referenceService.GetUniversitiesAsync(request);
        return Ok(result.Data);
    }

    [HttpGet("universities/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUniversity(int id)
    {
        var result = await referenceService.GetUniversityByIdAsync(id);
        if (!result.Success) return NotFound(new { message = result.Message });
        return Ok(result.Data);
    }

    [HttpPost("universities")]
    public async Task<IActionResult> CreateUniversity([FromBody] CreateUniversityRequest request)
    {
        var result = await referenceService.CreateUniversityAsync(request);
        return Ok(result.Data);
    }

    [HttpPut("universities/{id}")]
    public async Task<IActionResult> UpdateUniversity(int id, [FromBody] UpdateUniversityRequest request)
    {
        var result = await referenceService.UpdateUniversityAsync(id, request);
        if (!result.Success) return NotFound(new { message = result.Message });
        return Ok(result.Data);
    }

    [HttpDelete("universities/{id}")]
    public async Task<IActionResult> DeleteUniversity(int id)
    {
        var result = await referenceService.DeleteUniversityAsync(id);
        if (!result.Success) return NotFound(new { message = result.Message });
        return Ok(new { message = "Донишгоҳ нест карда шуд" });
    }
    #endregion

    #region Faculties
    [HttpGet("universities/{universityId}/faculties")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFaculties(int universityId)
    {
        var result = await referenceService.GetFacultiesByUniversityIdAsync(universityId);
        return Ok(result.Data);
    }

    [HttpPost("faculties")]
    public async Task<IActionResult> CreateFaculty([FromBody] CreateFacultyRequest request)
    {
        var result = await referenceService.CreateFacultyAsync(request);
        return Ok(result.Data);
    }

    [HttpPut("faculties/{id}")]
    public async Task<IActionResult> UpdateFaculty(int id, [FromBody] UpdateFacultyRequest request)
    {
        var result = await referenceService.UpdateFacultyAsync(id, request);
        if (!result.Success) return NotFound(new { message = result.Message });
        return Ok(result.Data);
    }

    [HttpDelete("faculties/{id}")]
    public async Task<IActionResult> DeleteFaculty(int id)
    {
        var result = await referenceService.DeleteFacultyAsync(id);
        if (!result.Success) return NotFound(new { message = result.Message });
        return Ok(new { message = "Факултет нест карда шуд" });
    }
    #endregion

    #region Majors
    [HttpGet("faculties/{facultyId}/majors")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMajors(int facultyId)
    {
        var result = await referenceService.GetMajorsByFacultyIdAsync(facultyId);
        return Ok(result.Data);
    }

    [HttpPost("majors")]
    public async Task<IActionResult> CreateMajor([FromBody] CreateMajorRequest request)
    {
        var result = await referenceService.CreateMajorAsync(request);
        return Ok(result.Data);
    }

    [HttpPut("majors/{id}")]
    public async Task<IActionResult> UpdateMajor(int id, [FromBody] UpdateMajorRequest request)
    {
        var result = await referenceService.UpdateMajorAsync(id, request);
        if (!result.Success) return NotFound(new { message = result.Message });
        return Ok(result.Data);
    }

    [HttpDelete("majors/{id}")]
    public async Task<IActionResult> DeleteMajor(int id)
    {
        var result = await referenceService.DeleteMajorAsync(id);
        if (!result.Success) return NotFound(new { message = result.Message });
        return Ok(new { message = "Ихтисос нест карда шуд" });
    }
    #endregion
}
