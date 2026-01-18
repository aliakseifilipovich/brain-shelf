namespace BrainShelf.Core.Entities;

/// <summary>
/// Represents a project in the system
/// </summary>
public class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#3B82F6";
    public ICollection<Entry> Entries { get; set; } = new List<Entry>();
}
