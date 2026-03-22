# Codebase Concerns

**Analysis Date:** 2026-03-21

## Tech Debt

**Sync-over-Async Pattern:**
- Issue: The code uses `.Result` on asynchronous tasks in a synchronous context (e.g., `movieinfo.GetInfoAsync(parsedData).Result`).
- Files: `LetterboxdToCinephilesChannel/RSSReader.cs`
- Impact: Potential deadlocks and inefficient thread usage.
- Fix approach: Refactor `Execute` and other methods to be fully `async` and use `await`.

**HttpClient Instantiation:**
- Issue: A new `HttpClient` is created for every request in `GetInfoAsync`.
- Files: `LetterboxdToCinephilesChannel/GetMovieInfo.cs`
- Impact: Risk of socket exhaustion under frequent polling.
- Fix approach: Use a single static `HttpClient` instance or `IHttpClientFactory`.

**Hardcoded Database Configuration:**
- Issue: Database path (`entries.db`) and table names are hardcoded as private static fields.
- Files: `LetterboxdToCinephilesChannel/RSSReader.cs`
- Impact: Limited flexibility; difficult to manage in different environments.
- Fix approach: Move these to environment variables or a configuration file.

## Security Considerations

**Docker Persistence:**
- Risk: The SQLite database `entries.db` is stored in the local filesystem inside the container. Without a volume mount, all state (processed entries) is lost on restart, leading to duplicate Telegram posts.
- Files: `Dockerfile`, `LetterboxdToCinephilesChannel/RSSReader.cs`
- Current mitigation: None detected.
- Recommendations: Define a volume path for the database and update the `Dockerfile` or deployment instructions.

**SQL Table Initialization:**
- Risk: Table name is interpolated directly into the `CREATE TABLE` SQL command.
- Files: `LetterboxdToCinephilesChannel/RSSReader.cs`
- Current mitigation: The table name is currently a hardcoded private field.
- Recommendations: Avoid interpolation in SQL commands; use constant strings or strictly validated identifiers.

## Performance Bottlenecks

**Infinite Loop with Thread.Sleep:**
- Problem: The main execution loop uses `Thread.Sleep(10 * 60 * 1000)`.
- Files: `LetterboxdToCinephilesChannel/RSSReader.cs`
- Cause: Synchronous polling implementation.
- Improvement path: Migrate to `Task.Delay` with a `CancellationToken` within an `async` loop, or implement as a `BackgroundService`.

## Fragile Areas

**RSS/HTML Parsing:**
- Files: `LetterboxdToCinephilesChannel/RSSReader.cs`
- Why fragile: Relies on specific Letterboxd RSS namespaces (`letterboxd:filmTitle`) and specific HTML structures in the description (e.g., `//img`, `//p[last()]`).
- Safe modification: Implement more defensive checks for null nodes and unexpected HTML content.
- Test coverage: None.

## Test Coverage Gaps

**Core Logic and Integration:**
- What's not tested: RSS parsing logic, HTML extraction, OMDB API mapping, and Telegram message preparation.
- Files: `LetterboxdToCinephilesChannel/RSSReader.cs`, `LetterboxdToCinephilesChannel/GetMovieInfo.cs`, `LetterboxdToCinephilesChannel/ChannelCalls.cs`
- Risk: External changes in Letterboxd feeds or OMDB API will cause silent failures or crashes.
- Priority: High

---

*Concerns audit: 2026-03-21*
