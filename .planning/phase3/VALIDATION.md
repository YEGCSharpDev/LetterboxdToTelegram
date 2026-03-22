# Validation Plan: Phase 3 (Refinement & Reliability)

## 1. Requirement Coverage Mapping

| Requirement | Description | Verification Method |
|-------------|-------------|---------------------|
| NFR-2.2.1 | Clean Architecture | Manual/Log: Verify `src/` folder structure is clean and logically separated |
| NFR-2.2.2 | Structured Logging | CLI: `dotnet run` and verify JSON logs in console |
| NFR-2.3.1 | Multi-stage Dockerfile | CLI: `docker build -t app:test .` and verify image stages |
| NFR-2.3.2 | Env Var Config | CLI: `docker run -e ... app:test` and verify startup with environment variables |
| NFR-2.1.3 | Graceful Shutdown | Manual/Log: Send SIGTERM and verify logs show "Shutting down..." and WAL checkpointing |
| Monitoring | Health Checks | CLI: `ls /tmp/healthy` (or mapped path) and verify file is written by publisher |

## 2. Automated Test Strategy
- **Framework:** xUnit
- **Infrastructure Tests:**
    - `LoggingTests`: Verify Serilog is configured and writes to the console.
    - `HealthCheckTests`: Verify `HealthCheckFilePublisher` is registered in DI.
- **Docker Tests:**
    - `DockerBuildTests`: Verify `docker build` succeeds and uses the `app` user (UID 1654).

## 3. Manual Verification Steps
1. **Docker Health Probe:** Run `docker inspect --format='{{json .State.Health}}' <container_id>` to verify health status.
2. **Persistence Mount:** Run `docker volume inspect <volume_name>` and verify data persists after container restart.
3. **Structured Logging:** Verify log entries are in valid JSON format using a tool like `jq`.
