using BrainShelf.Core.Entities;
using BrainShelf.Infrastructure.Data;
using BrainShelf.Services;
using BrainShelf.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BrainShelf.Tests.Services;

[TestFixture]
public class SearchServiceTests
{
    private ApplicationDbContext _context;
    private SearchService _service;
    private Project _testProject;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new SearchService(_context);

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
    }

    [Test]
    public async Task SearchEntriesAsync_WithNoEntries_ReturnsEmptyList()
    {
        // Act
        var (entries, totalCount) = await _service.SearchEntriesAsync("test");

        // Assert
        Assert.That(entries, Is.Empty);
        Assert.That(totalCount, Is.EqualTo(0));
    }

    [Test]
    public async Task SearchEntriesAsync_WithProjectIdFilter_ReturnsOnlyMatchingEntries()
    {
        // Arrange
        var project1 = _testProject;
        var project2 = TestDataFactory.CreateProject(name: "Project 2");
        _context.Projects.Add(project2);
        _context.SaveChanges();

        var entries1 = TestDataFactory.CreateEntries(project1.Id, 3);
        var entries2 = TestDataFactory.CreateEntries(project2.Id, 2);
        _context.Entries.AddRange(entries1);
        _context.Entries.AddRange(entries2);
        await _context.SaveChangesAsync();

        // Act
        var (entries, totalCount) = await _service.SearchEntriesAsync("", projectId: project1.Id);

        // Assert
        Assert.That(entries.Count, Is.EqualTo(3));
        Assert.That(totalCount, Is.EqualTo(3));
        Assert.That(entries.All(e => e.ProjectId == project1.Id), Is.True);
    }

    [Test]
    public async Task SearchEntriesAsync_WithTypeFilter_ReturnsOnlyMatchingType()
    {
        // Arrange
        var noteEntries = TestDataFactory.CreateEntries(_testProject.Id, 3, EntryType.Note);
        var linkEntry = TestDataFactory.CreateLinkEntry(_testProject.Id);
        var settingEntry = TestDataFactory.CreateEntry(_testProject.Id, type: EntryType.Setting);

        _context.Entries.AddRange(noteEntries);
        _context.Entries.Add(linkEntry);
        _context.Entries.Add(settingEntry);
        await _context.SaveChangesAsync();

        // Act
        var (entries, totalCount) = await _service.SearchEntriesAsync("", type: EntryType.Note);

        // Assert
        Assert.That(entries.Count, Is.EqualTo(3));
        Assert.That(totalCount, Is.EqualTo(3));
        Assert.That(entries.All(e => e.Type == EntryType.Note), Is.True);
    }

    [Test]
    public async Task SearchEntriesAsync_WithDateRangeFilter_ReturnsEntriesInRange()
    {
        // Arrange
        var entry1 = TestDataFactory.CreateEntry(_testProject.Id, title: "Old Entry");
        entry1.CreatedAt = DateTime.UtcNow.AddDays(-10);

        var entry2 = TestDataFactory.CreateEntry(_testProject.Id, title: "Middle Entry");
        entry2.CreatedAt = DateTime.UtcNow.AddDays(-5);

        var entry3 = TestDataFactory.CreateEntry(_testProject.Id, title: "Recent Entry");
        entry3.CreatedAt = DateTime.UtcNow.AddDays(-1);

        _context.Entries.AddRange(entry1, entry2, entry3);
        await _context.SaveChangesAsync();

        // Act
        var fromDate = DateTime.UtcNow.AddDays(-6);
        var toDate = DateTime.UtcNow.AddDays(-2);
        var (entries, totalCount) = await _service.SearchEntriesAsync("", fromDate: fromDate, toDate: toDate);

        // Assert
        Assert.That(entries.Count, Is.EqualTo(1));
        Assert.That(totalCount, Is.EqualTo(1));
        Assert.That(entries[0].Title, Is.EqualTo("Middle Entry"));
    }

    [Test]
    public async Task SearchEntriesAsync_WithFromDateOnly_ReturnsEntriesAfterDate()
    {
        // Arrange
        var entry1 = TestDataFactory.CreateEntry(_testProject.Id, title: "Old Entry");
        entry1.CreatedAt = DateTime.UtcNow.AddDays(-10);

        var entry2 = TestDataFactory.CreateEntry(_testProject.Id, title: "Recent Entry");
        entry2.CreatedAt = DateTime.UtcNow.AddDays(-2);

        _context.Entries.AddRange(entry1, entry2);
        await _context.SaveChangesAsync();

        // Act
        var fromDate = DateTime.UtcNow.AddDays(-5);
        var (entries, totalCount) = await _service.SearchEntriesAsync("", fromDate: fromDate);

        // Assert
        Assert.That(entries.Count, Is.EqualTo(1));
        Assert.That(totalCount, Is.EqualTo(1));
        Assert.That(entries[0].Title, Is.EqualTo("Recent Entry"));
    }

    [Test]
    public async Task SearchEntriesAsync_WithToDateOnly_ReturnsEntriesBeforeDate()
    {
        // Arrange
        var entry1 = TestDataFactory.CreateEntry(_testProject.Id, title: "Old Entry");
        entry1.CreatedAt = DateTime.UtcNow.AddDays(-10);

        var entry2 = TestDataFactory.CreateEntry(_testProject.Id, title: "Recent Entry");
        entry2.CreatedAt = DateTime.UtcNow.AddDays(-2);

        _context.Entries.AddRange(entry1, entry2);
        await _context.SaveChangesAsync();

        // Act
        var toDate = DateTime.UtcNow.AddDays(-5);
        var (entries, totalCount) = await _service.SearchEntriesAsync("", toDate: toDate);

        // Assert
        Assert.That(entries.Count, Is.EqualTo(1));
        Assert.That(totalCount, Is.EqualTo(1));
        Assert.That(entries[0].Title, Is.EqualTo("Old Entry"));
    }

    [Test]
    public async Task SearchEntriesAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var entries = TestDataFactory.CreateEntries(_testProject.Id, 25);
        _context.Entries.AddRange(entries);
        await _context.SaveChangesAsync();

        // Act
        var (firstPage, totalCount) = await _service.SearchEntriesAsync("", pageNumber: 1, pageSize: 10);
        var (secondPage, _) = await _service.SearchEntriesAsync("", pageNumber: 2, pageSize: 10);

        // Assert
        Assert.That(firstPage.Count, Is.EqualTo(10));
        Assert.That(secondPage.Count, Is.EqualTo(10));
        Assert.That(totalCount, Is.EqualTo(25));
        Assert.That(firstPage.Select(e => e.Id), Is.Not.EquivalentTo(secondPage.Select(e => e.Id)));
    }

    [Test]
    public async Task SearchEntriesAsync_OrdersByUpdatedAtDescending()
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
        var (entries, _) = await _service.SearchEntriesAsync("");

        // Assert
        Assert.That(entries[0].Title, Is.EqualTo("Third"));
        Assert.That(entries[1].Title, Is.EqualTo("Second"));
        Assert.That(entries[2].Title, Is.EqualTo("First"));
    }

    [Test]
    public async Task SearchEntriesAsync_WithMultipleFilters_AppliesAllFilters()
    {
        // Arrange
        var project2 = TestDataFactory.CreateProject(name: "Project 2");
        _context.Projects.Add(project2);
        _context.SaveChanges();

        // Entry that matches all criteria
        var matchingEntry = TestDataFactory.CreateEntry(_testProject.Id, title: "Match", type: EntryType.Note);
        matchingEntry.CreatedAt = DateTime.UtcNow.AddDays(-3);

        // Entries that don't match
        var wrongProject = TestDataFactory.CreateEntry(project2.Id, title: "Wrong Project", type: EntryType.Note);
        wrongProject.CreatedAt = DateTime.UtcNow.AddDays(-3);

        var wrongType = TestDataFactory.CreateEntry(_testProject.Id, title: "Wrong Type", type: EntryType.Link);
        wrongType.CreatedAt = DateTime.UtcNow.AddDays(-3);

        var wrongDate = TestDataFactory.CreateEntry(_testProject.Id, title: "Wrong Date", type: EntryType.Note);
        wrongDate.CreatedAt = DateTime.UtcNow.AddDays(-10);

        _context.Entries.AddRange(matchingEntry, wrongProject, wrongType, wrongDate);
        await _context.SaveChangesAsync();

        // Act
        var (entries, totalCount) = await _service.SearchEntriesAsync(
            "",
            projectId: _testProject.Id,
            type: EntryType.Note,
            fromDate: DateTime.UtcNow.AddDays(-5),
            toDate: DateTime.UtcNow);

        // Assert
        Assert.That(entries.Count, Is.EqualTo(1));
        Assert.That(totalCount, Is.EqualTo(1));
        Assert.That(entries[0].Title, Is.EqualTo("Match"));
    }

    [Test]
    public async Task SearchEntriesAsync_IncludesNavigationProperties()
    {
        // Arrange
        var (entry, metadata) = TestDataFactory.CreateLinkEntryWithMetadata(_testProject.Id);
        var (taggedEntry, tags) = TestDataFactory.CreateEntryWithTags(_testProject.Id, "tag1", "tag2");

        _context.Entries.Add(entry);
        _context.Entries.Add(taggedEntry);
        _context.Tags.AddRange(tags);
        _context.Metadata.Add(metadata);
        await _context.SaveChangesAsync();

        // Act
        var (entries, _) = await _service.SearchEntriesAsync("");

        // Assert
        var entryWithMetadata = entries.FirstOrDefault(e => e.Id == entry.Id);
        var entryWithTags = entries.FirstOrDefault(e => e.Id == taggedEntry.Id);

        Assert.That(entryWithMetadata?.Metadata, Is.Not.Null);
        Assert.That(entryWithTags?.Tags, Is.Not.Null);
        Assert.That(entryWithTags?.Tags.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task SearchEntriesAsync_WithEmptyQuery_ReturnsAllEntries()
    {
        // Arrange
        var entries = TestDataFactory.CreateEntries(_testProject.Id, 5);
        _context.Entries.AddRange(entries);
        await _context.SaveChangesAsync();

        // Act
        var (results, totalCount) = await _service.SearchEntriesAsync("");

        // Assert
        Assert.That(results.Count, Is.EqualTo(5));
        Assert.That(totalCount, Is.EqualTo(5));
    }

    [Test]
    public async Task SearchEntriesAsync_WithNullQuery_ReturnsAllEntries()
    {
        // Arrange
        var entries = TestDataFactory.CreateEntries(_testProject.Id, 3);
        _context.Entries.AddRange(entries);
        await _context.SaveChangesAsync();

        // Act
        var (results, totalCount) = await _service.SearchEntriesAsync(null!);

        // Assert
        Assert.That(results.Count, Is.EqualTo(3));
        Assert.That(totalCount, Is.EqualTo(3));
    }

    [Test]
    public async Task SearchEntriesAsync_NoTracking_DoesNotTrackEntities()
    {
        // Arrange
        var entry = TestDataFactory.CreateEntry(_testProject.Id);
        _context.Entries.Add(entry);
        await _context.SaveChangesAsync();

        // Act
        var (entries, _) = await _service.SearchEntriesAsync("");

        // Assert
        var entityEntry = _context.Entry(entries[0]);
        Assert.That(entityEntry.State, Is.EqualTo(EntityState.Detached));
    }
}
