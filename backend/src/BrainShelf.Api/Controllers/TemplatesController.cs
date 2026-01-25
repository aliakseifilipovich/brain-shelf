using BrainShelf.Api.DTOs;
using BrainShelf.Core.Entities;
using BrainShelf.Services;
using Microsoft.AspNetCore.Mvc;

namespace BrainShelf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly ITemplateService _templateService;

    public TemplatesController(ITemplateService templateService)
    {
        _templateService = templateService;
    }

    /// <summary>
    /// Get all templates
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TemplateDto>>> GetAll([FromQuery] Guid? projectId = null)
    {
        var templates = await _templateService.GetAllAsync(projectId);

        var templateDtos = templates.Select(t => new TemplateDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            Type = t.Type,
            Title = t.Title,
            Content = t.Content,
            Tags = t.Tags,
            IsDefault = t.IsDefault,
            ProjectId = t.ProjectId,
            ProjectName = t.Project?.Name,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        });

        return Ok(templateDtos);
    }

    /// <summary>
    /// Get default templates
    /// </summary>
    [HttpGet("default")]
    public async Task<ActionResult<IEnumerable<TemplateDto>>> GetDefaults()
    {
        var templates = await _templateService.GetDefaultTemplatesAsync();

        var templateDtos = templates.Select(t => new TemplateDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            Type = t.Type,
            Title = t.Title,
            Content = t.Content,
            Tags = t.Tags,
            IsDefault = t.IsDefault,
            ProjectId = t.ProjectId,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        });

        return Ok(templateDtos);
    }

    /// <summary>
    /// Get template by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TemplateDto>> GetById(Guid id)
    {
        var template = await _templateService.GetByIdAsync(id);
        if (template == null)
        {
            return NotFound(new { message = $"Template with ID {id} not found" });
        }

        var templateDto = new TemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            Type = template.Type,
            Title = template.Title,
            Content = template.Content,
            Tags = template.Tags,
            IsDefault = template.IsDefault,
            ProjectId = template.ProjectId,
            ProjectName = template.Project?.Name,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };

        return Ok(templateDto);
    }

    /// <summary>
    /// Create a new template
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TemplateDto>> Create([FromBody] CreateTemplateDto createDto)
    {
        var template = new Template
        {
            Name = createDto.Name,
            Description = createDto.Description,
            Type = createDto.Type,
            Title = createDto.Title,
            Content = createDto.Content,
            Tags = createDto.Tags,
            IsDefault = createDto.IsDefault,
            ProjectId = createDto.ProjectId
        };

        var createdTemplate = await _templateService.CreateAsync(template);

        var templateDto = new TemplateDto
        {
            Id = createdTemplate.Id,
            Name = createdTemplate.Name,
            Description = createdTemplate.Description,
            Type = createdTemplate.Type,
            Title = createdTemplate.Title,
            Content = createdTemplate.Content,
            Tags = createdTemplate.Tags,
            IsDefault = createdTemplate.IsDefault,
            ProjectId = createdTemplate.ProjectId,
            CreatedAt = createdTemplate.CreatedAt,
            UpdatedAt = createdTemplate.UpdatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = templateDto.Id }, templateDto);
    }

    /// <summary>
    /// Update a template
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TemplateDto>> Update(Guid id, [FromBody] UpdateTemplateDto updateDto)
    {
        try
        {
            var existingTemplate = await _templateService.GetByIdAsync(id);
            if (existingTemplate == null)
            {
                return NotFound(new { message = $"Template with ID {id} not found" });
            }

            // Update only provided fields
            if (updateDto.Name != null) existingTemplate.Name = updateDto.Name;
            if (updateDto.Description != null) existingTemplate.Description = updateDto.Description;
            if (updateDto.Type != null) existingTemplate.Type = updateDto.Type.Value;
            if (updateDto.Title != null) existingTemplate.Title = updateDto.Title;
            if (updateDto.Content != null) existingTemplate.Content = updateDto.Content;
            if (updateDto.Tags != null) existingTemplate.Tags = updateDto.Tags;
            if (updateDto.IsDefault != null) existingTemplate.IsDefault = updateDto.IsDefault.Value;
            if (updateDto.ProjectId != null) existingTemplate.ProjectId = updateDto.ProjectId;

            var updatedTemplate = await _templateService.UpdateAsync(id, existingTemplate);

            var templateDto = new TemplateDto
            {
                Id = updatedTemplate.Id,
                Name = updatedTemplate.Name,
                Description = updatedTemplate.Description,
                Type = updatedTemplate.Type,
                Title = updatedTemplate.Title,
                Content = updatedTemplate.Content,
                Tags = updatedTemplate.Tags,
                IsDefault = updatedTemplate.IsDefault,
                ProjectId = updatedTemplate.ProjectId,
                ProjectName = updatedTemplate.Project?.Name,
                CreatedAt = updatedTemplate.CreatedAt,
                UpdatedAt = updatedTemplate.UpdatedAt
            };

            return Ok(templateDto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a template
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            await _templateService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
