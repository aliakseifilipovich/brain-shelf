using BrainShelf.Core.Entities;
using BrainShelf.Infrastructure.Data;
using BrainShelf.Services;
using BrainShelf.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BrainShelf.Tests.Services;

[TestFixture]
public class ProjectServiceTests
{
    private ApplicationDbContext _context;
    private ProjectService _service;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new ProjectService(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task GetAllAsync_WithNoProjects_ReturnsEmptyList()
    {
        // Act
        var (projects, totalCount) = await _service.GetAllAsync(1, 10);

        // Assert
        Assert.That(projects, Is.Empty);
        Assert.That(totalCount, Is.EqualTo(0));
    }

    [Test]
    public async Task GetAllAsync_WithProjects_ReturnsAllProjects()
    {
        // Arrange
        var testProjects = TestDataFactory.CreateProjects(5);
        _context.Projects.AddRange(testProjects);
        await _context.SaveChangesAsync();

        // Act
        var (projects, totalCount) = await _service.GetAllAsync(1, 10);

        // Assert
        Assert.That(projects.Count(), Is.EqualTo(5));
        Assert.That(totalCount, Is.EqualTo(5));
    }

    [Test]
    public async Task GetAllAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var testProjects = TestDataFactory.CreateProjects(15);
        _context.Projects.AddRange(testProjects);
        await _context.SaveChangesAsync();

        // Act
        var (firstPage, totalCount) = await _service.GetAllAsync(1, 5);
        var (secondPage, _) = await _service.GetAllAsync(2, 5);

        // Assert
        Assert.That(firstPage.Count(), Is.EqualTo(5));
        Assert.That(secondPage.Count(), Is.EqualTo(5));
        Assert.That(totalCount, Is.EqualTo(15));
        Assert.That(firstPage.Select(p => p.Id), Is.Not.EquivalentTo(secondPage.Select(p => p.Id)));
    }

    [Test]
    public async Task GetAllAsync_OrdersByCreatedAtDescending()
    {
        // Arrange
        var project1 = TestDataFactory.CreateProject(name: "First");
        project1.CreatedAt = DateTime.UtcNow.AddDays(-2);
        
        var project2 = TestDataFactory.CreateProject(name: "Second");
        project2.CreatedAt = DateTime.UtcNow.AddDays(-1);
        
        var project3 = TestDataFactory.CreateProject(name: "Third");
        project3.CreatedAt = DateTime.UtcNow;

        _context.Projects.AddRange(project1, project2, project3);
        await _context.SaveChangesAsync();

        // Act
        var (projects, _) = await _service.GetAllAsync(1, 10);
        var projectList = projects.ToList();

        // Assert
        Assert.That(projectList[0].Name, Is.EqualTo("Third"));
        Assert.That(projectList[1].Name, Is.EqualTo("Second"));
        Assert.That(projectList[2].Name, Is.EqualTo("First"));
    }

    [Test]
    public async Task GetAllAsync_WithInvalidPagination_AdjustsToValidValues()
    {
        // Arrange
        var testProjects = TestDataFactory.CreateProjects(5);
        _context.Projects.AddRange(testProjects);
        await _context.SaveChangesAsync();

        // Act - negative page number and page size
        var (projects1, _) = await _service.GetAllAsync(-1, -10);
        
        // Act - page size exceeds maximum
        var (projects2, _) = await _service.GetAllAsync(1, 150);

        // Assert
        Assert.That(projects1.Count(), Is.GreaterThan(0)); // Should default to valid values
        Assert.That(projects2.Count(), Is.EqualTo(5)); // Should be capped at 100 max
    }

    [Test]
    public async Task GetByIdAsync_WithExistingId_ReturnsProject()
    {
        // Arrange
        var project = TestDataFactory.CreateProject();
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(project.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(project.Id));
        Assert.That(result.Name, Is.EqualTo(project.Name));
    }

    [Test]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Act
        var result = await _service.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateAsync_WithValidProject_CreatesAndReturnsProject()
    {
        // Arrange
        var project = new Project
        {
            Name = "New Project",
            Description = "New Description"
        };

        // Act
        var result = await _service.CreateAsync(project);

        // Assert
        Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(result.Name, Is.EqualTo("New Project"));
        Assert.That(result.Description, Is.EqualTo("New Description"));
        Assert.That(result.CreatedAt, Is.Not.EqualTo(default(DateTime)));
        Assert.That(result.UpdatedAt, Is.Not.EqualTo(default(DateTime)));

        // Verify it's in database
        var dbProject = await _context.Projects.FindAsync(result.Id);
        Assert.That(dbProject, Is.Not.Null);
    }

    [Test]
    public async Task CreateAsync_SetsCreatedAtAndUpdatedAt()
    {
        // Arrange
        var project = new Project { Name = "Test Project" };
        var beforeCreate = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var result = await _service.CreateAsync(project);

        // Assert
        var afterCreate = DateTime.UtcNow.AddSeconds(1);
        Assert.That(result.CreatedAt, Is.GreaterThan(beforeCreate));
        Assert.That(result.CreatedAt, Is.LessThan(afterCreate));
        Assert.That(result.UpdatedAt, Is.GreaterThan(beforeCreate));
        Assert.That(result.UpdatedAt, Is.LessThan(afterCreate));
    }

    [Test]
    public async Task UpdateAsync_WithExistingProject_UpdatesAndReturnsProject()
    {
        // Arrange
        var project = TestDataFactory.CreateProject();
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var updateData = new Project
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Color = "#FF5733"
        };

        // Act
        var result = await _service.UpdateAsync(project.Id, updateData);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Updated Name"));
        Assert.That(result.Description, Is.EqualTo("Updated Description"));
        Assert.That(result.Color, Is.EqualTo("#FF5733"));
        Assert.That(result.UpdatedAt, Is.GreaterThan(project.UpdatedAt));
    }

    [Test]
    public async Task UpdateAsync_WithNonExistingProject_ReturnsNull()
    {
        // Arrange
        var updateData = new Project { Name = "Updated Name" };

        // Act
        var result = await _service.UpdateAsync(Guid.NewGuid(), updateData);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateAsync_UpdatesTimestamp()
    {
        // Arrange
        var project = TestDataFactory.CreateProject();
        project.UpdatedAt = DateTime.UtcNow.AddHours(-1);
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var originalUpdatedAt = project.UpdatedAt;
        var updateData = new Project { Name = "Updated Name" };

        // Act
        await Task.Delay(10); // Small delay to ensure timestamp difference
        var result = await _service.UpdateAsync(project.Id, updateData);

        // Assert
        Assert.That(result!.UpdatedAt, Is.GreaterThan(originalUpdatedAt));
    }

    [Test]
    public async Task DeleteAsync_WithExistingProject_DeletesAndReturnsTrue()
    {
        // Arrange
        var project = TestDataFactory.CreateProject();
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAsync(project.Id);

        // Assert
        Assert.That(result, Is.True);
        
        // Verify it's deleted from database
        var dbProject = await _context.Projects.FindAsync(project.Id);
        Assert.That(dbProject, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_WithNonExistingProject_ReturnsFalse()
    {
        // Act
        var result = await _service.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DeleteAsync_RemovesProjectFromDatabase()
    {
        // Arrange
        var project = TestDataFactory.CreateProject();
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var projectId = project.Id;

        // Act
        await _service.DeleteAsync(projectId);

        // Assert
        var projectsCount = await _context.Projects.CountAsync();
        Assert.That(projectsCount, Is.EqualTo(0));
    }
}
