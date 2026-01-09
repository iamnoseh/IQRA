using Application.DTOs.Testing.Management;
using Application.DTOs.Education;
using Application.Responses;

namespace Application.Interfaces;

public interface IQuestionManagementService
{
    Task<Response<QuestionImportResultDto>> ImportQuestionsAsync(BulkQuestionImportRequest request);
    Task<Response<QuestionImportResultDto>> ValidateImportAsync(BulkQuestionImportRequest request);
    
    Task<Response<QuestionDto>> CreateQuestionAsync(QuestionImportDto dto);
    Task<Response<QuestionDto>> UpdateQuestionAsync(long id, QuestionImportDto dto);
    Task<Response<bool>> DeleteQuestionAsync(long id);
    
    Task<Response<List<QuestionDto>>> GetQuestionsBySubjectAsync(int subjectId, int page = 1, int pageSize = 20);
    Task<Response<List<QuestionDto>>> GetQuestionsByTopicAsync(int topicId, int page = 1, int pageSize = 20);
    Task<Response<QuestionDto>> GetQuestionByIdAsync(long id);
    
    Task<Response<QuestionStatsDto>> GetQuestionStatsAsync();
}
