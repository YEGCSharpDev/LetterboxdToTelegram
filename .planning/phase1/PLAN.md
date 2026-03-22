---
phase: 01-foundation
plan: 01
type: execute
wave: 1
depends_on: []
files_modified: [
  "LetterboxdToCinephilesChannel/LetterboxdToCinephilesChannel.csproj",
  "LetterboxdToCinephilesChannel/Program.cs",
  "LetterboxdToCinephilesChannel/Worker.cs",
  "LetterboxdToCinephilesChannel/Domain/Entities/ProcessedMovie.cs",
  "LetterboxdToCinephilesChannel/Infrastructure/Data/AppDbContext.cs",
  "LetterboxdToCinephilesChannel/Infrastructure/Http/OmdbClient.cs",
  "LetterboxdToCinephilesChannel/Infrastructure/Http/RssClient.cs",
  "LetterboxdToCinephilesChannel/Infrastructure/Services/HistorySeedingService.cs",
  "LetterboxdToCinephilesChannel/Configuration/TelegramOptions.cs",
  "LetterboxdToCinephilesChannel/Configuration/OmdbOptions.cs",
  "LetterboxdToCinephilesChannel/Configuration/RssOptions.cs"
]
autonomous: true
requirements: [FR-1.4.1, FR-1.4.2, FR-1.4.3, FR-1.4.4, NFR-2.1.1, NFR-2.1.2, NFR-2.1.3, NFR-2.2.2, NFR-2.3.2]
user_setup:
  - service: telegram
    why: "MTProto API credentials for history seeding"
    env_vars:
      - name: TELEGRAM__APIID
        source: "https://my.telegram.org"
      - name: TELEGRAM__APIHASH
        source: "https://my.telegram.org"

must_haves:
  truths:
    - "Application starts as a .NET 10 Worker Service"
    - "Configuration fails fast if required environment variables are missing"
    - "Application handles graceful shutdown (SIGTERM/Ctrl+C) via StopAsync and CancellationTokens"
    - "SQLite database is initialized with WAL mode and correct schema"
    - "External API calls use resilient Typed Clients (Polly v8)"
    - "Telegram channel history is seeded into the database on first run"
  artifacts:
    - path: "LetterboxdToCinephilesChannel/Program.cs"
      provides: "Modern Host configuration with DI and validation"
    - path: "LetterboxdToCinephilesChannel/Infrastructure/Data/AppDbContext.cs"
      provides: "EF Core 10 Context with SQLite WAL configuration"
    - path: "LetterboxdToCinephilesChannel/Infrastructure/Http/OmdbClient.cs"
      provides: "Resilient HttpClient for OMDb"
    - path: "LetterboxdToCinephilesChannel/Infrastructure/Services/HistorySeedingService.cs"
      provides: "MTProto-based history import logic"
  key_links:
    - from: "LetterboxdToCinephilesChannel/Program.cs"
      to: "LetterboxdToCinephilesChannel/Infrastructure/Data/AppDbContext.cs"
      via: "AddDbContext and Database.Migrate()"
      pattern: "AddDbContext"
    - from: "LetterboxdToCinephilesChannel/Program.cs"
      to: "Infrastructure/Http/OmdbClient.cs"
      via: "AddHttpClient().AddStandardResilienceHandler()"
      pattern: "AddStandardResilienceHandler"
---

<objective>
Modernize the LetterboxdToTelegram application by initializing a .NET 10 Worker Service foundation, setting up resilient persistence with SQLite (WAL mode), and implementing history seeding from Telegram via MTProto.

Purpose: Establish a robust, scalable, and portable foundation for the core logic.
Output: A functional Worker Service with validated config, optimized DB access, resilient external integration layers, and graceful shutdown handling.
</objective>

<execution_context>
@C:/Users/shnkr/.gemini/get-shit-done/workflows/execute-plan.md
@C:/Users/shnkr/.gemini/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/ROADMAP.md
@.planning/STATE.md
@.planning/phase1/RESEARCH.md
@.planning/phase1/VALIDATION.md
@LetterboxdToCinephilesChannel/LetterboxdToCinephilesChannel.csproj
@LetterboxdToCinephilesChannel/Program.cs
</context>

<tasks>

<task type="auto">
  <name>Task 1: Modernize .NET SDK and Project Structure</name>
  <files>
    LetterboxdToCinephilesChannel/LetterboxdToCinephilesChannel.csproj,
    LetterboxdToCinephilesChannel/Program.cs,
    LetterboxdToCinephilesChannel/Worker.cs,
    LetterboxdToCinephilesChannel/Configuration/TelegramOptions.cs,
    LetterboxdToCinephilesChannel/Configuration/OmdbOptions.cs,
    LetterboxdToCinephilesChannel/Configuration/RssOptions.cs
  </files>
  <action>
    1. Update `LetterboxdToCinephilesChannel.csproj`:
       - Set `TargetFramework` to `net10.0`.
       - Change `Sdk` to `Microsoft.NET.Sdk.Worker`.
       - Add NuGet packages: `Microsoft.Extensions.Hosting` (10.0.0), `Microsoft.Extensions.Options.DataAnnotations` (10.0.0), `Microsoft.Extensions.Http.Resilience` (10.0.0).
    2. Organize directory structure:
       - Create `Domain/Entities`, `Infrastructure/Data`, `Infrastructure/Http`, `Infrastructure/Services`, and `Configuration` folders.
    3. Implement Options Classes with DataAnnotations:
       - `TelegramOptions`: `BotToken`, `ChannelId`, `ApiId`, `ApiHash`.
       - `OmdbOptions`: `ApiKey`, `BaseUrl`.
       - `RssOptions`: `FeedUrl`.
    4. Rewrite `Program.cs`:
       - Use `Host.CreateApplicationBuilder(args)`.
       - Register and validate options with `.ValidateOnStart()`.
       - Register `Worker` as a hosted service.
    5. Implement `Worker.cs` with Graceful Shutdown:
       - Inherit from `BackgroundService`.
       - Override `StopAsync` to handle cleanup.
       - Use `CancellationToken` from `ExecuteAsync` in all async calls (e.g., `Task.Delay`).
  </action>
  <verify>
    <automated>dotnet build LetterboxdToCinephilesChannel/LetterboxdToCinephilesChannel.csproj && dotnet run --project LetterboxdToCinephilesChannel/LetterboxdToCinephilesChannel.csproj -- --dry-run || echo "Missing configuration (EXPECTED)"</automated>
  </verify>
  <done>Project is upgraded to .NET 10, uses Worker SDK, validates configuration on startup, and supports graceful shutdown.</done>
</task>

<task type="auto">
  <name>Task 2: Setup Persistence with EF Core 10 and SQLite (WAL)</name>
  <files>
    LetterboxdToCinephilesChannel/LetterboxdToCinephilesChannel.csproj,
    LetterboxdToCinephilesChannel/Domain/Entities/ProcessedMovie.cs,
    LetterboxdToCinephilesChannel/Infrastructure/Data/AppDbContext.cs,
    LetterboxdToCinephilesChannel/Program.cs
  </files>
  <action>
    1. Add NuGet packages: `Microsoft.EntityFrameworkCore.Sqlite` (10.0.0), `Microsoft.EntityFrameworkCore.Design` (10.0.0).
    2. Create `ProcessedMovie` entity in `Domain/Entities/`.
    3. Implement `AppDbContext` in `Infrastructure/Data/`:
       - Configure SQLite connection.
       - Implement automatic migration logic in `Program.cs`.
       - **CRITICAL:** Enable WAL mode via `PRAGMA journal_mode=WAL;` on connection open or in `OnConfiguring`.
    4. Generate initial migration: `dotnet ef migrations add InitialCreate`.
  </action>
  <verify>
    <automated>dotnet build LetterboxdToCinephilesChannel/LetterboxdToCinephilesChannel.csproj && dotnet run --project LetterboxdToCinephilesChannel/LetterboxdToCinephilesChannel.csproj -- --db-check-wal</automated>
  </verify>
  <done>EF Core is configured with SQLite, migrations are applied on start, and WAL mode is active.</done>
</task>

<task type="auto">
  <name>Task 3: Implement Resilient Clients and History Seeding</name>
  <files>
    LetterboxdToCinephilesChannel/LetterboxdToCinephilesChannel.csproj,
    LetterboxdToCinephilesChannel/Infrastructure/Http/OmdbClient.cs,
    LetterboxdToCinephilesChannel/Infrastructure/Http/RssClient.cs,
    LetterboxdToCinephilesChannel/Infrastructure/Services/HistorySeedingService.cs,
    LetterboxdToCinephilesChannel/Program.cs
  </files>
  <action>
    1. Add `WTelegramClient` (4.1.0) package.
    2. Implement `OmdbClient` and `RssClient` as Typed HttpClients:
       - Use `AddStandardResilienceHandler()` in `Program.cs`.
    3. Implement `HistorySeedingService` using `WTelegramClient`:
       - Fetch channel history using MTProto (ApiId/ApiHash).
       - Parse legacy message formats for Letterboxd IDs.
       - Seed `ProcessedMovies` table.
    4. Integrate seeding into `Program.cs` startup (run only if DB is empty).
  </action>
  <verify>
    <automated>dotnet build LetterboxdToCinephilesChannel/LetterboxdToCinephilesChannel.csproj && grep -q "AddStandardResilienceHandler" LetterboxdToCinephilesChannel/Program.cs</automated>
  </verify>
  <done>External clients are resilient, and history seeding logic is ready for execution.</done>
</task>

</tasks>

<verification>
1. **Config Check:** Run app without `TELEGRAM__BOTTOKEN` env var; confirm it fails to start.
2. **Persistence Check:** Run app; verify `movies.db` is created and `PRAGMA journal_mode;` returns `wal`.
3. **Graceful Shutdown:** Verify `Worker.cs` overrides `StopAsync` and uses `CancellationToken`.
4. **Integration Check:** Verify Typed Clients are registered in DI with resilience handlers.
5. **Seeding Check:** (Manual) Run with valid API credentials; check logs for history processing.
</verification>

<success_criteria>
- Project targets `net10.0`.
- All requested folders (`Domain`, `Infrastructure`, `Configuration`) populated.
- SQLite database operational with WAL mode.
- Application handles graceful shutdown signals correctly.
- Resilience handlers active for all external HTTP calls.
- MTProto-based history seeding logic implemented and integrated.
</success_criteria>

<output>
After completion, create `.planning/phases/01-foundation/01-01-SUMMARY.md`
</output>
