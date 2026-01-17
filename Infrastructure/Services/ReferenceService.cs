using Application.DTOs.Reference;
using Application.Interfaces;
using Application.Responses;
using Domain.Entities.Reference;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infrastructure.Services;

public class ReferenceService(ApplicationDbContext context) : IReferenceService
{
    #region Schools
    public async Task<Response<PaginatedResponse<SchoolDto>>> GetSchoolsAsync(SchoolSearchRequest request)
    {
        var query = context.Schools.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(term) || 
                                    s.Province.ToLower().Contains(term) || 
                                    s.District.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.Province))
        {
            query = query.Where(s => s.Province == request.Province);
        }

        if (!string.IsNullOrWhiteSpace(request.District))
        {
            query = query.Where(s => s.District == request.District);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(s => s.Name)
            .Skip(request.Skip)
            .Take(request.PageSize)
            .Select(s => new SchoolDto
            {
                Id = s.Id,
                Name = s.Name,
                Province = s.Province,
                District = s.District
            })
            .ToListAsync();

        var response = new PaginatedResponse<SchoolDto>(items, totalCount, request.PageNumber, request.PageSize);
        return new Response<PaginatedResponse<SchoolDto>>(response);
    }

    public async Task<Response<SchoolDto>> GetSchoolByIdAsync(int id)
    {
        var school = await context.Schools
            .Where(s => s.Id == id)
            .Select(s => new SchoolDto
            {
                Id = s.Id,
                Name = s.Name,
                Province = s.Province,
                District = s.District
            })
            .FirstOrDefaultAsync();

        return school == null 
            ? new Response<SchoolDto>(HttpStatusCode.NotFound, "Мактаб ёфт нашуд") 
            : new Response<SchoolDto>(school);
    }

    public async Task<Response<SchoolDto>> CreateSchoolAsync(CreateSchoolRequest request)
    {
        var school = new School
        {
            Name = request.Name,
            Province = request.Province,
            District = request.District
        };

        context.Schools.Add(school);
        await context.SaveChangesAsync();

        return new Response<SchoolDto>(new SchoolDto
        {
            Id = school.Id,
            Name = school.Name,
            Province = school.Province,
            District = school.District
        });
    }

    public async Task<Response<SchoolDto>> UpdateSchoolAsync(int id, UpdateSchoolRequest request)
    {
        var school = await context.Schools.FindAsync(id);
        if (school == null)
            return new Response<SchoolDto>(HttpStatusCode.NotFound, "Мактаб ёфт нашуд");

        school.Name = request.Name;
        school.Province = request.Province;
        school.District = request.District;

        await context.SaveChangesAsync();

        return new Response<SchoolDto>(new SchoolDto
        {
            Id = school.Id,
            Name = school.Name,
            Province = school.Province,
            District = school.District
        });
    }

    public async Task<Response<bool>> DeleteSchoolAsync(int id)
    {
        var school = await context.Schools.FindAsync(id);
        if (school == null)
            return new Response<bool>(HttpStatusCode.NotFound, "Мактаб ёфт нашуд");

        context.Schools.Remove(school);
        await context.SaveChangesAsync();

        return new Response<bool>(true);
    }
    #endregion

    #region Universities
    public async Task<Response<PaginatedResponse<UniversityDto>>> GetUniversitiesAsync(UniversitySearchRequest request)
    {
        var query = context.Universities.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(u => u.Name.ToLower().Contains(term) || 
                                    u.City.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            query = query.Where(u => u.City == request.City);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(u => u.Name)
            .Skip(request.Skip)
            .Take(request.PageSize)
            .Select(u => new UniversityDto
            {
                Id = u.Id,
                Name = u.Name,
                City = u.City
            })
            .ToListAsync();

        var response = new PaginatedResponse<UniversityDto>(items, totalCount, request.PageNumber, request.PageSize);
        return new Response<PaginatedResponse<UniversityDto>>(response);
    }

    public async Task<Response<UniversityDto>> GetUniversityByIdAsync(int id)
    {
        var university = await context.Universities
            .Where(u => u.Id == id)
            .Select(u => new UniversityDto
            {
                Id = u.Id,
                Name = u.Name,
                City = u.City
            })
            .FirstOrDefaultAsync();

        return university == null 
            ? new Response<UniversityDto>(HttpStatusCode.NotFound, "Донишгоҳ ёфт нашуд") 
            : new Response<UniversityDto>(university);
    }

    public async Task<Response<UniversityDto>> CreateUniversityAsync(CreateUniversityRequest request)
    {
        var university = new University
        {
            Name = request.Name,
            City = request.City
        };

        context.Universities.Add(university);
        await context.SaveChangesAsync();

        return new Response<UniversityDto>(new UniversityDto
        {
            Id = university.Id,
            Name = university.Name,
            City = university.City
        });
    }

    public async Task<Response<UniversityDto>> UpdateUniversityAsync(int id, UpdateUniversityRequest request)
    {
        var university = await context.Universities.FindAsync(id);
        if (university == null)
            return new Response<UniversityDto>(HttpStatusCode.NotFound, "Донишгоҳ ёфт нашуд");

        university.Name = request.Name;
        university.City = request.City;

        await context.SaveChangesAsync();

        return new Response<UniversityDto>(new UniversityDto
        {
            Id = university.Id,
            Name = university.Name,
            City = university.City
        });
    }

    public async Task<Response<bool>> DeleteUniversityAsync(int id)
    {
        var university = await context.Universities.FindAsync(id);
        if (university == null)
            return new Response<bool>(HttpStatusCode.NotFound, "Донишгоҳ ёфт нашуд");

        context.Universities.Remove(university);
        await context.SaveChangesAsync();

        return new Response<bool>(true);
    }
    #endregion

    #region Faculties
    public async Task<Response<List<FacultyDto>>> GetFacultiesByUniversityIdAsync(int universityId)
    {
        var faculties = await context.Faculties
            .Where(f => f.UniversityId == universityId)
            .Select(f => new FacultyDto
            {
                Id = f.Id,
                UniversityId = f.UniversityId,
                Name = f.Name
            })
            .ToListAsync();

        return new Response<List<FacultyDto>>(faculties);
    }

    public async Task<Response<FacultyDto>> CreateFacultyAsync(CreateFacultyRequest request)
    {
        var faculty = new Faculty
        {
            UniversityId = request.UniversityId,
            Name = request.Name
        };

        context.Faculties.Add(faculty);
        await context.SaveChangesAsync();

        return new Response<FacultyDto>(new FacultyDto
        {
            Id = faculty.Id,
            UniversityId = faculty.UniversityId,
            Name = faculty.Name
        });
    }

    public async Task<Response<FacultyDto>> UpdateFacultyAsync(int id, UpdateFacultyRequest request)
    {
        var faculty = await context.Faculties.FindAsync(id);
        if (faculty == null)
            return new Response<FacultyDto>(HttpStatusCode.NotFound, "Факултет ёфт нашуд");

        faculty.UniversityId = request.UniversityId;
        faculty.Name = request.Name;

        await context.SaveChangesAsync();

        return new Response<FacultyDto>(new FacultyDto
        {
            Id = faculty.Id,
            UniversityId = faculty.UniversityId,
            Name = faculty.Name
        });
    }

    public async Task<Response<bool>> DeleteFacultyAsync(int id)
    {
        var faculty = await context.Faculties.FindAsync(id);
        if (faculty == null)
            return new Response<bool>(HttpStatusCode.NotFound, "Факултет ёфт нашуд");

        context.Faculties.Remove(faculty);
        await context.SaveChangesAsync();

        return new Response<bool>(true);
    }
    #endregion

    #region Majors
    public async Task<Response<List<MajorDto>>> GetMajorsByFacultyIdAsync(int facultyId)
    {
        var majors = await context.Majors
            .Where(m => m.FacultyId == facultyId)
            .Select(m => new MajorDto
            {
                Id = m.Id,
                FacultyId = m.FacultyId,
                Name = m.Name,
                MinScore2024 = m.MinScore2024,
                MinScore2025 = m.MinScore2025
            })
            .ToListAsync();

        return new Response<List<MajorDto>>(majors);
    }

    public async Task<Response<MajorDto>> CreateMajorAsync(CreateMajorRequest request)
    {
        var major = new Major
        {
            FacultyId = request.FacultyId,
            Name = request.Name,
            MinScore2024 = request.MinScore2024,
            MinScore2025 = request.MinScore2025
        };

        context.Majors.Add(major);
        await context.SaveChangesAsync();

        return new Response<MajorDto>(new MajorDto
        {
            Id = major.Id,
            FacultyId = major.FacultyId,
            Name = major.Name,
            MinScore2024 = major.MinScore2024,
            MinScore2025 = major.MinScore2025
        });
    }

    public async Task<Response<MajorDto>> UpdateMajorAsync(int id, UpdateMajorRequest request)
    {
        var major = await context.Majors.FindAsync(id);
        if (major == null)
            return new Response<MajorDto>(HttpStatusCode.NotFound, "Ихтисос ёфт нашуд");

        major.FacultyId = request.FacultyId;
        major.Name = request.Name;
        major.MinScore2024 = request.MinScore2024;
        major.MinScore2025 = request.MinScore2025;

        await context.SaveChangesAsync();

        return new Response<MajorDto>(new MajorDto
        {
            Id = major.Id,
            FacultyId = major.FacultyId,
            Name = major.Name,
            MinScore2024 = major.MinScore2024,
            MinScore2025 = major.MinScore2025
        });
    }

    public async Task<Response<bool>> DeleteMajorAsync(int id)
    {
        var major = await context.Majors.FindAsync(id);
        if (major == null)
            return new Response<bool>(HttpStatusCode.NotFound, "Ихтисос ёфт нашуд");

        context.Majors.Remove(major);
        await context.SaveChangesAsync();

        return new Response<bool>(true);
    }
    #endregion
}
