# Feature Research

**Domain:** .NET CRUD Microservices + API Gateway + Identity + React Frontend
**Researched:** 2026-04-24
**Confidence:** HIGH

## Feature Landscape

### Table Stakes (Users Expect These)

Features users assume exist. Missing these = product feels incomplete.

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| CRUD operations on all entities | This is the entire point of the APIs — create, read, update, delete | Medium | Standard EF Core + Minimal API pattern. Must support list (with pagination), get by ID, create, update, partial update, delete |
| Authentication (login/logout) | Every protected web app requires user identity | High | Keycloak OIDC + APISIX `openid-connect` plugin. Covers login page redirect, session, and bearer token auth |
| Role-based access control | Protected writes implies who-can-do-what modeling | High | Keycloak roles mapped to APISIX consumer groups. .NET API checks fine-grained claims. Two roles minimum: `viewer` (read-only), `editor` (full CRUD) |
| Pagination on list endpoints | Returning all records is unusable with real data | Low | Shared `PagedResult<T>` pattern with `page`, `pageSize`, `totalCount`, `totalPages`. Default 10, client-overridable, max enforced server-side |
| Data validation on create/update | Bad data corrupts the database | Medium | FluentValidation or Data Annotations on API input models. Server-side validation (client-side is UX only, not security) |
| Error responses with meaningful messages | Raw stack traces in JSON are useless | Low | Consistent `ProblemDetails` (RFC 7807) responses. Correlation ID in error body for debugging |
| CORS support for frontend | Browser blocks cross-origin requests by default | Low | APISIX `cors` plugin handles this. Must allow Vite dev server (`localhost:5173`) in development |
| Health check endpoint | Container orchestration needs liveness probes | Low | `/healthz` or `/health` endpoint with deep checks (DB connectivity, Keycloak reachability) |
| OpenAPI/Swagger documentation | Developers need to understand API surface | Low | Built-in .NET 10 OpenAPI support or Swashbuckle. Separate docs per API or aggregated via APISIX |
| Response filtering (select fields) | Clients shouldn't receive unwanted data | Low | EF Core `.Select()` projections or AutoMapper. Reduces payload size and prevents over-fetching |

### Differentiators (Competitive Advantage)

Features that set the product apart. Not required, but valuable.

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| Correlation ID tracking across all services | Full request tracing from React → APISIX → .NET API → PostgreSQL | Low | `X-Correlation-ID` header propagated through entire request chain. Exposed in API responses and logs. Minimal implementation effort, high debugging value |
| Public reads / protected writes (dual auth model) | Anyone can browse data without login; authenticated users can write. Best UX for content platforms | High | Requires separate APISIX routes per HTTP method (GET no auth / POST auth). More complex than "all authed" but dramatically better UX |
| Schema-based data isolation (one DB, three schemas) | Isolated data for DragonBall and Music domains without separate database servers. Lower ops cost than separate databases | Medium | PostgreSQL schemas + EF Core `HasDefaultSchema()` per DbContext. Keycloak gets its own schema too. All in one database for simple backup/restore |
| OpenTelemetry distributed tracing | End-to-end visibility into request performance across gateway, APIs, and database | Medium | Aspire provides OTel wiring. APISIX's OTel plugin + .NET SDK + OTLP collector. Traces in Jaeger/Grafana. Critical for debugging performance issues |
| Minimal API + REPR pattern | Clean, focused endpoints without MVC overhead. Each endpoint is a function, not a controller method | Low | Request-Endpoint-Response pattern. One file per endpoint. Easier to reason about than monolithic controllers. Fits CRUD API well |
| Aspire-managed container lifecycle | One `dotnet run` starts all services (PostgreSQL, Keycloak, APISIX, both APIs, frontend). No manual Docker Compose needed for development | Medium | Aspire AppHost orchestrates everything. Resource dependencies (`WaitFor`) ensure correct startup order. `docker compose` can still be used for production |

### Anti-Features (Commonly Requested, Often Problematic)

| Feature | Why Requested | Why Problematic | Alternative |
|---------|---------------|-----------------|-------------|
| Separate database per service | "Microservices should each have their own database" — conventional wisdom | 2x-3x ops complexity, harder backups, no cross-schema queries, more connection overhead for this project size | Single PostgreSQL with schema-based isolation. Can split later if scale demands it |
| API versioning (v1, v2) | "We need versioning from day one for future changes" | Premature. Adds URL complexity, route duplication, and cognitive load with no current benefit | Start without versioning. Add `/api/v1/` prefix when you make a breaking change (may be never) |
| GraphQL endpoint | "GraphQL lets clients query exactly what they need" | Dramatically more complex than REST CRUD. Requires resolver architecture, N+1 prevention is harder, auth model is different | REST with `fields` query parameter for field selection if needed |
| Real-time updates (WebSockets/SignalR) | "Live updates are cool and modern" | Adds state management complexity, requires sticky sessions or pub/sub, no use case identified yet | Polling with `If-Modified-Since` / `ETag` is sufficient for CRUD data |
| Event sourcing / CQRS | "It's the pure microservices pattern" | Massive overkill for CRUD APIs. Requires event store, separate read/write models, eventual consistency handling | Standard CRUD with EF Core. Add CQRS only if read/write workloads diverge significantly |
| Full-text search (Elasticsearch) | "Search across all entities" | Entire additional infrastructure (Elasticsearch cluster). Unclear search use case for Dragon Ball/Music data | PostgreSQL `tsvector` full-text search is sufficient for millions of records |
| Rate limiting per user | "Prevent API abuse" | Requires user identification in rate limiting logic (APISIX can do it via consumer groups, but adds complexity) | APISIX `limit-req` plugin with global defaults first. Per-user limiting when abuse patterns emerge |

## Feature Dependencies

```
Database Schema (schemas: dragonball, music, keycloak)
    └──requires──> PostgreSQL Container Running
                        └──requires──> Docker

EF Core DbContext Setup
    └──requires──> Database Schema (correct schema)
    └──requires──> NpgsqlConnectionString configured

Keycloak Identity Server
    └──requires──> Database Schema (keycloak schema)
    └──requires──> PostgreSQL Container Running

APISIX Gateway
    └──requires──> etcd (APISIX config store)
    └──requires──> Keycloak Running (for OIDC discovery)
    └──requires──> .NET APIs Running (upstream targets)

Authentication Flow
    └──requires──> Keycloak Realm configured (clients, users, roles)
    └──requires──> APISIX openid-connect plugin configured
    └──requires──> Frontend login redirect mechanism

Authorization (Protected Writes)
    └──requires──> Authentication flow working
    └──requires──> Role/permission definitions in Keycloak
    └──requires──> APISIX consumer-group mapping
    └──requires──> .NET API claim-based authorization

OpenTelemetry Tracing
    └──requires──> OTel collector configured (Aspire provides this)
    └──requires──> APISIX OTel plugin enabled
    └──requires──> .NET OTel SDK configured in ServiceDefaults

Correlation ID Propagation
    └──enhances──> OpenTelemetry Tracing (adds business context to traces)

Pagination
    └──enhances──> List Endpoints (all GET list responses)

Frontend Integration
    └──requires──> All API endpoints working
    └──requires──> CORS configured in APISIX
    └──requires──> Authentication flow working (login, logout, token refresh)

Dual Auth Model (Public Reads / Protected Writes)
    └──requires──> Authentication flow working
    └──requires──> Separate APISIX routes for GET vs POST/PUT/DELETE
    └──requires──> .NET role checking for create/update/delete endpoints
```

### Dependency Notes

- **Keycloak requires PostgreSQL:** Keycloak uses its own database (not the app's tables, but the same server). The `keycloak` schema must exist before Keycloak starts.
- **APISIX requires running upstreams to verify routes:** Health checks will fail if upstreams aren't ready. Use APISIX's passive health checks (don't fail routes on initial unreachable state).
- **Protected Writes requires Authentication:** Can't implement authorization (who can write) without authentication (who is the user). This is a hard ordering constraint.
- **Correlation ID enhances OTel tracing:** They're independent technically, but Correlation IDs add business-semantic identifiers to OTel traces, making them much more useful for debugging.

## MVP Definition

### Launch With (v1)

Minimum viable product — what's needed to validate the concept.

- [x] **Database Foundation** — PostgreSQL container, three schemas created, Keycloak database user, app database users
- [x] **DragonBall CRUD API** — Characters, Transformations, Planets, Fights endpoints with pagination, validation, and error handling
- [x] **Music CRUD API** — Artists, Albums, Songs, Genres endpoints with pagination, validation, and error handling
- [x] **API Gateway** — APISIX routing to both APIs, CORS, correlation ID, rate limiting basics
- [x] **Authentication** — Keycloak realm with OIDC client, login/logout, session management
- [x] **Authorization** — Role-based access: viewers (read-only) vs editors (read-write)
- [x] **OpenTelemetry** — Distributed tracing across APISIX, both APIs, PostgreSQL
- [x] **React Frontend** — Login page, data browsing (read), data management (write with auth)
- [x] **Aspire Orchestration** — All components start with one command, health checks, resource dependencies

### Add After Validation (v1.x)

Features to add once core is working.

- [ ] **Admin API Dashboard** — Web UI for managing Keycloak users/roles without direct Keycloak Admin Console access
- [ ] **API Usage Metrics Dashboard** — Requests per endpoint, auth failures, slow queries visualized via OTel data
- [ ] **ETags / Conditional Requests** — `If-None-Match` / `If-Modified-Since` support for caching
- [ ] **Refresh Token Flow** — Access token refresh without full re-login (improves UX for long sessions)
- [ ] **Bulk Operations** — Batch create/update/delete endpoints for power users
- [ ] **Soft Delete** — Add `deleted_at` instead of physical delete for data recovery

### Future Consideration (v2+)

Features to defer until product-market fit is established.

- [ ] **Multi-replica deployment** — Kubernetes migration, horizontal scaling, load balancing
- [ ] **Full-text search** — PostgreSQL `tsvector` or Elasticsearch for advanced search across entities
- [ ] **Event-driven features** — Webhooks, audit log, async notification on data changes
- [ ] **API versioning** — `/api/v1/` prefix when breaking changes are needed
- [ ] **Self-service API keys** — Developer portal for external API access (different from user auth)
- [ ] **WebSocket push** — Real-time updates via SignalR for collaborative features

## Feature Prioritization Matrix

| Feature | User Value | Implementation Cost | Priority |
|---------|------------|---------------------|----------|
| DragonBall CRUD API | HIGH | MEDIUM | P1 |
| Music CRUD API | HIGH | MEDIUM | P1 |
| Authentication (login/logout) | HIGH | HIGH | P1 |
| Authorization (roles) | HIGH | MEDIUM | P1 |
| API Gateway (APISIX + routing) | MEDIUM | MEDIUM | P1 |
| Pagination on list endpoints | HIGH | LOW | P1 |
| Data validation | HIGH | LOW | P1 |
| Error responses (ProblemDetails) | MEDIUM | LOW | P1 |
| Correlation ID | MEDIUM | LOW | P1 |
| Health check endpoints | MEDIUM | LOW | P1 |
| OpenAPI docs | MEDIUM | LOW | P1 |
| Public reads / protected writes | HIGH | HIGH | P1 |
| OpenTelemetry tracing | HIGH | MEDIUM | P1 |
| CORS configuration | HIGH | LOW | P1 |
| React frontend (basic CRUD) | HIGH | HIGH | P2 |
| Admin API Dashboard | MEDIUM | MEDIUM | P2 |
| Rate limiting | MEDIUM | LOW | P2 |
| ETags / Conditional Requests | LOW | LOW | P2 |
| Refresh Token Flow | HIGH | MEDIUM | P2 |
| Bulk operations | MEDIUM | MEDIUM | P2 |
| Full-text search | MEDIUM | HIGH | P3 |
| Event-driven features | LOW | HIGH | P3 |
| API versioning | LOW | LOW | P3 |
| Self-service API keys | LOW | HIGH | P3 |
| WebSocket push | LOW | HIGH | P3 |

**Priority key:**
- P1: Must have for launch
- P2: Should have, add when possible
- P3: Nice to have, future consideration

## Competitor Feature Analysis

No direct competitors — this is a learning/infrastructure project. However, the feature set aligns with best practices observed in:

| Source | Features Observed |
|--------|-------------------|
| Microsoft eShopOnContainers reference app | Microservice patterns, OIDC integration, API gateway patterns, OTel setup |
| Keycloak quickstart tutorials | OIDC client setup, realm management, role mapping |
| APISIX + Keycloak integration guides | Route-level auth, consumer groups, plugin configuration patterns |
| .NET Aspire samples (eShopLite, AspireShop) | AppHost orchestration, container lifecycle, service discovery |
| Real-world .NET microservices (Stack Overflow, Jet.com case studies) | Pagination patterns, EF Core best practices in production, OTel in practice |

## Sources

- **PROJECT.md:** Full requirements specification for OpenCode (HIGH confidence — authoritative project definition)
- **APISIX openid-connect plugin docs:** OIDC integration patterns (HIGH confidence)
- **Keycloak 26.x documentation:** Realm setup, roles, OIDC clients (HIGH confidence)
- **EF Core documentation:** Pagination, validation, projection patterns (HIGH confidence)
- **.NET Aspire documentation:** AppHost patterns, resource dependencies, container lifecycle (MEDIUM confidence)
- **Microsoft eShopOnContainers reference architecture:** https://github.com/dotnet-architecture/eShopOnContainers (MEDIUM confidence — reference patterns)
- **RFC 7807 Problem Details:** https://www.rfc-editor.org/rfc/rfc7807 (HIGH confidence)
- **W3C Trace Context:** https://www.w3.org/TR/trace-context/ (HIGH confidence)

---

*Feature research for: OpenCode .NET 10 + Aspire + Keycloak + APISIX + PostgreSQL stack*
*Researched: 2026-04-24*
