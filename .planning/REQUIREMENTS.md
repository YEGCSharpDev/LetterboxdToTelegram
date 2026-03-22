# Requirements: LetterboxdToTelegram (Rewrite)

**Project Goal:** Complete lean and mean rewrite of the application with a .NET version upgrade, portable persistence, and OMDB integration.

## 1. Functional Requirements

### 1.1 RSS Polling & Parsing
- **FR-1.1.1:** Poll the Letterboxd RSS feed every 10 minutes.
- **FR-1.1.2:** Handle XML namespace issues (e.g., `letterboxd:filmTitle`) and leading whitespace in the feed.
- **FR-1.1.3:** Extract film title, year, rating, and high-resolution poster URL from the feed.
- **FR-1.1.4:** Detect "Liked", "Rewatch", and "Heart" status from custom XML tags.

### 1.2 Metadata Enrichment (OMDb)
- **FR-1.2.1:** Query OMDb API for additional metadata (Plot, Genres, IMDB Rating, Runtime).
- **FR-1.2.2:** Handle "N/A" strings returned by OMDb gracefully.
- **FR-1.2.3:** Implement retries using Polly for transient API failures.

### 1.3 Telegram Delivery
- **FR-1.3.1:** Send rich movie cards using `Telegram.Bot` library.
- **FR-1.3.2:** Use HTML parse mode for robust message formatting.
- **FR-1.3.3:** Include high-resolution posters and interactive buttons (IMDb link, Letterboxd link).
- **FR-1.3.4:** Handle caption length limits (truncate plots if > 1024 characters).

### 1.4 Persistence & State
- **FR-1.4.1:** Store processed movie IDs in a portable SQLite database.
- **FR-1.4.2:** Use EF Core with automatic migrations on startup.
- **FR-1.4.3:** Ensure database file is easily mountable via Docker volumes.

## 2. Non-Functional Requirements

### 2.1 Performance & Reliability
- **NFR-2.1.1:** Use .NET 8/9 `BackgroundService` (Worker Service) for non-blocking execution.
- **NFR-2.1.2:** Use `IHttpClientFactory` with Typed Clients to prevent socket exhaustion.
- **NFR-2.1.3:** Implement graceful shutdown handling for the worker.

### 2.2 Maintainability
- **NFR-2.2.1:** Clean Architecture principles (Separation of concerns between RSS, OMDb, and Telegram).
- **NFR-2.2.2:** Structured logging using `ILogger`.

### 2.3 Deployment
- **NFR-2.3.1:** Multi-stage Dockerfile for lean runtime images.
- **NFR-2.3.2:** Configuration via Environment Variables (Secrets, API Keys, Feed URLs).

## 3. Success Criteria
- [ ] Application successfully polls RSS and parses data correctly.
- [ ] OMDb metadata is fetched and merged without crashing on missing data.
- [ ] Telegram messages are sent with posters and interactive links.
- [ ] Duplicate posts are prevented via SQLite persistence.
- [ ] Application runs stably in a Docker container with persistent state.
