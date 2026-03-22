# Validation Plan: Phase 2 (Core Logic & Integration)

## 1. Requirement Coverage Mapping

| Requirement | Description | Verification Method |
|-------------|-------------|---------------------|
| FR-1.1.1 | Parameterized polling | Manual/Log: Verify `PeriodicTimer` honors `WORKER__POLLINGINTERVALMINUTES` |
| FR-1.1.2 | RSS Namespace & Whitespace | Unit Test: Parse RSS with leading whitespace and `letterboxd` tags |
| FR-1.1.3 | High-res Posters | Unit Test: Verify 150x225 URLs are converted to 1000x1500 |
| FR-1.1.4 | Liked/Heart Status | Unit Test: Verify `letterboxd:liked` XML parsing |
| FR-1.2.1 | TMDB Enrichment | Integration Test: Mock TMDB and verify metadata mapping (genres, language) |
| FR-1.2.2 | TMDB Bearer Token | Unit Test: Verify `Authorization` header is correctly set |
| FR-1.3.1 | Telegram Movie Cards | Manual: Send rich message with poster to test channel |
| FR-1.3.2 | HTML Parse Mode | Manual: Verify message formatting in Telegram client |
| FR-1.5.1 | Error Reporting | Manual/Script: Trigger failure and verify notification to `ERROR_TELEGRAM_CHAT_ID` |
| FR-1.4.1 | Duplicate Prevention | Integration Test: Attempt to process same ID twice, check DB/logs |

## 2. Automated Test Strategy
- **Framework:** xUnit
- **Unit Tests:**
    - `RssParserTests`: Verify whitespace trimming, namespace handling, and poster resolution enhancement.
    - `TmdbMappingTests`: Verify JSON deserialization and metadata extraction.
- **Integration Tests:**
    - `WorkerOrchestrationTests`: Mock clients and verify the full "Fetch -> Enrich -> Save" flow.

## 3. Manual Verification Steps
1. **Polling Check:** Observe application logs for two consecutive polling cycles with different interval settings.
2. **Telegram Visual Check:** Verify the layout, poster quality, and genre hashtags in the Telegram channel (match shell script format).
3. **Error reporting:** Stop the TMDB service/API and verify an error notification is sent.
