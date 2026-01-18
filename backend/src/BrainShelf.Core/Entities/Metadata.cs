namespace BrainShelf.Core.Entities;

/// <summary>
/// Represents metadata extracted from a URL for an entry
/// </summary>
public class Metadata : BaseEntity
{
    public Guid EntryId { get; set; }
    
    public Entry Entry { get; set; } = null!;
    
    public string? PageTitle { get; set; }
    
    public string? MetaDescription { get; set; }
    
    public string? Keywords { get; set; }
    
    public string? PreviewImageUrl { get; set; }
    
    public DateTime ExtractedAt { get; set; }
}
