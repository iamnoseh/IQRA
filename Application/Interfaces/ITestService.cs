using Application.DTOs.Testing;
using Application.Responses;

namespace Application.Interfaces;

public interface ITestService
{
    Task<Response<Guid>> StartTestAsync(Guid userId, StartTestRequest request);
    Task<Response<List<QuestionWithAnswersDto>>> GetTestQuestionsAsync(Guid testSessionId);
    Task<Response<bool>> SubmitAnswerAsync(Guid userId, SubmitAnswerRequest request);
    Task<Response<TestResultDto>> FinishTestAsync(Guid userId, Guid testSessionId);
    Task<Response<List<TestSessionDto>>> GetUserTestHistoryAsync(Guid userId, int page = 1, int pageSize = 10);
}
