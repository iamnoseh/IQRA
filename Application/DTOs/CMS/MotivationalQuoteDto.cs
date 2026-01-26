namespace Application.DTOs.CMS;

public class MotivationalQuoteDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
}
