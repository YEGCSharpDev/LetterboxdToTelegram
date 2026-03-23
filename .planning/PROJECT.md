# Project: LetterboxdToTelegram

**Goal:** Complete lean and mean rewrite of the application with a .NET 10 upgrade, portable persistence, and OMDB integration.

## Context
Existing brownfield codebase: `LetterboxdToCinephilesChannel.sln`
Current mapping: See `.planning/codebase/`

## Key Objectives
- **Architecture**: Move from sync-heavy polling to a modern, async-first .NET worker/background service.
- **Version**: Upgrade to .NET 10.
- **Persistence**: Ensure portable SQLite database management for Docker environments.
- **Integrations**: Formalize TMDB API usage (replacing OMDb) for enhanced movie metadata and implement Error Reporting via Telegram.

## Milestones
- **Milestone 1**: Architectural Overhaul & Modernization (Completed)
- **Milestone 2**: CI/CD & Automated Image Distribution (In Progress)
  - Goal: Automate the Docker build and push process to Docker Hub using GitHub Actions.
  - Integration: Continuous deployment trigger on `main` branch.

## Workflow Configuration
- **Standard**: GSD-v1
- **Domain Research**: Required (Telegram/Letterboxd APIs)
