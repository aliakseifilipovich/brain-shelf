using BrainShelf.Core.Entities;
using BrainShelf.Infrastructure.Data;
using BrainShelf.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BrainShelf.Services;

/// <summary>
/// Service implementation for managing entries
/// Handles business logic, data access operations, filtering, and tag management for entries
/// </summary>
public class EntryService : IEntryService
{
    private readonly ApplicationDbContext _context;
    private readonly IMetadataExtractionService _metadataExtractionService;

    public EntryService(ApplicationDbContext context, IMetadataExtractionService metadataExtractionService)
    {
        _context = context;
        _metadataExtractionService = metadataExtractionService;
    }

    public async Task<(IEnumerable<Entry> Entries, int TotalCount)> GetAllAsync(
        Guid? projectId,
        EntryType? type,
        List<string>? tags,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Ensure valid pagination parameters
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, Math.Min(100, pageSize));

        var query = _context.Entries
            .Include(e => e.Project)
            .Include(e => e.Tags)
            .Include(e => e.Metadata)
            .AsQueryable();

        // Apply projectId filter
        if (projectId.HasValue)
        {
            query = query.Where(e => e.ProjectId == projectId.Value);
        }

        // Apply type filter
        if (type.HasValue)
        {
            query = query.Where(e => e.Type == type.Value);
        }

        // Apply tags filter (OR logic - entry must have ANY of the specified tags)
        if (tags is not null && tags.Count > 0)
        {
            query = query.Where(e => e.Tags.Any(t => tags.Contains(t.Name)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var entries = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (entries, totalCount);
    }

    public async Task<(IEnumerable<Entry> Entries, int TotalCount)> GetByProjectIdAsync(
        Guid projectId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Ensure valid pagination parameters
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, Math.Min(100, pageSize));

        var query = _context.Entries
            .Include(e => e.Project)
            .Include(e => e.Tags)
            .Include(e => e.Metadata)
            .Where(e => e.ProjectId == projectId);

        var totalCount = await query.CountAsync(cancellationToken);

        var entries = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (entries, totalCount);
    }

    public async Task<Entry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Entries
            .Include(e => e.Project)
            .Include(e => e.Tags)
            .Include(e => e.Metadata)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Entry> CreateAsync(Entry entry, List<string> tagNames, CancellationToken cancellationToken = default)
    {
        entry.Id = Guid.NewGuid();
        entry.CreatedAt = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;

        // Handle tags - auto-create if they don't exist
        if (tagNames.Count > 0)
        {
            var normalizedTagNames = tagNames.Select(t => t.Trim().ToLowerInvariant()).Distinct().ToList();
            
            // Get existing tags
            var existingTags = await _context.Tags
                .Where(t => normalizedTagNames.Contains(t.Name.ToLower()))
                .ToListAsync(cancellationToken);

            var existingTagNames = existingTags.Select(t => t.Name.ToLowerInvariant()).ToHashSet();

            // Create new tags for names that don't exist
            var newTagNames = normalizedTagNames.Except(existingTagNames).ToList();
            var newTags = newTagNames.Select(name => new Tag
            {
                Id = Guid.NewGuid(),
                Name = name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            if (newTags.Count > 0)
            {
                _context.Tags.AddRange(newTags);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Assign all tags to entry
            entry.Tags = existingTags.Concat(newTags).ToList();
        }

        _context.Entries.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);

        // Trigger async metadata extraction for Link entries
        if (entry.Type == EntryType.Link && !string.IsNullOrWhiteSpace(entry.Url))
        {
            _ = Task.Run(async () =>
            {
                await _metadataExtractionService.ExtractAndSaveMetadataAsync(entry.Id, entry.Url, CancellationToken.None);
            }, cancellationToken);
        }

        // Reload to get navigation properties
        return (await GetByIdAsync(entry.Id, cancellationToken))!;
    }

    public async Task<Entry?> UpdateAsync(Guid id, Entry entry, List<string> tagNames, CancellationToken cancellationToken = default)
    {
        var existingEntry = await _context.Entries
            .Include(e => e.Tags)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (existingEntry is null)
        {
            return null;
        }

        existingEntry.Title = entry.Title;
        existingEntry.Description = entry.Description;
        existingEntry.Type = entry.Type;
        existingEntry.Content = entry.Content;
        
        // Track if URL changed for metadata re-extraction
        var urlChanged = existingEntry.Url != entry.Url;
        existingEntry.Url = entry.Url;
        existingEntry.UpdatedAt = DateTime.UtcNow;

        // Update tags - clear existing and add new ones
        existingEntry.Tags.Clear();

        if (tagNames.Count > 0)
        {
            var normalizedTagNames = tagNames.Select(t => t.Trim().ToLowerInvariant()).Distinct().ToList();
            
            // Get existing tags
            var existingTags = await _context.Tags
                .Where(t => normalizedTagNames.Contains(t.Name.ToLower()))
                .ToListAsync(cancellationToken);

            var existingTagNamesSet = existingTags.Select(t => t.Name.ToLowerInvariant()).ToHashSet();

            // Create new tags for names that don't exist
            var newTagNames = normalizedTagNames.Except(existingTagNamesSet).ToList();
            var newTags = newTagNames.Select(name => new Tag
            {
                Id = Guid.NewGuid(),
                Name = name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            if (newTags.Count > 0)
            {
                _context.Tags.AddRange(newTags);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Assign all tags to entry
            existingEntry.Tags = existingTags.Concat(newTags).ToList();
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Trigger async metadata re-extraction if URL changed for Link entries
        if (urlChanged && existingEntry.Type == EntryType.Link && !string.IsNullOrWhiteSpace(existingEntry.Url))
        {
            _ = Task.Run(async () =>
            {
                await _metadataExtractionService.ExtractAndSaveMetadataAsync(id, existingEntry.Url, CancellationToken.None);
            }, cancellationToken);
        }

        // Reload to get navigation properties
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Entries
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entry is null)
        {
            return false;
        }

        _context.Entries.Remove(entry);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
