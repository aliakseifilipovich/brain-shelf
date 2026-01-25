using System.Net;
using System.Net.Http.Json;
using BrainShelf.Api.DTOs;
using BrainShelf.Core.Entities;
using BrainShelf.Infrastructure.Data;
using BrainShelf.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace BrainShelf.Tests.Integration.Controllers;

[TestFixture]
public class ProjectsControllerTests
{
    private CustomWebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private ApplicationDbContext _context;

    [SetUp]
    public void Setup()
    {
        _factory = new CustomWebApplicationFactory<Program>();
        _client = _factory.CreateClient();

        // Get database context for test data setup
        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
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
    public async Task GetAll_ReturnsSuccessWithProjects()
    {
        // Arrange
        var projects = TestDataFactory.CreateProjects(3);
        _context.Projects.AddRange(projects);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/projects");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Items.Count(), Is.EqualTo(3));
        Assert.That(result.TotalCount, Is.EqualTo(3));
    }

    [Test]
    public async Task GetAll_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var projects = TestDataFactory.CreateProjects(15);
        _context.Projects.AddRange(projects);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/projects?pageNumber=2&pageSize=5");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Items.Count(), Is.EqualTo(5));
        Assert.That(result.PageNumber, Is.EqualTo(2));
        Assert.That(result.PageSize, Is.EqualTo(5));
        Assert.That(result.TotalCount, Is.EqualTo(15));
    }

    [Test]
    public async Task GetAll_EmptyDatabase_ReturnsEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/projects");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Items, Is.Empty);
        Assert.That(result.TotalCount, Is.EqualTo(0));
    }

    [Test]
    public async Task GetById_ExistingProject_ReturnsProject()
    {
        // Arrange
        var project = TestDataFactory.CreateProject(name: "Test Project", description: "Test Description");
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/projects/{project.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ProjectDto>();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(project.Id));
        Assert.That(result.Name, Is.EqualTo("Test Project"));
        Assert.That(result.Description, Is.EqualTo("Test Description"));
    }

    [Test]
    public async Task GetById_NonExistingProject_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/projects/{Guid.NewGuid()}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Create_ValidProject_ReturnsCreatedProject()
    {
        // Arrange
        var createDto = new CreateProjectDto
        {
            Name = "New Project",
            Description = "New Description",
            Color = "#FF5733"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/projects", createDto);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        
        var result = await response.Content.ReadFromJsonAsync<ProjectDto>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("New Project"));
        Assert.That(result.Description, Is.EqualTo("New Description"));
        Assert.That(result.Color, Is.EqualTo("#FF5733"));
        Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));

        // Verify Location header
        Assert.That(response.Headers.Location, Is.Not.Null);
        Assert.That(response.Headers.Location!.ToString(), Does.Contain($"/api/projects/{result.Id}"));
    }

    [Test]
    public async Task Create_ProjectPersistedInDatabase()
    {
        // Arrange
        var createDto = new CreateProjectDto
        {
            Name = "Persisted Project",
            Description = "Test Persistence"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/projects", createDto);
        var result = await response.Content.ReadFromJsonAsync<ProjectDto>();

        // Assert
        var dbProject = await _context.Projects.FindAsync(result!.Id);
        Assert.That(dbProject, Is.Not.Null);
        Assert.That(dbProject!.Name, Is.EqualTo("Persisted Project"));
    }

    [Test]
    public async Task Update_ExistingProject_ReturnsUpdatedProject()
    {
        // Arrange
        var project = TestDataFactory.CreateProject();
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateProjectDto
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Color = "#00FF00"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/projects/{project.Id}", updateDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ProjectDto>();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Updated Name"));
        Assert.That(result.Description, Is.EqualTo("Updated Description"));
        Assert.That(result.Color, Is.EqualTo("#00FF00"));
    }

    [Test]
    public async Task Update_NonExistingProject_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateProjectDto { Name = "Updated Name" };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/projects/{Guid.NewGuid()}", updateDto);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Update_UpdatesTimestamp()
    {
        // Arrange
        var project = TestDataFactory.CreateProject();
        project.UpdatedAt = DateTime.UtcNow.AddHours(-1);
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var originalUpdatedAt = project.UpdatedAt;
        var updateDto = new UpdateProjectDto { Name = "Updated Name" };

        // Act
        await Task.Delay(10); // Small delay to ensure timestamp difference
        var response = await _client.PutAsJsonAsync($"/api/projects/{project.Id}", updateDto);
        var result = await response.Content.ReadFromJsonAsync<ProjectDto>();

        // Assert
        Assert.That(result!.UpdatedAt, Is.GreaterThan(originalUpdatedAt));
    }

    [Test]
    public async Task Delete_ExistingProject_ReturnsNoContent()
    {
        // Arrange
        var project = TestDataFactory.CreateProject();
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/projects/{project.Id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task Delete_NonExistingProject_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/projects/{Guid.NewGuid()}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_RemovesProjectFromDatabase()
    {
        // Arrange
        var project = TestDataFactory.CreateProject();
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var projectId = project.Id;

        // Act
        await _client.DeleteAsync($"/api/projects/{projectId}");

        // Assert
        var dbProject = await _context.Projects.FindAsync(projectId);
        Assert.That(dbProject, Is.Null);
    }

    [Test]
    public async Task Create_InvalidData_ReturnsBadRequest()
    {
        // Arrange - empty name which should be invalid
        var createDto = new CreateProjectDto
        {
            Name = "", // Invalid: required field
            Description = "Test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/projects", createDto);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetAll_WithLargeDataset_HandlesCorrectly()
    {
        // Arrange
        var projects = TestDataFactory.CreateProjects(100);
        _context.Projects.AddRange(projects);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/projects?pageSize=50");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProjectDto>>();
        
        Assert.That(result!.Items.Count(), Is.EqualTo(50));
        Assert.That(result.TotalCount, Is.EqualTo(100));
        Assert.That(result.TotalPages, Is.EqualTo(2));
    }
}
