using BrainShelf.Core.Entities;

namespace BrainShelf.Services.Interfaces;

/// <summary>
/// Service interface for managing entries
/// Provides CRUD operations, filtering, and tag management
/// </summary>
public interface IEntryService
{
    /// <summary>
    /// Retrieves all entries with filtering and pagination support
    /// </summary>
    /// <param name="projectId">Optional project ID filter</param>
    /// <param name="type">Optional entry type filter</param>
    /// <param name="tags">Optional tag names filter (OR logic)</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple containing list of entries and total count</returns>
    Task<(IEnumerable<Entry> Entries, int TotalCount)> GetAllAsync(
        Guid? projectId,
        EntryType? type,
        List<string>? tags,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves entries for a specific project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple containing list of entries and total count</returns>
    Task<(IEnumerable<Entry> Entries, int TotalCount)> GetByProjectIdAsync(
        Guid projectId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an entry by its ID
    /// </summary>
    /// <param name="id">Entry ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Entry if found, null otherwise</returns>
    Task<Entry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new entry with tags
    /// Tags are auto-created if they don't exist
    /// </summary>
    /// <param name="entry">Entry to create</param>
    /// <param name="tagNames">List of tag names</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created entry with tags</returns>
    Task<Entry> CreateAsync(Entry entry, List<string> tagNames, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entry
    /// </summary>
    /// <param name="id">Entry ID to update</param>
    /// <param name="entry">Updated entry data</param>
    /// <param name="tagNames">Updated list of tag names</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated entry if found, null otherwise</returns>
    Task<Entry?> UpdateAsync(Guid id, Entry entry, List<string> tagNames, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entry by its ID
    /// </summary>
    /// <param name="id">Entry ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
