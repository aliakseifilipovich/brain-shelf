using BrainShelf.Core.Entities;
using BrainShelf.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BrainShelf.Services;

/// <summary>
/// Service for tag management operations
/// </summary>
public class TagService : ITagService
{
    private readonly ApplicationDbContext _context;

    public TagService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Tag>> GetAllTagsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Tag>> GetTagsByUsageAsync(int limit = 20, CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .Include(t => t.Entries)
            .OrderByDescending(t => t.Entries.Count)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Tag>> GetRecentTagsAsync(int limit = 20, CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .Include(t => t.Entries)
            .Where(t => t.Entries.Any())
            .OrderByDescending(t => t.Entries.Max(e => e.UpdatedAt))
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Tag>> GetUnusedTagsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .Include(t => t.Entries)
            .Where(t => !t.Entries.Any())
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Tag>> SearchTagsAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAllTagsAsync(cancellationToken);
        }

        var normalizedQuery = query.Trim().ToLowerInvariant();

        return await _context.Tags
            .Where(t => t.Name.ToLower().Contains(normalizedQuery))
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Tag?> GetTagByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .Include(t => t.Entries)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Tag> CreateTagAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLowerInvariant();

        // Check if tag already exists
        var existingTag = await _context.Tags
            .FirstOrDefaultAsync(t => t.Name.ToLower() == normalizedName, cancellationToken);

        if (existingTag != null)
        {
            return existingTag;
        }

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync(cancellationToken);

        return tag;
    }

    public async Task<Tag> RenameTagAsync(Guid id, string newName, CancellationToken cancellationToken = default)
    {
        var tag = await GetTagByIdAsync(id, cancellationToken);
        if (tag == null)
        {
            throw new KeyNotFoundException($"Tag with ID {id} not found");
        }

        var normalizedNewName = newName.Trim().ToLowerInvariant();

        // Check if a tag with the new name already exists
        var existingTag = await _context.Tags
            .FirstOrDefaultAsync(t => t.Name.ToLower() == normalizedNewName && t.Id != id, cancellationToken);

        if (existingTag != null)
        {
            throw new InvalidOperationException($"A tag with name '{normalizedNewName}' already exists. Use merge instead.");
        }

        tag.Name = normalizedNewName;
        tag.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return tag;
    }

    public async Task MergeTagsAsync(Guid sourceTagId, Guid targetTagId, CancellationToken cancellationToken = default)
    {
        if (sourceTagId == targetTagId)
        {
            throw new InvalidOperationException("Cannot merge a tag with itself");
        }

        var sourceTag = await GetTagByIdAsync(sourceTagId, cancellationToken);
        if (sourceTag == null)
        {
            throw new KeyNotFoundException($"Source tag with ID {sourceTagId} not found");
        }

        var targetTag = await GetTagByIdAsync(targetTagId, cancellationToken);
        if (targetTag == null)
        {
            throw new KeyNotFoundException($"Target tag with ID {targetTagId} not found");
        }

        // Get all entries with source tag
        var entriesWithSourceTag = await _context.Entries
            .Include(e => e.Tags)
            .Where(e => e.Tags.Any(t => t.Id == sourceTagId))
            .ToListAsync(cancellationToken);

        // Replace source tag with target tag in all entries
        foreach (var entry in entriesWithSourceTag)
        {
            // Remove source tag if present
            var sourceTagInEntry = entry.Tags.FirstOrDefault(t => t.Id == sourceTagId);
            if (sourceTagInEntry != null)
            {
                entry.Tags.Remove(sourceTagInEntry);
            }

            // Add target tag if not already present
            if (!entry.Tags.Any(t => t.Id == targetTagId))
            {
                entry.Tags.Add(targetTag);
            }

            entry.UpdatedAt = DateTime.UtcNow;
        }

        // Delete source tag
        _context.Tags.Remove(sourceTag);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTagAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await GetTagByIdAsync(id, cancellationToken);
        if (tag == null)
        {
            throw new KeyNotFoundException($"Tag with ID {id} not found");
        }

        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Dictionary<string, int>> GetTagStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var totalTags = await _context.Tags.CountAsync(cancellationToken);
        var totalUsages = await _context.Tags
            .Include(t => t.Entries)
            .SumAsync(t => t.Entries.Count, cancellationToken);

        return new Dictionary<string, int>
        {
            { "TotalTags", totalTags },
            { "TotalUsages", totalUsages }
        };
    }
}
