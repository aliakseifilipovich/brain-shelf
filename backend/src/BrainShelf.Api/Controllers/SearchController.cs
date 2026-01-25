using BrainShelf.Api.DTOs;
using BrainShelf.Core.Entities;
using BrainShelf.Services;
using Microsoft.AspNetCore.Mvc;

namespace BrainShelf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly ILogger<SearchController> _logger;

    public SearchController(ISearchService searchService, ILogger<SearchController> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    /// <summary>
    /// Performs full-text search across entries with optional filters.
    /// </summary>
    /// <param name="q">Search query (searches across title, content, URL, metadata, and tags)</param>
    /// <param name="projectId">Filter by project ID</param>
    /// <param name="type">Filter by entry type (0=Note, 1=Link, 2=Code, 3=Task)</param>
    /// <param name="fromDate">Filter entries created on or after this date</param>
    /// <param name="toDate">Filter entries created on or before this date</param>
    /// <param name="pageNumber">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of results per page (default: 20, max: 100)</param>
    /// <returns>Paginated search results with relevance ranking</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EntryListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<EntryListDto>>> Search(
        [FromQuery] string? q = null,
        [FromQuery] Guid? projectId = null,
        [FromQuery] EntryType? type = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        if (pageNumber < 1)
        {
            return BadRequest(new { error = "Page number must be greater than 0" });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { error = "Page size must be between 1 and 100" });
        }

        if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
        {
            return BadRequest(new { error = "From date must be before or equal to to date" });
        }

        _logger.LogInformation(
            "Search request: Query='{Query}', ProjectId={ProjectId}, Type={Type}, FromDate={FromDate}, ToDate={ToDate}, Page={PageNumber}, PageSize={PageSize}",
            q, projectId, type, fromDate, toDate, pageNumber, pageSize);

        var (entries, totalCount) = await _searchService.SearchEntriesAsync(
            searchQuery: q ?? string.Empty,
            projectId: projectId,
            type: type,
            fromDate: fromDate,
            toDate: toDate,
            pageNumber: pageNumber,
            pageSize: pageSize);

        var result = new PagedResult<EntryListDto>
        {
            Items = entries.Select(e => new EntryListDto
            {
                Id = e.Id,
                ProjectId = e.ProjectId,
                ProjectName = e.Project?.Name ?? string.Empty,
                Title = e.Title,
                Description = e.Metadata?.Description,
                Type = e.Type,
                Url = e.Url,
                TagNames = e.Tags?.Select(t => t.Name).ToList() ?? new List<string>(),
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(result);
    }
}

