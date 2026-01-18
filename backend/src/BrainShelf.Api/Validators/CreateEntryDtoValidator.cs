using BrainShelf.Api.DTOs;
using BrainShelf.Core.Entities;
using FluentValidation;

namespace BrainShelf.Api.Validators;

/// <summary>
/// Validator for CreateEntryDto
/// Ensures entry creation requests meet business requirements with type-specific validation
/// </summary>
public class CreateEntryDtoValidator : AbstractValidator<CreateEntryDto>
{
    public CreateEntryDtoValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(500)
            .WithMessage("Title cannot exceed 500 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Description cannot exceed 2000 characters")
            .When(x => x.Description is not null);

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Entry type must be Link, Note, Setting, or Instruction");

        // Type-specific validation for Link entries
        When(x => x.Type == EntryType.Link, () =>
        {
            RuleFor(x => x.Url)
                .NotEmpty()
                .WithMessage("URL is required for Link entries")
                .Must(BeAValidUrl)
                .WithMessage("URL must be a valid HTTP or HTTPS URL");

            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Content is required for Link entries");
        });

        // Type-specific validation for Note, Setting, and Instruction entries
        When(x => x.Type == EntryType.Note || x.Type == EntryType.Setting || x.Type == EntryType.Instruction, () =>
        {
            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage($"Content is required for {nameof(EntryType.Note)}, {nameof(EntryType.Setting)}, and {nameof(EntryType.Instruction)} entries");
        });

        RuleFor(x => x.Url)
            .Must(BeAValidUrl)
            .WithMessage("URL must be a valid HTTP or HTTPS URL")
            .When(x => !string.IsNullOrWhiteSpace(x.Url));

        RuleForEach(x => x.Tags)
            .NotEmpty()
            .WithMessage("Tag names cannot be empty")
            .MaximumLength(100)
            .WithMessage("Tag name cannot exceed 100 characters");
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
