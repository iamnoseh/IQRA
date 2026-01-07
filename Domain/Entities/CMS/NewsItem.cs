namespace Domain.Entities.CMS;

public class NewsItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    
    public DateTime PublishedAt { get; set; }
    public bool IsPublished { get; set; }
    
    public Guid AuthorId { get; set; }
}
