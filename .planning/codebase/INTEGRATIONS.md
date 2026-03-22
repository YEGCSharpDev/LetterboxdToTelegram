# External Integrations

**Analysis Date:** 2024-10-18

## APIs & External Services

**Letterboxd RSS:**
- Letterboxd RSS Feeds - Source of film reviews and ratings.
  - SDK/Client: `System.Xml.XmlDocument` for parsing XML content.
  - Auth: None (public RSS feed).
  - Implementation: `LetterboxdToCinephilesChannel/RSSReader.cs`

**Telegram Bot API:**
- Telegram - Target for posting film reviews to a channel.
  - SDK/Client: `Telegram.Bot` (NuGet package).
  - Auth: `CINEPHILE_TOKEN` (Bot Token) and `CHAT_ID` (Target Chat/Channel).
  - Implementation: `LetterboxdToCinephilesChannel/ChannelCalls.cs`

**OMDB API:**
- OMDB (The Open Movie Database) - Used to retrieve metadata for film titles (Genre, IMDB Rating, Plot, etc.).
  - SDK/Client: `System.Net.Http.HttpClient` for raw API calls.
  - Auth: `API_KEY` env var.
  - Implementation: `LetterboxdToCinephilesChannel/GetMovieInfo.cs`

## Data Storage

**Databases:**
- SQLite - Local persistent storage to keep track of processed/sent RSS entries.
  - Connection: `Data Source=entries.db`
  - Client: `Microsoft.Data.Sqlite`
  - Implementation: `LetterboxdToCinephilesChannel/RSSReader.cs`

**File Storage:**
- Local filesystem only - For the SQLite database (`entries.db`).

**Caching:**
- None detected - The app relies on the SQLite database for basic state persistence.

## Authentication & Identity

**Auth Provider:**
- Custom (Token-based) - External services use direct tokens or API keys provided via environment variables.
  - Implementation: `Environment.GetEnvironmentVariable()` in various classes.

## Monitoring & Observability

**Error Tracking:**
- None - Errors are written to the console output.

**Logs:**
- Console Logging - Using `Console.WriteLine()` throughout the application.

## CI/CD & Deployment

**Hosting:**
- Docker - Containerized application meant for hosting on Docker-compatible platforms.

**CI Pipeline:**
- None detected - No GitHub Actions or other CI configs found.

## Environment Configuration

**Required env vars:**
- `CINEPHILE_TOKEN`: Telegram bot API token.
- `CHAT_ID`: Telegram chat/channel ID for message delivery.
- `RSS_URLS`: Comma-separated list of Letterboxd RSS feed URLs.
- `API_KEY`: OMDB API key for film metadata retrieval.
- `USERNAME_CREATOR_MAPPING`: Comma-separated `username:creatorName` mapping for multi-user support.

**Secrets location:**
- Not committed - Passed via runtime environment variables.

## Webhooks & Callbacks

**Incoming:**
- None - The application polls the Letterboxd RSS feeds.

**Outgoing:**
- Telegram Bot API: Messages sent via HTTP requests in `LetterboxdToCinephilesChannel/ChannelCalls.cs`.

---

*Integration audit: 2024-10-18*
