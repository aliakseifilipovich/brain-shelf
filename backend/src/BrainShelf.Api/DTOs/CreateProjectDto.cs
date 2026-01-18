namespace BrainShelf.Api.DTOs;

/// <summary>
/// Data transfer object for creating a new project
/// </summary>
public class CreateProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#3B82F6";
}
