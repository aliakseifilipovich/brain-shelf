using BrainShelf.Core.Entities;
using BrainShelf.Infrastructure.Data;
using BrainShelf.Services;
using Microsoft.EntityFrameworkCore;

namespace BrainShelf.Tests.Services;

[TestFixture]
public class EntryServiceTests
{
    private ApplicationDbContext _context = null!;
    private EntryService _service = null!;
    private Project _testProject = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new EntryService(_context);

        // Create test project
        _testProject = new Project
        {
            Name = "Test Project",
            Description = "Test Description"
        };
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
    public async Task GetAllAsync_WithNoFilters_ReturnsAllEntries()
    {
        // Arrange
        var entry1 = new Entry
        {
            ProjectId = _testProject.Id,
            Title = "Entry 1",
            Type = EntryType.Note,
            Content = "Content 1"
        };
        var entry2 = new Entry
        {
            ProjectId = _testProject.Id,
            Title = "Entry 2",
            Type = EntryType.Link,
            Content = "Content 2",
            Url = "https://example.com"
        };
        _context.Entries.AddRange(entry1, entry2);
        await _context.SaveChangesAsync();

        // Act
        var (entries, totalCount) = await _service.GetAllAsync(null, null, null, 1, 20);

        // Assert
        Assert.That(entries, Has.Count.EqualTo(2));
        Assert.That(totalCount, Is.EqualTo(2));
    }

    [Test]
    public async Task GetAllAsync_WithProjectIdFilter_ReturnsFilteredEntries()
    {
        // Arrange
        var project2 = new Project { Name = "Project 2" };
        _context.Projects.Add(project2);
        await _context.SaveChangesAsync();

        var entry1 = new Entry { ProjectId = _testProject.Id, Title = "Entry 1", Type = EntryType.Note, Content = "Content" };
        var entry2 = new Entry { ProjectId = project2.Id, Title = "Entry 2", Type = EntryType.Note, Content = "Content" };
        _context.Entries.AddRange(entry1, entry2);
        await _context.SaveChangesAsync();

        // Act
        var (entries, totalCount) = await _service.GetAllAsync(_testProject.Id, null, null, 1, 20);

        // Assert
        Assert.That(entries, Has.Count.EqualTo(1));
        Assert.That(entries.First().ProjectId, Is.EqualTo(_testProject.Id));
        Assert.That(totalCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAllAsync_WithTypeFilter_ReturnsFilteredEntries()
    {
        // Arrange
        var entry1 = new Entry { ProjectId = _testProject.Id, Title = "Note", Type = EntryType.Note, Content = "Content" };
        var entry2 = new Entry { ProjectId = _testProject.Id, Title = "Link", Type = EntryType.Link, Content = "Content", Url = "https://example.com" };
        _context.Entries.AddRange(entry1, entry2);
        await _context.SaveChangesAsync();

        // Act
        var (entries, totalCount) = await _service.GetAllAsync(null, EntryType.Note, null, 1, 20);

        // Assert
        Assert.That(entries, Has.Count.EqualTo(1));
        Assert.That(entries.First().Type, Is.EqualTo(EntryType.Note));
        Assert.That(totalCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAllAsync_WithTagsFilter_ReturnsEntriesWithAnyTag()
    {
        // Arrange
        var tag1 = new Tag { Name = "csharp" };
        var tag2 = new Tag { Name = "dotnet" };
        var tag3 = new Tag { Name = "typescript" };
        _context.Tags.AddRange(tag1, tag2, tag3);

        var entry1 = new Entry { ProjectId = _testProject.Id, Title = "Entry 1", Type = EntryType.Note, Content = "Content" };
        entry1.Tags.Add(tag1);
        entry1.Tags.Add(tag2);

        var entry2 = new Entry { ProjectId = _testProject.Id, Title = "Entry 2", Type = EntryType.Note, Content = "Content" };
        entry2.Tags.Add(tag3);

        var entry3 = new Entry { ProjectId = _testProject.Id, Title = "Entry 3", Type = EntryType.Note, Content = "Content" };

        _context.Entries.AddRange(entry1, entry2, entry3);
        await _context.SaveChangesAsync();

        // Act - Should return entries with csharp OR dotnet (entry1 has both)
        var (entries, totalCount) = await _service.GetAllAsync(null, null, new List<string> { "csharp", "dotnet" }, 1, 20);

        // Assert
        Assert.That(totalCount, Is.EqualTo(1));
        Assert.That(entries.First().Id, Is.EqualTo(entry1.Id));
    }

    [Test]
    public async Task GetAllAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        for (var i = 1; i <= 25; i++)
        {
            _context.Entries.Add(new Entry
            {
                ProjectId = _testProject.Id,
                Title = $"Entry {i}",
                Type = EntryType.Note,
                Content = "Content"
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var (entries, totalCount) = await _service.GetAllAsync(null, null, null, 2, 10);

        // Assert
        Assert.That(entries, Has.Count.EqualTo(10));
        Assert.That(totalCount, Is.EqualTo(25));
    }

    [Test]
    public async Task GetAllAsync_WithPageSizeOver100_ClampsTo100()
    {
        // Arrange
        for (var i = 1; i <= 150; i++)
        {
            _context.Entries.Add(new Entry
            {
                ProjectId = _testProject.Id,
                Title = $"Entry {i}",
                Type = EntryType.Note,
                Content = "Content"
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var (entries, totalCount) = await _service.GetAllAsync(null, null, null, 1, 200);

        // Assert
        Assert.That(entries, Has.Count.EqualTo(100));
        Assert.That(totalCount, Is.EqualTo(150));
    }

    [Test]
    public async Task GetByProjectIdAsync_ReturnsEntriesForProject()
    {
        // Arrange
        var project2 = new Project { Name = "Project 2" };
        _context.Projects.Add(project2);
        await _context.SaveChangesAsync();

        var entry1 = new Entry { ProjectId = _testProject.Id, Title = "Entry 1", Type = EntryType.Note, Content = "Content" };
        var entry2 = new Entry { ProjectId = project2.Id, Title = "Entry 2", Type = EntryType.Note, Content = "Content" };
        _context.Entries.AddRange(entry1, entry2);
        await _context.SaveChangesAsync();

        // Act
        var (entries, totalCount) = await _service.GetByProjectIdAsync(_testProject.Id, 1, 20);

        // Assert
        Assert.That(entries, Has.Count.EqualTo(1));
        Assert.That(entries.First().ProjectId, Is.EqualTo(_testProject.Id));
        Assert.That(totalCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetByIdAsync_WithValidId_ReturnsEntry()
    {
        // Arrange
        var entry = new Entry
        {
            ProjectId = _testProject.Id,
            Title = "Test Entry",
            Type = EntryType.Note,
            Content = "Test Content"
        };
        _context.Entries.Add(entry);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(entry.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(entry.Id));
        Assert.That(result.Title, Is.EqualTo("Test Entry"));
    }

    [Test]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _service.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateAsync_WithoutTags_CreatesEntry()
    {
        // Arrange
        var entry = new Entry
        {
            ProjectId = _testProject.Id,
            Title = "New Entry",
            Description = "Description",
            Type = EntryType.Note,
            Content = "Content"
        };

        // Act
        var result = await _service.CreateAsync(entry, null);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("New Entry"));
        Assert.That(result.Tags, Is.Empty);

        var dbEntry = await _context.Entries.FindAsync(result.Id);
        Assert.That(dbEntry, Is.Not.Null);
    }

    [Test]
    public async Task CreateAsync_WithNewTags_CreatesTagsAndEntry()
    {
        // Arrange
        var entry = new Entry
        {
            ProjectId = _testProject.Id,
            Title = "New Entry",
            Type = EntryType.Note,
            Content = "Content"
        };
        var tags = new List<string> { "CSharp", "DotNet" };

        // Act
        var result = await _service.CreateAsync(entry, tags);

        // Assert
        Assert.That(result.Tags, Has.Count.EqualTo(2));
        Assert.That(result.Tags.Select(t => t.Name), Is.EquivalentTo(new[] { "csharp", "dotnet" }));

        var dbTags = await _context.Tags.ToListAsync();
        Assert.That(dbTags, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task CreateAsync_WithExistingTags_ReusesExistingTags()
    {
        // Arrange
        var existingTag = new Tag { Name = "csharp" };
        _context.Tags.Add(existingTag);
        await _context.SaveChangesAsync();

        var entry = new Entry
        {
            ProjectId = _testProject.Id,
            Title = "New Entry",
            Type = EntryType.Note,
            Content = "Content"
        };
        var tags = new List<string> { "CSharp", "NewTag" };

        // Act
        var result = await _service.CreateAsync(entry, tags);

        // Assert
        Assert.That(result.Tags, Has.Count.EqualTo(2));
        Assert.That(result.Tags.Any(t => t.Id == existingTag.Id), Is.True);

        var dbTags = await _context.Tags.ToListAsync();
        Assert.That(dbTags, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task CreateAsync_WithDuplicateTagNames_RemovesDuplicates()
    {
        // Arrange
        var entry = new Entry
        {
            ProjectId = _testProject.Id,
            Title = "New Entry",
            Type = EntryType.Note,
            Content = "Content"
        };
        var tags = new List<string> { "CSharp", "csharp", "CSHARP" };

        // Act
        var result = await _service.CreateAsync(entry, tags);

        // Assert
        Assert.That(result.Tags, Has.Count.EqualTo(1));
        Assert.That(result.Tags.First().Name, Is.EqualTo("csharp"));
    }

    [Test]
    public async Task UpdateAsync_WithValidId_UpdatesEntry()
    {
        // Arrange
        var entry = new Entry
        {
            ProjectId = _testProject.Id,
            Title = "Original Title",
            Type = EntryType.Note,
            Content = "Original Content"
        };
        _context.Entries.Add(entry);
        await _context.SaveChangesAsync();

        var updates = new Entry
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Type = EntryType.Instruction,
            Content = "Updated Content"
        };

        // Act
        var result = await _service.UpdateAsync(entry.Id, updates, null);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("Updated Title"));
        Assert.That(result.Description, Is.EqualTo("Updated Description"));
        Assert.That(result.Type, Is.EqualTo(EntryType.Instruction));
        Assert.That(result.Content, Is.EqualTo("Updated Content"));
    }

    [Test]
    public async Task UpdateAsync_WithTags_UpdatesTags()
    {
        // Arrange
        var tag1 = new Tag { Name = "oldtag" };
        _context.Tags.Add(tag1);
        await _context.SaveChangesAsync();

        var entry = new Entry
        {
            ProjectId = _testProject.Id,
            Title = "Entry",
            Type = EntryType.Note,
            Content = "Content"
        };
        entry.Tags.Add(tag1);
        _context.Entries.Add(entry);
        await _context.SaveChangesAsync();

        var updates = new Entry
        {
            Title = "Updated Entry",
            Type = EntryType.Note,
            Content = "Content"
        };
        var newTags = new List<string> { "newtag" };

        // Act
        var result = await _service.UpdateAsync(entry.Id, updates, newTags);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Tags, Has.Count.EqualTo(1));
        Assert.That(result.Tags.First().Name, Is.EqualTo("newtag"));
    }

    [Test]
    public async Task UpdateAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var updates = new Entry
        {
            Title = "Updated Title",
            Type = EntryType.Note,
            Content = "Content"
        };

        // Act
        var result = await _service.UpdateAsync(Guid.NewGuid(), updates, null);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_WithValidId_DeletesEntry()
    {
        // Arrange
        var entry = new Entry
        {
            ProjectId = _testProject.Id,
            Title = "Entry to Delete",
            Type = EntryType.Note,
            Content = "Content"
        };
        _context.Entries.Add(entry);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAsync(entry.Id);

        // Assert
        Assert.That(result, Is.True);

        var dbEntry = await _context.Entries.FindAsync(entry.Id);
        Assert.That(dbEntry, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_WithInvalidId_ReturnsFalse()
    {
        // Act
        var result = await _service.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GetByIdAsync_IncludesNavigationProperties()
    {
        // Arrange
        var tag = new Tag { Name = "testtag" };
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        var entry = new Entry
        {
            ProjectId = _testProject.Id,
            Title = "Entry",
            Type = EntryType.Note,
            Content = "Content"
        };
        entry.Tags.Add(tag);
        _context.Entries.Add(entry);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(entry.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Project, Is.Not.Null);
        Assert.That(result.Project.Name, Is.EqualTo("Test Project"));
        Assert.That(result.Tags, Has.Count.EqualTo(1));
        Assert.That(result.Tags.First().Name, Is.EqualTo("testtag"));
    }
}
