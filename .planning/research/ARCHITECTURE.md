# Architecture Patterns

**Domain:** RSS Polling and Notification
**Researched:** January 2025

## Recommended Architecture

A background worker service that follows a linear pipeline:
`Poll -> Parse -> Filter -> Persist -> Dispatch`

### Component Boundaries

| Component | Responsibility | Communicates With |
|-----------|---------------|-------------------|
| RSS Poller | Fetches raw XML via HttpClient. | Letterboxd RSS |
| XML Parser | Extracts metadata (Title, Rating, etc.). | RSS Poller, HTML Parser |
| HTML Parser | Extracts poster URL and cleans review text. | XML Parser |
| Entry Store | Records processed GUIDs/Links in SQLite. | Filter |
| Telegram Client | Formats and sends photo/message to API. | Dispatcher |

### Data Flow

1. **Poller** retrieves multiple RSS URLs from configuration.
2. **Parser** iterates through *all* `<item>` nodes in the XML (not just the first one).
3. **Filter** checks if the `guid` or `link` exists in the **SQLite** database.
4. If new, **HTML Parser** extracts the poster and review from the `<description>`.
5. **Dispatcher** sends a formatted `SendPhoto` request to Telegram.
6. **Persistence** saves the entry to avoid duplicate posts.

---

## Patterns to Follow

### Use of GUID for Deduplication
Avoid using title/concatenated strings for database keys. Use the `guid` tag from the RSS item.
```xml
<guid isPermaLink="false">letterboxd-review-123456</guid>
```

### Async Operations
Use `await HttpClient.GetAsync()` and `await botClient.SendPhotoAsync()`. The current codebase uses some `.Result` and synchronous calls which can block.

## Anti-Patterns to Avoid

### Single-Item Polling
**Bad:** Only checking the latest item (`item[1]`).
**Good:** Iterate through all items in the feed that are newer than the last tracked timestamp.

### WebClient Usage
**Bad:** Using `WebClient` (deprecated).
**Good:** Using `HttpClient` or `IHttpClientFactory`.

## Scalability Considerations

| Concern | At 100 users | At 10K users |
|---------|--------------|--------------|
| RSS Polling | Negligible load, keep intervals 5-15m. | Consider proxying or randomized jitter to avoid IP bans. |
| DB Storage | SQLite is more than sufficient. | SQLite is still sufficient (~1GB for millions of entries). |
| Telegram API | No issues for single bot. | Be mindful of bot rate limits (30 msgs/sec). |

## Sources
- C# Background Service best practices
- Telegram Bot API Rate Limits
