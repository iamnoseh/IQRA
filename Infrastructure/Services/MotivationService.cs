using Application.DTOs.CMS;
using Application.Interfaces;
using Domain.Entities.CMS;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class MotivationService(ApplicationDbContext context) : IMotivationService
{
    public async Task<IEnumerable<MotivationalQuoteDto>> GetAllAsync()
    {
        var quotes = await context.MotivationalQuotes.ToListAsync();
        return quotes.Select(q => new MotivationalQuoteDto
        {
            Id = q.Id,
            Content = q.Content,
            Author = q.Author,
            Language = q.Language
        });
    }

    public async Task<MotivationalQuoteDto?> GetByIdAsync(int id)
    {
        var q = await context.MotivationalQuotes.FindAsync(id);
        if (q == null) return null;

        return new MotivationalQuoteDto
        {
            Id = q.Id,
            Content = q.Content,
            Author = q.Author,
            Language = q.Language
        };
    }

    public async Task<MotivationalQuoteDto> GetRandomQuoteAsync()
    {
        var q = await context.MotivationalQuotes
            .OrderBy(r => Guid.NewGuid())
            .FirstOrDefaultAsync();

        if (q == null)
        {
            return new MotivationalQuoteDto
            {
                Content = "Зи гаҳвора то гӯр дониш биҷӯй.",
                Author = "Hadith",
                Language = "tj"
            };
        }

        return new MotivationalQuoteDto
        {
            Id = q.Id,
            Content = q.Content,
            Author = q.Author,
            Language = q.Language
        };
    }

    public async Task<MotivationalQuoteDto> CreateAsync(CreateMotivationalQuoteDto dto)
    {
        var quote = new MotivationalQuote
        {
            Content = dto.Content,
            Author = dto.Author,
            Language = dto.Language
        };

        context.MotivationalQuotes.Add(quote);
        await context.SaveChangesAsync();

        return new MotivationalQuoteDto
        {
            Id = quote.Id,
            Content = quote.Content,
            Author = quote.Author,
            Language = quote.Language
        };
    }

    public async Task UpdateAsync(MotivationalQuoteDto dto)
    {
        var quote = await context.MotivationalQuotes.FindAsync(dto.Id);
        if (quote != null)
        {
            quote.Content = dto.Content;
            quote.Author = dto.Author;
            quote.Language = dto.Language;

            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var quote = await context.MotivationalQuotes.FindAsync(id);
        if (quote != null)
        {
            context.MotivationalQuotes.Remove(quote);
            await context.SaveChangesAsync();
        }
    }
}
