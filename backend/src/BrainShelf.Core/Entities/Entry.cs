namespace BrainShelf.Core.Entities;

/// <summary>
/// Represents an entry (link, note, setting, or instruction) in the system
/// </summary>
public class Entry : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EntryType Type { get; set; }
    public string? Content { get; set; }
    public string? Url { get; set; }
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}

/// <summary>
/// Types of entries supported by the system
/// </summary>
public enum EntryType
{
    Link = 0,
    Note = 1,
    Setting = 2,
    Instruction = 3
}
