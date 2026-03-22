# Codebase Structure

**Analysis Date:** 2024-05-24

## Directory Layout

```
LetterboxdToTelegram/
├── LetterboxdToCinephilesChannel/ # Main project source code
│   ├── ChannelCalls.cs           # Telegram API interaction & formatting
│   ├── GetMovieInfo.cs            # OMDB API enrichment & movie models
│   ├── Program.cs                 # Application entry point
│   ├── RSSReader.cs               # Main orchestration & parsing logic
│   ├── TextMessage.cs             # DTO for Letterboxd entries
│   └── LetterboxdToCinephilesChannel.csproj # Project configuration
├── Dockerfile                     # Containerization instructions
├── .dockerignore                  # Docker build exclusions
├── .gitignore                     # Git version control exclusions
├── build_and_push.ps1             # Build and push script for container
├── LetterboxdToCinephilesChannel.sln # Visual Studio Solution file
└── README.md                      # Project overview and setup instructions
```

## Directory Purposes

**LetterboxdToCinephilesChannel/:**
- Purpose: Contains all the source code for the .NET 8.0 application.
- Contains: C# source files, project files, and build artifacts (in `bin` and `obj` directories).
- Key files: `Program.cs`, `RSSReader.cs`, `ChannelCalls.cs`.

**.planning/:**
- Purpose: Stores project planning and analysis documentation.
- Contains: Codebase maps (like this file) and implementation plans.
- Key files: `ARCHITECTURE.md`, `STRUCTURE.md`.

## Key File Locations

**Entry Points:**
- `LetterboxdToCinephilesChannel/Program.cs`: The starting point of the application execution.

**Configuration:**
- `LetterboxdToCinephilesChannel/LetterboxdToCinephilesChannel.csproj`: Manages project dependencies and framework version.
- `Dockerfile`: Configures the runtime environment for the application.

**Core Logic:**
- `LetterboxdToCinephilesChannel/RSSReader.cs`: Orchestrates the main workflow loop and database interactions.
- `LetterboxdToCinephilesChannel/GetMovieInfo.cs`: Fetches movie metadata from OMDB.
- `LetterboxdToCinephilesChannel/ChannelCalls.cs`: Interacts with the Telegram Bot API.

**Testing:**
- Not detected: No automated tests were found in the current codebase structure.

## Naming Conventions

**Files:**
- PascalCase for C# source files: `ChannelCalls.cs`, `RSSReader.cs`.
- lower_snake_case for build scripts: `build_and_push.ps1`.

**Directories:**
- PascalCase for project directories: `LetterboxdToCinephilesChannel`.
- Dot-prefixed for tool directories: `.planning`, `.vs`.

## Where to Add New Code

**New Feature (e.g., more sources):**
- Primary code: Create a new service in `LetterboxdToCinephilesChannel/` and integrate it into `RSSReader.cs`.
- Tests: No existing test directory; recommended to create `LetterboxdToCinephilesChannel.Tests/`.

**New Enrichment Source:**
- Implementation: Create a new class similar to `GetMovieInfo.cs` in `LetterboxdToCinephilesChannel/`.

**Utilities:**
- Shared helpers: Create a `Utilities` or `Helpers` directory within `LetterboxdToCinephilesChannel/` if enough shared code is identified. Currently, helpers are internal to the classes they serve (e.g., `EscapeForMarkdown` in `ChannelCalls.cs`).

## Special Directories

**bin/ and obj/:**
- Purpose: Contain compiled binaries and intermediate build files.
- Generated: Yes
- Committed: No

**entries.db:**
- Purpose: SQLite database file (generated at runtime in the working directory).
- Generated: Yes
- Committed: No (typically excluded via `.gitignore`)

---

*Structure analysis: 2024-05-24*
