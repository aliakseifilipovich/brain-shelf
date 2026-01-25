using BrainShelf.Core.Entities;
using BrainShelf.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BrainShelf.Services;

/// <summary>
/// Service for template operations
/// </summary>
public class TemplateService : ITemplateService
{
    private readonly ApplicationDbContext _context;

    public TemplateService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Template>> GetAllAsync(Guid? projectId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Templates
            .Include(t => t.Project)
            .AsQueryable();

        if (projectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == projectId || t.ProjectId == null);
        }

        return await query
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Template>> GetDefaultTemplatesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Templates
            .Where(t => t.IsDefault)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Template?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Templates
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Template> CreateAsync(Template template, CancellationToken cancellationToken = default)
    {
        template.Id = Guid.NewGuid();
        template.CreatedAt = DateTime.UtcNow;
        template.UpdatedAt = DateTime.UtcNow;

        _context.Templates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);

        return template;
    }

    public async Task<Template> UpdateAsync(Guid id, Template template, CancellationToken cancellationToken = default)
    {
        var existingTemplate = await GetByIdAsync(id, cancellationToken);
        if (existingTemplate == null)
        {
            throw new KeyNotFoundException($"Template with ID {id} not found");
        }

        existingTemplate.Name = template.Name;
        existingTemplate.Description = template.Description;
        existingTemplate.Type = template.Type;
        existingTemplate.Title = template.Title;
        existingTemplate.Content = template.Content;
        existingTemplate.Tags = template.Tags;
        existingTemplate.IsDefault = template.IsDefault;
        existingTemplate.ProjectId = template.ProjectId;
        existingTemplate.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return existingTemplate;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await GetByIdAsync(id, cancellationToken);
        if (template == null)
        {
            throw new KeyNotFoundException($"Template with ID {id} not found");
        }

        _context.Templates.Remove(template);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
