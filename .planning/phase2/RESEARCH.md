# Phase 2: Core Logic & Integration - Research

**Researched:** 2024-05-14
**Domain:** RSS Parsing, OMDb Enrichment, Worker Service Orchestration, EF Core Persistence
**Confidence:** HIGH

## Summary

Phase 2 focuses on the core functional engine of LetterboxdToTelegram. The research confirms that .NET 10 Worker Services should use `PeriodicTimer` for precise polling intervals (10 minutes as per FR-1.1.1). Robust RSS parsing requires handling the `https://letterboxd.com/` XML namespace and extracting poster URLs from the embedded HTML in the `description` field using `HtmlAgilityPack`.

OMDb API returns `"N/A"` for missing data, which requires a custom `JsonConverter` or manual mapping to prevent deserialization errors when using nullable types. Resilience is handled natively in .NET 10 via `Microsoft.Extensions.Http.Resilience` (Polly v8), providing standard retry and timeout policies. Duplicate prevention is best achieved through set-based ID checks with EF Core to avoid N+1 query patterns.

**Primary recommendation:** Use `PeriodicTimer` within `BackgroundService` to orchestrate the "Fetch -> Enrich -> Filter -> Notify" loop, processing items in chronological order (oldest first).

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| .NET 10 SDK | 10.0.0-* | Runtime & Framework | Latest LTS-track (in preview/dev) for modern features. |
| HtmlAgilityPack | 1.11.57 | HTML Parsing | Industry standard for parsing non-well-formed HTML in RSS descriptions. |
| System.Xml.Linq | 10.0.0-* | XML Parsing | Built-in high-performance LINQ-to-XML for RSS structure. |
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.0-* | Persistence | Lightweight, portable database for duplicate prevention. |
| Telegram.Bot | 19.0.0 | Telegram Integration | Most widely used .NET wrapper for Telegram Bot API. |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|--------------|
| Microsoft.Extensions.Http.Resilience | 10.0.0-* | Resilience | Native .NET 8+ Polly v8 wrapper for HTTP clients. |
| System.Text.Json | 10.0.0-* | JSON Serialization | High-performance JSON handling for OMDb API. |

**Installation:**
Already configured in `.csproj`.

## Architecture Patterns

### Recommended Project Structure
```
LetterboxdToCinephilesChannel/
├── Infrastructure/
│   ├── Http/
│   │   ├── RssClient.cs      # RSS fetching and parsing logic
│   │   └── OmdbClient.cs     # OMDb enrichment logic
│   ├── Services/
│   │   └── TelegramService.cs # Telegram messaging logic
│   └── Data/
│       └── AppDbContext.cs   # EF Core context
└── Worker.cs                 # Main orchestration loop
```

### Pattern 1: Periodic Worker with Scoped Services
**What:** Using `PeriodicTimer` to run logic every 10 minutes and manual scope creation for DB operations.
**When to use:** In `BackgroundService.ExecuteAsync`.
**Example:**
```typescript
// Source: Microsoft Official Docs / Community Best Practices 2025
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    using PeriodicTimer timer = new(TimeSpan.FromMinutes(10));
    while (await timer.WaitForNextTickAsync(stoppingToken))
    {
        using var scope = _scopeFactory.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<CoreProcessor>();
        await processor.ProcessUpdatesAsync(stoppingToken);
    }
}
```

### Anti-Patterns to Avoid
- **N+1 Checks:** Checking each movie ID against the database one-by-one. Use `Where(x => incomingIds.Contains(x.Id))` instead.
- **Overlapping Executions:** Using `System.Threading.Timer` which can fire before the previous execution finishes. `PeriodicTimer` naturally avoids this if work is awaited.
- **Ignoring "N/A":** Direct mapping of OMDb strings to numbers will throw. Always check for `"N/A"`.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Retry Logic | Custom while-loops | `AddStandardResilienceHandler` | Handles backoff, jitter, and circuit breaking natively. |
| XML Namespaces | String replacement | `XNamespace` | Correctly handles scoped XML elements (e.g., `letterboxd:filmTitle`). |
| HTML Parsing | Regex | `HtmlAgilityPack` | Correctly handles malformed HTML and CDATA inside RSS. |

## Common Pitfalls

### Pitfall 1: Letterboxd Namespace Blindness
**What goes wrong:** `XDocument.Element("filmTitle")` returns null because it lacks the namespace.
**How to avoid:** Define `XNamespace lb = "https://letterboxd.com/"` and use `lb + "filmTitle"`.

### Pitfall 2: OMDb "N/A" values
**What goes wrong:** Trying to parse `"N/A"` into a `double?` for ratings.
**How to avoid:** Custom `JsonConverter` or a helper method to return `null` if the string is `"N/A"`.

### Pitfall 3: Telegram Caption Limits
**What goes wrong:** Plots longer than 1024 characters cause `400 Bad Request`.
**How to avoid:** Use `plot.Length > 1024 ? plot[..1021] + "..." : plot`.

## Code Examples

### RSS Parsing with Namespaces & Poster Extraction
```csharp
// Source: Community verified pattern for Letterboxd RSS
XNamespace lb = "https://letterboxd.com/";
var items = doc.Descendants("item").Select(item => {
    var desc = item.Element("description")?.Value;
    var html = new HtmlDocument();
    html.LoadHtml(desc);
    var posterUrl = html.DocumentNode.SelectSingleNode("//img")?.GetAttributeValue("src", null);
    
    return new {
        Title = item.Element(lb + "filmTitle")?.Value,
        Year = item.Element(lb + "filmYear")?.Value,
        Poster = posterUrl
    };
});
```

### OMDb Resilience Configuration
```csharp
// Source: Microsoft.Extensions.Http.Resilience Docs
builder.Services.AddHttpClient<OmdbClient>()
    .AddStandardResilienceHandler(options => {
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.Delay = TimeSpan.FromSeconds(2);
    });
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `Task.Delay` | `PeriodicTimer` | .NET 6 | Eliminates drift in long-running services. |
| Polly Policies | Resilience Handlers | .NET 8 | Better performance and built-in telemetry. |
| XPath | LINQ to XML | .NET 3.5+ | More readable and type-safe XML traversal. |

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.0+ |
| Assertion | FluentAssertions 6.12.0+ |
| Mocking | NSubstitute 5.1.0+ |
| Quick run command | `dotnet test --filter Category=Unit` |
| Full suite command | `dotnet test` |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| FR-1.1.2 | Handle namespaces | unit | `dotnet test --filter "FullyQualifiedName~RssParserTests"` | ❌ Wave 0 |
| FR-1.2.2 | Handle "N/A" | unit | `dotnet test --filter "FullyQualifiedName~OmdbMappingTests"` | ❌ Wave 0 |
| FR-1.4.1 | Duplicate prevention | integration | `dotnet test --filter "FullyQualifiedName~PersistenceTests"` | ❌ Wave 0 |

### Wave 0 Gaps
- [ ] Create `LetterboxdToCinephilesChannel.Tests` project.
- [ ] Add `RssParserTests.cs` to verify namespace and HTML poster extraction.
- [ ] Add `OmdbMappingTests.cs` to verify "N/A" handling.
- [ ] Setup `TestDatabaseFixture.cs` for SQLite integration tests.

## Sources

### Primary (HIGH confidence)
- [.NET Official Docs] - `PeriodicTimer` and `BackgroundService` implementation.
- [Microsoft.Extensions.Http.Resilience] - Standard resilience pipeline configuration.
- [Letterboxd RSS Feed] - Observed schema and namespace definitions (`https://letterboxd.com/`).

### Secondary (MEDIUM confidence)
- [HtmlAgilityPack Docs] - Standard node selection patterns.
- [OMDb API Docs] - Documentation of `"N/A"` responses for missing data.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - Libraries are well-established.
- Architecture: HIGH - Follows .NET Worker Service best practices.
- Pitfalls: HIGH - Based on common community issues with OMDb and Letterboxd.

**Research date:** 2024-05-14
**Valid until:** 2025-05-14 (Stable domain)
