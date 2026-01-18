using BrainShelf.Core.Entities;
using BrainShelf.Infrastructure.Data;
using BrainShelf.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace BrainShelf.Tests.Services;

[TestFixture]
public class ProjectServiceTests
{
    private ApplicationDbContext _context = null!;
    private ProjectService _projectService = null!;

    [SetUp]
    public void Setup()
    {
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _projectService = new ProjectService(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task CreateAsync_ShouldCreateProject_WithGeneratedIdAndTimestamps()
    {
        var project = new Project
        {
            Name = "Test Project",
            Description = "Test Description",
            Color = "#FF5733"
        };

        var result = await _projectService.CreateAsync(project);

        Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(result.Name, Is.EqualTo("Test Project"));
        Assert.That(result.Description, Is.EqualTo("Test Description"));
        Assert.That(result.Color, Is.EqualTo("#FF5733"));
        Assert.That(result.CreatedAt, Is.Not.EqualTo(default(DateTime)));
        Assert.That(result.UpdatedAt, Is.Not.EqualTo(default(DateTime)));
        Assert.That(result.CreatedAt, Is.EqualTo(result.UpdatedAt));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnProject_WhenProjectExists()
    {
        var project = new Project
        {
            Name = "Test Project",
            Description = "Test Description",
            Color = "#FF5733"
        };
        var created = await _projectService.CreateAsync(project);

        var result = await _projectService.GetByIdAsync(created.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(created.Id));
        Assert.That(result.Name, Is.EqualTo("Test Project"));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnNull_WhenProjectDoesNotExist()
    {
        var result = await _projectService.GetByIdAsync(Guid.NewGuid());

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllProjects_WithCorrectPagination()
    {
        // Create multiple projects
        for (int i = 1; i <= 25; i++)
        {
            await _projectService.CreateAsync(new Project
            {
                Name = $"Project {i}",
                Color = "#3B82F6"
            });
        }

        var (projects, totalCount) = await _projectService.GetAllAsync(1, 20);

        Assert.That(totalCount, Is.EqualTo(25));
        Assert.That(projects.Count(), Is.EqualTo(20));
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnSecondPage_WhenPageNumberIsTwo()
    {
        // Create multiple projects
        for (int i = 1; i <= 25; i++)
        {
            await _projectService.CreateAsync(new Project
            {
                Name = $"Project {i}",
                Color = "#3B82F6"
            });
        }

        var (projects, totalCount) = await _projectService.GetAllAsync(2, 20);

        Assert.That(totalCount, Is.EqualTo(25));
        Assert.That(projects.Count(), Is.EqualTo(5));
    }

    [Test]
    public async Task GetAllAsync_ShouldOrderByCreatedAtDescending()
    {
        var project1 = await _projectService.CreateAsync(new Project
        {
            Name = "First Project",
            Color = "#3B82F6"
        });

        await Task.Delay(10); // Small delay to ensure different timestamps

        var project2 = await _projectService.CreateAsync(new Project
        {
            Name = "Second Project",
            Color = "#3B82F6"
        });

        var (projects, _) = await _projectService.GetAllAsync(1, 20);
        var projectList = projects.ToList();

        Assert.That(projectList[0].Id, Is.EqualTo(project2.Id));
        Assert.That(projectList[1].Id, Is.EqualTo(project1.Id));
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateProject_WhenProjectExists()
    {
        var project = await _projectService.CreateAsync(new Project
        {
            Name = "Original Name",
            Description = "Original Description",
            Color = "#FF5733"
        });

        var updateData = new Project
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Color = "#00FF00"
        };

        var result = await _projectService.UpdateAsync(project.Id, updateData);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Updated Name"));
        Assert.That(result.Description, Is.EqualTo("Updated Description"));
        Assert.That(result.Color, Is.EqualTo("#00FF00"));
        Assert.That(result.UpdatedAt, Is.GreaterThan(result.CreatedAt));
    }

    [Test]
    public async Task UpdateAsync_ShouldReturnNull_WhenProjectDoesNotExist()
    {
        var updateData = new Project
        {
            Name = "Updated Name",
            Color = "#00FF00"
        };

        var result = await _projectService.UpdateAsync(Guid.NewGuid(), updateData);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_ShouldDeleteProject_WhenProjectExists()
    {
        var project = await _projectService.CreateAsync(new Project
        {
            Name = "Test Project",
            Color = "#FF5733"
        });

        var result = await _projectService.DeleteAsync(project.Id);

        Assert.That(result, Is.True);

        var deletedProject = await _projectService.GetByIdAsync(project.Id);
        Assert.That(deletedProject, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_ShouldReturnFalse_WhenProjectDoesNotExist()
    {
        var result = await _projectService.DeleteAsync(Guid.NewGuid());

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GetAllAsync_ShouldHandleInvalidPageNumbers()
    {
        await _projectService.CreateAsync(new Project
        {
            Name = "Test Project",
            Color = "#3B82F6"
        });

        // Test with page number 0 (should be treated as 1)
        var (projects, totalCount) = await _projectService.GetAllAsync(0, 20);

        Assert.That(totalCount, Is.EqualTo(1));
        Assert.That(projects.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetAllAsync_ShouldLimitPageSize_WhenExceedsMaximum()
    {
        // Create 150 projects
        for (int i = 1; i <= 150; i++)
        {
            await _projectService.CreateAsync(new Project
            {
                Name = $"Project {i}",
                Color = "#3B82F6"
            });
        }

        // Request with page size > 100 (should be limited to 100)
        var (projects, totalCount) = await _projectService.GetAllAsync(1, 200);

        Assert.That(totalCount, Is.EqualTo(150));
        Assert.That(projects.Count(), Is.EqualTo(100));
    }
}
