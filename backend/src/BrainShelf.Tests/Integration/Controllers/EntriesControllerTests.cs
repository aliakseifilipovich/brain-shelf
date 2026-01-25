using System.Net;
using System.Net.Http.Json;
using BrainShelf.Api.DTOs;
using BrainShelf.Core.Entities;
using BrainShelf.Infrastructure.Data;
using BrainShelf.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace BrainShelf.Tests.Integration.Controllers;

[TestFixture]
public class EntriesControllerTests
{
    private CustomWebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private ApplicationDbContext _context;
    private Project _testProject;

    [SetUp]
    public void Setup()
    {
        _factory = new CustomWebApplicationFactory<Program>();
        _client = _factory.CreateClient();

        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Create test project
        _testProject = TestDataFactory.CreateProject();
        _context.Projects.Add(_testProject);
        _context.SaveChanges();
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task GetAll_ReturnsSuccessWithEntries()
    {
        // Arrange
        var entries = TestDataFactory.CreateEntries(_testProject.Id, 3);
        _context.Entries.AddRange(entries);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/entries");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<EntryDto>>();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Items.Count(), Is.EqualTo(3));
        Assert.That(result.TotalCount, Is.EqualTo(3));
    }

    [Test]
    public async Task GetAll_WithProjectIdFilter_ReturnsFilteredEntries()
    {
        // Arrange
        var project2 = TestDataFactory.CreateProject(name: "Project 2");
        _context.Projects.Add(project2);
        _context.SaveChanges();

        var entries1 = TestDataFactory.CreateEntries(_testProject.Id, 2);
        var entries2 = TestDataFactory.CreateEntries(project2.Id, 3);
        _context.Entries.AddRange(entries1);
        _context.Entries.AddRange(entries2);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/entries?projectId={_testProject.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<EntryDto>>();
        
        Assert.That(result!.Items.Count(), Is.EqualTo(2));
        Assert.That(result.TotalCount, Is.EqualTo(2));
    }

    [Test]
    public async Task GetAll_WithTypeFilter_ReturnsFilteredEntries()
    {
        // Arrange
        var noteEntries = TestDataFactory.CreateEntries(_testProject.Id, 2, EntryType.Note);
        var linkEntry = TestDataFactory.CreateLinkEntry(_testProject.Id);
        _context.Entries.AddRange(noteEntries);
        _context.Entries.Add(linkEntry);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/entries?type=0"); // Note = 0

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<EntryDto>>();
        
        Assert.That(result!.Items.Count(), Is.EqualTo(2));
        Assert.That(result.Items.All(e => e.Type == EntryType.Note), Is.True);
    }

    [Test]
    public async Task GetById_ExistingEntry_ReturnsEntry()
    {
        // Arrange
        var entry = TestDataFactory.CreateEntry(_testProject.Id, title: "Test Entry");
        _context.Entries.Add(entry);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/entries/{entry.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<EntryDto>();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(entry.Id));
        Assert.That(result.Title, Is.EqualTo("Test Entry"));
    }

    [Test]
    public async Task GetById_NonExistingEntry_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/entries/{Guid.NewGuid()}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Create_ValidNoteEntry_ReturnsCreatedEntry()
    {
        // Arrange
        var createDto = new CreateEntryDto
        {
            ProjectId = _testProject.Id,
            Title = "New Note",
            Description = "Description",
            Type = EntryType.Note,
            Content = "Note content",
            Tags = new List<string> { "tag1", "tag2" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/entries", createDto);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        
        var result = await response.Content.ReadFromJsonAsync<EntryDto>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Title, Is.EqualTo("New Note"));
        Assert.That(result.Content, Is.EqualTo("Note content"));
        Assert.That(result.Tags.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task Create_ValidLinkEntry_ReturnsCreatedEntry()
    {
        // Arrange
        var createDto = new CreateEntryDto
        {
            ProjectId = _testProject.Id,
            Title = "New Link",
            Type = EntryType.Link,
            Content = "Link content",
            Url = "https://example.com",
            Tags = new List<string>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/entries", createDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<EntryDto>();
        
        Assert.That(result!.Type, Is.EqualTo(EntryType.Link));
        Assert.That(result.Url, Is.EqualTo("https://example.com"));
    }

    [Test]
    public async Task Create_InvalidProjectId_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateEntryDto
        {
            ProjectId = Guid.NewGuid(), // Non-existent project
            Title = "Test",
            Type = EntryType.Note,
            Content = "Content",
            Tags = new List<string>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/entries", createDto);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Update_ExistingEntry_ReturnsUpdatedEntry()
    {
        // Arrange
        var entry = TestDataFactory.CreateEntry(_testProject.Id);
        _context.Entries.Add(entry);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateEntryDto
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Content = "Updated Content"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/entries/{entry.Id}", updateDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<EntryDto>();
        
        Assert.That(result!.Title, Is.EqualTo("Updated Title"));
        Assert.That(result.Description, Is.EqualTo("Updated Description"));
        Assert.That(result.Content, Is.EqualTo("Updated Content"));
    }

    [Test]
    public async Task Update_NonExistingEntry_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateEntryDto { Title = "Updated" };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/entries/{Guid.NewGuid()}", updateDto);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_ExistingEntry_ReturnsNoContent()
    {
        // Arrange
        var entry = TestDataFactory.CreateEntry(_testProject.Id);
        _context.Entries.Add(entry);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/entries/{entry.Id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task Delete_NonExistingEntry_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/entries/{Guid.NewGuid()}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Create_WithTags_AutoCreatesNewTags()
    {
        // Arrange
        var createDto = new CreateEntryDto
        {
            ProjectId = _testProject.Id,
            Title = "Entry with new tags",
            Type = EntryType.Note,
            Content = "Content",
            Tags = new List<string> { "newtag1", "newtag2" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/entries", createDto);
        var result = await response.Content.ReadFromJsonAsync<EntryDto>();

        // Assert
        Assert.That(result!.Tags.Count, Is.EqualTo(2));
        
        // Verify tags were created in database
        var tagsInDb = _context.Tags.Where(t => t.Name == "newtag1" || t.Name == "newtag2").ToList();
        Assert.That(tagsInDb.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetAll_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var entries = TestDataFactory.CreateEntries(_testProject.Id, 15);
        _context.Entries.AddRange(entries);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/entries?pageNumber=2&pageSize=5");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<EntryDto>>();
        
        Assert.That(result!.Items.Count(), Is.EqualTo(5));
        Assert.That(result.PageNumber, Is.EqualTo(2));
        Assert.That(result.TotalCount, Is.EqualTo(15));
    }
}
