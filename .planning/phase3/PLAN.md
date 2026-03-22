---
phase: 03-refinement-reliability
plan: 01
type: execute
wave: 1
depends_on: ["02-01"]
validation: .planning/phase3/VALIDATION.md
files_modified: [
  "LetterboxdToCinephilesChannel/LetterboxdToCinephilesChannel.csproj",
  "LetterboxdToCinephilesChannel/Program.cs",
  "LetterboxdToCinephilesChannel/Worker.cs",
  "LetterboxdToCinephilesChannel/Infrastructure/Services/HealthCheckFilePublisher.cs",
  "Dockerfile",
  "docker-compose.yml",
  ".dockerignore"
]
autonomous: true
requirements: [NFR-2.2.2, NFR-2.1.3, NFR-2.3.1, NFR-2.3.2, FR-1.4.3]

must_haves:
  truths:
    - "Application logs are structured (JSON) using Serilog and include enrichment (NFR-2.2.2)."
    - "Application writes a '/tmp/healthy' file when healthy for Docker health monitoring."
    - "On shutdown, SQLite WAL is checkpointed with TRUNCATE to ensure disk persistence (NFR-2.1.3)."
    - "Docker container runs as non-root 'app' user (NFR-2.3.1)."
    - "Database file is stored in a persistent volume mount (FR-1.4.3)."
  artifacts:
    - path: "LetterboxdToCinephilesChannel/Infrastructure/Services/HealthCheckFilePublisher.cs"
      provides: "File-based health check signaling for Docker"
    - path: "Dockerfile"
      provides: "Multi-stage lean production image with non-root security"
    - path: "docker-compose.yml"
      provides: "Standard deployment with volumes and healthchecks"
  key_links:
    - from: "Dockerfile"
      to: "/app/data"
      via: "Directory creation and ownership"
    - from: "docker-compose.yml"
      to: "HealthCheckFilePublisher.cs"
      via: "Health check command 'cat /tmp/healthy'"
---

<objective>
Harden the application for production by implementing structured JSON logging with Serilog, file-based health checks, graceful shutdown with SQLite WAL checkpointing, and a secure non-root multi-stage Docker deployment.
</objective>

<execution_context>
@C:/Users/shnkr/.gemini/get-shit-done/workflows/execute-plan.md
</execution_context>

<context>
@.planning/ROADMAP.md
@.planning/STATE.md
@.planning/REQUIREMENTS.md
@.planning/phase3/RESEARCH.md
@.planning/phase3/VALIDATION.md
@LetterboxdToCinephilesChannel/Program.cs
@LetterboxdToCinephilesChannel/Worker.cs
</context>

<tasks>

<task type="auto">
  <name>Task 1: Implement Structured JSON Logging and Health Checks</name>
  <files>
    LetterboxdToCinephilesChannel/LetterboxdToCinephilesChannel.csproj,
    LetterboxdToCinephilesChannel/Program.cs,
    LetterboxdToCinephilesChannel/Infrastructure/Services/HealthCheckFilePublisher.cs
  </files>
  <action>
    1. Update `LetterboxdToCinephilesChannel.csproj`:
       - Add `Serilog.Extensions.Hosting`, `Serilog.Sinks.Console`, `Serilog.Sinks.File` (v9.0.0+).
       - Add `Microsoft.Extensions.Diagnostics.HealthChecks` (v10.0.0).
    2. Create `Infrastructure/Services/HealthCheckFilePublisher.cs`:
       - Implement `IHealthCheckPublisher`.
       - On `PublishAsync`, write a file to `/tmp/healthy` if status is `Healthy`, otherwise delete it.
    3. Update `Program.cs`:
       - Initialize Serilog with explicit JSON formatting: `Log.Logger = new LoggerConfiguration().WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter()).WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(), "logs/log-.json", rollingInterval: RollingInterval.Day).CreateLogger();`.
       - Use Serilog: `builder.Services.AddSerilog();`.
       - Add HealthChecks: `builder.Services.AddHealthChecks();`.
       - Register Publisher: `builder.Services.AddSingleton<IHealthCheckPublisher, HealthCheckFilePublisher>();`.
       - Configure HealthCheck options to publish every 30 seconds.
  </action>
  <verify>
    <automated>dotnet build LetterboxdToCinephilesChannel &amp;&amp; grep -q "new Serilog.Formatting.Json.JsonFormatter()" LetterboxdToCinephilesChannel/Program.cs</automated>
  </verify>
  <done>Logging and Health Checks are integrated and publishing status to a file with JSON formatting.</done>
</task>

<task type="auto">
  <name>Task 2: Enhance Graceful Shutdown with WAL Checkpoint</name>
  <files>
    LetterboxdToCinephilesChannel/Worker.cs
  </files>
  <action>
    1. Update `Worker.cs`:
       - Override `StopAsync(CancellationToken cancellationToken)`.
       - In `StopAsync`, resolve `AppDbContext` via `scopeFactory`.
       - Execute `db.Database.ExecuteSqlRawAsync("PRAGMA wal_checkpoint(TRUNCATE);", cancellationToken)`.
       - Ensure `Log.Information` records the checkpoint start/end.
       - Call `await base.StopAsync(cancellationToken)`.
  </action>
  <verify>
    <automated>dotnet build LetterboxdToCinephilesChannel &amp;&amp; grep -q "wal_checkpoint(TRUNCATE)" LetterboxdToCinephilesChannel/Worker.cs</automated>
  </verify>
  <done>Worker service properly checkpoints SQLite WAL to the main database file on shutdown.</done>
</task>

<task type="auto">
  <name>Task 3: Implement Multi-Stage Dockerization and Deployment</name>
  <files>
    Dockerfile,
    docker-compose.yml,
    .dockerignore
  </files>
  <action>
    1. Create `.dockerignore`: Ignore `bin/`, `obj/`, `logs/`, `.git/`, `.vs/`.
    2. Create `Dockerfile` (Multi-stage):
       - Stage 1 (build): `FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build`. Restore, build, and publish.
       - Stage 2 (runtime): `FROM mcr.microsoft.com/dotnet/runtime:10.0`.
       - In runtime stage:
         - As `root`: `mkdir -p /app/data &amp;&amp; chown -R 1654:1654 /app/data`.
         - Switch to `USER 1654` (Standard .NET 'app' user).
         - Copy published output from build stage to `/app`.
         - Set `VOLUME /app/data`.
         - Entrypoint: `["dotnet", "LetterboxdToCinephilesChannel.dll"]`.
    3. Create `docker-compose.yml`:
       - Service `app`: build `.`
       - Volumes: `./data:/app/data`.
       - Environment:
         - `ConnectionStrings__Default=Data Source=/app/data/movies.db`
         - `Rss__Url`, `Tmdb__ReadAccessToken`, `Telegram__BotToken`, `Telegram__ChannelId`, `ERROR_TELEGRAM_CHAT_ID`, `WORKER__POLLINGINTERVALMINUTES`.
       - Healthcheck: `test: ["CMD", "cat", "/tmp/healthy"]`, interval 1m, timeout 5s, retries 3.
  </action>
  <verify>
    <automated>docker build -t letterboxd-test . &amp;&amp; docker inspect letterboxd-test | grep -q "\"User\": \"1654\""</automated>
  </verify>
  <done>Secure, persistent, and monitorable multi-stage Docker deployment is ready.</done>
</task>

</tasks>

<verification>
1. **Logging:** Run the app and verify `logs/` directory contains structured JSON logs.
2. **Health Check:** Run the app and verify `/tmp/healthy` is created and contains 'Healthy'.
3. **Shutdown:** Stop the app via Ctrl+C and verify logs show "Performing SQLite WAL checkpoint".
4. **Docker:** Run `docker compose up -d`, wait 1 min, and verify `docker compose ps` shows "healthy". Verify `./data/movies.db` is created.
</verification>

<success_criteria>
- Serilog structured logging is active with JSON formatting in console and file sinks.
- Health status is published to a file for Docker monitoring.
- SQLite WAL is merged on graceful shutdown via explicit checkpoint.
- Docker image uses multi-stage build, runs as non-root user, and persists data to a host-mounted volume.
</success_criteria>

<output>
After completion, create `.planning/phases/03-refinement-reliability/03-01-SUMMARY.md`
</output>
