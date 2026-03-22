# Roadmap: LetterboxdToTelegram Migration

**Milestone: Architectural Overhaul & Feature Enrichment**

## Phase 1: Foundation & Modernization
**Goal:** Establish the .NET 10 Worker Service foundation with resilient integrations and portable persistence.
**Plans:** 1 plan

Plans:
- [x] .planning/phases/01-foundation/01-01-PLAN.md — Foundation & Modernization (Worker SDK, EF Core, Resilience, Seeding)

- [x] Initialize .NET 10 Worker Service project.
- [x] Configure `IHttpClientFactory` and Typed Clients (OMDb, RSS, Telegram).
- [x] Implement EF Core with SQLite for portable persistence.
- [x] Implement initial database seeding logic from Telegram history (MTProto/WTelegramClient).
- [x] Set up basic logging and configuration (Env Vars).

## Phase 2: Core Logic & Integration
**Goal:** Implement robust RSS parsing, OMDb enrichment, and Telegram delivery within a 10-minute polling loop.
**Plans:** 1 plan

Plans:
- [x] .planning/phase2/PLAN.md — Core processing engine (RSS, OMDb, Telegram, Worker)

- [x] Implement robust RSS Fetcher and Parser (Handle XML/HTML nuances, whitespace, high-res posters).
- [x] Implement OMDb Enrichment Client with Polly resilience and 'N/A' handling.
- [x] Implement Telegram Bot Client with HTML-rich cards and interactive buttons.
- [x] Implement multi-item processing loop in `BackgroundService` with PeriodicTimer.

## Phase 3: Refinement & Reliability
**Goal:** Harden the application with structured logging, health monitoring, and secure Dockerization.
**Plans:** 1 plan

Plans:
- [x] .planning/phase3/PLAN.md — Refinement & Reliability (Docker, Serilog, Health Checks, Graceful Shutdown)

- [x] Implement structured logging with Serilog (File and Console).
- [x] Implement file-based health check publisher for Docker monitoring.
- [x] Implement graceful shutdown with SQLite WAL checkpoint (TRUNCATE).
- [x] Finalize multi-stage non-root Dockerfile and docker-compose.yml.

## Project Complete
**Status:** Modernization and architectural overhaul finished. The application is ready for production.
