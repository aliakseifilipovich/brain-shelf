namespace BrainShelf.Api.DTOs;

/// <summary>
/// Response DTO for tag information
/// </summary>
public class TagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for tag statistics
/// </summary>
public class TagStatisticsDto
{
    public int TotalTags { get; set; }
    public int TotalUsages { get; set; }
    public List<TagDto> MostUsedTags { get; set; } = new();
    public List<TagDto> RecentlyUsedTags { get; set; } = new();
    public List<TagDto> UnusedTags { get; set; } = new();
}

/// <summary>
/// DTO for creating a new tag
/// </summary>
public class CreateTagDto
{
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// DTO for merging tags
/// </summary>
public class MergeTagsDto
{
    public Guid SourceTagId { get; set; }
    public Guid TargetTagId { get; set; }
}

/// <summary>
/// DTO for renaming a tag
/// </summary>
public class RenameTagDto
{
    public string NewName { get; set; } = string.Empty;
}
