# Project Research Summary

**Project:** OpenCode — .NET 10 Multi-API Solution with API Gateway + Keycloak Auth + React Frontend
**Domain:** .NET Microservices with API Gateway (APISIX) + Keycloak Auth + PostgreSQL + Aspire Orchestration
**Researched:** 2026-04-24
**Confidence:** HIGH

## Executive Summary

OpenCode is a two-service CRUD platform (DragonBall characters/transformations/fights and Music artists/albums/songs) built on a modern .NET 10 stack. The recommended architecture uses a **gateway-mediated microservices pattern**: Apache APISIX handles routing, authentication (via Keycloak OIDC), CORS, and rate limiting at the edge, while two independent .NET 10 Minimal API services handle domain-specific CRUD operations against a shared PostgreSQL instance with schema-based isolation. The entire stack is orchestrated via .NET Aspire 10 for a seamless development inner loop.

The research strongly recommends a **dual auth model (public reads / protected writes)** as the core differentiator — anyone can browse data without login, but creating/updating/deleting requires Keycloak-authenticated sessions with role-based authorization (viewer vs editor). This delivers the best UX for a content platform while maintaining security. The REPR (Request-Endpoint-Response) pattern keeps each API endpoint in its own file, avoiding monolithic controllers while staying lean.

**Three critical risks** require explicit mitigation in the roadmap: (1) Keycloak's PostgreSQL schema must be pre-created before Keycloak starts or it will enter a restart loop; (2) dual EF Core DbContexts sharing one database require disciplined schema management (`HasDefaultSchema()`) to avoid table leakage into the wrong PostgreSQL schema; (3) the `Aspire.Hosting.Keycloak` NuGet package is in preview and can break between versions — pin it with an exact version or fall back to managing Keycloak via `builder.AddContainer()` directly.

## Key Findings

### Recommended Stack

The stack is well-established with high-confidence sources. Every technology has a clear rationale and documented alternatives.

**Core technologies:**
- **.NET 10 + Minimal APIs**: Application runtime with native AOT, superior Aspire integration, and modern API patterns. Uses Minimal APIs (not MVC controllers) for leaner CRUD endpoints with better AOT compatibility.
- **Apache APISIX 3.11+**: API Gateway with native OIDC plugin, dynamic routing, and OpenTelemetry support. Preferred over Kong (lighter — no Cassandra dependency) and YARP/Ocelot (native OIDC integration at the gateway reduces .NET API complexity).
- **Keycloak 26.1+ (Quarkus)**: Self-hosted OIDC/OAuth2 identity server. Replaces ASP.NET Core Identity entirely. The 26.x line uses Quarkus (not WildFly) for faster startup and better container support.
- **PostgreSQL 17**: Single database with schema-based isolation (`dragonball`, `music`, `keycloak` schemas). Chosen for mature EF Core + Npgsql support and schema-based multi-tenancy.
- **.NET Aspire 10**: Cloud-native orchestration — handles service discovery, container lifecycle, health checks, and OpenTelemetry wiring across all components.
- **React 19 + Vite**: SPA frontend with OIDC authorization code + PKCE flow. No SSR needed since auth is handled via APISIX/Keycloak redirects.

**Key version constraints:**
- Npgsql.EntityFrameworkCore.PostgreSQL 10.0.x is strictly coupled to .NET 10.0 — using 9.x with 10.x will fail at runtime.
- Aspire.Hosting.Keycloak is in preview (13.2.3-preview.x) — MUST be pinned with exact version to avoid build breaks.
- Keycloak 26.x works with any PostgreSQL 12+ — no driver mismatch concerns.

**What NOT to use:** ASP.NET Core Identity (conflicts with Keycloak), Dapper for all queries (EF Core is sufficient for CRUD), MySQL/MariaDB (weaker EF Core support), pre-Keycloak-20 (WildFly-based, EOL), RabbitMQ/Kafka (no event-driven requirements yet), Istio/Linklit service mesh (overkill at this scale).

### Expected Features

**Must have (table stakes):**
- **CRUD operations** on all entities with pagination (`PagedResult<T>`), data validation (FluentValidation), and ProblemDetails error responses
- **Authentication (login/logout)** via Keycloak OIDC + APISIX `openid-connect` plugin
- **Role-based access control** — `viewer` (read-only) and `editor` (full CRUD) roles mapped to APISIX consumer groups
- **CORS** configured at APISIX (not in .NET APIs — browser never talks to APIs directly)
- **Health check endpoints** (`/healthz`) for container orchestration
- **OpenAPI/Swagger** documentation per API

**Should have (competitive differentiators):**
- **Public reads / protected writes** (dual auth model) — separate APISIX routes per HTTP method. GET/HEAD/OPTIONS pass without auth; POST/PUT/PATCH/DELETE require OIDC. This is the single highest-value differentiator.
- **Correlation ID tracking** (`X-Correlation-ID`) propagated from browser through APISIX to .NET APIs to PostgreSQL — minimal effort, massive debugging value
- **Schema-based data isolation** — one PostgreSQL database with three schemas (`dragonball`, `music`, `keycloak`) instead of separate database servers. Lowers ops cost while maintaining logical isolation.
- **OpenTelemetry distributed tracing** across APISIX, both APIs, and PostgreSQL — enabled by Aspire's built-in OTel wiring
- **REPR pattern (one file per endpoint)** instead of monolithic controllers — easier to find, test, and modify individual endpoints
- **Aspire-managed container lifecycle** — one `dotnet run` starts all containers (PostgreSQL, Keycloak, APISIX, both APIs, frontend)

**Defer (v1.x / v2+):**
- Admin API Dashboard, ETags/Conditional Requests, Refresh Token Flow, Bulk Operations, Soft Delete (v1.x)
- Multi-replica K8s deployment, Full-text search (PostgreSQL tsvector), Event-driven features (webhooks), API versioning, Self-service API keys, WebSocket push (v2+)

**Anti-features to avoid:** Separate database per service, API versioning from day one, GraphQL, WebSockets/SignalR, Event Sourcing/CQRS, Elasticsearch full-text search, per-user rate limiting — all add complexity without proportional benefit at this scale.

### Architecture Approach

The system follows a **gateway-mediated microservices architecture** with strong observability foundations. All external HTTP traffic enters through APISIX on port 8000, which enforces security policies before routing to internal services. No external request reaches the .NET APIs directly.

**Major components:**
1. **APISIX Gateway** — Single entry point. Routes requests by URI prefix (`/api/dragonball/*`, `/api/music/*`). Applies plugins in order: `cors` (preflight), `openid-connect` (auth on write routes), `request-id` (correlation ID), `limit-req` (rate limiting), `opentelemetry` (tracing). Dual route model: GET routes are unauthenticated, POST/PUT/DELETE routes require OIDC validation via Keycloak.
2. **DragonBall API (.NET 10 Minimal API)** — CRUD for Characters, Transformations, Planets, Fights. Port 8080 internal. EF Core DbContext targets `dragonball` schema. REPR endpoint pattern under `Endpoints/Characters/`, `Endpoints/Transformations/`, etc.
3. **Music API (.NET 10 Minimal API)** — CRUD for Artists, Albums, Songs, Genres. Port 8081 internal. EF Core DbContext targets `music` schema. Parallel structure to DragonBall API.
4. **PostgreSQL 17** — Single database `opencode` with three schemas: `dragonball`, `music`, `keycloak`. Each API uses a different database user with schema-level permissions.
5. **Keycloak 26.x** — Self-hosted in Docker. Realm `opencode` with three OIDC clients: `dragonball-api`, `music-api` (confidential, back-channel), and `frontend` (public, PKCE). Roles: `viewer`, `editor`, `admin`.
6. **Aspire AppHost** — Orchestrates all 7 containers. Uses `WaitFor()` dependencies for correct startup order. Wires connection strings, health checks, and OTel exporters.
7. **React Frontend** — SPA that communicates exclusively through APISIX (port 8000). Never calls .NET APIs directly. OIDC login via Authorization Code + PKCE flow with Keycloak.

**Key architectural patterns:**
- **REPR (Request-Endpoint-Response):** One file per REST endpoint. Each file defines the request model, handler function, and response type. No controllers, no service layers unless complexity demands extraction. Scales better than monolithic controllers.
- **Correlation ID Middleware:** `X-Correlation-ID` header propagated through full request chain. Generated by APISIX `request-id` plugin, forwarded to .NET APIs, included in response headers and structured logs.
- **Schema-Based Isolation:** `modelBuilder.HasDefaultSchema("dragonball")` in `OnModelCreating`. Each DbContext is explicitly scoped to its schema. Connection strings use `Search Schema=dragonball,public` for correct resolution.
- **OTel Distributed Tracing:** W3C `traceparent` propagated across all services. APISIX OTel plugin generates gateway span. .NET SDK creates child spans. Npgsql instrumentation creates database spans. All sent to OTLP collector → Aspire Dashboard.

**Data flows:**
- Unauthenticated read: Browser → APISIX (cors, request-id) → .NET API (Correlation ID middleware) → EF Core (Include/AsSplitQuery) → PostgreSQL → response back through chain
- Authenticated write: Browser → APISIX (openid-connect checks session → Keycloak introspects token → session cookie set) → .NET API (authorize attribute checks role) → EF Core → PostgreSQL → 201 Created with correlation ID

### Critical Pitfalls

1. **Keycloak schema not created before startup** — Keycloak 26.x auto-creates tables within a schema but does NOT auto-create the schema itself. If the `keycloak` schema doesn't exist, Keycloak enters a restart loop with cryptic errors. **Prevention:** Use Aspire's `WithInitBindMount()` or a Docker init SQL script to `CREATE SCHEMA IF NOT EXISTS keycloak` before Keycloak starts. Also set `KC_DB_SCHEMA=keycloak` environment variable.

2. **Multiple EF Core DbContexts sharing one database — migration confusion** — Two DbContexts targeting the same database with different schemas create two `__EFMigrationsHistory` tables. Without `HasDefaultSchema()`, tables leak into the `public` schema. **Prevention:** Always call `modelBuilder.HasDefaultSchema()` in every DbContext. Use `dotnet ef migrations add --context DragonBallContext --project src/DragonBall.Api` explicitly. Add a unit test checking `HasPendingModelChanges()` to catch forgotten migrations.

3. **Aspire.Hosting.Keycloak preview package breaking between updates** — The NuGet package changes API surface between preview versions. A `dotnet restore` on a different machine may pull a breaking preview version. **Prevention:** Pin the exact version (`13.2.3-preview.1.26217.6`, not `13.2.3-preview.*`). Document a fallback plan: manage Keycloak as a standalone Docker Compose service if the Aspire package breaks.

4. **APISIX proxying breaks Scalar/OpenAPI URLs** — The `Microsoft.AspNetCore.OpenApi` package auto-generates server URLs based on the request host. When accessed through APISIX, Scalar's "Try It" requests go directly to the API's internal port (bypassing APISIX), causing CORS failures. **Prevention:** Configure the OpenAPI document's server URL explicitly via `AddDocumentTransformer` to point to `http://localhost:9080/api/dragonball`.

5. **Keycloak OIDC redirect loop in Docker** — The `redirect_uri` doesn't match registered valid redirect URIs for the client, causing an infinite browser redirect loop. **Prevention:** Register all valid URIs (`http://localhost:5173/*`, `http://localhost:3000/*`). Pin Keycloak to a stable port (`builder.AddKeycloak("keycloak", port: 8080)`). Use environment-specific frontend config for Keycloak URL resolution.

**Moderate pitfalls:** EF Core N+1 queries (use `Include()` + `AsSplitQuery()`), Npgsql connection pool exhaustion (use DI-scoped DbContexts, never `new DbContext()`), Aspire Dashboard port conflicts (configure custom ports), APISIX route ordering (specific routes before catch-all), OTel export startup delay (set OTLP timeout to 1 second).

## Implications for Roadmap

Based on dependency analysis across all research files, the recommended phase structure is:

### Phase 0: Foundation & Solution Scaffolding

**Rationale:** The entire stack depends on the solution structure, Aspire AppHost, and ServiceDefaults. This establishes the development environment before any business logic is written. Must happen first because all downstream phases reference these projects.

**Delivers:** .NET 10 solution with Aspire AppHost + ServiceDefaults, Docker config skeleton, project references set up, NuGet package versions pinned (including exact Aspire.Hosting.Keycloak preview version to avoid Pitfall 3).

**Addresses:** Infrastructure setup from ARCHITECTURE.md (project structure, solution skeleton)

**Avoids:** Pitfall 3 (Aspire.Hosting.Keycloak preview breakage) by pinning versions from day one

### Phase 1: Database Foundation & Data Models

**Rationale:** PostgreSQL schemas and EF Core models are prerequisites for all API endpoints. The schema-initialization SQL script (creating `dragonball`, `music`, `keycloak` schemas) must exist before Keycloak or any API can start.

**Delivers:** PostgreSQL init scripts (`CREATE SCHEMA IF NOT EXISTS`), domain models (Character, Transformation, Planet, Fight, Artist, Album, Song, Genre), EF Core DbContexts with `HasDefaultSchema()`, initial migrations, `PagedResult<T>` shared type.

**Addresses:** FEATURES.md table stakes (CRUD foundation, pagination), ARCHITECTURE.md schema isolation pattern

**Avoids:** Pitfall 1 (Keycloak schema not created) — the init script creates the keycloak schema. Pitfall 2 (migration confusion) — each DbContext gets `HasDefaultSchema()` and explicit migration targets.

**Research flag:** Standard patterns — EF Core + PostgreSQL + Npgsql migrations are well-documented. Skip research-phase.

### Phase 2: API Endpoints — DragonBall & Music CRUD

**Rationale:** Core business logic. Both APIs follow the parallel REPR pattern. Can be built independently and tested against the database. Authentication comes in the next phase — for now, endpoints work but only allow anonymous access.

**Delivers:** Complete CRUD endpoints for DragonBall (Characters, Transformations, Planets, Fights) and Music (Artists, Albums, Songs, Genres). Pagination, FluentValidation, ProblemDetails error responses, Correlation ID middleware, health check endpoints.

**Addresses:** FEATURES.md P1 features (CRUD APIs, pagination, data validation, error responses, health checks, OpenAPI docs)

**Avoids:** Pitfall 6 (N+1 queries) — use `Include()` + `AsSplitQuery()` for related data. Pitfall 4 (Scalar URLs broken) — configure OpenAPI server URLs via `AddDocumentTransformer`.

**Research flag:** Standard patterns — Minimal APIs + EF Core CRUD are well-documented. Skip research-phase.

### Phase 3: Keycloak Authentication & Authorization

**Rationale:** Auth is a hard dependency for protected writes. Must come before APISIX gateway configuration because APISIX needs Keycloak's OIDC discovery endpoint to configure the `openid-connect` plugin. The Keycloak schema was already created in Phase 1.

**Delivers:** Keycloak 26.x Docker container via Aspire, realm `opencode` with OIDC clients (`dragonball-api`, `music-api`, `frontend`), roles (`viewer`, `editor`, `admin`), user accounts, `[Authorize]` attribute enforcement in .NET APIs, role-based claim checking.

**Addresses:** FEATURES.md P1 (authentication, authorization, role-based access), ARCHITECTURE.md public-reads/protected-writes auth flow

**Avoids:** Pitfall 1 (schema not created — already handled in Phase 1). Pitfall 5 (redirect loop) — register `http://localhost:5173/*` and `http://localhost:3000/*` as Valid Redirect URIs, pin Keycloak port to 8080. Pitfall 13 (admin password) — always set `KEYCLOAK_ADMIN` and `KEYCLOAK_ADMIN_PASSWORD` env vars.

**Research flag:** Needs deeper research during planning — Keycloak 26.x Quarkus-based config differs from older WildFly-based docs. Verify `Aspire.Hosting.Keycloak` API surface at time of implementation. Keep fallback plan ready (`builder.AddContainer()` approach).

### Phase 4: APISIX Gateway Configuration

**Rationale:** APISIX ties the architecture together. It needs Keycloak running (Phase 3) for OIDC discovery, and both APIs running (Phase 2) as upstream targets. The dual auth model (public reads / protected writes) is implemented here as separate route definitions per HTTP method.

**Delivers:** APISIX 3.11+ container via Aspire, route definitions for DragonBall (`/api/dragonball/*`) and Music (`/api/music/*`), dual auth model (GET no-auth / POST+PUT+DELETE OIDC), CORS configuration, correlation ID (`request-id` plugin), rate limiting (`limit-req` plugin), etcd config store.

**Addresses:** FEATURES.md P1 (API Gateway, CORS, dual auth model), ARCHITECTURE.md gateway pattern

**Avoids:** Pitfall 4 (Scalar URLs through APISIX) — already mitigated in Phase 2 with explicit OpenAPI server URL configuration. Pitfall 9 (route order) — specific routes before catch-all. Pitfall 12 (DNS resolution) — use `discovery.dns.lazy: true` or `depends_on`.

**Research flag:** Standard patterns — APISIX Docker + openid-connect plugin is well-documented. Skip research-phase but verify route configuration syntax for APISIX 3.11+ specific changes.

### Phase 5: OpenTelemetry & Observability

**Rationale:** OTel is wired by Aspire's ServiceDefaults by default, but configuring the APISIX OTel plugin and verifying traces flow correctly across all components requires dedicated attention. Can be done after core routing works (Phase 4).

**Delivers:** APISIX `opentelemetry` plugin configured with OTLP collector endpoint, .NET SDK OTel instrumentation working (ASP.NET Core + Npgsql spans), Aspire Dashboard showing distributed traces from browser → APISIX → .NET API → PostgreSQL, structured logs with Correlation ID, W3C Trace Context propagation verified end-to-end.

**Addresses:** FEATURES.md differentiator (OTel distributed tracing), ARCHITECTURE.md observability flow

**Avoids:** Pitfall 10 (OTLP startup delay) — set OTLP exporter timeout to 1000ms, verify Aspire Dashboard starts before API projects.

**Research flag:** Standard patterns — Aspire provides OTel wiring out of the box. Skip research-phase but verify APISIX OTel plugin configuration for 3.11+.

### Phase 6: React Frontend (Basic CRUD)

**Rationale:** Frontend consumes all the API infrastructure. Requires working APIs (Phase 2), auth (Phase 3), gateway (Phase 4), and CORS (Phase 4). Build login page first, then data browsing (public reads), then data management (authenticated writes).

**Delivers:** React 19 + Vite SPA with OIDC login (Authorization Code + PKCE via Keycloak), data browsing pages for DragonBall and Music entities, data create/update forms (authenticated), pagination UI, error handling, correlation ID in API client.

**Addresses:** FEATURES.md P2 (React frontend), ARCHITECTURE.md frontend component (communicates only through APISIX)

**Avoids:** Pitfall 5 (CORS/redirect) — CORS already configured in APISIX Phase 4. Token stored in memory (not localStorage) for security.

**Research flag:** Standard patterns — React + OIDC + API consumption is well-documented. Skip research-phase.

### Phase 7: Production Readiness & Docker Compose

**Rationale:** Aspire is used for development inner loop. Production uses a standalone Docker Compose configuration without Aspire. All components need production hardening: pinned image tags, health check configurations, environment variable management, non-root container users, resource limits.

**Delivers:** Production Docker Compose file, pinned container image tags, production environment variable files, health check configurations, non-root user setup, network security (internal networks), backup considerations.

**Addresses:** FEATURES.md v1.x features (production readiness), ARCHITECTURE.md scaling considerations

**Avoids:** Pitfall 12 (Docker network name resolution) — use `depends_on` and health checks. Pitfall 7 (connection pool exhaustion under load) — configure `MaxPoolSize` and `Connection Lifetime`.

**Research flag:** Needs standard Docker Compose production patterns — well-documented but requires careful configuration. Low reseach risk.

### Phase Ordering Rationale

1. **Foundation → Data → APIs → Auth → Gateway → OTel → Frontend → Production** follows the hard dependency chain. Each phase produces something the next phase needs.
2. **Auth before Gateway** because APISIX's `openid-connect` plugin needs Keycloak's OIDC discovery endpoint to validate requests. If Gateway comes before Auth, APISIX has nowhere to validate tokens.
3. **APIs before Auth** permits building and testing CRUD endpoints with anonymous access first. Auth enforcement is added in Phase 3-4.
4. **OTel after Gateway** because APISIX generates the first span in the trace chain. Verifying end-to-end traces requires all upstream services to be running.
5. **Frontend last** because it consumes all services. Building it earlier risks rework when API contracts change, and it can't be tested without auth and gateway working.
6. **Production readiness last** because production concerns (hardening, scaling) aren't needed until the system works end-to-end.

### Research Flags

Phases likely needing deeper research during planning:
- **Phase 3 (Keycloak Auth):** Keycloak 26.x (Quarkus-based) configuration differs from older WildFly-based tutorials. The `Aspire.Hosting.Keycloak` package is in preview and its API surface may change. Needs verification of the exact `AddKeycloak()` API, realm import mechanism, and OIDC client configuration at time of implementation. Fallback to `builder.AddContainer()` must be planned.

Phases with standard patterns (skip research-phase):
- **Phase 0 (Foundation):** Aspire solution scaffolding — well-documented, templates exist
- **Phase 1 (Database):** EF Core + Npgsql + PostgreSQL — established patterns
- **Phase 2 (API Endpoints):** Minimal APIs + CRUD — well-documented, REPR pattern is straightforward
- **Phase 4 (APISIX Gateway):** APISIX Docker + route config — well-documented, configuration is deterministic
- **Phase 5 (OTel):** Aspire provides OTel wiring by default — verify and test
- **Phase 6 (Frontend):** React + Vite + OIDC — established patterns
- **Phase 7 (Production):** Docker Compose production patterns — well-documented

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | All technologies verified against official documentation (MS Learn, Apache APISIX docs, Keycloak docs, PostgreSQL docs). Version compatibilities explicitly checked. |
| Features | HIGH | Features derived from PROJECT.md requirements, verified against reference implementations (eShopOnContainers, Aspire samples). MVP definition is clear and well-prioritized. |
| Architecture | HIGH | Architecture patterns verified against official docs and community reference implementations. REPR pattern (community) has lower confidence but is straightforward. |
| Pitfalls | HIGH | Every pitfall verified against official documentation or community post-mortems. Phase-specific warnings are mapped to implementation phases. |

**Overall confidence:** HIGH — all research areas are well-supported by official documentation with clear, actionable recommendations.

### Gaps to Address

- **Aspire.Hosting.Keycloak API surface:** The exact API may change between now and implementation time. Mitigation: Document the fallback plan (manage Keycloak via `builder.AddContainer()`) and pin the NuGet package version explicitly. Verify the API surface during Phase 3 planning.
- **Keycloak realm export/import:** The research assumes realm configuration can be exported as JSON and imported on container startup. The exact Keycloak 26.x import mechanism (Admin API vs Docker entrypoint) needs verification during Phase 3 planning.
- **APISIX route config management:** Research suggests two approaches — startup `config.yaml` or Admin API after container start. The trade-off (immutable config vs dynamic runtime) needs a decision during Phase 4 planning.
- **Frontend OIDC library choice:** Research hasn't selected between `oidc-client-ts`, `keycloak-js`, or raw fetch-based OIDC flow. Mitigation: Evaluate during Phase 6 planning. `oidc-client-ts` is the most maintainable choice for a non-Keycloak-specific OIDC implementation.

## Sources

### Primary (HIGH confidence)
- [context7 aspnetcore, efcore] — Confirmed patterns for Minimal APIs, DbContext pooling, and OpenTelemetry integration
- [APISIX openid-connect plugin docs] — OIDC integration patterns, route configuration
- [APISIX architecture docs] — Plugin system, route matching, admin API
- [APISIX cors plugin docs] — CORS configuration at gateway
- [Keycloak 26.x server configuration docs] — Server config, DB config, admin setup
- [Keycloak Docker getting started] — Container setup, environment variables
- [Npgsql EF Core provider docs] — Connection string params, pooling, version compatibility
- [EF Core docs] — Migration strategies, split queries, DbContext lifetime, partial indexes
- [PostgreSQL docs] — Schema management, Docker init scripts
- [OpenTelemetry .NET SDK docs] — OTel configuration, W3C trace context, known issues
- [RFC 7807 Problem Details] — Error response specification

### Secondary (MEDIUM confidence)
- [Microsoft .NET Aspire docs] — Orchestration, container networking, lifecycles (version-specific features may vary)
- [Microsoft eShopOnContainers] — Reference patterns for microservices, OIDC, API gateway
- [OpenAPI/Scalar docs] — OpenAPI server URL transformer patterns, community issue tracking
- [REPR Pattern (Ardalis)] — Community pattern, not official Microsoft

### Tertiary (LOW confidence)
- [Real-world .NET microservices (Stack Overflow, Jet.com)] — Case studies for production patterns

---

*Research completed: 2026-04-24*
*Ready for roadmap: yes*
