# Requirements: Dragon Ball & Music APIs

**Defined:** 2026-04-24
**Last Updated:** 2026-04-29
**Core Value:** Validate that the full stack (.NET 10 + Aspire + Keycloak + Kong + OpenTelemetry + EF Core + PostgreSQL) works together as a coherent, observable, and secure architecture for API development.

## v1 Requirements

### Infrastructure

- [x] **INFRA-01**: Single PostgreSQL 17 database with three schemas (`dragonball`, `music`, `keycloak`)
- [x] **INFRA-02**: PostgreSQL init script creates all schemas before any service starts
- [x] **INFRA-03**: .NET Aspire AppHost orchestrates all services (PostgreSQL, Keycloak, Kong, both APIs, frontend) during local development
- [x] **INFRA-04**: Docker Compose file for non-Aspire workspace deployment
- [x] **INFRA-05**: All Docker images use latest stable official tags (no Bitnami images)
- [x] **INFRA-06**: Separate database users per schema with schema-level permissions

### Dragon Ball API

- [x] **DBALL-01**: Dragon Ball API is a .NET 10 Minimal API project
- [x] **DBALL-02**: EF Core DbContext targets `dragonball` schema with `HasDefaultSchema()`
- [x] **DBALL-03**: Repository pattern with Entity Framework Core for data access
- [x] **DBALL-04**: Character entity has fields: name, isEarthling (bool), introductionPhase (string), pictureUrl (string)
- [x] **DBALL-05**: CRUD endpoints: POST (create), GET by ID, PUT (update), DELETE
- [x] **DBALL-06**: GET list endpoint with pagination (page, pageSize, totalCount, totalPages), default 10 items
- [x] **DBALL-07**: List endpoint supports filtering by name and introductionPhase
- [x] **DBALL-08**: Scalar UI serves OpenAPI documentation
- [x] **DBALL-09**: OpenAPI server URL configured to work through Kong proxy
- [x] **DBALL-10**: FluentValidation on create/update inputs
- [x] **DBALL-11**: ProblemDetails (RFC 7807) error responses

### Music API

- [x] **MUSIC-01**: Music API is a .NET 10 Minimal API project
- [x] **MUSIC-02**: EF Core DbContext targets `music` schema with `HasDefaultSchema()`
- [x] **MUSIC-03**: Repository pattern with Entity Framework Core for data access
- [x] **MUSIC-04**: Genre entity with name and description
- [x] **MUSIC-05**: Artist/Band entity with name, biography, genre association
- [x] **MUSIC-06**: Album entity with title, releaseDate, coverUrl, artist association
- [x] **MUSIC-07**: Track entity with name, trackNumber, duration, lyrics, album association (nullable for singles)
- [x] **MUSIC-08**: Standalone singles modeled as tracks with nullable albumId + isStandalone flag
- [x] **MUSIC-09**: CRUD endpoints for Genre, Artist, Album, Track
- [x] **MUSIC-10**: Nested endpoints: `artists/{id}/albums`, `albums/{id}/tracks`
- [x] **MUSIC-11**: GET list endpoints with pagination (page, pageSize, totalCount, totalPages), default 10 items
- [x] **MUSIC-12**: List endpoints support filtering by name, musical style (genre), and release date
- [x] **MUSIC-13**: Scalar UI serves OpenAPI documentation
- [x] **MUSIC-14**: OpenAPI server URL configured to work through Kong proxy
- [x] **MUSIC-15**: FluentValidation on create/update inputs
- [x] **MUSIC-16**: ProblemDetails (RFC 7807) error responses

### Authentication & Authorization

- [x] **AUTH-01**: Keycloak 26+ container with dedicated PostgreSQL `keycloak` schema
- [x] **AUTH-02**: Keycloak realm `opencode` with OIDC clients for both APIs and frontend
- [x] **AUTH-03**: Roles defined: `viewer` (read-only), `editor` (full CRUD)
- [x] **AUTH-04**: GET endpoints are public (no authentication required)
- [x] **AUTH-05**: POST, PUT, DELETE endpoints require valid Keycloak JWT with appropriate role
- [x] **AUTH-06**: .NET APIs validate JWTs via middleware (claims checking)
- [x] **AUTH-07**: Test user accounts for both roles

### API Gateway

- [x] **GATE-01**: Kong as single entry point on port 8000
- [x] **GATE-02**: Route `/api/dragonball/*` to Dragon Ball API upstream
- [x] **GATE-03**: Route `/api/music/*` to Music API upstream
- [x] **GATE-04**: Dual auth model: GET routes pass without auth, POST/PUT/DELETE routes require OIDC
- [x] **GATE-05**: Kong `openid-connect` plugin (or JWT plugin) validates tokens via Keycloak JWKS endpoint
- [x] **GATE-06**: CORS configured at Kong level (not in .NET APIs)
- [x] **GATE-07**: Kong `correlation-id` plugin generates correlation IDs

### Observability

- [x] **OTEL-01**: OpenTelemetry configured in .NET Aspire ServiceDefaults
- [x] **OTEL-02**: Correlation ID (`X-Correlation-ID`) header on all requests/responses
- [x] **OTEL-03**: Correlation ID propagated through all services and included in structured logs
- [x] **OTEL-04**: Distributed traces flow: browser → Kong → .NET API → PostgreSQL
- [x] **OTEL-05**: Kong OpenTelemetry plugin configured with OTLP collector
- [x] **OTEL-06**: Npgsql instrumentation creates database spans
- [x] **OTEL-07**: Aspire Dashboard shows distributed traces

### Frontend

- [x] **FE-01**: React 19 + Vite SPA
- [x] **FE-02**: Communicates exclusively through Kong (port 8000), never directly to APIs
- [x] **FE-03**: OIDC login via Keycloak (Authorization Code + PKCE flow)
- [x] **FE-04**: Login page with redirect to Keycloak
- [x] **FE-05**: Browse Dragon Ball characters with pagination
- [x] **FE-06**: Browse Music catalog (artists, albums, tracks) with pagination
- [x] **FE-07**: Create/edit forms for authenticated users (editor role)
- [x] **FE-08**: Role-aware UI (read-only viewers vs editors with write controls)
- [x] **FE-09**: Error handling with correlation ID display

## v2 Requirements

### Testing & Quality

- [x] **TEST-01**: FluentValidation unit tests for all create/update validators
- [x] **TEST-02**: DTO mapping unit tests for all entity-to-response conversions
- [x] **TEST-03**: Service unit tests for middleware (CorrelationId) and auth (KeycloakRolesClaimsTransformation)
- [x] **TEST-04**: PagedResult pagination edge case tests
- [ ] **TEST-05**: PostgreSQL integration tests using TestContainers for all repositories
- [ ] **TEST-06**: Full API E2E integration tests with TestServer + TestContainers
- [ ] **TEST-07**: Schema isolation verified via integration tests
- [ ] **TEST-08**: CRUD operation verification against real database

### Infrastructure

- **INFRA-07**: Kubernetes deployment manifests
- **INFRA-08**: CI/CD pipelines
- **INFRA-09**: Production-grade secrets management

### Dragon Ball API

- **DBALL-12**: Transformations, Planets, Fights entities and CRUD
- **DBALL-13**: Character image upload (not just URL)

### Music API

- **MUSIC-17**: Full-text search across tracks and artists
- **MUSIC-18**: Bulk import endpoints

### Frontend

- **FE-10**: Admin dashboard for Keycloak user management
- **FE-11**: API usage metrics dashboard
- **FE-12**: Refresh token flow (avoid re-login on token expiry)

## Out of Scope

| Feature | Reason |
|---------|--------|
| Kubernetes deployment | PoC scope, Docker Compose sufficient |
| CI/CD pipelines | PoC, not needed for architecture validation |
| File upload for character pictures | URL-only keeps scope minimal |
| GraphQL endpoint | REST is sufficient for CRUD |
| WebSockets / real-time updates | No use case identified for CRUD data |
| Event sourcing / CQRS | Overkill for CRUD APIs; standard EF Core sufficient |
| Elasticsearch full-text search | PostgreSQL tsvector sufficient if needed later |
| Mobile app | Web-only frontend |
| API versioning | Premature optimization; add when breaking changes occur |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| INFRA-01 | Phase 2 | ✅ Complete |
| INFRA-02 | Phase 2 | ✅ Complete |
| INFRA-03 | Phase 1 | ✅ Complete |
| INFRA-04 | Phase 8 | ✅ Complete |
| INFRA-05 | Phase 1 | ✅ Complete |
| INFRA-06 | Phase 2 | ✅ Complete |
| DBALL-01 | Phase 1 | ✅ Complete |
| DBALL-02 | Phase 2 | ✅ Complete |
| DBALL-03 | Phase 2 | ✅ Complete |
| DBALL-04 | Phase 2 | ✅ Complete |
| DBALL-05 | Phase 3 | ✅ Complete |
| DBALL-06 | Phase 3 | ✅ Complete |
| DBALL-07 | Phase 3 | ✅ Complete |
| DBALL-08 | Phase 3 | ✅ Complete |
| DBALL-09 | Phase 3 | ✅ Complete |
| DBALL-10 | Phase 3 | ✅ Complete |
| DBALL-11 | Phase 3 | ✅ Complete |
| MUSIC-01 | Phase 1 | ✅ Complete |
| MUSIC-02 | Phase 2 | ✅ Complete |
| MUSIC-03 | Phase 2 | ✅ Complete |
| MUSIC-04 | Phase 2 | ✅ Complete |
| MUSIC-05 | Phase 2 | ✅ Complete |
| MUSIC-06 | Phase 2 | ✅ Complete |
| MUSIC-07 | Phase 2 | ✅ Complete |
| MUSIC-08 | Phase 2 | ✅ Complete |
| MUSIC-09 | Phase 3 | ✅ Complete |
| MUSIC-10 | Phase 3 | ✅ Complete |
| MUSIC-11 | Phase 3 | ✅ Complete |
| MUSIC-12 | Phase 3 | ✅ Complete |
| MUSIC-13 | Phase 3 | ✅ Complete |
| MUSIC-14 | Phase 3 | ✅ Complete |
| MUSIC-15 | Phase 3 | ✅ Complete |
| MUSIC-16 | Phase 3 | ✅ Complete |
| AUTH-01 | Phase 4 | ✅ Complete |
| AUTH-02 | Phase 4 | ✅ Complete |
| AUTH-03 | Phase 4 | ✅ Complete |
| AUTH-04 | Phase 4 | ✅ Complete |
| AUTH-05 | Phase 4 | ✅ Complete |
| AUTH-06 | Phase 4 | ✅ Complete |
| AUTH-07 | Phase 4 | ✅ Complete |
| GATE-01 | Phase 5 | ✅ Complete |
| GATE-02 | Phase 5 | ✅ Complete |
| GATE-03 | Phase 5 | ✅ Complete |
| GATE-04 | Phase 5 | ✅ Complete |
| GATE-05 | Phase 5 | ✅ Complete |
| GATE-06 | Phase 5 | ✅ Complete |
| GATE-07 | Phase 5 | ✅ Complete |
| OTEL-01 | Phase 1 | ✅ Complete |
| OTEL-02 | Phase 6 | ✅ Complete |
| OTEL-03 | Phase 6 | ✅ Complete |
| OTEL-04 | Phase 6 | ✅ Complete |
| OTEL-05 | Phase 6 | ✅ Complete |
| OTEL-06 | Phase 6 | ✅ Complete |
| OTEL-07 | Phase 6 | ✅ Complete |
| FE-01 | Phase 7 | ✅ Complete |
| FE-02 | Phase 7 | ✅ Complete |
| FE-03 | Phase 7 | ✅ Complete |
| FE-04 | Phase 7 | ✅ Complete |
| FE-05 | Phase 7 | ✅ Complete |
| FE-06 | Phase 7 | ✅ Complete |
| FE-07 | Phase 7 | ✅ Complete |
| FE-08 | Phase 7 | ✅ Complete |
| FE-09 | Phase 7 | ✅ Complete |
| TEST-01 | Phase 10 | ✅ Complete |
| TEST-02 | Phase 10 | ✅ Complete |
| TEST-03 | Phase 10 | ✅ Complete |
| TEST-04 | Phase 10 | ✅ Complete |
| TEST-05 | Phase 11 | ⬜ Planned |
| TEST-06 | Phase 11 | ⬜ Planned |
| TEST-07 | Phase 11 | ⬜ Planned |
| TEST-08 | Phase 11 | ⬜ Planned |

**Coverage:**
- v1 requirements: 63 total — all implemented ✅
- v2 requirements: 12 total — 4 implemented, 8 planned
- Mapped to phases: 75
- Unmapped: 0 ✓

---

*Requirements defined: 2026-04-24*
*Last updated: 2026-04-29 — v1 all complete, v2 TEST requirements added*
