namespace BrainShelf.Api.DTOs;

/// <summary>
/// Data transfer object for project response
/// </summary>
public class ProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#3B82F6";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
