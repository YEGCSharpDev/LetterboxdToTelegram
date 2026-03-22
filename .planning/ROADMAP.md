# Roadmap: LetterboxdToTelegram Migration

**Milestone: Architectural Overhaul & Feature Enrichment**

## Phase 1: Foundation & Modernization
**Goal:** Establish the .NET 10 Worker Service foundation with resilient integrations and portable persistence.
**Plans:** 3 plans

Plans:
- [ ] 01-01-PLAN.md — Project Foundation & Configuration (Worker SDK, Options, Logging)
- [ ] 01-02-PLAN.md — Persistence & Data Access (EF Core, SQLite, WAL, Migrations)
- [ ] 01-03-PLAN.md — Integrations & Seeding (Typed Clients, Resilience, History Seeding)

- [x] Initialize .NET 10 Worker Service project.
- [x] Configure `IHttpClientFactory` and Typed Clients (OMDb, RSS, Telegram).
- [x] Implement EF Core with SQLite for portable persistence.
- [x] Implement initial database seeding logic from Telegram history (MTProto/WTelegramClient).
- [x] Set up basic logging and configuration (Env Vars).

## Phase 2: Core Logic & Integration
- [ ] Implement robust RSS Fetcher and Parser (Handle XML/HTML nuances).
- [ ] Implement OMDb Enrichment Client with Polly resilience.
- [ ] Implement Telegram Bot Client with HTML-rich cards and interactive buttons.
- [ ] Implement multi-item processing loop in `BackgroundService`.

## Phase 3: Refinement & Reliability
- [ ] Implement poster resolution enhancement logic.
- [ ] Implement plot truncation and robust escaping for Telegram.
- [ ] Add unit tests for RSS parsing and OMDb mapping.
- [ ] Finalize Dockerfile and deployment documentation.

## Phase 4: Validation & Delivery
- [ ] End-to-end integration testing.
- [ ] Verification of SQLite persistence in Docker.
- [ ] Delivery of final "lean and mean" application.
