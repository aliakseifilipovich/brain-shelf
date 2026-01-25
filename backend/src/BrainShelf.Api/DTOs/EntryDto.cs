using BrainShelf.Core.Entities;

namespace BrainShelf.Api.DTOs;

/// <summary>
/// Data transfer object for entry response with full details
/// </summary>
public class EntryDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EntryType Type { get; set; }
    public string? Content { get; set; }
    public string? Url { get; set; }
    public List<TagDto> Tags { get; set; } = [];
    public MetadataDto? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Data transfer object for metadata information
/// </summary>
public class MetadataDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Keywords { get; set; }
    public string? ImageUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string? Author { get; set; }
    public string? SiteName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
