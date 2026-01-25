# Brain Shelf 🧠📚

Brain Shelf is a centralized storage application for organizing and managing useful project-related information. It helps you collect instructions, settings, links, and notes from work chats into a structured and easily searchable knowledge base.

## Features

- **Multi-Project Support**: Organize information across multiple projects
- **Intelligent Search**: 
  - Full-text search across all content
  - Filter by descriptions, tags, and entry types
  - Autocomplete suggestions
- **Automatic Metadata Extraction**: 
  - Automatically extract page titles, descriptions, keywords, and preview images from links
  - Minimal manual input required
- **Entry Types**:
  - 🔗 Links (with automatic metadata extraction)
  - 📝 Notes
  - 💻 Code
  - 📋 Instructions

## Technology Stack

### Backend
- **.NET 9.0** - Modern, high-performance web API framework
- **Entity Framework Core** - ORM for database operations
- **PostgreSQL** - Robust relational database

### Frontend
- **React 18** - Modern UI library
- **TypeScript** - Type-safe JavaScript
- **Vite** - Fast build tool and dev server

### Infrastructure
- **Docker & Docker Compose** - Containerization for easy deployment
- **PostgreSQL Full-Text Search** - Powerful full-text search capabilities

## Getting Started

### Prerequisites
- Docker and Docker Compose installed
- Git

### Quick Start (Development)

1. **Clone the repository**
   ```bash
   git clone https://github.com/aliakseifilipovich/brain-shelf.git
   cd brain-shelf
   ```

2. **Start the application with Docker**
   ```bash
   docker-compose up -d
   ```
   
   This command will:
   - Start PostgreSQL database on port 5432
   - Build and start the .NET backend API on port 5000
   - Build and start the React frontend on port 3000

3. **Access the application**
   - **Frontend**: http://localhost:3000
   - **Backend API**: http://localhost:5000/api/health
   - **Swagger Documentation**: http://localhost:5000/swagger

4. **Stop the application**
   ```bash
   docker-compose down
   ```

### Production Deployment

For production environments, Brain Shelf provides optimized Docker configurations with:
- Multi-stage builds for smaller images
- Nginx reverse proxy with caching and compression
- Health checks and resource limits
- Automated backup and restore scripts
- Comprehensive monitoring and logging

See the [Deployment Guide](docs/DEPLOYMENT.md) for detailed production setup instructions.

**Quick Production Start**:
```bash
# Copy environment template
cp .env.prod.example .env.prod

# Edit .env.prod with your configuration
nano .env.prod

# Start production services
docker-compose -f docker-compose.prod.yml up -d

# Verify health
curl http://localhost/health
```

### Development Setup

#### Running Backend Locally (without Docker)

**Prerequisites**: .NET 10.0 SDK

```bash
cd backend/src/BrainShelf.Api
dotnet restore
dotnet run
```

The API will be available at http://localhost:5000

#### Running Frontend Locally (without Docker)

**Prerequisites**: Node.js 20+

```bash
cd frontend
npm install
npm run dev
```

The frontend will be available at http://localhost:3000

### Database Configuration

The default database credentials are:
- **Host**: localhost (or `database` within Docker network)
- **Port**: 5432
- **Database**: brainshelf
- **Username**: admin
- **Password**: admin

To connect to the database:
```bash
docker-compose exec database psql -U admin -d brainshelf
```

### Health Checks

The backend provides health check endpoints:
- **Basic Health**: GET `/api/health` - Returns service status and version
- **Full Health**: GET `/health` - Includes database connectivity check

### Docker Services

The `docker-compose.yml` defines three services:

1. **database** - PostgreSQL 15 with persistent volume
2. **backend** - .NET 9.0 Web API with hot reload support
3. **frontend** - React + Vite with hot reload support

### Troubleshooting

**Container logs**:
```bash
# View all logs
docker-compose logs

# View specific service logs
docker-compose logs backend
docker-compose logs frontend
docker-compose logs database

# Follow logs in real-time
docker-compose logs -f
```

**Rebuild containers**:
```bash
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

**Database issues**:
```bash
# Reset database (WARNING: destroys all data)
docker-compose down -v
docker-compose up -d
```

## Project Structure

```
brain-shelf/
├── backend/          # .NET Web API
│   ├── src/         # Source code
│   └── tests/       # Unit and integration tests
├── frontend/        # React + TypeScript
│   ├── src/         # Source code
│   └── public/      # Static assets
├── docs/            # Documentation
├── docker-compose.yml
└── README.md
```

## Development Roadmap

See our [GitHub Issues](https://github.com/aliakseifilipovich/brain-shelf/issues) for the complete development roadmap.

### Phase 1: Foundation (Issues #1-2)
- ✅ Project infrastructure setup (Issue #1)
- [ ] Database schema and migrations (Issue #2)

### Phase 2: Core Backend (Issues #3-6)
- [ ] Projects API
- [ ] Entries API
- [ ] Link metadata extraction
- [ ] Full-text search

### Phase 3: Frontend (Issues #7-10)
- [ ] Project setup and base UI
- [ ] Projects management UI
- [ ] Entries management UI
- [ ] Search interface

### Phase 4: Enhancement (Issues #11-15)
- [ ] Internationalization (i18n)
- [ ] Production Docker configuration
- [ ] Comprehensive testing
- [ ] Tags autocomplete
- [ ] Entry templates and quick actions

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Author

**Aliaksei Filipovich**
- GitHub: [@aliakseifilipovich](https://github.com/aliakseifilipovich)
- Repository: [brain-shelf](https://github.com/aliakseifilipovich/brain-shelf)

## Acknowledgments

- Built with modern web technologies
- Inspired by the need for better knowledge management in development teams

## AI-Assisted Development

This project was developed as a **GitHub Copilot Practical Task** where **~95% of the code was generated by AI** tools.

### Tools & Models Used

- **GitHub Copilot** (in VS Code) - Primary code generation tool
- **Claude Sonnet 4.5** - Complex problem solving and architecture decisions
- **Codex 5.2** - Code completion and refactoring
- **Gemini 2.5 Pro** - Additional problem solving and code generation
- **Model Context Protocol (MCP) Servers**:
  - GitHub MCP Server - Repository management, PR creation, issue tracking
  - Docker MCP Server - Container management and inspection

### Development Approach

The project was built using an **iterative, issue-driven approach**:

1. **Planning Phase**: AI generated comprehensive GitHub Issues with acceptance criteria
2. **Implementation Phase**: Each issue implemented incrementally with AI assistance
3. **Review Phase**: Developer acted primarily as reviewer, testing, and providing feedback
4. **Refinement Phase**: AI fixed bugs and improved code based on feedback

### Prompt Strategy

See [docs/prompts.md](docs/prompts.md) for the complete prompt history (41+ prompts).

**Key Successful Patterns**:
- ✅ Start with architecture/planning before implementation
- ✅ Implement one feature at a time with clear acceptance criteria
- ✅ Provide context about existing code and constraints
- ✅ Request clarification questions before proceeding
- ✅ Ask AI to verify changes before committing

**Lessons Learned**:
- Multiple small edits to the same file can cause corruption - better to replace larger blocks
- Always ask AI to check for errors before committing
- Provide clear specifications (e.g., "20 rows per page, server-side pagination")
- Let AI handle routine tasks (CRUD, forms, tables) while reviewing logic

### Code Generation Statistics

- **Backend**: ~90% AI-generated (Controllers, Services, DbContext, Migrations)
- **Frontend**: ~95% AI-generated (Components, Pages, State Management, Routing)
- **Infrastructure**: ~100% AI-generated (Docker, docker-compose, CI/CD)
- **Documentation**: ~85% AI-generated (README, API docs, comments)

**Manual Contributions**:
- Architecture decisions and requirements definition
- Code review and testing
- Bug reporting and clarifications
- Final verification and deployment

For detailed insights into the development workflow, see [docs/prompts.md](docs/prompts.md).
