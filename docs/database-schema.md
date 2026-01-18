# Database Schema Documentation

## Overview

The Brain Shelf database uses PostgreSQL 15 and is managed by Entity Framework Core 9.0. The schema supports organizing projects, entries, tags, and metadata for a knowledge management system.

## Entity Relationship Diagram

```
┌─────────────────┐
│    Projects     │
├─────────────────┤
│ Id (PK)         │
│ Name            │
│ Description     │
│ Color           │
│ CreatedAt       │
│ UpdatedAt       │
└────────┬────────┘
         │
         │ 1:N
         │
         ▼
┌─────────────────┐       ┌─────────────────┐
│     Entries     │       │      Tags       │
├─────────────────┤       ├─────────────────┤
│ Id (PK)         │       │ Id (PK)         │
│ ProjectId (FK)  │       │ Name (UK)       │
│ Title           │       │ CreatedAt       │
│ Description     │       │ UpdatedAt       │
│ Type            │       └────────┬────────┘
│ Content         │                │
│ Url             │                │
│ CreatedAt       │                │
│ UpdatedAt       │                │
└────────┬────────┘                │
         │                         │
         │ 1:1                     │ N:M
         │                         │
         ▼                         ▼
┌─────────────────┐       ┌─────────────────┐
│    Metadata     │       │   EntryTags     │
├─────────────────┤       ├─────────────────┤
│ Id (PK)         │       │ EntriesId (PK)  │
│ EntryId (FK,UK) │       │ TagsId (PK)     │
│ PageTitle       │       └─────────────────┘
│ MetaDescription │
│ Keywords        │
│ PreviewImageUrl │
│ ExtractedAt     │
│ CreatedAt       │
│ UpdatedAt       │
└─────────────────┘
```

Legend:
- PK = Primary Key
- FK = Foreign Key
- UK = Unique Key

## Tables

### Projects

Stores project information for organizing entries.

| Column      | Type                 | Constraints        | Description                    |
|-------------|----------------------|--------------------|--------------------------------|
| Id          | uuid                 | PRIMARY KEY        | Unique identifier              |
| Name        | varchar(200)         | NOT NULL           | Project name                   |
| Description | varchar(1000)        | NULL               | Optional project description   |
| Color       | varchar(7)           | NOT NULL           | Hex color code (e.g., #3B82F6)|
| CreatedAt   | timestamptz          | NOT NULL           | Creation timestamp (UTC)       |
| UpdatedAt   | timestamptz          | NOT NULL           | Last update timestamp (UTC)    |

**Relationships:**
- One-to-Many with Entries (CASCADE DELETE)

---

### Entries

Stores individual entries (links, notes, settings, instructions) within projects.

| Column      | Type                 | Constraints        | Description                          |
|-------------|----------------------|--------------------|--------------------------------------|
| Id          | uuid                 | PRIMARY KEY        | Unique identifier                    |
| ProjectId   | uuid                 | NOT NULL, FK       | Reference to parent project          |
| Title       | varchar(500)         | NOT NULL           | Entry title                          |
| Description | varchar(2000)        | NULL               | Optional entry description           |
| Type        | integer              | NOT NULL           | Entry type (0=Link, 1=Note, 2=Setting, 3=Instruction) |
| Content     | text                 | NULL               | Main content/notes                   |
| Url         | varchar(2000)        | NULL               | Optional URL for link entries        |
| CreatedAt   | timestamptz          | NOT NULL           | Creation timestamp (UTC)             |
| UpdatedAt   | timestamptz          | NOT NULL           | Last update timestamp (UTC)          |

**Indexes:**
- `IX_Entries_ProjectId` - For efficient project-based queries
- `IX_Entries_Title` - For title-based searches
- `IX_Entries_Content` - For full-text content searches

**Relationships:**
- Many-to-One with Projects (CASCADE DELETE)
- One-to-One with Metadata (CASCADE DELETE)
- Many-to-Many with Tags via EntryTags junction table

**Entry Types:**
- `0` - Link: External resource reference
- `1` - Note: Personal note or documentation
- `2` - Setting: Configuration or preference
- `3` - Instruction: Step-by-step guide or procedure

---

### Tags

Stores reusable tags for categorizing entries.

| Column    | Type                 | Constraints        | Description                    |
|-----------|----------------------|--------------------|--------------------------------|
| Id        | uuid                 | PRIMARY KEY        | Unique identifier              |
| Name      | varchar(100)         | NOT NULL, UNIQUE   | Tag name                       |
| CreatedAt | timestamptz          | NOT NULL           | Creation timestamp (UTC)       |
| UpdatedAt | timestamptz          | NOT NULL           | Last update timestamp (UTC)    |

**Indexes:**
- `IX_Tags_Name` - Unique index for tag name lookups

**Relationships:**
- Many-to-Many with Entries via EntryTags junction table

---

### EntryTags (Junction Table)

Manages the many-to-many relationship between Entries and Tags.

| Column    | Type                 | Constraints        | Description                    |
|-----------|----------------------|--------------------|--------------------------------|
| EntriesId | uuid                 | PRIMARY KEY, FK    | Reference to Entry             |
| TagsId    | uuid                 | PRIMARY KEY, FK    | Reference to Tag               |

**Indexes:**
- `IX_EntryTags_TagsId` - For efficient tag-based queries
- Composite primary key on (EntriesId, TagsId)

**Relationships:**
- Many-to-One with Entries (CASCADE DELETE)
- Many-to-One with Tags (CASCADE DELETE)

---

### Metadata

Stores extracted metadata from URLs for link entries.

| Column          | Type                 | Constraints        | Description                              |
|-----------------|----------------------|--------------------|------------------------------------------|
| Id              | uuid                 | PRIMARY KEY        | Unique identifier                        |
| EntryId         | uuid                 | NOT NULL, FK, UNIQUE | Reference to parent entry              |
| PageTitle       | varchar(500)         | NULL               | Extracted page title                     |
| MetaDescription | varchar(2000)        | NULL               | Extracted meta description               |
| Keywords        | varchar(1000)        | NULL               | Extracted keywords                       |
| PreviewImageUrl | varchar(2000)        | NULL               | URL to preview/og:image                  |
| ExtractedAt     | timestamptz          | NOT NULL           | When metadata was extracted (UTC)        |
| CreatedAt       | timestamptz          | NOT NULL           | Creation timestamp (UTC)                 |
| UpdatedAt       | timestamptz          | NOT NULL           | Last update timestamp (UTC)              |

**Indexes:**
- `IX_Metadata_EntryId` - Unique index ensuring one metadata record per entry

**Relationships:**
- One-to-One with Entries (CASCADE DELETE)

---

## Migration Information

**Migration Name:** `InitialCreate`  
**Created:** 2026-01-18 16:32:36 UTC  
**EF Core Version:** 9.0.0  
**Database Provider:** Npgsql.EntityFrameworkCore.PostgreSQL 9.0.2

### Applied Constraints

1. **Foreign Key Constraints:**
   - All foreign keys use `ON DELETE CASCADE` to maintain referential integrity
   - When a project is deleted, all its entries are automatically removed
   - When an entry is deleted, its metadata and tag associations are removed

2. **Unique Constraints:**
   - Tag names must be unique across the system
   - Each entry can have at most one metadata record

3. **NOT NULL Constraints:**
   - All entities have required Id, CreatedAt, and UpdatedAt fields
   - Projects require Name and Color
   - Entries require ProjectId, Title, and Type
   - Tags require Name
   - Metadata requires EntryId and ExtractedAt

## Automatic Timestamp Management

The `ApplicationDbContext` automatically manages timestamps:
- `CreatedAt` is set when an entity is first added
- `UpdatedAt` is updated whenever an entity is modified
- All timestamps use UTC to ensure consistency across time zones

## Connection String

**Development:**
```
Host=localhost;Port=5432;Database=brainshelf;Username=admin;Password=admin
```

**Docker:**
```
Host=database;Port=5432;Database=brainshelf;Username=admin;Password=admin
```

## Database Operations

### Create Migration
```bash
dotnet ef migrations add <MigrationName> \
  --project src/BrainShelf.Infrastructure/BrainShelf.Infrastructure.csproj \
  --startup-project src/BrainShelf.Api/BrainShelf.Api.csproj
```

### Apply Migration
```bash
dotnet ef database update \
  --project src/BrainShelf.Infrastructure/BrainShelf.Infrastructure.csproj \
  --startup-project src/BrainShelf.Api/BrainShelf.Api.csproj
```

### Remove Last Migration
```bash
dotnet ef migrations remove \
  --project src/BrainShelf.Infrastructure/BrainShelf.Infrastructure.csproj \
  --startup-project src/BrainShelf.Api/BrainShelf.Api.csproj
```

### View Migration SQL
```bash
dotnet ef migrations script \
  --project src/BrainShelf.Infrastructure/BrainShelf.Infrastructure.csproj \
  --startup-project src/BrainShelf.Api/BrainShelf.Api.csproj
```

## Performance Considerations

1. **Indexes for Search:**
   - Title and Content fields are indexed to support efficient full-text search
   - Consider adding GIN indexes for better text search performance in production

2. **Cascade Deletes:**
   - Be cautious when deleting projects as all associated data will be removed
   - Consider implementing soft deletes for important data retention

3. **Query Optimization:**
   - Use `.Include()` for eager loading related entities
   - Consider implementing pagination for large result sets
   - Monitor query performance using EF Core logging

## Future Enhancements

Potential schema improvements for future iterations:

1. **Full-Text Search:**
   - Add PostgreSQL full-text search (tsvector) columns
   - Implement GIN indexes for better search performance

2. **Audit Trail:**
   - Add audit logging table for tracking changes
   - Store user information for multi-user scenarios

3. **File Attachments:**
   - Add support for file uploads and attachments
   - Store file metadata and references

4. **User Management:**
   - Add Users table for authentication
   - Implement user-project associations
   - Add sharing and permissions

5. **Soft Deletes:**
   - Add IsDeleted flag to entities
   - Implement automatic filtering of deleted records
