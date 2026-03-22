# Tech Stack: .NET 8/9 Worker Service with SQLite EF Core

This document outlines the best practices for a portable, Docker-ready worker service using .NET 8/9, SQLite, and Entity Framework Core.

## 1. Portable Persistence Strategy

To ensure the application remains portable while persisting data correctly in Docker, use the following approach:

### Configuration
In `appsettings.json`, use a relative path for development:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=data/entries.db"
  }
}
```

### Runtime Resolution
In Docker, override the path via environment variables in `docker-compose.yml`:
```yaml
environment:
  - ConnectionStrings__DefaultConnection=Data Source=/app/data/entries.db
```

## 2. Docker Readiness

### Dockerfile Setup
Ensure the data directory exists and has correct permissions for the non-root user (`$APP_UID`).

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
# Create data folder and set ownership to the app user
RUN mkdir -p /app/data && chown -R $APP_UID:$APP_UID /app/data
COPY . .
USER $APP_UID
ENTRYPOINT ["dotnet", "YourApp.dll"]
```

### Persistence via Volumes
```yaml
services:
  worker:
    build: .
    volumes:
      - sqlite_data:/app/data
volumes:
  sqlite_data:
```

## 3. Entity Framework Core Best Practices

### Automatic Migrations at Startup
Apply migrations at runtime to ensure the SQLite database is created automatically in the container volume on first run.

```csharp
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    db.Database.Migrate();
}
```

### Performance: WAL Mode
Enable Write-Ahead Logging (WAL) for better concurrency and performance in SQLite, especially important for containerized I/O.

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder options)
    => options.UseSqlite("Data Source=/app/data/entries.db", o => {
        // Additional SQLite specific config if needed
    });
```

## 4. Worker Service Patterns

- **Use `BackgroundService`:** For polling loops, inherit from `BackgroundService` and override `ExecuteAsync`.
- **Scoped Services:** Inject `IServiceScopeFactory` to resolve the `DbContext` within each loop iteration.
- **Graceful Shutdown:** Always pass the `CancellationToken` (from `ExecuteAsync`) to `Task.Delay` and async database calls.
- **`PeriodicTimer`:** Use `PeriodicTimer` instead of `Task.Delay` for more consistent timing intervals.

## 5. Recommended Packages

```bash
dotnet add package Microsoft.Extensions.Hosting
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
```
