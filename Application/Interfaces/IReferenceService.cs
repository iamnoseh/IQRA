using Application.DTOs.Reference;
using Application.Responses;

namespace Application.Interfaces;

public interface IReferenceService
{
    // Schools
    Task<Response<PaginatedResponse<SchoolDto>>> GetSchoolsAsync(SchoolSearchRequest request);
    Task<Response<SchoolDto>> GetSchoolByIdAsync(int id);
    Task<Response<SchoolDto>> CreateSchoolAsync(CreateSchoolRequest request);
    Task<Response<SchoolDto>> UpdateSchoolAsync(int id, UpdateSchoolRequest request);
    Task<Response<bool>> DeleteSchoolAsync(int id);

    // Universities
    Task<Response<PaginatedResponse<UniversityDto>>> GetUniversitiesAsync(UniversitySearchRequest request);
    Task<Response<UniversityDto>> GetUniversityByIdAsync(int id);
    Task<Response<UniversityDto>> CreateUniversityAsync(CreateUniversityRequest request);
    Task<Response<UniversityDto>> UpdateUniversityAsync(int id, UpdateUniversityRequest request);
    Task<Response<bool>> DeleteUniversityAsync(int id);

    // Faculties
    Task<Response<List<FacultyDto>>> GetFacultiesByUniversityIdAsync(int universityId);
    Task<Response<FacultyDto>> CreateFacultyAsync(CreateFacultyRequest request);
    Task<Response<FacultyDto>> UpdateFacultyAsync(int id, UpdateFacultyRequest request);
    Task<Response<bool>> DeleteFacultyAsync(int id);

    // Majors
    Task<Response<List<MajorDto>>> GetMajorsByFacultyIdAsync(int facultyId);
    Task<Response<MajorDto>> CreateMajorAsync(CreateMajorRequest request);
    Task<Response<MajorDto>> UpdateMajorAsync(int id, UpdateMajorRequest request);
    Task<Response<bool>> DeleteMajorAsync(int id);
}
