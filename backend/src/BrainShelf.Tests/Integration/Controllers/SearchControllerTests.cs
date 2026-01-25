using System.Net;
using System.Net.Http.Json;
using BrainShelf.Api.DTOs;
using BrainShelf.Core.Entities;
using BrainShelf.Infrastructure.Data;
using BrainShelf.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace BrainShelf.Tests.Integration.Controllers;

[TestFixture]
public class SearchControllerTests
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
    public async Task Search_WithEmptyQuery_ReturnsAllEntries()
    {
        // Arrange
        var entries = TestDataFactory.CreateEntries(_testProject.Id, 3);
        _context.Entries.AddRange(entries);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/search");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<EntryDto>>();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Items.Count(), Is.EqualTo(3));
        Assert.That(result.TotalCount, Is.EqualTo(3));
    }

    [Test]
    public async Task Search_WithProjectIdFilter_ReturnsOnlyMatchingEntries()
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
        var response = await _client.GetAsync($"/api/search?projectId={_testProject.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<EntryDto>>();
        
        Assert.That(result!.Items.Count(), Is.EqualTo(2));
        Assert.That(result.Items.All(e => e.ProjectId == _testProject.Id), Is.True);
    }

    [Test]
    public async Task Search_WithTypeFilter_ReturnsOnlyMatchingType()
    {
        // Arrange
        var noteEntries = TestDataFactory.CreateEntries(_testProject.Id, 2, EntryType.Note);
        var linkEntry = TestDataFactory.CreateLinkEntry(_testProject.Id);
        _context.Entries.AddRange(noteEntries);
        _context.Entries.Add(linkEntry);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/search?type=0"); // Note = 0

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<EntryDto>>();
        
        Assert.That(result!.Items.Count(), Is.EqualTo(2));
        Assert.That(result.Items.All(e => e.Type == EntryType.Note), Is.True);
    }

    [Test]
    public async Task Search_WithDateRange_ReturnsEntriesInRange()
    {
        // Arrange
        var entry1 = TestDataFactory.CreateEntry(_testProject.Id);
        entry1.CreatedAt = DateTime.UtcNow.AddDays(-10);

        var entry2 = TestDataFactory.CreateEntry(_testProject.Id);
        entry2.CreatedAt = DateTime.UtcNow.AddDays(-5);

        var entry3 = TestDataFactory.CreateEntry(_testProject.Id);
        entry3.CreatedAt = DateTime.UtcNow.AddDays(-1);

        _context.Entries.AddRange(entry1, entry2, entry3);
        await _context.SaveChangesAsync();

        // Act
        var fromDate = DateTime.UtcNow.AddDays(-6).ToString("o");
        var toDate = DateTime.UtcNow.AddDays(-2).ToString("o");
        var response = await _client.GetAsync($"/api/search?fromDate={fromDate}&toDate={toDate}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<EntryDto>>();
        
        Assert.That(result!.Items.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task Search_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var entries = TestDataFactory.CreateEntries(_testProject.Id, 25);
        _context.Entries.AddRange(entries);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/search?pageNumber=2&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<EntryDto>>();
        
        Assert.That(result!.Items.Count(), Is.EqualTo(10));
        Assert.That(result.PageNumber, Is.EqualTo(2));
        Assert.That(result.TotalCount, Is.EqualTo(25));
    }

    [Test]
    public async Task Search_WithMultipleFilters_AppliesAllFilters()
    {
        // Arrange
        var project2 = TestDataFactory.CreateProject();
        _context.Projects.Add(project2);
        _context.SaveChanges();

        var matchingEntry = TestDataFactory.CreateEntry(_testProject.Id, type: EntryType.Note);
        matchingEntry.CreatedAt = DateTime.UtcNow.AddDays(-3);

        var wrongProject = TestDataFactory.CreateEntry(project2.Id, type: EntryType.Note);
        wrongProject.CreatedAt = DateTime.UtcNow.AddDays(-3);

        var wrongType = TestDataFactory.CreateEntry(_testProject.Id, type: EntryType.Link);
        wrongType.CreatedAt = DateTime.UtcNow.AddDays(-3);

        _context.Entries.AddRange(matchingEntry, wrongProject, wrongType);
        await _context.SaveChangesAsync();

        // Act
        var fromDate = DateTime.UtcNow.AddDays(-5).ToString("o");
        var response = await _client.GetAsync(
            $"/api/search?projectId={_testProject.Id}&type=1&fromDate={fromDate}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<EntryDto>>();
        
        Assert.That(result!.Items.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task Search_EmptyDatabase_ReturnsEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/search?q=test");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<EntryDto>>();
        
        Assert.That(result!.Items, Is.Empty);
        Assert.That(result.TotalCount, Is.EqualTo(0));
    }

    [Test]
    public async Task Search_IncludesNavigationProperties()
    {
        // Arrange
        var entry = TestDataFactory.CreateLinkEntry(_testProject.Id);
        var metadata = TestDataFactory.CreateMetadata(entryId: entry.Id);
        entry.Metadata = metadata;
        
        var tag = TestDataFactory.CreateTag("testtag");
        entry.Tags.Add(tag);

        _context.Entries.Add(entry);
        _context.Tags.Add(tag);
        _context.Metadata.Add(metadata);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/search");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<EntryDto>>();
        
        var firstEntry = result!.Items.First();
        Assert.That(firstEntry.Metadata, Is.Not.Null);
        Assert.That(firstEntry.Tags.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task Search_OrdersByUpdatedAtDescending()
    {
        // Arrange
        var entry1 = TestDataFactory.CreateEntry(_testProject.Id, title: "First");
        entry1.UpdatedAt = DateTime.UtcNow.AddDays(-2);

        var entry2 = TestDataFactory.CreateEntry(_testProject.Id, title: "Second");
        entry2.UpdatedAt = DateTime.UtcNow.AddDays(-1);

        var entry3 = TestDataFactory.CreateEntry(_testProject.Id, title: "Third");
        entry3.UpdatedAt = DateTime.UtcNow;

        _context.Entries.AddRange(entry1, entry2, entry3);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/search");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<EntryDto>>();
        var items = result!.Items.ToList();
        
        Assert.That(items[0].Title, Is.EqualTo("Third"));
        Assert.That(items[1].Title, Is.EqualTo("Second"));
        Assert.That(items[2].Title, Is.EqualTo("First"));
    }

    [Test]
    public async Task Search_WithInvalidParameters_UsesDefaults()
    {
        // Arrange
        var entries = TestDataFactory.CreateEntries(_testProject.Id, 5);
        _context.Entries.AddRange(entries);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/search?pageNumber=-1&pageSize=0");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<EntryDto>>();
        
        // Should still return results with default/corrected pagination
        Assert.That(result!.Items, Is.Not.Empty);
    }
}
