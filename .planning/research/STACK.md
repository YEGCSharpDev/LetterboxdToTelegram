# Technology Stack

**Project:** Letterboxd to Telegram
**Researched:** January 2025

## Recommended Stack

### Core Framework
| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| .NET 8 | 8.0 | Application Runtime | Current LTS version, high performance. |
| Microsoft.Extensions.Hosting | 8.0 | Worker Service | Standard for background polling. |

### Database
| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| SQLite | 3.x | Persistent tracking | Lightweight, file-based, sufficient for entry tracking. |

### Libraries
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| HtmlAgilityPack | Latest | HTML parsing | Extracting posters from RSS descriptions. |
| Telegram.Bot | Latest | Telegram API | Interacting with Telegram channels. |
| System.ServiceModel.Syndication | Latest | RSS Parsing | Standard .NET library for feed processing. |
| Microsoft.Data.Sqlite | Latest | DB Connection | ADO.NET provider for SQLite. |

## Alternatives Considered

| Category | Recommended | Alternative | Why Not |
|----------|-------------|-------------|---------|
| HTTP Client | HttpClient | WebClient | WebClient is deprecated in .NET 8. |
| HTML Parsing | HtmlAgilityPack | AngleSharp | HAP is already in the project and sufficient. |

## Installation

```bash
# Nuget packages
dotnet add package HtmlAgilityPack
dotnet add package Telegram.Bot
dotnet add package Microsoft.Data.Sqlite
dotnet add package System.ServiceModel.Syndication
```

## Sources
- Microsoft Documentation (.NET 8, HttpClient)
- Telegram.Bot official docs
- Project source code (RSSReader.cs)
