using BrainShelf.Core.Entities;
using BrainShelf.Infrastructure.Data;
using BrainShelf.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BrainShelf.Services;

/// <summary>
/// Service implementation for managing projects
/// Handles business logic and data access operations for projects
/// </summary>
public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _context;

    public ProjectService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Project> Projects, int TotalCount)> GetAllAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Ensure valid pagination parameters
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, Math.Min(100, pageSize));

        var query = _context.Projects.AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        var projects = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (projects, totalCount);
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default)
    {
        project.Id = Guid.NewGuid();
        project.CreatedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        return project;
    }

    public async Task<Project?> UpdateAsync(Guid id, Project project, CancellationToken cancellationToken = default)
    {
        var existingProject = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (existingProject is null)
        {
            return null;
        }

        existingProject.Name = project.Name;
        existingProject.Description = project.Description;
        existingProject.Color = project.Color;
        existingProject.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return existingProject;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project is null)
        {
            return false;
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
