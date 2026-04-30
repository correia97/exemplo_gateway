# Dragon Ball & Music APIs

## What This Is

A .NET 10 proof-of-concept showcasing enterprise backend patterns. Two independent CRUD APIs (Dragon Ball characters and Music catalog) share a single PostgreSQL database with isolated schemas, fronted by Kong and secured via Keycloak. The stack includes .NET Aspire orchestration, OpenTelemetry observability, and a basic React frontend.

## Core Value

Validate that the full stack (.NET 10 + Aspire + Keycloak + Kong + OpenTelemetry + EF Core + PostgreSQL) works together as a coherent, observable, and secure architecture for API development.

## Requirements

### Validated

(None yet — ship to validate)

### Active

- [ ] Dragon Ball API: CRUD for characters with name, isEarthling, introductionPhase, pictureUrl
- [ ] Music API: CRUD for musical styles, artists/bands, albums, tracks
- [ ] Paginated list endpoints with filtering by attributes (name, phase, musical style, launch date)
- [ ] Single PostgreSQL database with separate schemas (dragonball, music, keycloak)
- [ ] .NET Aspire orchestration for local development
- [ ] Docker Compose for workspace deployment
- [ ] Scalar UI for OpenAPI documentation
- [ ] Correlation ID header on all requests/responses
- [ ] Repository pattern with Entity Framework Core
- [ ] OpenTelemetry for metrics, logs, and traces
- [ ] Keycloak authentication (public reads, protected writes)
- [ ] Kong as API gateway
- [ ] React frontend consuming both APIs
- [ ] Track metadata: name, track number, duration, lyrics
- [ ] Artists can have albums with tracks, plus standalone singles
- [ ] Default pagination: 10 items per page (client-overridable)
- [ ] Dragon Ball character picture: URL string (no file upload)
- [ ] No Bitnami Docker images, latest stable versions throughout

### Out of Scope

- Full production readiness / real user data — PoC scope
- Kubernetes deployment — deferred to v2
- CI/CD pipelines — deferred
- File upload for character pictures — URL-only
- Mobile app — web frontend only

## Context

Proof-of-concept architecture validation project. The goal is to prove that the selected stack integrates cleanly before committing to a production build. Both APIs expose similar CRUD patterns with pagination and filtering, making this a good benchmark for the repository pattern + EF Core approach. Keycloak + Kong provides a realistic enterprise auth/gateway layer. OpenTelemetry + Aspire gives full observability out of the box.

## Constraints

- **Tech stack**: .NET 10 SDK, ASP.NET Core, EF Core, PostgreSQL 17
- **Docker**: No Bitnami images, always use `latest` stable official tags
- **Auth**: Keycloak 26+ with dedicated PostgreSQL schema
- **Gateway**: Kong
- **Frontend**: React with Vite
- **Orchestration**: .NET Aspire 9.x
- **Observability**: OpenTelemetry (OTLP)

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Public reads, protected writes | PoC simplicity, allows easy exploration via browser | — Pending |
| URL strings for character pictures | PoC scope, avoids file storage complexity | — Pending |
| Single DB with schemas | Simpler than multi-DB, still enforces isolation | — Pending |
| Full track metadata | Covers realistic music domain requirements | — Pending |
| Artists → albums → tracks + singles | Covers both album-oriented and single-release patterns | — Pending |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd-transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd-complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-04-24 after initialization*
