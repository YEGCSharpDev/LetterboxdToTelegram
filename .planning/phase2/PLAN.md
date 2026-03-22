---
phase: 02-core-logic
plan: 01
type: execute
wave: 1
depends_on: ["01-01"]
files_modified: [
  "LetterboxdToCinephilesChannel/Infrastructure/Http/RssClient.cs",
  "LetterboxdToCinephilesChannel/Infrastructure/Http/OmdbClient.cs",
  "LetterboxdToCinephilesChannel/Infrastructure/Services/TelegramService.cs",
  "LetterboxdToCinephilesChannel/Worker.cs",
  "LetterboxdToCinephilesChannel/Infrastructure/Data/AppDbContext.cs",
  "LetterboxdToCinephilesChannel.Tests/RssParserTests.cs",
  "LetterboxdToCinephilesChannel.Tests/OmdbMappingTests.cs"
]
autonomous: true
requirements: [FR-1.1.1, FR-1.1.2, FR-1.1.3, FR-1.1.4, FR-1.2.1, FR-1.2.2, FR-1.3.1, FR-1.3.2, FR-1.4.1]

must_haves:
  truths:
    - "RSS Client trims leading whitespace and handles 'letterboxd' namespaces (FR-1.1.2)."
    - "RSS Client extracts high-res posters (1000x1500) and liked status (FR-1.1.3, FR-1.1.4)."
    - "OMDb Client handles 'N/A' strings by mapping to nullable types (FR-1.2.2)."
    - "Worker Service polls every 10 minutes using PeriodicTimer (FR-1.1.1)."
    - "Telegram messages use rich HTML formatting and include posters (FR-1.3.1, FR-1.3.2)."
  artifacts:
    - path: "LetterboxdToCinephilesChannel/Infrastructure/Http/RssClient.cs"
      provides: "Namespace-aware XML parsing, whitespace trimming, and poster resolution enhancement"
    - path: "LetterboxdToCinephilesChannel/Infrastructure/Http/OmdbClient.cs"
      provides: "Resilient OMDb enrichment with N/A handling"
    - path: "LetterboxdToCinephilesChannel/Infrastructure/Services/TelegramService.cs"
      provides: "Rich card delivery with HTML parse mode"
  key_links:
    - from: "Worker.cs"
      to: "AppDbContext.cs"
      via: "Set-based duplicate check (Where-In)"
    - from: "Worker.cs"
      to: "TelegramService.cs"
      via: "Movie card delivery"
    - from: "RssClient.cs"
      to: "HtmlAgilityPack"
      via: "Poster extraction from description CDATA"
---

<objective>
Implement the core processing engine: robust RSS parsing with whitespace trimming and high-res posters, resilient OMDb enrichment with 'N/A' handling, set-based duplicate prevention, and rich Telegram delivery within a 10-minute polling loop.
</objective>

<execution_context>
@C:/Users/shnkr/.gemini/get-shit-done/workflows/execute-plan.md
</execution_context>

<context>
@.planning/ROADMAP.md
@.planning/STATE.md
@.planning/REQUIREMENTS.md
@.planning/phase2/RESEARCH.md
@.planning/phase2/VALIDATION.md
@LetterboxdToCinephilesChannel/Infrastructure/Http/RssClient.cs
@LetterboxdToCinephilesChannel/Infrastructure/Http/OmdbClient.cs
</context>

<tasks>

<task type="auto" tdd="true">
  <name>Task 1: Implement Robust RSS and OMDb Clients with Unit Tests</name>
  <files>
    LetterboxdToCinephilesChannel/Infrastructure/Http/RssClient.cs,
    LetterboxdToCinephilesChannel/Infrastructure/Http/OmdbClient.cs,
    LetterboxdToCinephilesChannel.Tests/RssParserTests.cs,
    LetterboxdToCinephilesChannel.Tests/OmdbMappingTests.cs
  </files>
  <behavior>
    - RssClient must trim leading/trailing whitespace from the raw RSS feed string (FR-1.1.2).
    - RssClient must handle 'https://letterboxd.com/' namespace correctly for lb tags.
    - RssClient must parse 'letterboxd:liked' XML tag to detect Heart/Liked status (FR-1.1.4).
    - RssClient must extract 'src' from '<img>' tag inside 'description' field and convert resolution from 150x225 (or 0-150-0-225) to 1000x1500 (or 0-1000-0-1500) (FR-1.1.3).
    - OmdbClient must convert 'N/A' strings in JSON to null in C# records (FR-1.2.2).
  </behavior>
  <action>
    1. Create XUnit test project `LetterboxdToCinephilesChannel.Tests` if it doesn't exist.
    2. Refactor `RssClient`:
       - Implement trimming of the raw RSS content before `XDocument.Parse`.
       - Use `XNamespace lb = "https://letterboxd.com/";` to access `lb:filmTitle`, `lb:filmYear`, `lb:memberRating`, and `lb:liked`.
       - Use `HtmlAgilityPack` to parse the `description` CDATA and extract the first `<img>` src.
       - Implement URL string transformation for posters: replace resolution tokens (e.g., `-0-150-0-225-`) with high-res equivalents (`-0-1000-0-1500-`).
    3. Refactor `OmdbClient`:
       - Use `JsonConverter` or custom mapping logic to ensure "N/A" results in `null` properties for metadata fields.
    4. Implement comprehensive unit tests in `RssParserTests.cs` covering whitespace, liked tags, and poster resolution.
  </action>
  <verify>
    <automated>dotnet test LetterboxdToCinephilesChannel.Tests</automated>
  </verify>
  <done>Clients are robust, tested, and handle external data quirks (whitespace, namespaces, CDATA, resolution scaling, N/A values).</done>
</task>

<task type="auto">
  <name>Task 2: Implement Orchestration, Persistence, and Telegram Delivery</name>
  <files>
    LetterboxdToCinephilesChannel/Worker.cs,
    LetterboxdToCinephilesChannel/Infrastructure/Data/AppDbContext.cs,
    LetterboxdToCinephilesChannel/Infrastructure/Services/TelegramService.cs
  </files>
  <action>
    1. Implement `TelegramService` using `Telegram.Bot`:
       - Method `SendMovieCardAsync` accepting posters, plots (truncated to 1024), and buttons (IMDb/Letterboxd links).
       - Ensure `ParseMode.Html` is used.
    2. Update `Worker.cs`:
       - Use `PeriodicTimer` set to 10 minutes (FR-1.1.1).
       - Implement loop: Fetch RSS -> Filter New (using bulk check) -> Enrich OMDb -> Send Telegram -> Save DB.
    3. Optimize `AppDbContext`:
       - Ensure efficient set-based duplicate check: `.Where(x => incomingIds.Contains(x.LetterboxdId)).Select(x => x.LetterboxdId).ToListAsync()`.
    4. Ensure graceful shutdown is passed to all async calls.
  </action>
  <verify>
    <automated>dotnet build LetterboxdToCinephilesChannel && grep -q "PeriodicTimer" LetterboxdToCinephilesChannel/Worker.cs</automated>
  </verify>
  <done>Core loop is operational with duplicate prevention, high-res posters, and rich messaging.</done>
</task>

</tasks>

<verification>
1. **Unit Tests:** All parsing/mapping tests pass (including whitespace and resolution).
2. **RSS Extraction:** Verify poster URLs are extracted and upgraded from raw XML samples.
3. **Loop Integrity:** Verify the 10-minute timer and scoped DB access work without overlap.
4. **Duplicate Prevention:** Manually run with existing IDs in DB; verify no new messages sent.
</verification>

<success_criteria>
- RssClient trims whitespace and successfully extracts 1000x1500 posters from Letterboxd CDATA.
- RssClient correctly identifies 'Liked' status from XML tags.
- OmdbClient handles "N/A" without deserialization errors.
- Worker Service runs on a 10-minute interval with PeriodicTimer.
- SQLite successfully prevents duplicate processing using set-based checks.
- Telegram messages arrive with high-res posters and HTML-formatted captions.
</success_criteria>

<output>
After completion, create `.planning/phases/02-core-logic/02-01-SUMMARY.md`
</output>
