using BrainShelf.Core.Entities;
using BrainShelf.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BrainShelf.Services;

/// <summary>
/// Service for performing full-text search operations across entries and metadata.
/// </summary>
public interface ISearchService
{
    Task<(List<Entry> Entries, int TotalCount)> SearchEntriesAsync(
        string searchQuery,
        Guid? projectId = null,
        EntryType? type = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 20);
}

/// <summary>
/// Implementation of full-text search service using PostgreSQL full-text search capabilities.
/// </summary>
public class SearchService : ISearchService
{
    private readonly ApplicationDbContext _context;

    public SearchService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Searches entries using PostgreSQL full-text search with ranking.
    /// Supports multilingual search (English and Russian) and various filters.
    /// </summary>
    public async Task<(List<Entry> Entries, int TotalCount)> SearchEntriesAsync(
        string searchQuery,
        Guid? projectId = null,
        EntryType? type = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 20)
    {
        IQueryable<Entry> query = _context.Entries
            .Include(e => e.Metadata)
            .Include(e => e.Tags)
            .AsNoTracking();

        // Apply filters
        if (projectId.HasValue)
        {
            query = query.Where(e => e.ProjectId == projectId.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(e => e.Type == type.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(e => e.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.CreatedAt <= toDate.Value);
        }

        // Apply full-text search if query is provided
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            // Sanitize search query for tsquery
            var sanitizedQuery = SanitizeSearchQuery(searchQuery);

            // Use EF.Functions for PostgreSQL full-text search
            query = query.Where(e =>
                EF.Functions.ToTsVector("english", e.Title ?? "") .Matches(EF.Functions.ToTsQuery("english", sanitizedQuery)) ||
                EF.Functions.ToTsVector("russian", e.Title ?? "").Matches(EF.Functions.ToTsQuery("russian", sanitizedQuery)) ||
                EF.Functions.ToTsVector("english", e.Content ?? "").Matches(EF.Functions.ToTsQuery("english", sanitizedQuery)) ||
                EF.Functions.ToTsVector("russian", e.Content ?? "").Matches(EF.Functions.ToTsQuery("russian", sanitizedQuery)) ||
                EF.Functions.ToTsVector("english", e.Url ?? "").Matches(EF.Functions.ToTsQuery("english", sanitizedQuery)) ||
                (e.Metadata != null && (
                    EF.Functions.ToTsVector("english", e.Metadata.Title ?? "").Matches(EF.Functions.ToTsQuery("english", sanitizedQuery)) ||
                    EF.Functions.ToTsVector("russian", e.Metadata.Title ?? "").Matches(EF.Functions.ToTsQuery("russian", sanitizedQuery)) ||
                    EF.Functions.ToTsVector("english", e.Metadata.Description ?? "").Matches(EF.Functions.ToTsQuery("english", sanitizedQuery)) ||
                    EF.Functions.ToTsVector("russian", e.Metadata.Description ?? "").Matches(EF.Functions.ToTsQuery("russian", sanitizedQuery))
                )) ||
                e.Tags.Any(t => EF.Functions.ILike(t.Name, $"%{searchQuery}%"))
            );

            // Order by updated date (relevance ranking available via search vector indexes)
            query = query.OrderByDescending(e => e.UpdatedAt);
        }
        else
        {
            // No search query - just order by most recent
            query = query.OrderByDescending(e => e.UpdatedAt);
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination
        var entries = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (entries, totalCount);
    }

    /// <summary>
    /// Sanitizes the search query for use with PostgreSQL tsquery.
    /// Handles special characters and converts to proper tsquery format.
    /// </summary>
    private string SanitizeSearchQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return string.Empty;
        }

        // Remove special characters that could break tsquery
        var sanitized = query
            .Replace("'", "''")  // Escape single quotes
            .Replace("&", " ")
            .Replace("|", " ")
            .Replace("!", " ")
            .Replace("(", " ")
            .Replace(")", " ")
            .Trim();

        // Split into words and join with AND operator
        var words = sanitized
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(w => w.Trim())
            .Where(w => !string.IsNullOrWhiteSpace(w));

        return string.Join(" & ", words);
    }
}
