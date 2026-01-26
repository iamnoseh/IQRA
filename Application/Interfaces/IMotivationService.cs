using Application.DTOs.CMS;

namespace Application.Interfaces;

public interface IMotivationService
{
    Task<IEnumerable<MotivationalQuoteDto>> GetAllAsync();
    Task<MotivationalQuoteDto?> GetByIdAsync(int id);
    Task<MotivationalQuoteDto> GetRandomQuoteAsync();
    Task<MotivationalQuoteDto> CreateAsync(CreateMotivationalQuoteDto dto);
    Task UpdateAsync(MotivationalQuoteDto dto);
    Task DeleteAsync(int id);
}
