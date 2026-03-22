# Phase 1: Foundation & Modernization - Research

**Researched:** 2026-03-21
**Domain:** .NET 10 Worker Service, EF Core SQLite, WTelegramClient, Configuration
**Confidence:** HIGH

## Summary
Phase 1 establishes the .NET 10 foundation for the LetterboxdToTelegram rewrite. The architecture will center on a `BackgroundService` using `IHttpClientFactory` with the new `StandardResilienceHandler` for all external integrations (OMDb, RSS, Telegram). Persistence via EF Core 10 on SQLite is optimized with WAL mode to prevent locking issues in Docker environments.

**Primary recommendation:** Use the **Options Pattern** with `ValidateOnStart()` and `AddStandardResilienceHandler()` to ensure a robust, fail-fast application.

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Microsoft.Extensions.Hosting | 10.0.0 | Worker Service host | Native .NET 10 background service support. |
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.0 | Persistence | Portable, self-contained database for Docker. |
| Microsoft.Extensions.Http.Resilience | 10.0.0 | Resilience | Provides `AddStandardResilienceHandler` (Polly v8). |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| WTelegramClient | 4.1.0+ | MTProto Client | History seeding from Telegram channels. |
| System.Text.Json | 10.0.0 | JSON Parsing | High-performance native parsing. |

## Architecture Patterns

### Recommended Project Structure
```
src/
├── Application/      # Business logic, Interfaces
├── Domain/           # Entities (Movie, ProcessedUpdate)
├── Infrastructure/   # EF Core DbContext, Typed Clients
└── Worker/           # BackgroundService implementation
```

### Pattern 1: Typed Clients with Resilience
**What:** Encapsulate API calls in specific classes injected with `HttpClient`.
**When to use:** For OMDb, Telegram Bot API, and RSS fetching.
**Example:**
```csharp
builder.Services.AddHttpClient<IOmdbClient, OmdbClient>(client => {
    client.BaseAddress = new Uri("http://www.omdbapi.com/");
})
.AddStandardResilienceHandler(); // Handles retries, timeouts, circuit breaking
```

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Resilience | Custom try-catch loops | `StandardResilienceHandler` | Handles timeouts, retries, and circuit breaking correctly via Polly v8. |
| DB Migrations | Custom SQL scripts | EF Core Migrations | Integrated lifecycle management and schema safety. |
| Config Validation | Manual null checks | `ValidateOnStart()` | Fails at startup if environment variables are missing. |

## Common Pitfalls

### Pitfall 1: SQLite Concurrency
**What goes wrong:** "Database is locked" errors during simultaneous read/write operations.
**How to avoid:** Enable **WAL (Write-Ahead Logging)** mode.
**Prevention:** Include `PRAGMA journal_mode=WAL;` in the startup logic.

### Pitfall 2: Telegram Rate Limits (MTProto)
**What goes wrong:** `FloodWait` exceptions when fetching history for seeding.
**How to avoid:** Implement 1-second delays between pagination chunks when using `Messages_GetHistory`.

## Code Examples

### .NET 10 Configuration with Validation
```csharp
// Program.cs
builder.Services.AddOptions<TelegramOptions>()
    .BindConfiguration("Telegram")
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

### WTelegramClient History Fetching
```csharp
// Use Messages_GetHistory for seeding
var history = await client.Messages_GetHistory(channel, limit: 100);
foreach (var msgBase in history.Messages) {
    if (msgBase is Message msg) {
        // Parse message for existing movies
    }
}
```

## Validation Architecture
- **Framework:** xUnit
- **Test Types:** Unit tests for RSS Parsing; Integration tests for SQLite persistence.
- **Mocking:** Use `MockHttpMessageHandler` to test Typed Clients without hitting real APIs.
