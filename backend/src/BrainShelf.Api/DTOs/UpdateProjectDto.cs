namespace BrainShelf.Api.DTOs;

/// <summary>
/// Data transfer object for updating an existing project
/// </summary>
public class UpdateProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#3B82F6";
}
