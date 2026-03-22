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
- [ ] .planning/phase2/PLAN.md — Core processing engine (RSS, OMDb, Telegram, Worker)

- [ ] Implement robust RSS Fetcher and Parser (Handle XML/HTML nuances, whitespace, high-res posters).
- [ ] Implement OMDb Enrichment Client with Polly resilience and 'N/A' handling.
- [ ] Implement Telegram Bot Client with HTML-rich cards and interactive buttons.
- [ ] Implement multi-item processing loop in `BackgroundService` with PeriodicTimer.

## Phase 3: Refinement & Reliability
- [ ] Implement plot truncation and robust escaping for Telegram.
- [ ] Add unit tests for RSS parsing and OMDb mapping.
- [ ] Finalize Dockerfile and deployment documentation.

## Phase 4: Validation & Delivery
- [ ] End-to-end integration testing.
- [ ] Verification of SQLite persistence in Docker.
- [ ] Delivery of final "lean and mean" application.
