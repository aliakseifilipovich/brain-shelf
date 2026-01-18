using BrainShelf.Core.Entities;

namespace BrainShelf.Api.DTOs;

/// <summary>
/// Data transfer object for entry list item (summary view)
/// </summary>
public class EntryListDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EntryType Type { get; set; }
    public string? Url { get; set; }
    public List<string> TagNames { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
