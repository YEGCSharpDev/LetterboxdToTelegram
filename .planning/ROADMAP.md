# Roadmap: Milestone 2 (CI/CD & Automated Image Distribution)

**Milestone Status:** In Progress
**Current Goal:** Automate Docker build/push and transition to pre-built image usage.

## Phase 4: CI/CD Pipeline Setup
**Goal:** Create and verify the GitHub Actions workflow.

- [ ] Create `.github/workflows/docker-publish.yml` with multi-stage build support.
- [ ] Configure `docker/login-action` and `docker/build-push-action`.
- [ ] Define repository secrets for `DOCKERHUB_USERNAME` and `DOCKERHUB_TOKEN`.
- [ ] Test the pipeline with a manual trigger or a dummy push.

## Phase 5: Deployment Transition
**Goal:** Update deployment configuration to use the Docker Hub image.

- [ ] Update `docker-compose.yml` to point to the Docker Hub image instead of `build: .`.
- [ ] Verify that the application starts correctly when pulling the pre-built image.
- [ ] Update `README.md` with CI/CD status and instructions for using the official image.

---
**Completed Milestones:**
- Milestone 1: Architectural Overhaul & Modernization
