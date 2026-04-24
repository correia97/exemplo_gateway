# Architecture Research

**Domain:** .NET Microservices with API Gateway (APISIX) + Keycloak Auth + PostgreSQL + Aspire Orchestration
**Researched:** 2026-04-24
**Confidence:** HIGH

## Standard Architecture

### System Overview

```
                        ┌──────────────────────────┐
                        │    React Frontend (SPA)   │
                        │  (Vite + React 19 + TS)  │
                        └────────────┬─────────────┘
                                     │ HTTP (CORS)
                                     ▼
                  ┌──────────────────────────────────────┐
                  │        APISIX API Gateway             │
                  │  (port 8000 — public endpoint)        │
                  │                                      │
                  │  Plugins:                             │
                  │  • cors (preflight)                   │
                  │  • openid-connect (auth)              │
                  │  • request-id (correlation/trace)     │
                  │  • limit-req (rate limiting)          │
                  │  • opentelemetry (tracing)            │
                  └─────┬────────────────────┬────────────┘
                        │                    │
              (public)  │          (protected)│
              ┌─────────▼──────┐   ┌─────────▼──────────┐
              │   GET routes   │   │ POST/PUT/DEL routes │
              │   (no auth)    │   │  (OIDC required)    │
              └────────┬───────┘   └────────┬────────────┘
                       │                    │
          ┌────────────┼────────────────────┼────────────┐
          │            ▼                    ▼             │
          │  ┌──────────────────────────────────────┐    │
          │  │   .NET 10 Minimal API (DragonBall)    │    │
          │  │   Port: 8080 (container internal)     │    │
          │  │                                       │    │
          │  │   Endpoints:                          │    │
          │  │   • /api/dragonball/characters        │    │
          │  │   • /api/dragonball/transformations   │    │
          │  │   • /api/dragonball/planets           │    │
          │  │   • /api/dragonball/fights            │    │
          │  │   • /healthz                          │    │
          │  │   EF Core DbContext: DragonBallContext │    │
          │  │   Schema: dragonball                  │    │
          │  └───────────────┬───────────────────────┘    │
          │                  │                             │
          │  ┌───────────────▼───────────────────────┐    │
          │  │   .NET 10 Minimal API (Music)          │    │
          │  │   Port: 8081 (container internal)      │    │
          │  │                                        │    │
          │  │   Endpoints:                           │    │
          │  │   • /api/music/artists                 │    │
          │  │   • /api/music/albums                  │    │
          │  │   • /api/music/songs                   │    │
          │  │   • /api/music/genres                  │    │
          │  │   • /healthz                           │    │
          │  │   EF Core DbContext: MusicContext       │    │
          │  │   Schema: music                        │    │
          │  └───────────────┬───────────────────────┘    │
          │                  │                             │
          │                  ▼                             │
          │  ┌────────────────────────────────────────┐   │
          │  │   PostgreSQL 17 (Single Database)       │   │
          │  │   Database: opencode                    │   │
          │  │   Schemas:                              │   │
          │  │   • dragonball  — DragonBall API tables │   │
          │  │   • music       — Music API tables      │   │
          │  │   • keycloak    — Keycloak identity     │   │
          │  │   • public      — (not used, reserved)  │   │
          │  └────────────────────────────────────────┘   │
          │                                                │
          │  ┌────────────────────────────────────────┐   │
          │  │   .NET Aspire AppHost                   │   │
          │  │   Orchestrates all containers:          │   │
          │  │   • postgres: PostgreSQL 17             │   │
          │  │   • keycloak: Keycloak 26.x             │   │
          │  │   • etcd: APISIX config store           │   │
          │  │   • apisix: APISIX 3.11+                │   │
          │  │   • dragonball-api: .NET 10 DragonBall  │   │
          │  │   • music-api: .NET 10 Music            │   │
          │  │   • frontend: React (Vite)              │   │
          │  └────────────────────────────────────────┘   │
          │                                                │
          │  ┌────────────────────────────────────────┐   │
          │  │   Keycloak 26.x (Quarkus)               │   │
          │  │   Realm: opencode                       │   │
          │  │   Clients:                              │   │
          │  │   • dragonball-api (confidential)       │   │
          │  │   • music-api (confidential)            │   │
          │  │   • frontend (public, PKCE)             │   │
          │  │   Roles: viewer, editor, admin          │   │
          │  └────────────────────────────────────────┘   │
          │                                                │
          └────────────────────────────────────────────────┘

   Observability (Aspire/OpenTelemetry):
   ┌─────────────────────────────────────────────────────┐
   │  OTLP Collector                                      │
   │  • Traces: APISIX + .NET APIs + Npgsql queries      │
   │  • Metrics: request count, latency, pool stats      │
   │  • Logs: structured logs with Correlation ID        │
   │  • Forwarded to: Aspire Dashboard / Jaeger          │
   └─────────────────────────────────────────────────────┘
```

### Component Responsibilities

| Component | Responsibility | Typical Implementation |
|-----------|----------------|------------------------|
| **APISIX** | API Gateway — routing, auth (OIDC), CORS, rate limiting, correlation ID, tracing | APISIX container + etcd. Routes configured via Admin API or `config.yaml`. All external HTTP traffic enters here |
| **DragonBall API** | .NET 10 Minimal API — CRUD for Dragon Ball entities | ASP.NET Core Minimal API + EF Core + Npgsql. Schema: `dragonball`. DbContext: `DragonBallDbContext` |
| **Music API** | .NET 10 Minimal API — CRUD for Music entities | ASP.NET Core Minimal API + EF Core + Npgsql. Schema: `music`. DbContext: `MusicDbContext` |
| **PostgreSQL** | Primary database — all data storage | PostgreSQL 17 Docker image. Single database `opencode` with 3 schemas |
| **Keycloak** | Identity provider — authentication, session, role management | Keycloak 26.x (Quarkus) Docker image. Realm `opencode` with OIDC clients |
| **.NET Aspire AppHost** | Container orchestration and development inner loop | Aspire 10 AppHost project. Wires resource dependencies, health checks, service discovery, OTel exporters |
| **React Frontend** | Single-page application — login, data browsing, data management | Vite + React 19 + TypeScript. Communicates ONLY through APISIX (port 8000), never directly to APIs |
| **OpenTelemetry Collector** | Trace/metric/log aggregation | Aspire provides OTel forwarding. APISIX sends OTLP, .NET SDK sends OTLP. All collected in Aspire Dashboard / Jaeger |

## Recommended Project Structure

```
OpenCode/
├── src/
│   ├── OpenCode.AppHost/               # .NET Aspire orchestrator
│   │   ├── Program.cs                   # AppHost builder, resource definitions
│   │   ├── OpenCode.AppHost.csproj
│   │   └── Properties/
│   │       └── launchSettings.json
│   │
│   ├── OpenCode.ServiceDefaults/        # Shared .NET configuration
│   │   ├── Extensions.cs               # AddServiceDefaults, AddOpenTelemetry, AddHealthChecks
│   │   ├── OpenCode.ServiceDefaults.csproj
│   │   └── Properties/
│   │       └── launchSettings.json
│   │
│   ├── OpenCode.DragonBall.Api/         # DragonBall CRUD API
│   │   ├── Program.cs                   # Minimal API endpoints, DI setup, middleware
│   │   ├── Data/
│   │   │   ├── DragonBallDbContext.cs   # EF Core DbContext
│   │   │   └── Migrations/             # EF Core migrations (dotnet ef migrations add)
│   │   ├── Endpoints/                   # REPR pattern: one file per endpoint
│   │   │   ├── Characters/
│   │   │   │   ├── Create.cs           # POST /api/dragonball/characters
│   │   │   │   ├── Get.cs              # GET /api/dragonball/characters/{id}
│   │   │   │   ├── List.cs             # GET /api/dragonball/characters
│   │   │   │   ├── Update.cs           # PUT /api/dragonball/characters/{id}
│   │   │   │   └── Delete.cs           # DELETE /api/dragonball/characters/{id}
│   │   │   ├── Transformations/
│   │   │   ├── Planets/
│   │   │   └── Fights/
│   │   ├── Models/                      # Domain entities
│   │   │   ├── Character.cs
│   │   │   ├── Transformation.cs
│   │   │   ├── Planet.cs
│   │   │   └── Fight.cs
│   │   ├── Dtos/                        # Request/Response DTOs
│   │   ├── Mapping/                     # AutoMapper profiles (if using AutoMapper)
│   │   ├── Validation/                  # FluentValidation validators
│   │   ├── Infrastructure/
│   │   │   ├── CorrelationIdMiddleware.cs
│   │   │   └── PagedResult.cs           # Shared pagination model
│   │   ├── appsettings.json
│   │   └── OpenCode.DragonBall.Api.csproj
│   │
│   ├── OpenCode.Music.Api/              # Music CRUD API (parallel structure)
│   │   ├── Program.cs
│   │   ├── Data/
│   │   │   ├── MusicDbContext.cs
│   │   │   └── Migrations/
│   │   ├── Endpoints/
│   │   │   ├── Artists/
│   │   │   ├── Albums/
│   │   │   ├── Songs/
│   │   │   └── Genres/
│   │   ├── Models/
│   │   ├── Dtos/
│   │   ├── Validation/
│   │   └── Infrastructure/              # (same shared infrastructure)
│   │
│   ├── OpenCode.Frontend/               # React SPA (Vite)
│   │   ├── src/
│   │   │   ├── api/                     # API client (fetch wrapper, auth headers)
│   │   │   │   ├── client.ts           # Base HTTP client with Correlation ID
│   │   │   │   ├── dragonball.ts       # DragonBall API calls
│   │   │   │   └── music.ts            # Music API calls
│   │   │   ├── auth/                    # OIDC login flow (redirect + callback)
│   │   │   │   ├── AuthProvider.tsx     # React context for auth state
│   │   │   │   ├── LoginPage.tsx
│   │   │   │   └── CallbackPage.tsx
│   │   │   ├── components/             # Shared UI components
│   │   │   │   ├── Layout.tsx
│   │   │   │   ├── Pagination.tsx
│   │   │   │   ├── DataTable.tsx
│   │   │   │   └── ...
│   │   │   ├── pages/                  # Page components
│   │   │   │   ├── DragonBall/
│   │   │   │   └── Music/
│   │   │   ├── hooks/                  # Custom React hooks
│   │   │   ├── types/                  # TypeScript type definitions
│   │   │   └── App.tsx
│   │   ├── package.json
│   │   ├── vite.config.ts
│   │   └── index.html
│   │
│   └── apisix/                          # APISIX configuration
│       ├── config.yaml                  # APISIX main config (admin key, plugins)
│       ├── routes/                      # Route definitions (Admin API format)
│       │   ├── dragonball-routes.yaml
│       │   └── music-routes.yaml
│       └── Dockerfile                   # Custom APISIX image with preloaded config
│
├── docker/                              # Docker support files
│   ├── init-scripts/
│   │   └── 001-create-schemas.sql      # CREATE SCHEMA IF NOT EXISTS dragonball/music/keycloak
│   └── keycloak/
│       └── realm-export.json            # Keycloak realm configuration (clients, roles, users)
│
├── docs/
│   ├── adr/                             # Architecture Decision Records
│   └── diagrams/                        # Architecture diagrams
│
├── tests/
│   ├── OpenCode.DragonBall.Api.Tests/   # Unit + Integration tests
│   ├── OpenCode.Music.Api.Tests/
│   └── OpenCode.Frontend.Tests/         # Component tests (Vitest)
│
├── .planning/                           # GSD project planning
│   ├── PROJECT.md
│   ├── config.json
│   ├── ROADMAP.md
│   ├── research/
│   │   ├── SUMMARY.md
│   │   ├── STACK.md
│   │   ├── FEATURES.md
│   │   ├── ARCHITECTURE.md
│   │   └── PITFALLS.md
│   └── phases/
│       └── ...
│
├── .gitignore
├── OpenCode.sln
└── README.md
```

### Structure Rationale

- **`OpenCode.AppHost/`:** The Aspire orchestrator. This is the entry point for running everything. It references all API projects and configures containers. One `dotnet run` from this project starts the entire system.
- **`OpenCode.ServiceDefaults/`:** Shared OpenTelemetry, health check, and service discovery configuration. Referenced by all API projects. Ensures consistent observability setup across all services.
- **`OpenCode.DragonBall.Api/` + `OpenCode.Music.Api/`:** Independent API projects. Each has its own `Program.cs`, `DbContext`, and endpoints. They share only `ServiceDefaults`. No direct dependency between them.
- **`Endpoints/` directory (REPR pattern):** Each REST operation is a single file. This scales better than monolithic controllers — easier to find, test, and modify individual endpoints. The pattern is: Route Handler (endpoint) → DbContext → Response.
- **`Infrastructure/`:** Cross-cutting concerns shared within an API project. Correlation ID middleware and pagination model go here. These are technical infrastructure, not business logic.
- **`apisix/` configuration directory:** APISIX config lives alongside the .NET code for version control. The `config.yaml` is mounted into the APISIX container. Routes can be loaded at startup or via Admin API after container starts.
- **`docker/init-scripts/`:** Database initialization scripts mounted into the PostgreSQL container. Run once on first database creation. `CREATE SCHEMA IF NOT EXISTS` ensures idempotent setup.

## Architectural Patterns

### Pattern 1: REPR (Request-Endpoint-Response) per Endpoint File

**What:** Each REST API endpoint gets its own file. The file defines the request model, endpoint handler, and response type. No controllers, no service layers unless complexity demands them.

**When to use:** For CRUD APIs where most endpoints are direct Create/Read/Update/Delete operations on database entities. Avoids the boilerplate of controllers+services+repositories when the logic is thin.

**Trade-offs:**
- + Easy to find, modify, and test individual endpoints
- + No "fat controller" problem — each file does one thing
- + Better AOT compatibility (no reflection-heavy controller activation)
- - Can lead to code duplication if endpoints share validation or transformation logic
- - Without discipline, can become disorganized (too many files)
- - Not suitable for complex business logic — if an endpoint has more than ~50 lines, extract a service

**Example:**
```csharp
// Endpoints/Characters/Create.cs
public record CreateCharacterRequest(
    string Name,
    string? Ki,
    string Race,
    string? Gender,
    string? Description,
    string? PlanetName
);

public static class CreateCharacterEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/dragonball/characters", async (
            CreateCharacterRequest request,
            DragonBallDbContext db,
            CancellationToken ct) =>
        {
            var character = new Character
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Ki = request.Ki,
                Race = request.Race,
                Gender = request.Gender,
                Description = request.Description,
                PlanetName = request.PlanetName,
                CreatedAt = DateTime.UtcNow
            };

            db.Characters.Add(character);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/dragonball/characters/{character.Id}", character);
        })
        .RequireAuthorization(Policies.EditorOnly);  // Protected write
    }
}
```

### Pattern 2: Public-Reads / Protected-Writes via APISIX Route Split

**What:** Two APISIX route definitions per URI prefix — one for read-only methods (no auth), one for write methods (OIDC auth required). This enforces the "anyone can read, only authed users can write" policy at the gateway level.

**When to use:** When the application requires public access to data but user accounts for write operations. This is the core auth model of the project.

**Trade-offs:**
- + Security at the gateway — unauthenticated requests never reach the .NET API for write operations
- + Clear separation of concerns — auth policy is defined in one place (APISIX routes)
- + NET API can implement fine-grained authorization without re-validating the JWT
- - Requires maintaining two route definitions per resource — more APISIX config
- - Must ensure OPTIONS preflight works for both route types (CORS applies to both)
- - User info extraction for read endpoints (e.g., "show my favorites") requires different mechanism

**Example APISIX config:**
```yaml
# APISIX route: Public reads
- uri: /api/dragonball/characters
  methods: ["GET", "HEAD", "OPTIONS"]
  upstream_id: dragonball-api
  plugins:
    cors:
      allow_origins: "http://localhost:5173"
      allow_credential: true

# APISIX route: Protected writes
- uri: /api/dragonball/characters
  methods: ["POST", "PUT", "PATCH", "DELETE"]
  upstream_id: dragonball-api
  plugins:
    cors:
      allow_origins: "http://localhost:5173"
      allow_credential: true
    openid-connect:
      client_id: dragonball-api
      client_secret: ${OIDC_CLIENT_SECRET}
      discovery: http://keycloak:8080/realms/opencode/.well-known/openid-configuration
      bearer_only: false
      session_secret: ${SESSION_SECRET}
```

### Pattern 3: Schema-Based Data Isolation (Single Database, Separate Schemas)

**What:** One PostgreSQL database with multiple schemas. Each service's EF Core DbContext targets its own schema via `HasDefaultSchema()`. Keycloak also uses its own schema. All data is in one database for operational simplicity, but logically isolated by schema.

**When to use:** When you have multiple services that share a database server but must not step on each other's tables. Ideal for projects with 2-5 services that don't need full database-per-service isolation. Also useful for "modular monolith" architectures.

**Trade-offs:**
- + Single database to backup, restore, and manage
- + No cross-database query limitations (can query across schemas if needed)
- + Lower resource usage than separate databases
- - Schema-level permissions are more complex than database-level permissions
- - No native schema-level replication or failover in PostgreSQL
- - Migrations must be coordinated (no schema conflicts)
- - Breaking into separate databases later (when services diverge) is a migration project

**Example EF Core configuration:**
```csharp
// DragonBall API
public class DragonBallDbContext : DbContext
{
    public DbSet<Character> Characters => Set<Character>();
    // ... other DbSets

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dragonball");
        // Entity configuration...
    }
}

// Connection string
// "Host=postgres;Database=opencode;Username=dragonball;Password=...;Search Schema=dragonball,public"
```

### Pattern 4: Correlation ID Middleware (End-to-End Request Tracing)

**What:** A piece of middleware that ensures every HTTP request has a unique Correlation ID (`X-Correlation-ID`). It reads the header from the incoming request (or generates one if absent), stores it in the ambient context, and ensures it's propagated to outgoing HTTP calls and response headers.

**When to use:** Always — this is low-cost infrastructure with high debugging value. Makes it possible to trace a single user request across all services.

**Trade-offs:**
- + Debugging: "Request X failed" → find all log entries with the same Correlation ID
- + Cheap: ~10 lines of middleware, ~5 lines of HttpClient delegation
- - None significant — this is pure infrastructure improvement
- - One caution: ensure the header name is consistent across all services (APISIX, .NET, React)

**Example:**
```csharp
// CorrelationIdMiddleware.cs
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string HeaderName = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existing)
            ? existing.FirstOrDefault()
            : Guid.NewGuid().ToString("N");

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.TryAdd(HeaderName, correlationId);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
```

## Data Flow

### Request Flow: Unauthenticated Read

```
Browser                             APISIX                            DragonBall API                   PostgreSQL
  │                                   │                                   │                               │
  │  GET /api/dragonball/characters   │                                   │                               │
  │  X-Correlation-ID: abc-123       │                                   │                               │
  │──────────────────────────────►    │                                   │                               │
  │                                   │  cors plugin → pass              │                               │
  │                                   │  request-id plugin → add trace   │                               │
  │                                   │──────────────────────────────────►│                               │
  │                                   │  X-Correlation-ID: abc-123       │                               │
  │                                   │  X-Request-ID: trace-456         │                               │
  │                                   │                                   │                               │
  │                                   │                                   │  SELECT * FROM characters     │
  │                                   │                                   │  LIMIT 10 OFFSET 0            │
  │                                   │                                   │───────────────────────────►   │
  │                                   │                                   │◄───────────────────────────   │
  │                                   │                                   │                               │
  │                                   │◄──────────────────────────────────│                               │
  │                                   │  200 OK                           │                               │
  │                                   │  X-Correlation-ID: abc-123       │                               │
  │◄──────────────────────────────    │                                   │                               │
  │  200 OK                           │                                   │                               │
  │  X-Correlation-ID: abc-123       │                                   │                               │
```

### Request Flow: Authenticated Write

```
Browser                             APISIX                               Keycloak                      DragonBall API          PostgreSQL
  │                                   │                                   │                               │                       │
  │  POST /api/dragonball/characters   │                                   │                               │                       │
  │  (no auth cookie)                 │                                   │                               │                       │
  │──────────────────────────────►    │                                   │                               │                       │
  │                                   │  openid-connect → no session      │                               │                       │
  │                                   │──────────────────────────────────►│  Redirect to Keycloak login  │                       │
  │                                   │◄──────────────────────────────────│                               │                       │
  │◄──────────────────────────────    │                                   │                               │                       │
  │  302 → Keycloak login page        │                                   │                               │                       │
  │                                   │                                   │                               │                       │
  │  [User logs in at Keycloak]       │                                   │                               │                       │
  │  [Keycloak sets session cookie]   │                                   │                               │                       │
  │                                   │                                   │                               │                       │
  │  POST /api/dragonball/characters   │                                   │                               │                       │
  │  (with session cookie)            │                                   │                               │                       │
  │──────────────────────────────►    │                                   │                               │                       │
  │                                   │  openid-connect → verify session  │                               │                       │
  │                                   │──────────────────────────────────►│  Introspect token             │                       │
  │                                   │◄──────────────────────────────────│  Valid: user X, roles Y       │                       │
  │                                   │                                   │                               │                       │
  │                                   │  cors plugin → pass               │                               │                       │
  │                                   │  request-id → trace               │                               │                       │
  │                                   │──────────────────────────────────►│                               │                       │
  │                                   │  X-Correlation-ID: abc-123       │  Check role: [Authorize]      │                       │
  │                                   │  X-User-Id: user-x               │  INSERT INTO characters       │                       │
  │                                   │  X-User-Roles: editor             │──────────────────────────►   │                       │
  │                                   │                                   │◄──────────────────────────   │                       │
  │                                   │◄──────────────────────────────────│  201 Created                   │                       │
  │◄──────────────────────────────    │                                   │                               │                       │
  │  201 Created                      │                                   │                               │                       │
  │  X-Correlation-ID: abc-123       │                                   │                               │                       │
```

### Key Data Flows

1. **Authentication Flow (OIDC Authorization Code):**
   Browser → APISIX (openid-connect plugin) → 302 redirect → Keycloak login → user authenticates → Keycloak issues auth code → redirect back to APISIX callback → APISIX exchanges code for tokens → session cookie set → original request proceeds upstream

2. **Query Data Flow (Unauthenticated Read):**
   Browser → APISIX (no auth on GET) → .NET API (with Correlation ID) → EF Core (Include/AsSplitQuery) → PostgreSQL (schema: dragonball) → response flows back through the chain

3. **Mutate Data Flow (Authenticated Write):**
   Browser → APISIX (OIDC validates session, adds user headers) → .NET API (user info from forwarded headers, [Authorize] checks role) → EF Core (tracking, save changes) → PostgreSQL (schema: dragonball) → response with correlation ID

4. **Observability Flow:**
   Every request: APISIX OTel plugin generates gateway span → forwards W3C traceparent to upstream → .NET OTel SDK creates child span → Npgsql instrumentation creates db span → all spans sent to OTLP collector → Aspire Dashboard / Jaeger

## Scaling Considerations

| Scale | Architecture Adjustments |
|-------|--------------------------|
| 0-1k users | Current architecture is sufficient. One replica of each API, single PostgreSQL instance, single APISIX node. All in Docker Compose |
| 1k-100k users | Add APISIX replicas (horizontal scale gateway). Switch to read replicas for PostgreSQL (one primary + one read replica). Add Redis cache for frequently accessed GET endpoints. Add Npgsql connection pooling tuning |
| 100k+ users | Move to Kubernetes. Split to separate database instances per schema if contention becomes an issue. Add CDN for static content. Consider event-driven architecture for write operations (queue-based load leveling) |

### Scaling Priorities

1. **First bottleneck:** PostgreSQL CPU from heavy list queries. Fix: add pagination (already planned), appropriate indexing, and read replicas.
2. **Second bottleneck:** Connection pool exhaustion. Fix: add DbContext pooling and connection string tuning (`MaxPoolSize`, `Connection Lifetime`).
3. **Third bottleneck:** Auth redirect latency under load. Fix: add APISIX replicas + load balancer, front Keycloak with APISIX + caching for OIDC discovery.

## Anti-Patterns

### Anti-Pattern 1: Calling `MigrateAsync()` on Startup in Containers

**What people do:** Call `dbContext.Database.MigrateAsync()` in `Program.cs` to auto-apply migrations on startup.

**Why it's wrong:** Race conditions in multi-replica deployments, crashes if database isn't ready, "model mismatch" errors if different versions deploy during rolling update.

**Do this instead:** Generate idempotent SQL migration scripts and apply them via a separate init container or CI/CD step. In development, apply migrations via `dotnet ef database update` manually or through Aspire's lifecycle hooks.

### Anti-Pattern 2: Putting CORS Configuration on the .NET API Instead of APISIX

**What people do:** Add `app.UseCors()` in .NET API's `Program.cs` with origins, methods, and headers.

**Why it's wrong:** The .NET API is behind APISIX — all external requests come from APISIX, not from the browser directly. CORS on the .NET API either a) never matches (because the origin is the APISIX container, not the browser), or b) duplicates CORS headers if APISIX also adds them. The browser sees the APISIX response, not the .NET API response.

**Do this instead:** Configure CORS only on APISIX via the `cors` plugin. Never add `app.UseCors()` to the .NET APIs.

### Anti-Pattern 3: Deeply Nested `.Include()` Calls Without `.AsSplitQuery()`

**What people do:** Write `context.Characters.Include(c => c.Transformations).Include(c => c.Fights).ThenInclude(f => f.Opponent).ToListAsync()`

**Why it's wrong:** EF Core generates a single SQL query with multiple JOINs. When including multiple collections, this creates a Cartesian product: if a character has 5 transformations and 10 fights, the result is 50 rows. Real databases with hundreds of records produce millions of intermediate rows — slow queries, massive memory usage.

**Do this instead:** Use `.AsSplitQuery()` to generate separate SQL queries per collection include. Or use `.ProjectTo()` / `.Select()` to fetch only needed fields. For list endpoints, avoid including collections altogether — use separate endpoints for related data.

## Integration Points

### External Services

| Service | Integration Pattern | Notes |
|---------|---------------------|-------|
| **PostgreSQL** | Direct TCP connection via Npgsql | Connection string injected via Aspire's `WithReference(postgres)`. Search path set per API. SSL disabled in dev, required in production |
| **Keycloak** | OIDC discovery URL read by APISIX openid-connect plugin | Internal Docker network URL: `http://keycloak:8080/realms/opencode/.well-known/openid-configuration`. Admin API only accessible from AppHost |
| **APISIX Admin API** | HTTP REST API at port 9180 | Used by CI/CD to push route configs. Must be secured with admin key + IP whitelist. Not exposed to host in production |
| **OTLP Collector** | gRPC/HTTP export from APISIX + .NET SDK | Aspire provides the collector endpoint. APISIX configured with `collector.address` in OTel plugin |
| **APISIX etcd** | gRPC internal (APISIX ↔ etcd) | Both are internal containers. No external access needed |

### Internal Boundaries

| Boundary | Communication | Notes |
|----------|---------------|-------|
| React ↔ APISIX | HTTP (REST + OIDC redirects) | Only HTTP method. CORS handled at APISIX. Auth via cookies (browser flow) or Bearer token (API client flow) |
| APISIX ↔ DragonBall API | HTTP (REST) | APISIX forwards validated requests. Adds X-User-Id, X-User-Roles headers after OIDC validation for protected routes |
| APISIX ↔ Music API | HTTP (REST) | Same pattern as DragonBall API |
| DragonBall API ↔ PostgreSQL | TCP (Npgsql) | Connection via Docker service name `postgres:5432`. Schema: `dragonball` |
| Music API ↔ PostgreSQL | TCP (Npgsql) | Connection via `postgres:5432`. Schema: `music` |
| Keycloak ↔ PostgreSQL | TCP (JDBC) | Connection via `postgres:5432`. Schema: `keycloak`. Uses different database user than APIs |
| APISIX ↔ Keycloak | HTTP (OIDC) | OIDC discovery, token introspection, userinfo endpoint calls |
| AppHost ↔ All services | .NET resource references | Aspire manages dependency ordering, connection string injection, health check wiring |

## Sources

- **APISIX architecture docs:** https://apisix.apache.org/docs/apisix/architecture-design/plugin (HIGH confidence)
- **APISIX openid-connect plugin configuration:** https://apisix.apache.org/docs/apisix/plugins/openid-connect/ (HIGH confidence)
- **APISIX cors plugin:** https://apisix.apache.org/docs/apisix/plugins/cors/ (HIGH confidence)
- **Keycloak 26.x Server Installation:** https://www.keycloak.org/server/installation (HIGH confidence)
- **Keycloak Database Configuration:** https://www.keycloak.org/server/db (HIGH confidence)
- **EF Core DbContext Lifetime and Pooling:** https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/ (HIGH confidence)
- **EF Core Migration Application Strategies:** https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying (HIGH confidence)
- **EF Core Split Queries:** https://learn.microsoft.com/en-us/ef/core/querying/single-split-queries (HIGH confidence)
- **Npgsql Connection Resiliency:** https://www.npgsql.org/doc/connection-resiliency.html (HIGH confidence)
- **.NET Aspire Orchestration:** https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/orchestration-overview (MEDIUM confidence)
- **.NET Aspire Container Networking (inner-loop):** https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking/overview (MEDIUM confidence)
- **OpenTelemetry .NET SDK:** https://opentelemetry.io/docs/languages/net/ (HIGH confidence)
- **OpenTelemetry W3C Trace Context:** https://www.w3.org/TR/trace-context/ (HIGH confidence)
- **REPR Pattern (Request-Endpoint-Response):** Community pattern popularized by Steve Smith / Ardalis — https://github.com/ardalis/REPRPattern (LOW confidence — community pattern, not official Microsoft)
- **PostgreSQL Schema best practices:** https://www.postgresql.org/docs/current/ddl-schemas.html (HIGH confidence)
- **PROJECT.md:** OpenCode project requirements and architecture definition (HIGH confidence — authoritative)

---

*Architecture research for: OpenCode .NET 10 + Aspire + Keycloak + APISIX + PostgreSQL stack*
*Researched: 2026-04-24*
