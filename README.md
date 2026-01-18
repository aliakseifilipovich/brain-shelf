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

2. **Set up environment variables**
   ```bash
   # Copy example environment files (to be created)
   cp .env.example .env
   ```

3. **Start the application with Docker**
   ```bash
   docker-compose up -d
   ```

4. **Access the application**
   - Frontend: http://localhost:3000
   - Backend API: http://localhost:5000
   - Swagger Documentation: http://localhost:5000/swagger

### Development Setup

Coming soon - detailed instructions for running backend and frontend in development mode.

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
- [x] Project infrastructure setup
- [ ] Database schema and migrations

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
