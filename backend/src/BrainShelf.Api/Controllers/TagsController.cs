using BrainShelf.Api.DTOs;
using BrainShelf.Services;
using Microsoft.AspNetCore.Mvc;

namespace BrainShelf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    /// <summary>
    /// Get all tags
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetAll([FromQuery] string? search = null)
    {
        var tags = string.IsNullOrWhiteSpace(search)
            ? await _tagService.GetAllTagsAsync()
            : await _tagService.SearchTagsAsync(search);

        var tagDtos = tags.Select(t => new TagDto
        {
            Id = t.Id,
            Name = t.Name,
            UsageCount = t.Entries?.Count ?? 0,
            CreatedAt = t.CreatedAt
        });

        return Ok(tagDtos);
    }

    /// <summary>
    /// Get tag by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetById(Guid id)
    {
        var tag = await _tagService.GetTagByIdAsync(id);
        if (tag == null)
        {
            return NotFound(new { message = $"Tag with ID {id} not found" });
        }

        var tagDto = new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            UsageCount = tag.Entries?.Count ?? 0,
            CreatedAt = tag.CreatedAt
        };

        return Ok(tagDto);
    }

    /// <summary>
    /// Get tags by usage (most popular)
    /// </summary>
    [HttpGet("popular")]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetPopular([FromQuery] int limit = 20)
    {
        var tags = await _tagService.GetTagsByUsageAsync(limit);

        var tagDtos = tags.Select(t => new TagDto
        {
            Id = t.Id,
            Name = t.Name,
            UsageCount = t.Entries?.Count ?? 0,
            CreatedAt = t.CreatedAt
        });

        return Ok(tagDtos);
    }

    /// <summary>
    /// Get recently used tags
    /// </summary>
    [HttpGet("recent")]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetRecent([FromQuery] int limit = 20)
    {
        var tags = await _tagService.GetRecentTagsAsync(limit);

        var tagDtos = tags.Select(t => new TagDto
        {
            Id = t.Id,
            Name = t.Name,
            UsageCount = t.Entries?.Count ?? 0,
            CreatedAt = t.CreatedAt
        });

        return Ok(tagDtos);
    }

    /// <summary>
    /// Get unused tags
    /// </summary>
    [HttpGet("unused")]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetUnused()
    {
        var tags = await _tagService.GetUnusedTagsAsync();

        var tagDtos = tags.Select(t => new TagDto
        {
            Id = t.Id,
            Name = t.Name,
            UsageCount = 0,
            CreatedAt = t.CreatedAt
        });

        return Ok(tagDtos);
    }

    /// <summary>
    /// Get tag statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<TagStatisticsDto>> GetStatistics()
    {
        var stats = await _tagService.GetTagStatisticsAsync();
        var mostUsed = await _tagService.GetTagsByUsageAsync(10);
        var recentlyUsed = await _tagService.GetRecentTagsAsync(10);
        var unused = await _tagService.GetUnusedTagsAsync();

        var statisticsDto = new TagStatisticsDto
        {
            TotalTags = stats["TotalTags"],
            TotalUsages = stats["TotalUsages"],
            MostUsedTags = mostUsed.Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                UsageCount = t.Entries?.Count ?? 0,
                CreatedAt = t.CreatedAt
            }).ToList(),
            RecentlyUsedTags = recentlyUsed.Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                UsageCount = t.Entries?.Count ?? 0,
                CreatedAt = t.CreatedAt
            }).ToList(),
            UnusedTags = unused.Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                UsageCount = 0,
                CreatedAt = t.CreatedAt
            }).ToList()
        };

        return Ok(statisticsDto);
    }

    /// <summary>
    /// Create a new tag
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TagDto>> Create([FromBody] CreateTagDto createDto)
    {
        if (string.IsNullOrWhiteSpace(createDto.Name))
        {
            return BadRequest(new { message = "Tag name is required" });
        }

        var tag = await _tagService.CreateTagAsync(createDto.Name);

        var tagDto = new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            UsageCount = 0,
            CreatedAt = tag.CreatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = tag.Id }, tagDto);
    }

    /// <summary>
    /// Rename a tag
    /// </summary>
    [HttpPut("{id}/rename")]
    public async Task<ActionResult<TagDto>> Rename(Guid id, [FromBody] RenameTagDto renameDto)
    {
        if (string.IsNullOrWhiteSpace(renameDto.NewName))
        {
            return BadRequest(new { message = "New tag name is required" });
        }

        try
        {
            var tag = await _tagService.RenameTagAsync(id, renameDto.NewName);

            var tagDto = new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                UsageCount = tag.Entries?.Count ?? 0,
                CreatedAt = tag.CreatedAt
            };

            return Ok(tagDto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Merge two tags
    /// </summary>
    [HttpPost("merge")]
    public async Task<ActionResult> Merge([FromBody] MergeTagsDto mergeDto)
    {
        try
        {
            await _tagService.MergeTagsAsync(mergeDto.SourceTagId, mergeDto.TargetTagId);
            return Ok(new { message = "Tags merged successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a tag
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            await _tagService.DeleteTagAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
