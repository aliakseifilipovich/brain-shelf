namespace BrainShelf.Core.Entities;

/// <summary>
/// Represents a tag for categorizing entries
/// </summary>
public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<Entry> Entries { get; set; } = new List<Entry>();
}
