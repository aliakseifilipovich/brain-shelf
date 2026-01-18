using BrainShelf.Api.DTOs;
using BrainShelf.Api.Validators;
using NUnit.Framework;

namespace BrainShelf.Tests.Validators;

[TestFixture]
public class CreateProjectDtoValidatorTests
{
    private CreateProjectDtoValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateProjectDtoValidator();
    }

    [Test]
    public async Task ShouldHaveError_WhenNameIsEmpty()
    {
        var dto = new CreateProjectDto { Name = "", Color = "#3B82F6" };

        var result = await _validator.ValidateAsync(dto);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Name"), Is.True);
    }

    [Test]
    public async Task ShouldHaveError_WhenNameExceedsMaxLength()
    {
        var dto = new CreateProjectDto
        {
            Name = new string('a', 201),
            Color = "#3B82F6"
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Name"), Is.True);
    }

    [Test]
    public async Task ShouldHaveError_WhenDescriptionExceedsMaxLength()
    {
        var dto = new CreateProjectDto
        {
            Name = "Test Project",
            Description = new string('a', 1001),
            Color = "#3B82F6"
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Description"), Is.True);
    }

    [Test]
    public async Task ShouldNotHaveError_WhenDescriptionIsNull()
    {
        var dto = new CreateProjectDto
        {
            Name = "Test Project",
            Description = null,
            Color = "#3B82F6"
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.That(result.Errors.Any(e => e.PropertyName == "Description"), Is.False);
    }

    [Test]
    public async Task ShouldHaveError_WhenColorIsEmpty()
    {
        var dto = new CreateProjectDto
        {
            Name = "Test Project",
            Color = ""
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Color"), Is.True);
    }

    [Test]
    [TestCase("#FF5733")]
    [TestCase("#fff")]
    [TestCase("#ABC")]
    [TestCase("#123456")]
    public async Task ShouldNotHaveError_WhenColorIsValidHexCode(string color)
    {
        var dto = new CreateProjectDto
        {
            Name = "Test Project",
            Color = color
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.That(result.Errors.Any(e => e.PropertyName == "Color"), Is.False);
    }

    [Test]
    [TestCase("FF5733")]
    [TestCase("#GGG")]
    [TestCase("red")]
    [TestCase("#12345")]
    [TestCase("#1234567")]
    public async Task ShouldHaveError_WhenColorIsNotValidHexCode(string color)
    {
        var dto = new CreateProjectDto
        {
            Name = "Test Project",
            Color = color
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Color"), Is.True);
    }

    [Test]
    public async Task ShouldNotHaveAnyErrors_WhenAllFieldsAreValid()
    {
        var dto = new CreateProjectDto
        {
            Name = "Test Project",
            Description = "Test Description",
            Color = "#3B82F6"
        };

        var result = await _validator.ValidateAsync(dto);

        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors.Count, Is.EqualTo(0));
    }
}
