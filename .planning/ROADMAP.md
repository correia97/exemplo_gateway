# Roadmap: Dragon Ball & Music APIs

## Overview

A .NET 10 proof-of-concept with two independent CRUD APIs (Dragon Ball characters and Music catalog) sharing a single PostgreSQL database with schema-based isolation, fronted by Apache APISIX and secured via Keycloak. Eight phases build from solution scaffolding through production-grade Docker Compose deployment, following the dependency chain: foundation → data → APIs → auth → gateway → observability → frontend → deployment.

## Phases

- [ ] **Phase 1: Foundation & Solution Scaffolding** - .NET 10 solution with Aspire AppHost, ServiceDefaults, and pinned NuGet dependencies
- [ ] **Phase 2: Database & Models** - PostgreSQL schemas, domain models, EF Core DbContexts, shared pagination helper
- [ ] **Phase 3: API Endpoints — Dragon Ball & Music CRUD** - Complete CRUD endpoints with pagination, validation, OpenAPI docs
- [ ] **Phase 4: Keycloak Authentication & Authorization** - Keycloak realm, clients, roles, and .NET JWT validation
- [ ] **Phase 5: APISIX Gateway** - APISIX routing, dual auth model, CORS, correlation ID
- [ ] **Phase 6: OpenTelemetry & Observability** - End-to-end distributed tracing, correlation ID propagation, Aspire Dashboard
- [ ] **Phase 7: React Frontend** - React SPA with OIDC login, data browsing, role-aware CRUD UI
- [ ] **Phase 8: Docker Compose & Production Deployment** - Production-grade Docker Compose deployment without Aspire

## Phase Details

### Phase 1: Foundation & Solution Scaffolding
**Goal**: .NET 10 solution structure with Aspire AppHost, ServiceDefaults, and pinned NuGet dependencies — the skeleton everything else plugs into
**Depends on**: Nothing (first phase)
**Requirements**: INFRA-03, INFRA-05, DBALL-01, MUSIC-01, OTEL-01
**Success Criteria** (what must be TRUE):
  1. Developer can run `dotnet build` on the entire solution with zero errors
  2. Developer can run `dotnet run --project src/AppHost` and see the Aspire Dashboard at the configured URL
  3. All NuGet package versions are pinned (not floating) in `Directory.Packages.props`, including the exact Aspire.Hosting.Keycloak preview version
  4. ServiceDefaults project exists with OpenTelemetry SDK references and basic OTel configuration wired
  5. Both DragonBall.Api and Music.Api Minimal API project files exist and are referenced by the AppHost project
**Plans**: 4 plans

Plans:
- [ ] 01-01: Scaffold solution and project structure (AppHost, ServiceDefaults, DragonBall.Api, Music.Api)
- [ ] 01-02: Configure Aspire AppHost with container orchestration wiring
- [ ] 01-03: Pin NuGet package versions in Directory.Packages.props (including Aspire.Hosting.Keycloak preview)
- [ ] 01-04: Configure basic OpenTelemetry in ServiceDefaults project

### Phase 2: Database & Models
**Goal**: PostgreSQL database with three schemas (dragonball, music, keycloak), domain models, EF Core DbContexts, initial migrations, and shared pagination helper
**Depends on**: Phase 1
**Requirements**: INFRA-01, INFRA-02, INFRA-06, DBALL-02, DBALL-03, DBALL-04, MUSIC-02, MUSIC-03, MUSIC-04, MUSIC-05, MUSIC-06, MUSIC-07, MUSIC-08
**Success Criteria** (what must be TRUE):
  1. PostgreSQL container starts and the init SQL script creates `dragonball`, `music`, and `keycloak` schemas before any service connects
  2. EF Core migrations can be generated and applied independently for both DragonBall and Music DbContexts without schema leakage
  3. `PagedResult<T>` pagination helper returns correct `totalCount`, `page`, `pageSize`, and `totalPages` values
  4. Each DbContext uses `HasDefaultSchema()` and tables are created only in their designated schema
  5. Separate database users exist per schema with schema-scoped permissions
**Plans**: 4 plans

Plans:
- [ ] 02-01: Create PostgreSQL init SQL script (CREATE SCHEMA IF NOT EXISTS for all three schemas)
- [ ] 02-02: Implement domain models and EF Core DbContexts with HasDefaultSchema()
- [ ] 02-03: Create PagedResult<T> pagination helper and repository base class
- [ ] 02-04: Create database users/permissions script and integration test

### Phase 3: API Endpoints — Dragon Ball & Music CRUD
**Goal**: Complete CRUD endpoints for both APIs with pagination, filtering, FluentValidation, ProblemDetails error responses, and OpenAPI/Scalar documentation
**Depends on**: Phase 2
**Requirements**: DBALL-05, DBALL-06, DBALL-07, DBALL-08, DBALL-09, DBALL-10, DBALL-11, MUSIC-09, MUSIC-10, MUSIC-11, MUSIC-12, MUSIC-13, MUSIC-14, MUSIC-15, MUSIC-16
**Success Criteria** (what must be TRUE):
   1. User can send GET/POST/PUT/DELETE requests to Dragon Ball character endpoints and receive correct HTTP responses with valid data
   2. User can send GET/POST/PUT/DELETE requests to Music endpoints (Genre, Artist, Album, Track) and nested endpoints (artists/{id}/albums, albums/{id}/tracks)
   3. User can paginate through lists with `?page=1&pageSize=10` and receive `totalCount` and `totalPages` in the response envelope
   4. User can filter Dragon Ball characters by name and introductionPhase, and Music items by name, genre, and release date
   5. User can access Scalar UI at `/scalar` and see interactive OpenAPI documentation with server URL pointing through APISIX
   6. Invalid input returns FluentValidation errors wrapped in ProblemDetails (RFC 7807) format
**Plans**: 4 plans

Plans:
- [ ] 03-01: Foundation infrastructure — Repository<T>, FluentValidation, ProblemDetails, DI wiring, NuGet
- [ ] 03-02: Dragon Ball Character CRUD — repository, DTOs, endpoints, validators, Program.cs wiring
- [ ] 03-03: Music CRUD (Genre, Artist, Album, Track) — repositories, DTOs, endpoints, validators, nested routes
- [ ] 03-04: Scalar UI + OpenAPI server URL override for APISIX proxy

### Phase 4: Keycloak Authentication & Authorization
**Goal**: Keycloak 26+ instance with dedicated PostgreSQL schema, realm `opencode`, OIDC clients, roles, test users, and JWT validation in .NET APIs
**Depends on**: Phase 2 (Keycloak schema must exist), Phase 3 (APIs exist to protect)
**Requirements**: AUTH-01, AUTH-02, AUTH-03, AUTH-04, AUTH-05, AUTH-06, AUTH-07
**Success Criteria** (what must be TRUE):
  1. Keycloak container starts, connects to its `keycloak` PostgreSQL schema, and the realm `opencode` is created with OIDC clients for both APIs and the frontend
  2. Roles `viewer` and `editor` exist in the realm with distinct permission sets
  3. Test user `viewer1` can log in and has `viewer` role only; test user `editor1` has both `viewer` and `editor` roles
  4. GET endpoints return data without any authentication token (public reads)
  5. POST/PUT/DELETE endpoints return 401 Unauthorized when no token is provided, and 403 Forbidden when the token lacks the `editor` role
**Plans**: 4 plans

Plans:
- [ ] 04-01: Configure Keycloak container in Aspire with environment variables and schema connection
- [ ] 04-02: Create realm configuration JSON with clients, roles, and import mechanism
- [ ] 04-03: Implement JWT validation middleware and role-based authorization in .NET APIs
- [ ] 04-04: Create test user accounts and verify auth flow end-to-end

### Phase 5: APISIX Gateway
**Goal**: Apache APISIX 3.x gateway with route definitions for both APIs, dual auth model, CORS, and correlation ID generation
**Depends on**: Phase 3 (APIs must be running as upstream targets), Phase 4 (Keycloak must be available for OIDC discovery)
**Requirements**: GATE-01, GATE-02, GATE-03, GATE-04, GATE-05, GATE-06, GATE-07
**Success Criteria** (what must be TRUE):
  1. A request to `http://localhost:8000/api/dragonball/characters` routes to the Dragon Ball API upstream
  2. A request to `http://localhost:8000/api/music/artists` routes to the Music API upstream
  3. POST/PUT/DELETE requests without a valid Keycloak JWT return 401 Unauthorized from APISIX (gateway-level auth enforcement)
  4. CORS preflight (OPTIONS) requests succeed from a browser at `http://localhost:5173` without reaching the .NET APIs
  5. Every response from APISIX includes a unique `X-Correlation-ID` header generated by the `request-id` plugin
**Plans**: 4 plans

Plans:
- [x] 05-01: Configure APISIX container in Aspire with etcd and route definitions
- [x] 05-02: Create upstreams and GET routes with init-routes.sh
- [x] 05-03: Configure dual auth model (GET no-auth, POST/PUT/DELETE OIDC) and CORS plugin
- [x] 05-04: Configure request-id global rule and create phase verification document

### Phase 6: OpenTelemetry & Observability
**Goal**: End-to-end distributed tracing from browser through APISIX to .NET API to PostgreSQL, correlation ID propagation through all services, and Jaeger UI showing complete traces
**Depends on**: Phase 5 (APISIX must be routing to configure OTel plugin), Phase 1 (OTel SDK in ServiceDefaults)
**Requirements**: OTEL-02, OTEL-03, OTEL-04, OTEL-05, OTEL-06, OTEL-07
**Success Criteria** (what must be TRUE):
  1. Jaeger UI at `http://localhost:16686` shows distributed traces with spans from APISIX, .NET API, and Npgsql (PostgreSQL) for a single request
  2. W3C `traceparent` header propagates from APISIX request through .NET API to the PostgreSQL database query
  3. Correlation ID (`X-Correlation-ID`) appears in HTTP request/response headers and in .NET structured logs
  4. A single API request generates a complete trace chain: APISIX span → .NET API span → Npgsql database span
  5. Structured logs in both .NET APIs include `CorrelationId` and `TraceId` fields for correlation
**Plans**: 3 plans
**UI hint**: yes

Plans:
- [ ] 06-01: APISIX OTel plugin + Jaeger collector + OTLP endpoint wiring
- [ ] 06-02: Npgsql instrumentation + Correlation ID ILogger scope
- [ ] 06-03: End-to-end verification scenarios (06-VERIFICATION.md)

### Phase 7: React Frontend
**Goal**: React 19 + Vite SPA consuming both APIs exclusively through APISIX, with OIDC login via Keycloak (Authorization Code + PKCE), role-aware CRUD UI, and correlation ID error display
**Depends on**: Phase 5 (APISIX routing and CORS), Phase 4 (Keycloak OIDC)
**Requirements**: FE-01, FE-02, FE-03, FE-04, FE-05, FE-06, FE-07, FE-08, FE-09
**Success Criteria** (what must be TRUE):
  1. User can open `http://localhost:5173`, click "Login", and get redirected to Keycloak login page
  2. After logging in, user sees the application with navigation to Dragon Ball and Music browsing sections
  3. Unauthenticated user can browse Dragon Ball characters and Music catalog with pagination controls
  4. Authenticated user with the `editor` role sees "Create" and "Edit" buttons and can submit CRUD forms
  5. When an API error occurs, the UI displays the error message and the `X-Correlation-ID` value for debugging support
**Plans**: 4 plans
**UI hint**: yes

Plans:
- [x] 07-01: Scaffold React 19 + Vite SPA with OIDC login via oidc-client-ts
- [x] 07-02: Implement Dragon Ball character browsing (public) and CRUD forms (authenticated)
- [x] 07-03: Implement Music catalog browsing (public) and CRUD forms (authenticated)
- [x] 07-04: Add role-aware UI guards, error handling with correlation ID display

### Phase 8: Docker Compose & Production Deployment
**Goal**: Standalone Docker Compose configuration that deploys all services without Aspire, with health checks, pinned image tags, and proper environment configuration for workspace/production-like use
**Depends on**: Phase 5 (gateway routing), Phase 7 (frontend)
**Requirements**: INFRA-04
**Success Criteria** (what must be TRUE):
  1. `docker compose up` starts all 7 services (PostgreSQL, Keycloak, APISIX, DragonBall API, Music API, React frontend) successfully
  2. The full stack works without Aspire: APISIX routing, Keycloak auth, and both CRUD APIs respond correctly
  3. All container images use `latest` stable official tags (no Bitnami images)
  4. Health checks are configured for every service with proper restart policies
**Plans**: 3 plans

Plans:
- [ ] 08-01: Create production Docker Compose file with all 7 services
- [ ] 08-02: Configure health checks, environment variables, and non-root users
- [ ] 08-03: Verify full stack works end-to-end without Aspire orchestration

## Progress

**Execution Order:**
Phases execute in numeric order: 1 → 2 → 3 → 4 → 5 → 6 → 7 → 8

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Foundation & Solution Scaffolding | 0/4 | Not started | - |
| 2. Database & Models | 0/4 | Not started | - |
| 3. API Endpoints | 0/4 | Planned | - |
| 4. Keycloak Auth | 0/4 | Not started | - |
| 5. APISIX Gateway | 0/4 | Planned | - |
| 6. OpenTelemetry & Observability | 0/3 | Not started | - |
| 7. React Frontend | 4/4 | Planned | - |
| 8. Docker Compose & Production Deployment | 0/3 | Not started | - |
