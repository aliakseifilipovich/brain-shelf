# Prompts History

This file contains the history of all prompts used during the development of Brain Shelf.

## Prompts List

1. I need to develop a web application called "Brain Shelf" for centralized storage of useful project-related information. Main Goals: Collect instructions, settings, links, and notes from work chats into a structured and easily searchable storage. Requirements: (1) Support for multiple projects. (2) Convenient search, including: full-text search, filtering by descriptions, tags, and types, autocomplete. (3) Minimal manual input: if a link is submitted, automatically extract: page title, meta description, keywords, preview if possible. (4) Documentation: README with project description and run instructions, prompts.md file containing the history of all prompts - will be just only list with prompts as numeric list. (5) Technologies: Backend: latest .NET version, Database: PostgreSQL, Frontend: React + TypeScript, Docker. Repository: https://github.com/aliakseifilipovich/brain-shelf. What I need from you now: Create a development plan and cut it in GitHub Issues including: Number, Title, Description, Acceptance criteria, Component grouping and priorities. Each issue should represent a logically complete piece of work so that after it's done you can clearly see a concrete result. Create a .gitignore file and pull request with all this first files.

2. Let's implement the issue 1 and create pull request, also please add this promt to promts file with history. If you will need to clyrify something or any questions please ask me before procceed. [User provided clarifications: Ports 3000/5000 confirmed, Use BrainShelf naming, Database credentials: admin/admin, No .env.example needed now, Create full layered architecture, Create new feature branch]

3. Please implement the issue 2 and create the new PR with implementation, check that everything works as expected before creating PR, if u will need to clarify something please ask me before proceed. Please do not create seed data

4. Please implement the issue 3 and create the new PR with implementation, check that everything works as expected before creating PR, if you will need to clarify something please ask me before proceed. Please add this promt to a history as previous. [User provided clarifications: FluentValidation for validation, NUnit for testing, 20 items per page for pagination, RFC 7807 Problem Details format for errors]

5. Please implement the issue 4 and create the new PR with implementation, check that everything works as expected before creating PR, if you will need to clarify something please ask me before proceed. Please add this promt to a history as previous. [User provided clarifications: Tag management - accept tag names and auto-create if they don't exist (case-insensitive), Entry content validation - Link type requires both URL and Content, Note/Setting/Instruction types require Content only (Url is optional), Tag filtering uses OR logic (entry must have ANY of the specified tags), Skip integration tests for now]

6. Please implement the issue 5 and create the new PR with implementation, check that everything works as expected before creating PR, if you will need to clarify something please ask me before proceed. Please add this promt to a history as previous. [User provided clarifications: Execution mode - asynchronous (entry created immediately, metadata extracted in background), HTML parsing library - HtmlAgilityPack, Trigger behavior - only for Link type entries when creating/updating, URL update - automatically re-extract metadata when URL changes, Entry response - metadata as nested object in EntryDto]

7. Please implement the issue 6,7,8,9,10 and create the new PR with implementation, check that everything works as expected before creating PR, if you will need to clarify something please ask me before proceed. Please add this promt to a history as previous. [User provided clarifications: Issue #6 (Full-Text Search) - SKIP for now, will be implemented later, Question 2 (Autocomplete) - Use easiest approach for now, Question 3 (Search highlighting) - Yes to highlighting, UI Component Library - Material-UI (MUI), State Management - Redux Toolkit, Styling - CSS Modules, PR Strategy - Individual PRs for each issue (#7, #8, #9, #10)]

8. go [Continuing with Issue #8 implementation after Issue #7 PR #25 was created]

9. go [Continued with Issue #9 - Entries Management UI]

10. Let's continue with issue 6 [Implementing Full-Text Search backend - PostgreSQL FTS with tsvector/tsquery, GIN indexes, search across Entries, Metadata, and Tags with filtering and pagination]

11. We can continue with issue-10 [Implementing Search Interface frontend - Search page with debounced input, autocomplete from history, advanced filters (project, type, date range), result highlighting, pagination, empty states, and search history persistence in localStorage]

12. I thought, please close issue, we will skip internationalization, and update all docs removing all information about it [Decided to skip Issue #11 (Internationalization) - removed multilingual support requirements from documentation and closed the issue]

13. Let's implement issue 12 [Implementing Production Docker Configuration and Deployment - Multi-stage Docker builds for backend (.NET) and frontend (React + nginx), production docker-compose.yml with health checks, resource limits, logging configuration, nginx reverse proxy with caching/compression/security headers, non-root containers, environment variable management, automated backup/restore scripts, comprehensive deployment documentation]
