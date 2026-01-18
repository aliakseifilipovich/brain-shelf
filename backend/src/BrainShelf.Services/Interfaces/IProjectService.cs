using BrainShelf.Core.Entities;

namespace BrainShelf.Services.Interfaces;

/// <summary>
/// Service interface for managing projects
/// Provides CRUD operations and pagination support
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Retrieves all projects with pagination support
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple containing list of projects and total count</returns>
    Task<(IEnumerable<Project> Projects, int TotalCount)> GetAllAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a project by its ID
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Project if found, null otherwise</returns>
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new project
    /// </summary>
    /// <param name="project">Project to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created project</returns>
    Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing project
    /// </summary>
    /// <param name="id">Project ID to update</param>
    /// <param name="project">Updated project data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated project if found, null otherwise</returns>
    Task<Project?> UpdateAsync(Guid id, Project project, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a project by its ID
    /// </summary>
    /// <param name="id">Project ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
