---
phase: 02-core-logic
plan: 01
type: execute
wave: 1
depends_on: ["01-01"]
files_modified: [
  "LetterboxdToCinephilesChannel/Infrastructure/Http/RssClient.cs",
  "LetterboxdToCinephilesChannel/Infrastructure/Http/TmdbClient.cs",
  "LetterboxdToCinephilesChannel/Infrastructure/Services/TelegramService.cs",
  "LetterboxdToCinephilesChannel/Infrastructure/Services/ErrorReportingService.cs",
  "LetterboxdToCinephilesChannel/Worker.cs",
  "LetterboxdToCinephilesChannel/Infrastructure/Data/AppDbContext.cs",
  "LetterboxdToCinephilesChannel.Tests/RssParserTests.cs",
  "LetterboxdToCinephilesChannel.Tests/TmdbMappingTests.cs"
]
autonomous: true
requirements: [FR-1.1.1, FR-1.1.2, FR-1.1.3, FR-1.1.4, FR-1.2.1, FR-1.2.2, FR-1.2.3, FR-1.3.1, FR-1.3.2, FR-1.3.3, FR-1.3.4, FR-1.4.1, FR-1.5.1]

must_haves:
  truths:
    - "RSS Client trims leading whitespace and handles 'letterboxd' namespaces (FR-1.1.2)."
    - "RSS Client extracts high-res posters (1000x1500) and liked status (FR-1.1.3, FR-1.1.4)."
    - "TMDB Client uses Bearer Token auth and fetches movie details including genres and original language (FR-1.2.1, FR-1.2.2)."
    - "Worker Service polls at intervals defined by WORKER__POLLINGINTERVALMINUTES (FR-1.1.1)."
    - "Telegram messages use rich HTML formatting with genres, ratings, and posters (FR-1.3.1, FR-1.3.2)."
    - "Critical failures are reported to ERROR_TELEGRAM_CHAT_ID (FR-1.5.1)."
  artifacts:
    - path: "LetterboxdToCinephilesChannel/Infrastructure/Http/TmdbClient.cs"
      provides: "TMDB movie search and detail enrichment with Bearer Token"
    - path: "LetterboxdToCinephilesChannel/Infrastructure/Services/TelegramService.cs"
      provides: "Rich HTML card delivery with genres and ratings"
    - path: "LetterboxdToCinephilesChannel/Infrastructure/Services/ErrorReportingService.cs"
      provides: "Error notification to dedicated Telegram chat"
  key_links:
    - from: "Worker.cs"
      to: "TmdbClient.cs"
      via: "Movie metadata enrichment"
    - from: "Worker.cs"
      to: "ErrorReportingService.cs"
      via: "Failure notification"
---

<objective>
Implement the core processing engine using TMDB for enrichment: robust RSS parsing, resilient TMDB integration (Bearer Token), rich HTML Telegram delivery (genres, ratings, language), parameterized polling, and automated error reporting.
</objective>

<execution_context>
@C:/Users/shnkr/.gemini/get-shit-done/workflows/execute-plan.md
</execution_context>

<context>
@.planning/ROADMAP.md
@.planning/STATE.md
@.planning/REQUIREMENTS.md
@.planning/research/TMDB_API.md
@.planning/research/TELEGRAM_API.md
@LetterboxdToCinephilesChannel/Infrastructure/Http/RssClient.cs
</context>

<tasks>

<task type="auto" tdd="true">
  <name>Task 1: Implement Robust RSS and TMDB Clients with Unit Tests</name>
  <files>
    LetterboxdToCinephilesChannel/Infrastructure/Http/RssClient.cs,
    LetterboxdToCinephilesChannel/Infrastructure/Http/TmdbClient.cs,
    LetterboxdToCinephilesChannel.Tests/RssParserTests.cs,
    LetterboxdToCinephilesChannel.Tests/TmdbMappingTests.cs
  </files>
  <behavior>
    - RssClient must trim leading/trailing whitespace from the raw RSS feed string (FR-1.1.2).
    - RssClient must handle 'https://letterboxd.com/' namespace for lb tags.
    - RssClient must parse 'letterboxd:liked' tag for Liked status (FR-1.1.4).
    - RssClient must extract and upgrade poster resolution to 1000x1500 (FR-1.1.3).
    - TmdbClient must use Bearer Token authentication (FR-1.2.2).
    - TmdbClient must fetch movie details (genres, original language, ratings) after title search (FR-1.2.1).
  </behavior>
  <action>
    1. Refactor `RssClient`: Implement whitespace trimming, namespace-aware parsing, and poster resolution enhancement.
    2. Create `TmdbClient` (replacing OmdbClient):
       - Use `HttpClient` with Bearer Token header (from research/TMDB_API.md).
       - Implement `search/movie` lookup by title/year.
       - Implement `movie/{id}` fetch for genres and details.
    3. Implement unit tests in `RssParserTests.cs` and `TmdbMappingTests.cs`.
  </action>
  <verify>
    <automated>dotnet test LetterboxdToCinephilesChannel.Tests</automated>
  </verify>
  <done>RSS and TMDB clients are robust, tested, and handle high-res posters and rich metadata.</done>
</task>

<task type="auto">
  <name>Task 2: Implement Orchestration, Rich Telegram Delivery, and Error Reporting</name>
  <files>
    LetterboxdToCinephilesChannel/Worker.cs,
    LetterboxdToCinephilesChannel/Infrastructure/Services/TelegramService.cs,
    LetterboxdToCinephilesChannel/Infrastructure/Services/ErrorReportingService.cs
  </files>
  <action>
    1. Create `ErrorReportingService`:
       - Send failure messages to `ERROR_TELEGRAM_CHAT_ID` (FR-1.5.1).
    2. Implement `TelegramService` using rich HTML format:
       - Format: <b>Title (Year)</b>, Genres (hashtags), TMDB Rating, Original Language, Plot (truncated).
       - Use `ParseMode.Html` and include interactive buttons for IMDb/Letterboxd.
    3. Update `Worker.cs`:
       - Parameterize interval using `WORKER__POLLINGINTERVALMINUTES` (default 10) (FR-1.1.1).
       - Wrap loop in try-catch; call `ErrorReportingService` on critical failures.
       - Implement Fetch -> Filter -> TMDB Enrich -> Send -> Save loop.
  </action>
  <verify>
    <automated>dotnet build LetterboxdToCinephilesChannel && grep -q "POLLINGINTERVALMINUTES" LetterboxdToCinephilesChannel/Worker.cs</automated>
  </verify>
  <done>Core loop is operational with rich HTML delivery, parameterized polling, and automated error reporting.</done>
</task>

</tasks>

<verification>
1. **Unit Tests:** All parsing and TMDB mapping tests pass.
2. **Rich HTML:** Verify Telegram messages include hashtags for genres and correct HTML tags.
3. **Error Reporting:** Trigger a mock failure and verify notification to the error chat.
4. **Polling Interval:** Verify the worker honors the environment variable for timing.
</verification>

<success_criteria>
- RssClient extracts 1000x1500 posters and 'Liked' status.
- TmdbClient fetches genres, language, and ratings via Bearer Token.
- Telegram cards use HTML mode with rich formatting and interactive buttons.
- Worker uses parameterized polling interval and reports errors to a dedicated chat.
</success_criteria>
