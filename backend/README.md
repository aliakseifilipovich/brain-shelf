# Backend - Brain Shelf API

.NET 9.0 Web API backend for Brain Shelf application.

## Structure

```
backend/
├── src/
│   ├── BrainShelf.Api/           # Web API project (controllers, configuration)
│   ├── BrainShelf.Core/          # Domain models and entities
│   ├── BrainShelf.Infrastructure/ # Data access (EF Core, DbContext)
│   └── BrainShelf.Services/      # Business logic layer
└── tests/                         # Tests (coming in future issues)
```

## Technologies

- **.NET 9.0** - Latest LTS version of .NET
- **ASP.NET Core Web API** - RESTful API framework
- **Entity Framework Core 9.0** - ORM for database operations
- **Npgsql** - PostgreSQL provider for EF Core
- **Swagger/OpenAPI** - API documentation

## Getting Started

### Run with Docker (Recommended)

From the project root:
```bash
docker-compose up -d
```

The API will be available at:
- Health endpoint: http://localhost:5000/api/health
- Swagger UI: http://localhost:5000/swagger

### Run Locally

**Prerequisites**: .NET 8.0 SDK

```bash
cd src/BrainShelf.Api
dotnet restore
dotnet run
```

## Project Architecture

### BrainShelf.Api
- Entry point for the application
- Controllers for HTTP endpoints
- Dependency injection configuration
- Middleware pipeline setup

### BrainShelf.Core
- Domain entities (Project, Entry, Tag)
- Business interfaces
- Domain logic
- No external dependencies

### BrainShelf.Infrastructure
- ApplicationDbContext (EF Core)
- Database migrations
- Repository implementations
- External service integrations

### BrainShelf.Services
- Business logic services
- Orchestration layer between API and Infrastructure
- Coming in future issues

## Database

The application uses PostgreSQL with Entity Framework Core.

**Connection String** (configured in appsettings.json or environment variable):
```
Host=localhost;Port=5432;Database=brainshelf;Username=admin;Password=admin
```

## API Endpoints

### Health Check
- `GET /api/health` - Returns API status and version
- `GET /health` - ASP.NET Core health check (includes database connectivity)

### Future Endpoints
- Projects API (Issue #3)
- Entries API (Issue #4)
- Search API (Issue #6)

## Development

### Adding a New Migration

```bash
cd src/BrainShelf.Api
dotnet ef migrations add MigrationName -p ../BrainShelf.Infrastructure
```

### Updating Database

```bash
dotnet ef database update
```

### Building the Project

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

## Configuration

Environment variables can be set in:
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- Environment variables - Production configuration
- Docker Compose - Container-specific settings

## Health Monitoring

The application includes ASP.NET Core health checks that verify:
- API is running
- Database connectivity
- Response time

Access health status at `/health` endpoint.
