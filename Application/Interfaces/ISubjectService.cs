using Application.DTOs.Reference;
using Application.Responses;

namespace Application.Interfaces;

public interface ISubjectService
{
    Task<Response<List<SubjectDto>>> GetAllForSelectAsync();
    Task<Response<SubjectDto>> CreateSubjectAsync(CreateSubjectRequest request);
    Task<Response<SubjectDto>> GetByIdAsync(int id);
}
