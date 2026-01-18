using BrainShelf.Core.Entities;

namespace BrainShelf.Api.DTOs;

/// <summary>
/// Data transfer object for creating a new entry
/// </summary>
public class CreateEntryDto
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EntryType Type { get; set; }
    public string? Content { get; set; }
    public string? Url { get; set; }
    public List<string> Tags { get; set; } = [];
}
