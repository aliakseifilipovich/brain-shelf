using BrainShelf.Api.DTOs;
using BrainShelf.Core.Entities;
using BrainShelf.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BrainShelf.Api.Controllers;

/// <summary>
/// API controller for managing projects
/// Provides CRUD operations with validation and pagination support
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(IProjectService projectService, ILogger<ProjectsController> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all projects with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged list of projects</returns>
    /// <response code="200">Returns the paged list of projects</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProjectDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var (projects, totalCount) = await _projectService.GetAllAsync(pageNumber, pageSize, cancellationToken);

        var projectDtos = projects.Select(MapToDto);

        var result = new PagedResult<ProjectDto>
        {
            Items = projectDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific project by ID
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Project details</returns>
    /// <response code="200">Returns the project</response>
    /// <response code="404">Project not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _projectService.GetByIdAsync(id, cancellationToken);

        if (project is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Project not found",
                Detail = $"Project with ID {id} was not found"
            });
        }

        return Ok(MapToDto(project));
    }

    /// <summary>
    /// Creates a new project
    /// </summary>
    /// <param name="createDto">Project creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created project</returns>
    /// <response code="201">Project created successfully</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProjectDto>> Create(
        [FromBody] CreateProjectDto createDto,
        CancellationToken cancellationToken = default)
    {
        var project = new Project
        {
            Name = createDto.Name,
            Description = createDto.Description,
            Color = createDto.Color
        };

        var createdProject = await _projectService.CreateAsync(project, cancellationToken);

        var projectDto = MapToDto(createdProject);

        return CreatedAtAction(
            nameof(GetById),
            new { id = projectDto.Id },
            projectDto);
    }

    /// <summary>
    /// Updates an existing project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="updateDto">Updated project data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated project</returns>
    /// <response code="200">Project updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Project not found</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDto>> Update(
        Guid id,
        [FromBody] UpdateProjectDto updateDto,
        CancellationToken cancellationToken = default)
    {
        var project = new Project
        {
            Name = updateDto.Name,
            Description = updateDto.Description,
            Color = updateDto.Color
        };

        var updatedProject = await _projectService.UpdateAsync(id, project, cancellationToken);

        if (updatedProject is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Project not found",
                Detail = $"Project with ID {id} was not found"
            });
        }

        return Ok(MapToDto(updatedProject));
    }

    /// <summary>
    /// Deletes a project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Project deleted successfully</response>
    /// <response code="404">Project not found</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await _projectService.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Project not found",
                Detail = $"Project with ID {id} was not found"
            });
        }

        return NoContent();
    }

    private static ProjectDto MapToDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Color = project.Color,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };
    }
}
