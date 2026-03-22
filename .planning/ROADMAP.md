# Roadmap: LetterboxdToTelegram Migration

**Milestone: Architectural Overhaul & Feature Enrichment**

## Phase 1: Foundation & Modernization
- [ ] Initialize .NET 8/9 Worker Service project.
- [ ] Configure `IHttpClientFactory` and Typed Clients (OMDb, RSS, Telegram).
- [ ] Implement EF Core with SQLite for portable persistence.
- [ ] Set up basic logging and configuration (Env Vars).

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
