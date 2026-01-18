using BrainShelf.Api.DTOs;
using FluentValidation;

namespace BrainShelf.Api.Validators;

/// <summary>
/// Validator for UpdateProjectDto
/// Ensures project update requests meet business requirements
/// </summary>
public class UpdateProjectDtoValidator : AbstractValidator<UpdateProjectDto>
{
    public UpdateProjectDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Project name is required")
            .MaximumLength(200)
            .WithMessage("Project name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters")
            .When(x => x.Description is not null);

        RuleFor(x => x.Color)
            .NotEmpty()
            .WithMessage("Color is required")
            .Matches("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
            .WithMessage("Color must be a valid hex color code (e.g., #3B82F6)");
    }
}
