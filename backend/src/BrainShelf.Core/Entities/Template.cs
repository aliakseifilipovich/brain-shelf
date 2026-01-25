namespace BrainShelf.Core.Entities;

/// <summary>
/// Represents a template for creating entries
/// </summary>
public class Template : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EntryType Type { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool IsDefault { get; set; }
    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }
}
