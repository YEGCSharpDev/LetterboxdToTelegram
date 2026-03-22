# Phase 3: Refinement & Reliability - Research

**Researched:** 2026-03-21
**Domain:** .NET 10 Worker Service / Docker / SQLite
**Confidence:** HIGH

## Summary
Phase 3 focuses on moving the application from a functional "it works" state to a "production-ready" state. This involves hardening the deployment (Docker non-root), ensuring state persistence reliability (SQLite/Volumes), and implementing observability (Structured Logging, Health Checks).

**Primary recommendation:** Use the standard .NET 10 `app` user (UID 1654) but ensure the SQLite data directory is pre-provisioned in the Dockerfile with correct ownership.

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| .NET Runtime | 10.0 | Runtime | Latest LTS/Current |
| Serilog.Extensions.Hosting | 9.0.0+ | Logging Implementation | Best-in-class structured logging for .NET |
| Microsoft.Extensions.Diagnostics.HealthChecks | 10.0.0 | Monitoring | Official health check abstractions |

**Installation:**
```bash
dotnet add package Serilog.Extensions.Hosting
dotnet add package Serilog.Sinks.Console
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks
```

## Architecture Patterns

### Dockerfile Pattern (Non-Root + SQLite)
```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER root
RUN mkdir -p /app/data && chown -R 1654:1654 /app/data
USER 1654
WORKDIR /app
```

### Health Check Pattern (File-Based)
For a worker service, use a publisher to write a file that Docker can check via `test: ["CMD", "cat", "/tmp/healthy"]`.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Log Rotation | Custom File Writer | Serilog.Sinks.File | Handles locking, retention, and size limits |
| API Retries | Custom `while` loops | Polly / Resilience | Standard patterns for backoff and jitter |

## Common Pitfalls

### Pitfall 1: SQLite Permission Denied in Docker
**What goes wrong:** Container fails to start or database is read-only.
**Why it happens:** The `app` user (UID 1654) doesn't have permissions on the mounted volume.
**How to avoid:** Use `chown -R 1654:1654` on the host directory or in a `bootstrap` container.

### Pitfall 2: WAL File Loss
**What goes wrong:** Data loss on container restart.
**Why it happens:** SQLite WAL files aren't merged into the main .db file on abrupt shutdown.
**How to avoid:** Implement a `PRAGMA wal_checkpoint(TRUNCATE);` in the `StopAsync` method.

## Validation Architecture
- **Framework:** xUnit
- **Monitoring:** Health Check File Existence
- **Logging Verification:** JSON Console Output validation

## Sources
- Official .NET Docker Documentation (High)
- Microsoft Health Check Guidelines (High)
- SQLite Performance/WAL Best Practices (Medium)
