using BrainShelf.Core.Entities;

namespace BrainShelf.Api.DTOs;

/// <summary>
/// Response DTO for template information
/// </summary>
public class TemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EntryType Type { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool IsDefault { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new template
/// </summary>
public class CreateTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EntryType Type { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool IsDefault { get; set; }
    public Guid? ProjectId { get; set; }
}

/// <summary>
/// DTO for updating a template
/// </summary>
public class UpdateTemplateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public EntryType? Type { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public List<string>? Tags { get; set; }
    public bool? IsDefault { get; set; }
    public Guid? ProjectId { get; set; }
}

/// <summary>
/// DTO for duplicating an entry
/// </summary>
public class DuplicateEntryDto
{
    public Guid EntryId { get; set; }
    public string? NewTitle { get; set; }
}

/// <summary>
/// DTO for bulk delete operation
/// </summary>
public class BulkDeleteDto
{
    public List<Guid> EntryIds { get; set; } = new();
}

/// <summary>
/// DTO for bulk tag operation
/// </summary>
public class BulkTagDto
{
    public List<Guid> EntryIds { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}
