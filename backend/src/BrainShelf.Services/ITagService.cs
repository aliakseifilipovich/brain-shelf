using BrainShelf.Core.Entities;

namespace BrainShelf.Services;

/// <summary>
/// Service interface for tag management operations
/// </summary>
public interface ITagService
{
    Task<IEnumerable<Tag>> GetAllTagsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetTagsByUsageAsync(int limit = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetRecentTagsAsync(int limit = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetUnusedTagsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> SearchTagsAsync(string query, CancellationToken cancellationToken = default);
    Task<Tag?> GetTagByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tag> CreateTagAsync(string name, CancellationToken cancellationToken = default);
    Task<Tag> RenameTagAsync(Guid id, string newName, CancellationToken cancellationToken = default);
    Task MergeTagsAsync(Guid sourceTagId, Guid targetTagId, CancellationToken cancellationToken = default);
    Task DeleteTagAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetTagStatisticsAsync(CancellationToken cancellationToken = default);
}
