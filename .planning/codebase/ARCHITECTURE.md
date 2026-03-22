# Architecture

**Analysis Date:** 2024-05-24

## Pattern Overview

**Overall:** Service-Oriented Background Worker

**Key Characteristics:**
- **Synchronous Orchestration:** The `RSSReader` class orchestrates the entire flow from fetching to sending.
- **Stateless Logic with External Persistence:** Business logic is mostly stateless, relying on an SQLite database to maintain the state of processed items.
- **Environment-Driven Configuration:** Application behavior and secrets are managed entirely through environment variables.

## Layers

**Orchestration Layer:**
- Purpose: Coordinates the download, parsing, enrichment, and delivery of movie information.
- Location: `LetterboxdToCinephilesChannel/RSSReader.cs`
- Contains: `RSSReader.Execute()` loop, XML parsing logic, and database persistence logic.
- Depends on: `GetMovieInfo`, `ChannelCalls`, `TextMessage`, `Microsoft.Data.Sqlite`, `HtmlAgilityPack`
- Used by: `Program.cs`

**Enrichment Layer:**
- Purpose: Fetches additional metadata for movies from external APIs.
- Location: `LetterboxdToCinephilesChannel/GetMovieInfo.cs`
- Contains: `GetMovieInfo` class and the `MovieInfo` data model.
- Depends on: `Newtonsoft.Json`, `HttpClient`
- Used by: `RSSReader`

**Delivery Layer:**
- Purpose: Formats and sends the final message to the Telegram channel.
- Location: `LetterboxdToCinephilesChannel/ChannelCalls.cs`
- Contains: `ChannelCalls` class, Markdown formatting logic, and Telegram API interaction.
- Depends on: `Telegram.Bot`, `TextMessage`, `MovieInfo`
- Used by: `RSSReader`

**Data Model Layer:**
- Purpose: Defines the structures for internal data passing.
- Location: `LetterboxdToCinephilesChannel/TextMessage.cs` and `LetterboxdToCinephilesChannel/GetMovieInfo.cs` (nested `MovieInfo`)
- Contains: `TextMessage` class and `MovieInfo` class.
- Depends on: None
- Used by: All layers

## Data Flow

**RSS Processing Flow:**

1. **Fetch:** `RSSReader` downloads XML from Letterboxd RSS URLs defined in environment variables.
2. **Parse:** `RSSReader.ParseXml` extracts basic film data (Title, Year, Rating, Review) and uses `HtmlAgilityPack` to find the poster image.
3. **Deduplicate:** `RSSReader` checks if a hash of the entry already exists in the `entries.db` SQLite database.
4. **Enrich:** If new, `GetMovieInfo.GetInfoAsync` queries the OMDB API for additional details (IMDb Rating, Plot, Genre, etc.).
5. **Format & Send:** `ChannelCalls.SendPhotoAsync` constructs a MarkdownV2 caption and sends the image and caption to Telegram via `Telegram.Bot`.
6. **Persist:** Upon success, the entry hash is saved to `entries.db`.

**State Management:**
- **Local Persistence:** SQLite database (`entries.db`) stores `EntryContent` as a primary key to prevent duplicate posts.

## Key Abstractions

**RSSReader:**
- Purpose: The main engine of the application.
- Examples: `LetterboxdToCinephilesChannel/RSSReader.cs`
- Pattern: Orchestrator / Worker

**TextMessage:**
- Purpose: Intermediate data structure representing a Letterboxd entry.
- Examples: `LetterboxdToCinephilesChannel/TextMessage.cs`
- Pattern: Data Transfer Object (DTO)

**MovieInfo:**
- Purpose: Data structure representing enriched movie metadata from OMDB.
- Examples: `LetterboxdToCinephilesChannel/GetMovieInfo.cs`
- Pattern: Data Transfer Object (DTO)

## Entry Points

**Console Application:**
- Location: `LetterboxdToCinephilesChannel/Program.cs`
- Triggers: Application startup.
- Responsibilities: Instantiates `RSSReader` and calls `Execute()`.

## Error Handling

**Strategy:** Retry with Backoff

**Patterns:**
- **API Retries:** `GetMovieInfo` and `ChannelCalls` both implement `maxRetries` (3) for network calls.
- **Rate Limit Awareness:** `ChannelCalls.ParseRetryAfterTime` extracts wait times from Telegram's "Too Many Requests" errors.
- **Graceful Failure:** If enrichment fails, a placeholder `MovieInfo` is used instead of crashing the process.

## Cross-Cutting Concerns

**Logging:** Basic console logging (`Console.WriteLine`) is used throughout the application to track progress and errors.
**Validation:** Basic null checks and `string.IsNullOrWhiteSpace` checks are performed on API responses.
**Authentication:** Managed via environment variables (`CINEPHILE_TOKEN`, `API_KEY`) passed to API clients.

---

*Architecture analysis: 2024-05-24*
