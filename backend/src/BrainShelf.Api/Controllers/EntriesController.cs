using BrainShelf.Api.DTOs;
using BrainShelf.Core.Entities;
using BrainShelf.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BrainShelf.Api.Controllers;

/// <summary>
/// API controller for managing entries
/// Provides CRUD operations with filtering, tagging, and pagination support
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EntriesController : ControllerBase
{
    private readonly IEntryService _entryService;
    private readonly IProjectService _projectService;
    private readonly IMetadataExtractionService _metadataExtractionService;
    private readonly ILogger<EntriesController> _logger;

    public EntriesController(
        IEntryService entryService,
        IProjectService projectService,
        IMetadataExtractionService metadataExtractionService,
        ILogger<EntriesController> logger)
    {
        _entryService = entryService;
        _projectService = projectService;
        _metadataExtractionService = metadataExtractionService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all entries with optional filtering and pagination
    /// </summary>
    /// <param name="projectId">Filter by project ID</param>
    /// <param name="type">Filter by entry type</param>
    /// <param name="tags">Filter by tags (comma-separated, OR logic)</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged list of entries</returns>
    /// <response code="200">Returns the paged list of entries</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EntryListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<EntryListDto>>> GetAll(
        [FromQuery] Guid? projectId,
        [FromQuery] EntryType? type,
        [FromQuery] string? tags,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var tagList = string.IsNullOrWhiteSpace(tags)
            ? null
            : tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        var (entries, totalCount) = await _entryService.GetAllAsync(
            projectId, type, tagList, pageNumber, pageSize, cancellationToken);

        var entryDtos = entries.Select(MapToListDto);

        var result = new PagedResult<EntryListDto>
        {
            Items = entryDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(result);
    }

    /// <summary>
    /// Retrieves entries for a specific project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged list of entries for the project</returns>
    /// <response code="200">Returns the paged list of entries</response>
    /// <response code="404">Project not found</response>
    [HttpGet("/api/projects/{projectId:guid}/entries")]
    [ProducesResponseType(typeof(PagedResult<EntryListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<EntryListDto>>> GetByProjectId(
        Guid projectId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Verify project exists
        var project = await _projectService.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Project not found",
                Detail = $"Project with ID {projectId} was not found"
            });
        }

        var (entries, totalCount) = await _entryService.GetByProjectIdAsync(
            projectId, pageNumber, pageSize, cancellationToken);

        var entryDtos = entries.Select(MapToListDto);

        var result = new PagedResult<EntryListDto>
        {
            Items = entryDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific entry by ID
    /// </summary>
    /// <param name="id">Entry ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Entry details</returns>
    /// <response code="200">Returns the entry</response>
    /// <response code="404">Entry not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EntryDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await _entryService.GetByIdAsync(id, cancellationToken);

        if (entry is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Entry not found",
                Detail = $"Entry with ID {id} was not found"
            });
        }

        return Ok(MapToDto(entry));
    }

    /// <summary>
    /// Creates a new entry
    /// </summary>
    /// <param name="createDto">Entry creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created entry</returns>
    /// <response code="201">Entry created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Project not found</response>
    [HttpPost]
    [ProducesResponseType(typeof(EntryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EntryDto>> Create(
        [FromBody] CreateEntryDto createDto,
        CancellationToken cancellationToken = default)
    {
        // Verify project exists
        var project = await _projectService.GetByIdAsync(createDto.ProjectId, cancellationToken);
        if (project is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Project not found",
                Detail = $"Project with ID {createDto.ProjectId} was not found"
            });
        }

        var entry = new Entry
        {
            ProjectId = createDto.ProjectId,
            Title = createDto.Title,
            Description = createDto.Description,
            Type = createDto.Type,
            Content = createDto.Content,
            Url = createDto.Url
        };

        var createdEntry = await _entryService.CreateAsync(entry, createDto.Tags, cancellationToken);

        // Automatically extract metadata for Link type entries
        if (createDto.Type == EntryType.Link && !string.IsNullOrWhiteSpace(createDto.Url))
        {
            // Trigger metadata extraction asynchronously (fire and forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _metadataExtractionService.ExtractAndSaveMetadataAsync(
                        createdEntry.Id, 
                        createDto.Url, 
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Background metadata extraction failed for entry {EntryId}", createdEntry.Id);
                }
            }, cancellationToken);
        }

        var entryDto = MapToDto(createdEntry);

        return CreatedAtAction(
            nameof(GetById),
            new { id = entryDto.Id },
            entryDto);
    }

    /// <summary>
    /// Updates an existing entry
    /// </summary>
    /// <param name="id">Entry ID</param>
    /// <param name="updateDto">Updated entry data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated entry</returns>
    /// <response code="200">Entry updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Entry not found</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EntryDto>> Update(
        Guid id,
        [FromBody] UpdateEntryDto updateDto,
        CancellationToken cancellationToken = default)
    {
        var entry = new Entry
        {
            Title = updateDto.Title,
            Description = updateDto.Description,
            Type = updateDto.Type,
            Content = updateDto.Content,
            Url = updateDto.Url
        };

        var updatedEntry = await _entryService.UpdateAsync(id, entry, updateDto.Tags, cancellationToken);

        if (updatedEntry is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Entry not found",
                Detail = $"Entry with ID {id} was not found"
            });
        }

        // Automatically extract metadata for Link type entries
        if (updateDto.Type == EntryType.Link && !string.IsNullOrWhiteSpace(updateDto.Url))
        {
            // Trigger metadata extraction asynchronously (fire and forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _metadataExtractionService.ExtractAndSaveMetadataAsync(
                        updatedEntry.Id, 
                        updateDto.Url, 
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Background metadata extraction failed for entry {EntryId}", updatedEntry.Id);
                }
            }, cancellationToken);
        }

        return Ok(MapToDto(updatedEntry));
    }

    /// <summary>
    /// Deletes an entry
    /// </summary>
    /// <param name="id">Entry ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Entry deleted successfully</response>
    /// <response code="404">Entry not found</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await _entryService.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Entry not found",
                Detail = $"Entry with ID {id} was not found"
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Manually trigger metadata extraction for an entry
    /// </summary>
    /// <param name="id">Entry ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="202">Metadata extraction started</response>
    /// <response code="400">Entry is not a Link type or has no URL</response>
    /// <response code="404">Entry not found</response>
    [HttpPost("{id:guid}/extract-metadata")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExtractMetadata(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await _entryService.GetByIdAsync(id, cancellationToken);

        if (entry is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Entry not found",
                Detail = $"Entry with ID {id} was not found"
            });
        }

        if (entry.Type != EntryType.Link || string.IsNullOrWhiteSpace(entry.Url))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid entry for metadata extraction",
                Detail = "Only Link type entries with a valid URL can have metadata extracted"
            });
        }

        // Trigger extraction asynchronously (fire and forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await _metadataExtractionService.ExtractAndSaveMetadataAsync(id, entry.Url, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background metadata extraction failed for entry {EntryId}", id);
            }
        }, cancellationToken);

        return Accepted();
    }

    private static EntryDto MapToDto(Entry entry)
    {
        return new EntryDto
        {
            Id = entry.Id,
            ProjectId = entry.ProjectId,
            ProjectName = entry.Project?.Name ?? string.Empty,
            Title = entry.Title,
            Description = entry.Description,
            Type = entry.Type,
            Content = entry.Content,
            Url = entry.Url,
            Tags = entry.Tags.Select(t => new TagDto { Id = t.Id, Name = t.Name }).ToList(),
            Metadata = entry.Metadata != null ? new MetadataDto
            {
                Id = entry.Metadata.Id,
                Title = entry.Metadata.Title,
                Description = entry.Metadata.Description,
                Keywords = entry.Metadata.Keywords,
                ImageUrl = entry.Metadata.ImageUrl,
                FaviconUrl = entry.Metadata.FaviconUrl,
                Author = entry.Metadata.Author,
                SiteName = entry.Metadata.SiteName,
                CreatedAt = entry.Metadata.CreatedAt,
                UpdatedAt = entry.Metadata.UpdatedAt
            } : null,
            CreatedAt = entry.CreatedAt,
            UpdatedAt = entry.UpdatedAt
        };
    }

    private static EntryListDto MapToListDto(Entry entry)
    {
        return new EntryListDto
        {
            Id = entry.Id,
            ProjectId = entry.ProjectId,
            ProjectName = entry.Project?.Name ?? string.Empty,
            Title = entry.Title,
            Description = entry.Description,
            Type = entry.Type,
            Url = entry.Url,
            TagNames = entry.Tags.Select(t => t.Name).ToList(),
            CreatedAt = entry.CreatedAt,
            UpdatedAt = entry.UpdatedAt
        };
    }

    /// <summary>
    /// Duplicate an entry
    /// </summary>
    [HttpPost("{id}/duplicate")]
    public async Task<ActionResult<EntryDto>> Duplicate(Guid id, [FromBody] DuplicateEntryDto? duplicateDto = null)
    {
        var entry = await _entryService.GetByIdAsync(id);
        if (entry == null)
        {
            return NotFound(new { message = $"Entry with ID {id} not found" });
        }

        var newEntry = new Entry
        {
            ProjectId = entry.ProjectId,
            Title = duplicateDto?.NewTitle ?? $"{entry.Title} (Copy)",
            Description = entry.Description,
            Type = entry.Type,
            Url = entry.Url,
            Content = entry.Content
        };

        var tagNames = entry.Tags.Select(t => t.Name).ToList();
        var createdEntry = await _entryService.CreateAsync(newEntry, tagNames);

        return Ok(MapToDto(createdEntry));
    }

    /// <summary>
    /// Bulk delete entries
    /// </summary>
    [HttpPost("bulk-delete")]
    public async Task<ActionResult> BulkDelete([FromBody] BulkDeleteDto bulkDeleteDto)
    {
        var deletedCount = 0;
        var errors = new List<string>();

        foreach (var entryId in bulkDeleteDto.EntryIds)
        {
            try
            {
                await _entryService.DeleteAsync(entryId);
                deletedCount++;
            }
            catch (KeyNotFoundException)
            {
                errors.Add($"Entry {entryId} not found");
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to delete entry {entryId}: {ex.Message}");
            }
        }

        return Ok(new { deletedCount, totalRequested = bulkDeleteDto.EntryIds.Count, errors });
    }

    /// <summary>
    /// Bulk add tags to entries
    /// </summary>
    [HttpPost("bulk-tag")]
    public async Task<ActionResult> BulkTag([FromBody] BulkTagDto bulkTagDto)
    {
        var updatedCount = 0;
        var errors = new List<string>();

        foreach (var entryId in bulkTagDto.EntryIds)
        {
            try
            {
                var entry = await _entryService.GetByIdAsync(entryId);
                if (entry == null)
                {
                    errors.Add($"Entry {entryId} not found");
                    continue;
                }

                var existingTags = entry.Tags.Select(t => t.Name).ToList();
                var newTags = existingTags.Union(bulkTagDto.Tags).ToList();

                await _entryService.UpdateAsync(entryId, entry, newTags);
                updatedCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to update entry {entryId}: {ex.Message}");
            }
        }

        return Ok(new { updatedCount, totalRequested = bulkTagDto.EntryIds.Count, errors });
    }
}
