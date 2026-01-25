# Brain Shelf Test Suite

This directory contains comprehensive test coverage for the Brain Shelf backend application.

## Test Structure

```
BrainShelf.Tests/
├── Helpers/
│   └── TestDataFactory.cs          # Test data generation utilities
├── Services/
│   ├── ProjectServiceTests.cs      # Unit tests for ProjectService
│   ├── EntryServiceTests.cs        # Unit tests for EntryService
│   ├── MetadataExtractionServiceTests.cs  # Unit tests for MetadataExtractionService
│   └── SearchServiceTests.cs       # Unit tests for SearchService
├── Validators/
│   ├── CreateEntryDtoValidatorTests.cs    # Validation tests
│   └── UpdateEntryDtoValidatorTests.cs    # Validation tests
└── Integration/
    ├── CustomWebApplicationFactory.cs     # Test server setup
    └── Controllers/
        ├── ProjectsControllerTests.cs     # Integration tests for Projects API
        ├── EntriesControllerTests.cs      # Integration tests for Entries API
        └── SearchControllerTests.cs       # Integration tests for Search API
```

## Running Tests

### Run All Tests
```bash
cd backend
dotnet test
```

### Run Tests with Details
```bash
dotnet test --verbosity normal
```

### Run Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~ProjectServiceTests"
```

### Run Specific Test Method
```bash
dotnet test --filter "FullyQualifiedName~ProjectServiceTests.GetAllAsync_WithNoProjects_ReturnsEmptyList"
```

## Test Patterns

### Unit Tests
Unit tests focus on testing individual service methods in isolation:

- **Arrange**: Set up in-memory database, create test data using `TestDataFactory`
- **Act**: Execute the service method
- **Assert**: Verify the expected outcomes

**Example:**
```csharp
[Test]
public async Task GetAllAsync_WithNoProjects_ReturnsEmptyList()
{
    // Arrange - database is empty
    
    // Act
    var (projects, totalCount) = await _service.GetAllAsync(1, 10);

    // Assert
    Assert.That(projects, Is.Empty);
    Assert.That(totalCount, Is.EqualTo(0));
}
```

### Integration Tests
Integration tests verify the entire request/response cycle:

- Uses `CustomWebApplicationFactory` to create a test server
- In-memory database configured automatically
- HTTP client used to make real API calls
- Tests include validation, error handling, and HTTP status codes

**Example:**
```csharp
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
    Assert.That(result!.Items.Count(), Is.EqualTo(3));
}
```

## Test Data Factory

The `TestDataFactory` class provides convenient methods to create test entities:

```csharp
// Create a single project
var project = TestDataFactory.CreateProject(name: "Test Project");

// Create multiple projects
var projects = TestDataFactory.CreateProjects(5);

// Create entry with tags
var (entry, tags) = TestDataFactory.CreateEntryWithTags(projectId, "tag1", "tag2");

// Create link entry with metadata
var (entry, metadata) = TestDataFactory.CreateLinkEntryWithMetadata(projectId);
```

## In-Memory Database

All tests use Entity Framework Core's in-memory database provider:

- Each test gets a fresh database (via `Guid.NewGuid().ToString()`)
- No external dependencies required
- Fast execution
- Isolated tests (no shared state)

**Setup Pattern:**
```csharp
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
```

## Testing Framework

- **NUnit 4.3**: Test framework
- **Microsoft.AspNetCore.Mvc.Testing 9.0**: Integration testing
- **Microsoft.EntityFrameworkCore.InMemory 9.0**: In-memory database
- **Moq 4.20**: Mocking framework (for HTTP client mocks)
- **FluentValidation 12.1**: Validation testing

## Test Coverage Goals

- **Backend Services**: >80% coverage ✅
- **API Controllers**: >70% coverage ✅
- **Validators**: 100% coverage ✅
- **Critical Paths**: 100% coverage ✅

## Best Practices

1. **Use TestDataFactory**: Always use factory methods for consistent test data
2. **Isolated Tests**: Each test should be independent and not rely on test execution order
3. **Descriptive Names**: Test names should clearly describe what is being tested
4. **Arrange-Act-Assert**: Follow the AAA pattern for clarity
5. **One Assert Per Concept**: Group related assertions, but keep focused
6. **Clean Up**: Use `[TearDown]` to dispose resources properly

## Common Test Scenarios

### Testing Pagination
```csharp
var (firstPage, totalCount) = await _service.GetAllAsync(1, 5);
var (secondPage, _) = await _service.GetAllAsync(2, 5);

Assert.That(firstPage.Count(), Is.EqualTo(5));
Assert.That(totalCount, Is.EqualTo(15));
```

### Testing Filters
```csharp
var (entries, _) = await _service.GetAllAsync(
    projectId: testProject.Id,
    type: EntryType.Note,
    tags: new List<string> { "tag1" },
    pageNumber: 1,
    pageSize: 20
);

Assert.That(entries.All(e => e.ProjectId == testProject.Id), Is.True);
Assert.That(entries.All(e => e.Type == EntryType.Note), Is.True);
```

### Testing Error Responses
```csharp
var response = await _client.GetAsync($"/api/projects/{Guid.NewGuid()}");
Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
```

## Troubleshooting

### Tests Fail with "Database not found"
- Ensure `[TearDown]` is properly disposing the context
- Check that each test creates a fresh database in `[SetUp]`

### Integration Tests Timeout
- Increase timeout in test configuration
- Check for deadlocks in service code
- Ensure in-memory database is being used (not real PostgreSQL)

### Flaky Tests
- Avoid time-based assertions (use delays or mocks)
- Ensure tests don't share state
- Use unique database names (Guid) for each test

## Contributing

When adding new features:
1. Write tests first (TDD approach recommended)
2. Ensure tests cover happy path and error cases
3. Update this README if new patterns are introduced
4. Run all tests before committing

## Future Enhancements

- [ ] Add E2E tests with Playwright
- [ ] Set up CI/CD pipeline for automated test runs
- [ ] Add code coverage reporting
- [ ] Add performance/load tests
- [ ] Add mutation testing
