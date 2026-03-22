# Technology Stack

**Analysis Date:** 2024-10-18

## Languages

**Primary:**
- C# 12 - Core logic and service implementation throughout `LetterboxdToCinephilesChannel/`

## Runtime

**Environment:**
- .NET 8.0 - Target framework for the application

**Package Manager:**
- NuGet - Used for all dependencies defined in `LetterboxdToCinephilesChannel/LetterboxdToCinephilesChannel.csproj`
- Lockfile: `LetterboxdToCinephilesChannel/obj/project.assets.json` (generated)

## Frameworks

**Core:**
- .NET 8.0 Console Application - Orchestrates the RSS polling and Telegram updates

**Testing:**
- Not detected - No test projects or test files found in the repository

**Build/Dev:**
- Docker - Containerization configuration in `Dockerfile`
- MSBuild - Project building via `LetterboxdToCinephilesChannel.csproj`
- PowerShell - Build script `build_and_push.ps1`

## Key Dependencies

**Critical:**
- `Telegram.Bot` (19.0.0) - Interface with Telegram Bot API in `LetterboxdToCinephilesChannel/ChannelCalls.cs`
- `HtmlAgilityPack` (1.11.57) - Parsing Letterboxd description HTML in `LetterboxdToCinephilesChannel/RSSReader.cs`
- `Microsoft.Data.Sqlite` (8.0.1) - Persistent storage for entry tracking in `LetterboxdToCinephilesChannel/RSSReader.cs`
- `Newtonsoft.Json` (13.0.1) - JSON deserialization for OMDB API responses in `LetterboxdToCinephilesChannel/GetMovieInfo.cs`

**Infrastructure:**
- `Microsoft.VisualStudio.Azure.Containers.Tools.Targets` (1.20.1) - Docker integration for Visual Studio

## Configuration

**Environment:**
- System Environment Variables - Primary configuration source, accessed via `Environment.GetEnvironmentVariable()`

**Build:**
- `LetterboxdToCinephilesChannel/LetterboxdToCinephilesChannel.csproj`
- `Dockerfile`

## Platform Requirements

**Development:**
- .NET 8 SDK
- Docker Desktop (optional, for containerized dev)

**Production:**
- Docker Container (Linux-based via `mcr.microsoft.com/dotnet/runtime:8.0`)

---

*Stack analysis: 2024-10-18*
