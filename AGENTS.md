# Dragon Ball & Music APIs — GSD Project

## Project

.NET 10 PoC with two CRUD APIs (Dragon Ball characters + Music catalog), single PostgreSQL database (schemas: `dragonball`, `music`, `keycloak`), Apache Kong gateway, Keycloak auth, OpenTelemetry observability, .NET Aspire orchestration, and React frontend.

**Core value:** Validate that the full stack (.NET 10 + Aspire + Keycloak + Kong + OpenTelemetry + EF Core + PostgreSQL) works together as a coherent architecture.

## Workflow Commands

| Command | Action |
|---------|--------|
| `/gsd-progress` | Show project state and next steps |
| `/gsd-discuss-phase N` | Gather context and clarify approach for phase N |
| `/gsd-plan-phase N` | Plan phase N (create task breakdown) |
| `/gsd-execute-phase N` | Execute planned phase N |
| `/gsd-transition N` | Complete phase N, move to next |
| `/gsd-settings` | Update workflow preferences |

## Active Phase

**Phase 1: Foundation & Solution Scaffolding**
- .NET 10 solution with Aspire AppHost, ServiceDefaults, 2 API projects
- Pin NuGet versions (including Aspire.Hosting.Keycloak preview)
- Launch point: `/gsd-discuss-phase 1`

## Key Documents

- `.planning/PROJECT.md` — Project context and goals
- `.planning/REQUIREMENTS.md` — 63 v1 requirements with REQ-IDs
- `.planning/ROADMAP.md` — 8-phase execution roadmap
- `.planning/STATE.md` — Current project state
- `.planning/config.json` — Workflow preferences

## Rules

- No Bitnami Docker images; use latest stable official tags
- Public reads, protected writes (Keycloak + Kong dual auth)
- Single PostgreSQL DB with schema-based isolation
- Repository pattern with EF Core
- Correlation ID on all requests/responses
- YOLO mode — auto-approve and advance
