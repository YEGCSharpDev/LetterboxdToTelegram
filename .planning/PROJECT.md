# Project: LetterboxdToTelegram

**Goal:** Complete lean and mean rewrite of the application with a .NET version upgrade, portable persistence, and OMDB integration.

## Context
Existing brownfield codebase: `LetterboxdToCinephilesChannel.sln`
Current mapping: See `.planning/codebase/`

## Key Objectives
- **Architecture**: Move from sync-heavy polling to a modern, async-first .NET worker/background service.
- **Version**: Upgrade to latest .NET (8.0 or 9.0).
- **Persistence**: Ensure portable SQLite database management for Docker environments.
- **Integrations**: Formalize OMDB API usage for enhanced movie metadata.

## Workflow Configuration
- **Standard**: GSD-v1
- **Domain Research**: Required (Telegram/Letterboxd APIs)
