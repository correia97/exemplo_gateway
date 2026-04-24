# Requirements: Dragon Ball & Music APIs

**Defined:** 2026-04-24
**Core Value:** Validate that the full stack (.NET 10 + Aspire + Keycloak + APISIX + OpenTelemetry + EF Core + PostgreSQL) works together as a coherent, observable, and secure architecture for API development.

## v1 Requirements

### Infrastructure

- [ ] **INFRA-01**: Single PostgreSQL 17 database with three schemas (`dragonball`, `music`, `keycloak`)
- [ ] **INFRA-02**: PostgreSQL init script creates all schemas before any service starts
- [ ] **INFRA-03**: .NET Aspire AppHost orchestrates all services (PostgreSQL, Keycloak, APISIX, both APIs, frontend) during local development
- [ ] **INFRA-04**: Docker Compose file for non-Aspire workspace deployment
- [ ] **INFRA-05**: All Docker images use latest stable official tags (no Bitnami images)
- [ ] **INFRA-06**: Separate database users per schema with schema-level permissions

### Dragon Ball API

- [ ] **DBALL-01**: Dragon Ball API is a .NET 10 Minimal API project
- [ ] **DBALL-02**: EF Core DbContext targets `dragonball` schema with `HasDefaultSchema()`
- [ ] **DBALL-03**: Repository pattern with Entity Framework Core for data access
- [ ] **DBALL-04**: Character entity has fields: name, isEarthling (bool), introductionPhase (string), pictureUrl (string)
- [ ] **DBALL-05**: CRUD endpoints: POST (create), GET by ID, PUT (update), DELETE
- [ ] **DBALL-06**: GET list endpoint with pagination (page, pageSize, totalCount, totalPages), default 10 items
- [ ] **DBALL-07**: List endpoint supports filtering by name and introductionPhase
- [ ] **DBALL-08**: Scalar UI serves OpenAPI documentation
- [ ] **DBALL-09**: OpenAPI server URL configured to work through APISIX proxy
- [ ] **DBALL-10**: FluentValidation on create/update inputs
- [ ] **DBALL-11**: ProblemDetails (RFC 7807) error responses

### Music API

- [ ] **MUSIC-01**: Music API is a .NET 10 Minimal API project
- [ ] **MUSIC-02**: EF Core DbContext targets `music` schema with `HasDefaultSchema()`
- [ ] **MUSIC-03**: Repository pattern with Entity Framework Core for data access
- [ ] **MUSIC-04**: Genre entity with name and description
- [ ] **MUSIC-05**: Artist/Band entity with name, biography, genre association
- [ ] **MUSIC-06**: Album entity with title, releaseDate, coverUrl, artist association
- [ ] **MUSIC-07**: Track entity with name, trackNumber, duration, lyrics, album association (nullable for singles)
- [ ] **MUSIC-08**: Standalone singles modeled as tracks with nullable albumId + isStandalone flag
- [ ] **MUSIC-09**: CRUD endpoints for Genre, Artist, Album, Track
- [ ] **MUSIC-10**: Nested endpoints: `artists/{id}/albums`, `albums/{id}/tracks`
- [ ] **MUSIC-11**: GET list endpoints with pagination (page, pageSize, totalCount, totalPages), default 10 items
- [ ] **MUSIC-12**: List endpoints support filtering by name, musical style (genre), and release date
- [ ] **MUSIC-13**: Scalar UI serves OpenAPI documentation
- [ ] **MUSIC-14**: OpenAPI server URL configured to work through APISIX proxy
- [ ] **MUSIC-15**: FluentValidation on create/update inputs
- [ ] **MUSIC-16**: ProblemDetails (RFC 7807) error responses

### Authentication & Authorization

- [ ] **AUTH-01**: Keycloak 26+ container with dedicated PostgreSQL `keycloak` schema
- [ ] **AUTH-02**: Keycloak realm `opencode` with OIDC clients for both APIs and frontend
- [ ] **AUTH-03**: Roles defined: `viewer` (read-only), `editor` (full CRUD)
- [ ] **AUTH-04**: GET endpoints are public (no authentication required)
- [ ] **AUTH-05**: POST, PUT, DELETE endpoints require valid Keycloak JWT with appropriate role
- [ ] **AUTH-06**: .NET APIs validate JWTs via middleware (claims checking)
- [ ] **AUTH-07**: Test user accounts for both roles

### API Gateway

- [ ] **GATE-01**: Apache APISIX 3.x as single entry point on port 8000
- [ ] **GATE-02**: Route `/api/dragonball/*` to Dragon Ball API upstream
- [ ] **GATE-03**: Route `/api/music/*` to Music API upstream
- [ ] **GATE-04**: Dual auth model: GET routes pass without auth, POST/PUT/DELETE routes require OIDC
- [ ] **GATE-05**: APISIX `openid-connect` plugin validates tokens via Keycloak JWKS endpoint
- [ ] **GATE-06**: CORS configured at APISIX level (not in .NET APIs)
- [ ] **GATE-07**: APISIX `request-id` plugin generates correlation IDs

### Observability

- [ ] **OTEL-01**: OpenTelemetry configured in .NET Aspire ServiceDefaults
- [ ] **OTEL-02**: Correlation ID (`X-Correlation-ID`) header on all requests/responses
- [ ] **OTEL-03**: Correlation ID propagated through all services and included in structured logs
- [ ] **OTEL-04**: Distributed traces flow: browser → APISIX → .NET API → PostgreSQL
- [ ] **OTEL-05**: APISIX OpenTelemetry plugin configured with OTLP collector
- [ ] **OTEL-06**: Npgsql instrumentation creates database spans
- [ ] **OTEL-07**: Aspire Dashboard shows distributed traces

### Frontend

- [ ] **FE-01**: React 19 + Vite SPA
- [ ] **FE-02**: Communicates exclusively through APISIX (port 8000), never directly to APIs
- [ ] **FE-03**: OIDC login via Keycloak (Authorization Code + PKCE flow)
- [ ] **FE-04**: Login page with redirect to Keycloak
- [ ] **FE-05**: Browse Dragon Ball characters with pagination
- [ ] **FE-06**: Browse Music catalog (artists, albums, tracks) with pagination
- [ ] **FE-07**: Create/edit forms for authenticated users (editor role)
- [ ] **FE-08**: Role-aware UI (read-only viewers vs editors with write controls)
- [ ] **FE-09**: Error handling with correlation ID display

## v2 Requirements

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
| INFRA-01 | — | Pending |
| INFRA-02 | — | Pending |
| INFRA-03 | — | Pending |
| INFRA-04 | — | Pending |
| INFRA-05 | — | Pending |
| INFRA-06 | — | Pending |
| DBALL-01 | — | Pending |
| DBALL-02 | — | Pending |
| DBALL-03 | — | Pending |
| DBALL-04 | — | Pending |
| DBALL-05 | — | Pending |
| DBALL-06 | — | Pending |
| DBALL-07 | — | Pending |
| DBALL-08 | — | Pending |
| DBALL-09 | — | Pending |
| DBALL-10 | — | Pending |
| DBALL-11 | — | Pending |
| MUSIC-01 | — | Pending |
| MUSIC-02 | — | Pending |
| MUSIC-03 | — | Pending |
| MUSIC-04 | — | Pending |
| MUSIC-05 | — | Pending |
| MUSIC-06 | — | Pending |
| MUSIC-07 | — | Pending |
| MUSIC-08 | — | Pending |
| MUSIC-09 | — | Pending |
| MUSIC-10 | — | Pending |
| MUSIC-11 | — | Pending |
| MUSIC-12 | — | Pending |
| MUSIC-13 | — | Pending |
| MUSIC-14 | — | Pending |
| MUSIC-15 | — | Pending |
| MUSIC-16 | — | Pending |
| AUTH-01 | — | Pending |
| AUTH-02 | — | Pending |
| AUTH-03 | — | Pending |
| AUTH-04 | — | Pending |
| AUTH-05 | — | Pending |
| AUTH-06 | — | Pending |
| AUTH-07 | — | Pending |
| GATE-01 | — | Pending |
| GATE-02 | — | Pending |
| GATE-03 | — | Pending |
| GATE-04 | — | Pending |
| GATE-05 | — | Pending |
| GATE-06 | — | Pending |
| GATE-07 | — | Pending |
| OTEL-01 | — | Pending |
| OTEL-02 | — | Pending |
| OTEL-03 | — | Pending |
| OTEL-04 | — | Pending |
| OTEL-05 | — | Pending |
| OTEL-06 | — | Pending |
| OTEL-07 | — | Pending |
| FE-01 | — | Pending |
| FE-02 | — | Pending |
| FE-03 | — | Pending |
| FE-04 | — | Pending |
| FE-05 | — | Pending |
| FE-06 | — | Pending |
| FE-07 | — | Pending |
| FE-08 | — | Pending |
| FE-09 | — | Pending |

**Coverage:**
- v1 requirements: 57 total
- Mapped to phases: 0
- Unmapped: 57 ⚠️

---
*Requirements defined: 2026-04-24*
*Last updated: 2026-04-24 after initial definition*
