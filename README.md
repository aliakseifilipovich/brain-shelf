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
- **Multilingual Support**: English and Russian languages
- **Entry Types**:
  - 🔗 Links (with automatic metadata extraction)
  - 📝 Notes
  - ⚙️ Settings
  - 📋 Instructions

## Technology Stack

### Backend
- **.NET 8.0** - Modern, high-performance web API framework
- **Entity Framework Core** - ORM for database operations
- **PostgreSQL** - Robust relational database

### Frontend
- **React 18** - Modern UI library
- **TypeScript** - Type-safe JavaScript
- **Vite** - Fast build tool and dev server

### Infrastructure
- **Docker & Docker Compose** - Containerization for easy deployment
- **PostgreSQL Full-Text Search** - Powerful multilingual search capabilities

## Getting Started

### Prerequisites
- Docker and Docker Compose installed
- Git

### Quick Start

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

### Development Setup

#### Running Backend Locally (without Docker)

**Prerequisites**: .NET 8.0 SDK

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
2. **backend** - .NET 8.0 Web API with hot reload support
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
