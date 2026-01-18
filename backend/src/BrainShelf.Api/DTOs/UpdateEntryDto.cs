using BrainShelf.Core.Entities;

namespace BrainShelf.Api.DTOs;

/// <summary>
/// Data transfer object for updating an existing entry
/// </summary>
public class UpdateEntryDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EntryType Type { get; set; }
    public string? Content { get; set; }
    public string? Url { get; set; }
    public List<string> Tags { get; set; } = [];
}
