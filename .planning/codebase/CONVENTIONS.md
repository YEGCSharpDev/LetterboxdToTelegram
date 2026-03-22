# Coding Conventions

**Analysis Date:** 2024-05-14

## Naming Patterns

**Files:**
- PascalCase: `RSSReader.cs`, `ChannelCalls.cs`, `GetMovieInfo.cs`.

**Classes:**
- PascalCase: `RSSReader`, `ChannelCalls`, `GetMovieInfo`, `TextMessage`.

**Methods:**
- PascalCase: `Execute()`, `InitializeDatabase()`, `InsertEntry()`, `EntryExists()`, `DownloadXmlContent()`, `ParseXml()`, `ExtractImgUrlAndComments()`, `FindCreator()`, `SendPhotoAsync()`, `ParseRetryAfterTime()`, `EscapeForMarkdown()`, `PrepGenre()`, `PrepCaption()`.

**Variables:**
- **Private fields:** camelCase: `databasePath`, `tableName`, `calls`, `movieinfo` in `RSSReader.cs`.
- **Local variables:** Mixed, but mostly camelCase: `rss`, `urls`, `url`, `connection`, `command`, `message`, `xmlDoc`, `nsManager`, `latestItem`, `description`, `descriptionHtml`, `doc`, `lastPNode`, `defaultCreator`, `usernameMappingEnv`, `usernameToCreatorMapping`, `mappingPairs`, `pair`, `keyValue`.
- **Occasional PascalCase local variables:** `Token`, `ChatId`, `xmlContent`, `parsedData`, `textExtract`, `movieInfo`.

**Types:**
- PascalCase: `TextMessage`, `MovieInfo`.

## Code Style

**Formatting:**
- K&R style braces (open on new line): Standard C# practice.
- Indentation: 4 spaces.
- Top-level statements: Used in `Program.cs`.

**Linting:**
- Not explicitly configured via `.editorconfig` or other linting tools, defaults to .NET SDK analysis.

## Import Organization

**Order:**
1. `System` namespaces: `using System;`, `using System.Collections.Generic;`, `using System.Xml;`, `using System.Net;`.
2. Third-party namespaces: `using Microsoft.Data.Sqlite;`, `using HtmlAgilityPack;`, `using Telegram.Bot;`, `using Telegram.Bot.Polling;`, `using Telegram.Bot.Types;`, `using Telegram.Bot.Types.Enums;`.
3. Project internal namespaces: `using LetterboxdToCinephilesChannel;`.
4. Static imports: `using static LetterboxdToCinephilesChannel.GetMovieInfo;`.

**Path Aliases:**
- None detected.

## Error Handling

**Patterns:**
- `try-catch` blocks are used for operations that might fail, such as network calls and data parsing.
- Some methods return `null` (e.g., `DownloadXmlContent` returns `null` on failure) or use error logging and continue execution.
- Rate limiting/retry logic in `SendPhotoAsync` with error message parsing in `ParseRetryAfterTime`.

## Logging

**Framework:** `Console`

**Patterns:**
- `Console.WriteLine` used for:
  - Application progress ("Message sent successfully.")
  - Error messages ("Error downloading XML from...", "Telegram bot token is missing.")
  - Debugging/verification (printing `textExtract`).

## Comments

**When to Comment:**
- Classes and methods are commented with XML documentation.
- Occasional inline comments for explaining logic or sections (e.g., "Sleep for 10 minutes", "list of special characters in Markdown that need to be escaped").

**JSDoc/TSDoc:**
- XML Documentation Tags: `<summary>`, `<param>`, `<returns>`.

## Function Design

**Size:**
- Generally focused and moderately sized. The `Execute` method in `RSSReader.cs` is the largest, handling the main loop and logic.

**Parameters:**
- Methods use specific types for parameters: `string`, `TextMessage`, `MovieInfo`.

**Return Values:**
- Methods return specific types or `void`/`Task`. Async methods return `Task`.

## Module Design

**Exports:**
- `internal` access modifier is used for classes and methods, restricting them to the assembly.

**Barrel Files:**
- Not applicable to C# projects.

---

*Convention analysis: 2024-05-14*
