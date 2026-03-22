# Validation Plan: Phase 1 (Foundation & Modernization)

## 1. Requirement Coverage Mapping

| Requirement | Description | Verification Method |
|-------------|-------------|---------------------|
| FR-1.4.1 | SQLite persistence | Integration Test: Save and retrieve movie ID |
| FR-1.4.2 | EF Core Migrations | CLI: `dotnet ef migrations list` & `Database.Migrate()` check |
| FR-1.4.3 | Docker Mountable | Manual/Script: Verify DB path is configurable via Env Var |
| FR-1.4.4 | History Seeding | Integration Test: Mock WTelegramClient to "find" existing IDs |
| NFR-2.1.1 | .NET 10 Worker | CLI: `dotnet --version` & Project File check |
| NFR-2.1.2 | HttpClient Resilience | Unit Test: Verify `AddStandardResilienceHandler` is called |
| NFR-2.1.3 | Graceful Shutdown | Manual/Script: Send SIGTERM and verify logs show "Shutting down..." |

## 2. Automated Test Strategy
- **Framework:** xUnit
- **Infrastructure Tests:** 
    - `DbContextTests`: Verify WAL mode is enabled (`PRAGMA journal_mode`).
    - `ConfigTests`: Verify `ValidateOnStart` throws when Env Vars are missing.
- **Integration Tests:**
    - `SeedingTests`: Verify `WTelegramClient` logic correctly identifies movie IDs in text.

## 3. Manual Verification Steps
1. **Env Var Validation:** Run app without `TELEGRAM_TOKEN` and ensure it fails immediately with a validation error.
2. **Graceful Shutdown:** Run app, press `Ctrl+C`, and verify cleanup logic executes.
3. **Database WAL:** Open `entries.db` with a SQLite browser and run `PRAGMA journal_mode;`.
