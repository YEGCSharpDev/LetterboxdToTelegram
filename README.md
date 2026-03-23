# Letterboxd to Telegram Bot

## Overview
A modern .NET 10 Worker Service that automatically posts Letterboxd entries from specified members to a Telegram channel. It enriches movie metadata using the TMDB API, provides rich HTML cards, and is fully containerized for easy deployment.

## Features
- **Modern Architecture**: Built with .NET 10 Worker SDK.
- **Enriched Metadata**: High-quality movie posters and genre hashtags from TMDB.
- **Rich Telegram Cards**: Professional HTML-formatted messages with interactive buttons.
- **Resilient**: Polly-based retry policies for all external API calls.
- **Deduplication**: Robust SQLite-backed tracking using Letterboxd and IMDb identifiers.
- **Docker Ready**: Multi-stage, non-root Docker support with file-based health checks.

## Requirements
- Docker and Docker Compose
- Letterboxd RSS Feed URLs
- TMDB API Access Token (v3)
- Telegram Bot Token and Channel ID

## Configuration
The application is configured via environment variables (see `docker-compose.yml`):
- `ConnectionStrings__Default`: SQLite connection string (e.g., `Data Source=/app/data/movies.db`).
- `Rss__FeedUrl`: Your Letterboxd RSS feed URL.
- `Tmdb__ReadAccessToken`: Your TMDB API read access token.
- `Telegram__BotToken`: Your Telegram bot token.
- `Telegram__ChannelId`: The target Telegram channel ID.
- `Telegram__ErrorChatId`: Chat ID for error reporting.
- `WORKER__POLLINGINTERVALMINUTES`: Polling interval in minutes (default: 10).

## Deployment

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/shnkrr/LetterboxdToTelegram.git
   cd LetterboxdToTelegram
   ```

2. **Configure Environment:**
   Edit the `docker-compose.yml` file with your specific API tokens and channel IDs.

3. **Launch:**
   ```bash
   docker-compose up -d --build
   ```

## Development
- **Framework**: .NET 10
- **Database**: SQLite (EF Core with WAL mode)
- **Logging**: Serilog (Structured JSON for console and files)
- **Health Checks**: File-based check at `/tmp/healthy`
