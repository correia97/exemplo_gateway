---
gsd_state_version: 2.0
milestone: v2.0
milestone_name: Testing & Quality
status: completed
last_updated: "2026-04-30T23:59:59.000Z"
last_activity: 2026-04-30 -- Milestone v2.0 completed — all 11 phases fully executed and verified
progress:
  total_phases: 11
  completed_phases: 11
  total_plans: 44
  completed_plans: 44
  percent: 100
---

# Project State — Milestone v2.0 Complete 🎉

## Project Reference

See: .planning/PROJECT.md (updated 2026-04-30)

**Core value:** Validated — the full stack (.NET 10 + Aspire + Keycloak + Kong + OpenTelemetry + EF Core + PostgreSQL) works together as a coherent, observable, and secure architecture for API development.

**Milestone v2.0 (Testing & Quality):** All 11 phases completed. The project has 206 automated tests (162 unit/domain + 44 integration), 147 code review findings addressed (78 fixed), and 100% completion across 44 plans.

## Current Position

**Milestone v2.0 — Testing & Quality: ✅ COMPLETED**

All 11 phases have been fully executed, including their extended plans and code review fix iterations:

| Phase | Date | Plans | Status |
|-------|------|-------|--------|
| 01 — Foundation & Solution Scaffolding | 2026-04-24 | 4/4 | ✅ Complete |
| 02 — Database & Models | 2026-04-24 | 4/4 | ✅ Complete |
| 03 — API Endpoints (CRUD) | 2026-04-24 | 4/4 | ✅ Complete |
| 04 — Keycloak Auth & Authorization | 2026-04-24 | 4/4 | ✅ Complete |
| 05 — Kong Gateway | 2026-04-24 | 4/4 | ✅ Complete |
| 06 — OpenTelemetry & Observability | 2026-04-24 | 3/3 | ✅ Complete |
| 07 — React Frontend | 2026-04-25 | 4/4 | ✅ Complete |
| 08 — Docker Compose & Production Deployment | 2026-04-25 | 3/3 | ✅ Complete |
| 09 — Angular Frontend | 2026-04-25 | 4/4 | ✅ Complete |
| 10 — Unit Tests | 2026-04-29 | 7/7 | ✅ Complete |
| 11 — TestContainers Integration | 2026-04-29 | 3/3 | ✅ Complete |

Progress: [████████████████████████████████████] 100%

## Milestone v2.0 Summary — Testing & Quality

### Phase 10: Unit Tests (7 plans, 162 tests)

The unit test phase was executed in two waves:

**Wave 1 (Plans 10-01 through 10-04):** Initial test infrastructure and coverage:
- Created `OpenCode.Api.Tests` project with xUnit, FluentValidation.TestHelper, Moq, and ASP.NET TestHost
- Validator tests for all 10 validators (Character × 2, Genre × 2, Artist × 2, Album × 2, Track × 2)
- DTO mapping tests for 5 entity suites (Character, Genre, Artist, Album, Track)
- Service tests for CorrelationIdMiddleware and KeycloakRolesClaimsTransformation
- PagedResult pagination edge case tests

**Wave 2 (Plans 10-05 through 10-07):** Extended coverage:
- Package migration from NSubstitute to Moq, added EF Core InMemory provider
- In-memory EF Core repository tests for all 5 repositories
- FluentValidation auto-validation pipeline integration tests via TestHost

**Test Files — OpenCode.Api.Tests (137 tests):**
| Category | Test Files | Tests |
|----------|-----------|-------|
| Validators | 6 files (CreateCharacter, UpdateCharacter, Genre, Artist, Album, Track) | ~55 |
| DTO Mappings | 5 files (Character, Genre, Artist, Album, Track) | ~18 |
| Services | 3 files (CorrelationIdMiddleware, KeycloakClaimsTransformation, ValidationPipeline) | ~20 |
| Repositories | 5 files (Character, Genre, Artist, Album, Track) | ~44 |

**Test Files — OpenCode.Domain.Tests (25 tests):**
| Category | Test Files | Tests |
|----------|-----------|-------|
| Entity properties | `EntityPropertyTests.cs` | 8 |
| Schema isolation | `SchemaIsolationTests.cs` | 4 |
| PagedResult math | `PagedResultTests.cs` + `PagedResultEdgeCaseTests.cs` | 13 |

### Phase 11: TestContainers Integration Tests (3 plans, 44 tests)

Integration test suite using TestContainers for PostgreSQL with real database verification:

**Infrastructure:**
- `PostgresFixture`: Singleton container lifecycle (shared via collection fixture across all test classes)
- `IntegrationTestBase`: Base class with DbContext factory methods and test data helpers
- `IntegrationCollection`: xUnit collection definition for container sharing

**Repository Integration Tests (25 tests across 5 repos):**
- `CharacterRepositoryTests`: CRUD, pagination, filtering — real PostgreSQL
- `GenreRepositoryTests`: CRUD, pagination — real PostgreSQL
- `ArtistRepositoryTests`: CRUD, pagination, genre association — real PostgreSQL
- `AlbumRepositoryTests`: CRUD, pagination, artist association — real PostgreSQL
- `TrackRepositoryTests`: CRUD, pagination, album association, singles — real PostgreSQL

**API E2E Tests (13 tests):**
- `CharactersEndpointsTests`: Full CRUD via TestServer + TestContainers
- `MusicEndpointsTests`: Full CRUD via TestServer + TestContainers

**Schema Isolation Tests (6 tests):**
- `SchemaIsolationTests`: Positive (EF Core writes to correct schema) + negative (raw SQL cross-schema writes denied)

**Correlation ID Tests (3 tests):**
- `CorrelationIdTests`: Header propagation through middleware pipeline

### Code Review & Fix (147 findings, 78 fixed)

Every phase underwent systematic code review with a dedicated fix iteration. Results by phase:

| Phase | Findings | Critical | Warning | Info | Fixed |
|-------|----------|----------|---------|------|-------|
| 01 — Foundation | 19 | 2 | 10 | 7 | 11 |
| 02 — Database | 12 | 1 | 4 | 7 | 5 |
| 03 — API Endpoints | 12 | 3 | 4 | 5 | 7 |
| 04 — Keycloak Auth | 14 | 3 | 8 | 3 | 10 |
| 05 — Kong Gateway | 11 | 4 | 4 | 3 | 8 |
| 06 — Observability | 11 | 3 | 3 | 5 | 6 |
| 07 — React Frontend | 16 | 5 | 4 | 7 | 8 |
| 08 — Docker Compose | 14 | 3 | 5 | 6 | 6 |
| 09 — Angular Frontend | 16 | 0 | 6 | 10 | 6 |
| 10 — Unit Tests | 14 | 0 | 7 | 7 | 7 |
| 11 — TestContainers | 8 | 0 | 4 | 4 | 4 |
| **Total** | **147** | **24** | **59** | **64** | **78** |

Key findings fixed across phases:
- **Auth bypasses closed**: Authorization policies applied to all write endpoints (Phase 4), JWT audience/issuer validation enabled (Phases 1, 4), Keycloak realm hardened (Phase 4)
- **Gateway routing fixed**: Route init container dependency cycle resolved, auth plugins configured, upstream URLs corrected (Phase 5)
- **Observability enabled**: OTLP protocol mismatch fixed, service names configured, Npgsql tracing wired, health endpoints registered (Phase 6)
- **Build fixed**: ServiceDefaults compiled (missing FrameworkReference), FluentValidation version conflict resolved (Phase 1)
- **Frontend hardened**: CORS configured at Kong level, OIDC silent renewal error handling, toast notifications, correlation ID headers on all requests (Phase 7)
- **Testing rigour improved**: Validator gap tests added, schema isolation path traversal fixed, EF Core context disposal ensured, seed data coupling removed (Phases 10, 11)
- **Infrastructure hardened**: Non-root users in Docker images, security headers in nginx, container image tags pinned, admin ports closed (Phase 8)
- **Shared architecture fixed**: Repository pattern consistency (UpdateAsync returns entity), PagedResult division-by-zero guard, SQL syntax in init script (Phase 2)

### Overall Test Suite (206 tests)

| Test Suite | Count | Type |
|-----------|-------|------|
| OpenCode.Api.Tests | 137 | Unit (validators, mappings, services, repos, pipeline) |
| OpenCode.Domain.Tests | 25 | Unit (entities, pagination, schema isolation) |
| OpenCode.Integration.Tests | 44 | Integration (repos, E2E, schema, correlation ID) |
| **Total** | **206** | **All passing** |

## Performance Metrics

**Velocity:**
- Total plans: 44 (Phases 1-4: 4×4, 5: 4, 6: 3, 7: 4, 8: 3, 9: 4, 10: 7, 11: 3)
- Total plans executed: 44
- Average duration: ~5 min
- Total execution time: ~220 min

**By Phase:**

| Phase | Plans | Executed | Avg/Plan |
|-------|-------|----------|----------|
| 01 — Foundation | 4 | 4 | ~5 min |
| 02 — Database | 4 | 4 | ~6 min |
| 03 — API Endpoints | 4 | 4 | ~5 min |
| 04 — Keycloak Auth | 4 | 4 | ~6 min |
| 05 — Kong Gateway | 4 | 4 | ~5 min |
| 06 — Observability | 3 | 3 | ~5 min |
| 07 — React Frontend | 4 | 4 | ~5 min |
| 08 — Docker Compose | 3 | 3 | ~5 min |
| 09 — Angular Frontend | 4 | 4 | ~5 min |
| 10 — Unit Tests | 7 | 7 | ~6 min |
| 11 — TestContainers | 3 | 3 | ~8 min |

## Decisions

### Architecture
- **(01-01):** .NET 10 Minimal API with Aspire AppHost orchestrating all services during local development
- **(01-02):** `Directory.Packages.props` for centralized NuGet version pinning
- **(01-03):** ServiceDefaults project with OpenTelemetry SDK references and shared middleware
- **(02-01):** Single PostgreSQL database with three schemas (`dragonball`, `music`, `keycloak`) instead of multi-DB
- **(02-02):** Table-per-schema via `HasDefaultSchema()` in each DbContext
- **(02-03):** Repository pattern with generic `IRepository<T>`, custom repos for domain-specific queries
- **(03-01):** FluentValidation v12 for request validation (manual invocation in endpoints)
- **(03-02):** Scalar UI for OpenAPI documentation with Kong-aware server URL
- **(04-01):** Keycloak 26+ for OIDC auth with realm-level roles (`viewer`, `editor`)
- **(04-02):** Public reads (GET), protected writes (POST/PUT/DELETE)
- **(04-03):** JWT validation at both Kong gateway and .NET API level (defence in depth)
- **(05-01):** Kong as API gateway with `openid-connect` and `correlation-id` plugins
- **(05-02):** CORS configured exclusively at Kong level (not in .NET APIs)
- **(06-01):** OpenTelemetry OTLP export via gRPC to Jaeger all-in-one
- **(06-02):** Npgsql instrumentation via `.AddNpgsql()` on `TracerProviderBuilder`
- **(07-01):** React 19 + Vite SPA with `oidc-client-ts` for OIDC (Authorization Code + PKCE)
- **(08-01):** Docker Compose as non-Aspire deployment target
- **(09-01):** Angular 21 standalone SPA with `angular-auth-oidc-client`
- **(10-01):** FluentValidation v12 includes test helpers (no separate NuGet needed)
- **(10-02):** TestServer (WebApplication + TestHost) for middleware integration tests
- **(10-03):** Moq over NSubstitute for mocking (more active maintenance, broader EF Core InMemory support)
- **(11-01):** Collection fixture pattern (`[CollectionDefinition]`) for single TestContainers PostgreSQL instance across all integration test classes

### Code Review
- **(Phase 1):** `AddNpgsql()` on tracing uses `.AddNpgsql()` (Npgsql 10.x API), not `.AddNpgsqlInstrumentation()` (metrics only)
- **(Phase 4):** Duplicate `KeycloakRolesClaimsTransformation` deferred — shared infrastructure library out of scope for fix iteration
- **(Phase 5):** Kong authentication plugin uses `openid-connect` with Keycloak JWKS endpoint
- **(Phase 8):** CR-01 (Kong init dependency cycle) already fixed in prior Phase 5 session

## Accumulated Context

### Source Projects
| Project | Path | Purpose |
|---------|------|---------|
| OpenCode.Domain | `src/OpenCode.Domain/` | Domain models, EF Core DbContexts, migrations, repository interfaces, pagination |
| OpenCode.DragonBall.Api | `src/OpenCode.DragonBall.Api/` | Dragon Ball character CRUD Minimal API |
| OpenCode.Music.Api | `src/OpenCode.Music.Api/` | Music catalog CRUD Minimal API (Genre, Artist, Album, Track) |
| OpenCode.ServiceDefaults | `src/OpenCode.ServiceDefaults/` | Shared OpenTelemetry, correlation ID middleware, health checks |
| OpenCode.AppHost | `src/OpenCode.AppHost/` | .NET Aspire orchestrator for local development |
| OpenCode.Frontend | `src/OpenCode.Frontend/` | React 19 + Vite SPA |
| OpenCode.AngularFrontend | `src/OpenCode.AngularFrontend/` | Angular 21 standalone SPA |

### Test Projects
| Project | Path | Tests | Type |
|---------|------|-------|------|
| OpenCode.Api.Tests | `tests/OpenCode.Api.Tests/` | 137 | Unit (validators, mappings, services, repos, pipeline) |
| OpenCode.Domain.Tests | `tests/OpenCode.Domain.Tests/` | 25 | Unit (entities, pagination, schema isolation) |
| OpenCode.Integration.Tests | `tests/OpenCode.Integration.Tests/` | 44 | Integration (TestContainers + TestServer) |

### Infrastructure Files
| File | Purpose |
|------|---------|
| `docker-compose.yml` | Production-grade Docker Compose (7+ services) |
| `src/OpenCode.Domain/Data/init.sql` | PostgreSQL init script (3 schemas + users + permissions) |
| `deploy/keycloak/OpenCode-realm.json` | Keycloak realm configuration (clients, roles, users) |
| `deploy/kong/init-routes.sh` | Kong route initialization script |

## Blockers/Concerns

None. All 11 phases complete. Milestone v2.0 delivered.

## Milestone Completion

### What Was Built

The project validates that the full enterprise stack (.NET 10 + Aspire + Keycloak + Kong + OpenTelemetry + EF Core + PostgreSQL) works together coherently:

**Backend (Phase 1-3):** Two .NET 10 Minimal APIs serving Dragon Ball (characters) and Music (genre/artist/album/track) CRUD data, backed by a single PostgreSQL database with schema-based isolation (`dragonball`, `music`, `keycloak`). Repository pattern with EF Core, FluentValidation, ProblemDetails error responses, and Scalar UI documentation.

**Authentication (Phase 4):** Keycloak 26+ OIDC with public reads (GET) and role-protected writes (POST/PUT/DELETE require `editor` role). JWT validation at both the API layer and gateway layer.

**Gateway (Phase 5):** Kong routes, CORS handling, `openid-connect` token validation, `correlation-id` correlation ID generation.

**Observability (Phase 6):** End-to-end OpenTelemetry distributed tracing through Jaeger, Npgsql database span instrumentation, correlation ID propagation through all layers.

**Frontends (Phases 7, 9):** Two complete SPAs — React 19 + Vite and Angular 21 standalone — both with OIDC login, role-aware CRUD UIs, and correlation ID error display.

**Deployment (Phase 8):** Production-grade Docker Compose with 7+ services, health checks, pinned image tags, non-root users, and security hardening.

**Testing (Phases 10-11):** 206 automated tests including 162 unit tests (validators, mappings, services, repositories, pipeline) and 44 TestContainers integration tests (repositories, E2E, schema isolation, correlation ID).

**Quality Assurance:** 147 code review findings identified and 78 critical/warning issues fixed across all 11 phases.

### Key Achievements

1. **Architecture validated** — all components connect, authenticate, and pass data end-to-end
2. **Security hardened** — auth bypasses closed, secrets externalized, JWT validation enforced
3. **Observability proven** — distributed traces flow from browser through Kong to PostgreSQL
4. **Testing comprehensive** — 206 tests with real database integration via TestContainers
5. **Dual frontends** — both React and Angular SPAs demonstrate frontend-agnostic API design
6. **Docker Compose ready** — standalone production deployment without Aspire dependency

## Next Steps

The project is ready for future milestones beyond v2.0. Potential areas:

- **v3.0 (Infrastructure & DevOps):** Kubernetes manifests, CI/CD pipelines, secrets management (INFRA-07 through INFRA-09)
- **v3.0 (Domain Expansion):** Additional Dragon Ball entities (Planets, Transformations, Fights in DBALL-12), character image upload (DBALL-13), full-text search (MUSIC-17), bulk import (MUSIC-18)
- **v3.0 (Frontend Polish):** Admin dashboard (FE-10), usage metrics (FE-11), refresh token handling (FE-12)
- **v3.0 (Code Quality):** Shared infrastructure library refactoring, consolidated auth abstractions
