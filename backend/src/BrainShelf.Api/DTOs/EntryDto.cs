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
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Data transfer object for tag information
/// </summary>
public class TagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
