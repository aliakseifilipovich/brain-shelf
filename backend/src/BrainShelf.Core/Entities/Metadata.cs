namespace BrainShelf.Core.Entities;

/// <summary>
/// Represents metadata extracted from a URL for an entry
/// Stores page title, description, keywords, images, and other metadata
/// </summary>
public class Metadata : BaseEntity
{
    public Guid EntryId { get; set; }
    
    public Entry Entry { get; set; } = null!;
    
    public string? Title { get; set; }
    
    public string? Description { get; set; }
    
    public string? Keywords { get; set; }
    
    public string? ImageUrl { get; set; }
    
    public string? FaviconUrl { get; set; }
    
    public string? Author { get; set; }
    
    public string? SiteName { get; set; }
}
