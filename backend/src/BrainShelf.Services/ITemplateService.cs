using BrainShelf.Core.Entities;

namespace BrainShelf.Services;

/// <summary>
/// Service interface for template operations
/// </summary>
public interface ITemplateService
{
    Task<IEnumerable<Template>> GetAllAsync(Guid? projectId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Template>> GetDefaultTemplatesAsync(CancellationToken cancellationToken = default);
    Task<Template?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Template> CreateAsync(Template template, CancellationToken cancellationToken = default);
    Task<Template> UpdateAsync(Guid id, Template template, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
