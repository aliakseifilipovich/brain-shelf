namespace BrainShelf.Core.Entities;

/// <summary>
/// Represents an entry (link, note, code snippet, or task) in the system
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
    
    public Metadata? Metadata { get; set; }
    
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}

/// <summary>
/// Types of entries supported by the system
/// </summary>
public enum EntryType
{
    Note = 0,
    Link = 1,
    Code = 2,
    Task = 3
}
