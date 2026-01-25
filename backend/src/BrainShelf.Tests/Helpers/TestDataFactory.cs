using BrainShelf.Core.Entities;

namespace BrainShelf.Tests.Helpers;

/// <summary>
/// Factory class for creating test data entities
/// Provides consistent test data generation across all test classes
/// </summary>
public static class TestDataFactory
{
    /// <summary>
    /// Creates a test project with default or custom values
    /// </summary>
    public static Project CreateProject(
        string name = "Test Project",
        string description = "Test Description",
        Guid? id = null)
    {
        return new Project
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates multiple test projects
    /// </summary>
    public static List<Project> CreateProjects(int count)
    {
        var projects = new List<Project>();
        for (int i = 1; i <= count; i++)
        {
            projects.Add(CreateProject(
                name: $"Project {i}",
                description: $"Description {i}"
            ));
        }
        return projects;
    }

    /// <summary>
    /// Creates a test entry with default or custom values
    /// </summary>
    public static Entry CreateEntry(
        Guid? projectId = null,
        string title = "Test Entry",
        string? description = "Test Entry Description",
        EntryType type = EntryType.Note,
        string? content = "Test Content",
        string? url = null,
        Guid? id = null)
    {
        return new Entry
        {
            Id = id ?? Guid.NewGuid(),
            ProjectId = projectId ?? Guid.NewGuid(),
            Title = title,
            Description = description,
            Type = type,
            Content = content,
            Url = url,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test link entry with URL
    /// </summary>
    public static Entry CreateLinkEntry(
        Guid? projectId = null,
        string title = "Test Link",
        string url = "https://example.com",
        Guid? id = null)
    {
        return CreateEntry(
            projectId: projectId,
            title: title,
            description: "Link Description",
            type: EntryType.Link,
            content: "Link Content",
            url: url,
            id: id
        );
    }

    /// <summary>
    /// Creates multiple test entries
    /// </summary>
    public static List<Entry> CreateEntries(
        Guid projectId,
        int count,
        EntryType type = EntryType.Note)
    {
        var entries = new List<Entry>();
        for (int i = 1; i <= count; i++)
        {
            entries.Add(CreateEntry(
                projectId: projectId,
                title: $"Entry {i}",
                description: $"Description {i}",
                type: type,
                content: $"Content {i}"
            ));
        }
        return entries;
    }

    /// <summary>
    /// Creates a test tag with default or custom values
    /// </summary>
    public static Tag CreateTag(
        string name = "TestTag",
        Guid? id = null)
    {
        return new Tag
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates multiple test tags
    /// </summary>
    public static List<Tag> CreateTags(params string[] names)
    {
        return names.Select(name => CreateTag(name)).ToList();
    }

    /// <summary>
    /// Creates test metadata with default or custom values
    /// </summary>
    public static Metadata CreateMetadata(
        Guid? entryId = null,
        string? title = "Metadata Title",
        string? description = "Metadata Description",
        string? keywords = "keyword1,keyword2",
        string? imageUrl = "https://example.com/image.jpg",
        string? faviconUrl = "https://example.com/favicon.ico",
        string? author = "Test Author",
        string? siteName = "Test Site",
        Guid? id = null)
    {
        return new Metadata
        {
            Id = id ?? Guid.NewGuid(),
            EntryId = entryId ?? Guid.NewGuid(),
            Title = title,
            Description = description,
            Keywords = keywords,
            ImageUrl = imageUrl,
            FaviconUrl = faviconUrl,
            Author = author,
            SiteName = siteName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a project with associated entries
    /// </summary>
    public static (Project Project, List<Entry> Entries) CreateProjectWithEntries(
        int entryCount = 3,
        string projectName = "Test Project",
        EntryType entryType = EntryType.Note)
    {
        var project = CreateProject(name: projectName);
        var entries = CreateEntries(project.Id, entryCount, entryType);
        return (project, entries);
    }

    /// <summary>
    /// Creates an entry with associated tags
    /// </summary>
    public static (Entry Entry, List<Tag> Tags) CreateEntryWithTags(
        Guid? projectId = null,
        params string[] tagNames)
    {
        var entry = CreateEntry(projectId: projectId);
        var tags = CreateTags(tagNames);
        entry.Tags = tags;
        return (entry, tags);
    }

    /// <summary>
    /// Creates a link entry with metadata
    /// </summary>
    public static (Entry Entry, Metadata Metadata) CreateLinkEntryWithMetadata(
        Guid? projectId = null,
        string url = "https://example.com")
    {
        var entry = CreateLinkEntry(projectId: projectId, url: url);
        var metadata = CreateMetadata(entryId: entry.Id);
        entry.Metadata = metadata;
        return (entry, metadata);
    }
}
