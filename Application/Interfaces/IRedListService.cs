using Application.DTOs.Testing;
using Application.Responses;

namespace Application.Interfaces;

public interface IRedListService
{
    Task AddToRedListAsync(Guid userId, long questionId);
    Task<Response<List<RedListQuestionDto>>> GetRedListAsync(Guid userId);
    Task<Response<RedListPracticeFeedbackDto>> SubmitPracticeAnswerAsync(Guid userId, SubmitRedListAnswerRequest request);
    Task<Response<int>> GetRedListCountAsync(Guid userId);
    Task ProcessAnswerAsync(Guid userId, long questionId, bool isCorrect);
}
