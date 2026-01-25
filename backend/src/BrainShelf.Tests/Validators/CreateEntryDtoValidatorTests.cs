using BrainShelf.Api.DTOs;
using BrainShelf.Api.Validators;
using BrainShelf.Core.Entities;
using FluentValidation.TestHelper;

namespace BrainShelf.Tests.Validators;

[TestFixture]
public class CreateEntryDtoValidatorTests
{
    private CreateEntryDtoValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateEntryDtoValidator();
    }

    [Test]
    public void Should_HaveError_When_ProjectIdIsEmpty()
    {
        var dto = new CreateEntryDto { ProjectId = Guid.Empty };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ProjectId);
    }

    [Test]
    public void Should_HaveError_When_TitleIsEmpty()
    {
        var dto = new CreateEntryDto { Title = string.Empty };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Test]
    public void Should_HaveError_When_TitleExceeds200Characters()
    {
        var dto = new CreateEntryDto { Title = new string('a', 201) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Test]
    public void Should_NotHaveError_When_DescriptionIsNull()
    {
        var dto = new CreateEntryDto
        {
            ProjectId = Guid.NewGuid(),
            Title = "Valid Title",
            Type = EntryType.Note,
            Content = "Content",
            Description = null
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Test]
    public void Should_HaveError_When_DescriptionExceeds1000Characters()
    {
        var dto = new CreateEntryDto { Description = new string('a', 1001) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Test]
    public void Should_HaveError_When_LinkTypeHasNoUrl()
    {
        var dto = new CreateEntryDto
        {
            ProjectId = Guid.NewGuid(),
            Title = "Link Entry",
            Type = EntryType.Link,
            Content = "Content",
            Url = null
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Url);
    }

    [Test]
    public void Should_HaveError_When_LinkTypeHasInvalidUrl()
    {
        var dto = new CreateEntryDto
        {
            ProjectId = Guid.NewGuid(),
            Title = "Link Entry",
            Type = EntryType.Link,
            Content = "Content",
            Url = "not-a-valid-url"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Url);
    }

    [Test]
    public void Should_HaveError_When_LinkTypeHasNoContent()
    {
        var dto = new CreateEntryDto
        {
            ProjectId = Guid.NewGuid(),
            Title = "Link Entry",
            Type = EntryType.Link,
            Url = "https://example.com",
            Content = null
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Test]
    public void Should_NotHaveError_When_LinkTypeHasValidUrlAndContent()
    {
        var dto = new CreateEntryDto
        {
            ProjectId = Guid.NewGuid(),
            Title = "Link Entry",
            Type = EntryType.Link,
            Content = "Link Description",
            Url = "https://example.com"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Url);
        result.ShouldNotHaveValidationErrorFor(x => x.Content);
    }

    [Test]
    public void Should_HaveError_When_NoteTypeHasNoContent()
    {
        var dto = new CreateEntryDto
        {
            ProjectId = Guid.NewGuid(),
            Title = "Note Entry",
            Type = EntryType.Note,
            Content = null
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Test]
    public void Should_NotHaveError_When_NoteTypeHasContentAndNoUrl()
    {
        var dto = new CreateEntryDto
        {
            ProjectId = Guid.NewGuid(),
            Title = "Note Entry",
            Type = EntryType.Note,
            Content = "Note content"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Content);
    }

    [Test]
    public void Should_HaveError_When_SettingTypeHasNoContent()
    {
        var dto = new CreateEntryDto
        {
            ProjectId = Guid.NewGuid(),
            Title = "Setting Entry",
            Type = EntryType.Setting,
            Content = null
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Test]
    public void Should_NotHaveError_When_SettingTypeHasContent()
    {
        var dto = new CreateEntryDto
        {
            ProjectId = Guid.NewGuid(),
            Title = "Setting Entry",
            Type = EntryType.Setting,
            Content = "Setting value"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Content);
    }

    [Test]
    public void Should_HaveError_When_InstructionTypeHasNoContent()
    {
        var dto = new CreateEntryDto
        {
            ProjectId = Guid.NewGuid(),
            Title = "Instruction Entry",
            Type = EntryType.Instruction,
            Content = null
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Test]
    public void Should_NotHaveError_When_InstructionTypeHasContent()
    {
        var dto = new CreateEntryDto
        {
            ProjectId = Guid.NewGuid(),
            Title = "Instruction Entry",
            Type = EntryType.Instruction,
            Content = "Instruction steps"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Content);
    }

    [Test]
    public void Should_NotHaveError_When_TagsIsNull()
    {
        var dto = new CreateEntryDto
        {
            ProjectId = Guid.NewGuid(),
            Title = "Entry",
            Type = EntryType.Note,
            Content = "Content",
            Tags = null
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Tags);
    }

    [Test]
    public void Should_HaveError_When_TagIsEmpty()
    {
        var dto = new CreateEntryDto
        {
            ProjectId = Guid.NewGuid(),
            Title = "Entry",
            Type = EntryType.Note,
            Content = "Content",
            Tags = new List<string> { "" }
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor("Tags[0]");
    }

    [Test]
    public void Should_HaveError_When_TagExceeds100Characters()
    {
        var dto = new CreateEntryDto
        {
            ProjectId = Guid.NewGuid(),
            Title = "Entry",
            Type = EntryType.Note,
            Content = "Content",
            Tags = new List<string> { new string('a', 101) }
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor("Tags[0]");
    }

    [Test]
    public void Should_NotHaveError_When_TagsAreValid()
    {
        var dto = new CreateEntryDto
        {
            ProjectId = Guid.NewGuid(),
            Title = "Entry",
            Type = EntryType.Note,
            Content = "Content",
            Tags = new List<string> { "csharp", "dotnet", "api" }
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Tags);
    }

    [Test]
    public void Should_AcceptHttpsUrl()
    {
        var dto = new CreateEntryDto
        {
            ProjectId = Guid.NewGuid(),
            Title = "Link",
            Type = EntryType.Link,
            Content = "Content",
            Url = "https://example.com/path?query=value"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Url);
    }

    [Test]
    public void Should_AcceptHttpUrl()
    {
        var dto = new CreateEntryDto
        {
            ProjectId = Guid.NewGuid(),
            Title = "Link",
            Type = EntryType.Link,
            Content = "Content",
            Url = "http://example.com"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Url);
    }

    [Test]
    public void Should_RejectNonHttpUrls()
    {
        var dto = new CreateEntryDto
        {
            ProjectId = Guid.NewGuid(),
            Title = "Link",
            Type = EntryType.Link,
            Content = "Content",
            Url = "ftp://example.com"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Url);
    }
}
