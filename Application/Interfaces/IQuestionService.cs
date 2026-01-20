using Application.DTOs.Testing;

namespace Application.Interfaces;

public interface IQuestionService
{
    Task<List<QuestionWithAnswersDto>> GetRandomQuestionsAsync(int subjectId, int count);
    Task<List<QuestionWithAnswersDto>> GetTestQuestionsAsync(Guid userId, int subjectId, int count);
    Task<QuestionWithAnswersDto?> GetQuestionByIdAsync(long id);
}
