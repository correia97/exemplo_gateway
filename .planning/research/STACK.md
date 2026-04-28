# Stack Research

**Domain:** .NET Microservices with API Gateway (APISIX) + Keycloak Auth + PostgreSQL + Aspire Orchestration
**Researched:** 2026-04-24
**Confidence:** HIGH

## Recommended Stack

### Core Technologies

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| .NET | 10.0 | Application runtime | Latest LTS with native AOT improvements, minimal APIs, and superior Aspire integration. .NET 10 is the natural evolution of the .NET 8/9 line with better container support and OpenTelemetry primitives built in |
| ASP.NET Core Minimal APIs | 10.0 | HTTP API framework | Lighter than MVC controllers for CRUD APIs, better AOT compatibility, first-class Aspire support. Use controller-based if your API has complex validation or many endpoints — Minimal APIs are the modern default |
| Apache APISIX | 3.11+ | API Gateway | Best OSS API gateway for Kubernetes/Docker with native OIDC (openid-connect plugin), dynamic routing via Admin API, and OpenTelemetry support. Significantly lighter than Kong (no Cassandra dependency needed) and more configurable than Ocelot/YARP for auth integration |
| Keycloak | 26.1+ | Identity & Access Management | Gold standard OSS OIDC/OAuth2 provider. Supports OIDC, OAuth2, SAML2, social login, user federation. 26.x uses Quarkus (not WildFly) — faster startup, better container support, but requires different config than older docs |
| PostgreSQL | 17.x | Primary database | Best OSS relational database. EF Core + Npgsql support is excellent. Schema-based multi-tenancy support (separate schemas for dragonball/music/keycloak). JSONB support for flexible data needs |
| .NET Aspire | 10.0 | Cloud-native orchestration | Microsoft's official opinionated stack for .NET cloud-native apps. Handles service discovery, container orchestration, health checks, and OpenTelemetry wiring. The "glue" that connects all components |
| React | 19.x | Frontend framework | Industry standard for SPA development. Vite as build tool for faster dev experience. No SSR needed since authentication is handled by APISIX/Keycloak redirect |

### Supporting Libraries

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.x | EF Core provider for PostgreSQL | Required — every API project uses it |
| Npgsql | 10.0.x | ADO.NET provider for PostgreSQL | Required — dependency of EF Core provider. Version must match EF Core provider exactly |
| OpenTelemetry.Extensions.Hosting | 1.11.x | OTel SDK integration for .NET | Required — wired automatically by Aspire if using `AddOpenTelemetryExporters()` |
| OpenTelemetry.Instrumentation.AspNetCore | 1.11.x | ASP.NET Core request tracing | Required — captures incoming request spans |
| OpenTelemetry.Instrumentation.Npgsql | 1.11.x | PostgreSQL query tracing | Recommended — captures SQL query spans |
| OpenTelemetry.Exporter.OpenTelemetryProtocol | 1.11.x | OTLP exporter for OTel collector | Required — sends traces to OTel collector |
| Microsoft.EntityFrameworkCore.Design | 10.0.x | EF Core migrations CLI tools | Development dependency — needed for `dotnet ef migrations add` |
| Swashbuckle.AspNetCore | 7.x | OpenAPI/Swagger generation | Development — API documentation. Optional if using .NET 10's built-in OpenAPI support (Microsoft.OpenApi) |
| AutoMapper | 14.x | Object-object mapping | When APIs need DTO projection. Alternative: use `.Select()` projections directly in EF Core queries |
| FluentValidation | 11.x | Request validation | For complex input validation beyond data annotations. Alternative: Minimal API endpoint filters |
| Serilog.AspNetCore | 9.x | Structured logging | When you need more than `ILogger<T>` supports. Alternative: built-in ASP.NET Core logging is often sufficient |
| Aspire.Hosting.PostgreSQL | 10.0.x | Aspire PostgreSQL integration | Required — PostgreSQL resource management in Aspire |
| Aspire.Hosting.Keycloak | 10.0.x | Aspire Keycloak integration | Required — Keycloak resource management in Aspire |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.x | JWT validation in .NET APIs | For fine-grained authorization in .NET (after APISIX validates the token at the gateway) |

### Development Tools

| Tool | Purpose | Notes |
|------|---------|-------|
| Docker Desktop / Podman | Local container runtime | Required for Aspire inner-loop with containers |
| dotnet-ef CLI | EF Core migrations | `dotnet tool install --global dotnet-ef` — generate and apply migrations |
| .NET Aspire workload | Aspire project templates | `dotnet workload install aspire` — creates AppHost, ServiceDefaults projects |
| APISIX Dashboard (optional) | Visual APISIX management | Use Admin API directly in automation; dashboard for debugging only |
| Jaeger UI | Trace visualization | Built-in via Aspire's OTel forwarding. Alternative: Grafana Tempo |
| pgAdmin / DBeaver | Database management | Visual schema inspection, query debugging, migration verification |

## Installation

```bash
# Prerequisites: .NET 10 SDK, Docker Desktop

# Install Aspire workload
dotnet workload install aspire

# Install EF Core tools globally
dotnet tool install --global dotnet-ef

# Create solution structure
dotnet new sln -n OpenCode

# Create AppHost (Aspire orchestrator)
dotnet new aspire-apphost -n OpenCode.AppHost

# Create ServiceDefaults (shared OTel, health checks)
dotnet new aspire-servicedefaults -n OpenCode.ServiceDefaults

# Create API projects
dotnet new webapi -n OpenCode.DragonBall.Api
dotnet new webapi -n OpenCode.Music.Api

# Create React frontend
npm create vite@latest OpenCode.Frontend -- --template react-ts

# Add NuGet packages (per project)
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Aspire.Hosting.PostgreSQL
dotnet add package Aspire.Hosting.Keycloak
dotnet add package OpenTelemetry.Extensions.Hosting

# EF Core packages (for API projects)
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

## Alternatives Considered

| Recommended | Alternative | When to Use Alternative |
|-------------|-------------|-------------------------|
| APISIX (openid-connect plugin) | YARP / Ocelot (.NET reverse proxies) | If you want to stay entirely in .NET ecosystem and don't need Keycloak OIDC at the gateway. YARP is simpler but requires writing custom middleware for OIDC integration. APISIX is better for this project because OIDC at gateway reduces .NET API complexity |
| APISIX (openid-connect plugin) | Kong API Gateway | Kong has larger ecosystem but requires Cassandra/PostgreSQL for its own DB. APISIX uses etcd (lighter) and has equal OIDC support. For this project size, APISIX's lower operational overhead wins |
| Keycloak 26.x | Auth0 / Firebase Auth (SaaS) | If you don't want to self-host identity. Keycloak is chosen because the project runs fully on-prem/self-hosted with no external SaaS dependencies. Auth0 provides better UX but costs scale with users |
| PostgreSQL 17 | SQLite / SQL Server | SQLite is simpler for dev but doesn't support schemas (needed for this project's data isolation). SQL Server is heavier and costs in production. PostgreSQL wins for schema-based multi-tenancy and dev/prod parity |
| .NET 10 (Minimal APIs) | .NET 10 (MVC Controllers) | Use Minimal APIs for simpler CRUD endpoints. Switch to Controllers if the API grows beyond ~20 endpoints or needs complex model binding/validation. Both are supported — can mix within a project |
| Docker Compose (via Aspire) | Kubernetes | For production at 0-1000 users, Docker Compose is simpler and sufficient. Migrate to Kubernetes when you need auto-scaling, rolling deployments, or multi-node orchestration |
| FluentValidation | Data Annotations (`[Required]`, `[StringLength]`) | Data annotations are simpler for straightforward validation. Use FluentValidation when you need complex cross-field validation, conditional rules, or reusable validation logic |
| AutoMapper | Manual mapping / `Select()` projections | AutoMapper reduces boilerplate for simple DTO mapping but adds complexity. For most CRUD endpoints, EF Core's `.Select()` projection is simpler and more transparent. Use AutoMapper only when mapping between deeply different shapes |

## What NOT to Use

| Avoid | Why | Use Instead |
|-------|-----|-------------|
| Entity Framework Core (with Npgsql) older than 10.x | Version mismatch with .NET 10 causes runtime errors and missing APIs | Always use EF Core + Npgsql packages that match the .NET version (10.0.x) |
| `Microsoft.AspNetCore.Identity` + EF Core | This project uses Keycloak for identity, not ASP.NET Core Identity. They'd conflict at the DB schema level and add complexity | Keycloak handles all identity; .NET APIs use JWT Bearer tokens validated by APISIX |
| Dapper for all queries | While faster than EF Core, Dapper requires manual SQL and mapping. For a CRUD API with moderate complexity, EF Core's developer productivity (change tracking, migrations, LINQ) outweighs the perf difference | EF Core for CRUD; Dapper only for specific high-performance read paths if profiling shows a bottleneck |
| MySQL / MariaDB | EF Core support is weaker, Npgsql is more mature, and PostgreSQL has better schema/multi-tenancy support | PostgreSQL 17 |
| WildFly-based Keycloak (pre-20) | End-of-life, no security patches, different config model. Tutorials using `standalone.sh` etc. are outdated | Keycloak 26.x (Quarkus-based) Docker image from quay.io |
| Istio / Linkerd (service mesh) | Overkill for a single-node or Docker Compose deployment. Adds significant operational complexity without benefit at this scale | APISIX handles gateway concerns; service mesh is only needed at Kubernetes + multi-service scale |
| RabbitMQ / Kafka | The project has no event-driven requirements yet. Adding a message broker before needed creates infrastructure debt | Simple HTTP for now; add messaging only when async workflows are explicitly needed |

## Stack Patterns by Variant

**If database becomes a bottleneck (>50k users or >10k writes/second):**
- Move to read replicas: PostgreSQL streaming replication
- Add Redis cache layer in front of GET endpoints (using `IDistributedCache`)
- No architectural change needed — EF Core + Npgsql handles read replicas via connection string

**If you need event-driven features (notifications, webhooks, async processing):**
- Add a lightweight message broker: NATS (simple, fast) or RabbitMQ (familiar, well-supported)
- Use `MassTransit` or `NServiceBus` for .NET integration
- Keep APISIX as the entry point; use internal pub/sub for service-to-service

**If moving to Kubernetes:**
- Replace Aspire AppHost with Helm charts + Kustomize
- Use APISIX's Kubernetes Ingress Controller (apisix-ingress-controller)
- Replace Docker Compose health checks with K8s liveness/readiness probes
- Keep all other stack components (Keycloak, PostgreSQL, OTel) — they're K8s-ready

## Version Compatibility

| Package | Compatible With | Notes |
|---------|-----------------|-------|
| Npgsql.EntityFrameworkCore.PostgreSQL 10.0.x | .NET 10.0, Npgsql 10.0.x | Must use exact major version match. Npgsql 9.x with EF Core 10.x will fail |
| Aspire.Hosting.PostgreSQL 10.0.x | .NET 10.0 AppHost | Provided via Aspire workload; version is tied to .NET 10 SDK version |
| Aspire.Hosting.Keycloak 10.0.x | .NET 10.0 AppHost | Same — part of Aspire workload |
| Keycloak 26.x | PostgreSQL 12+ (any version) | Keycloak works with PostgreSQL 12 through 17. No driver mismatch issues |
| APISIX 3.11+ | etcd 3.5+ | APISIX uses etcd for config storage. Docker Compose setup includes an etcd container |
| APISIX openid-connect plugin | Keycloak 26.x | OIDC is a standard protocol — any OIDC provider works. No version coupling between APISIX and Keycloak |
| Serilog 9.x | .NET 10.0 | Serilog targets .NET Standard 2.0+ — compatible with .NET 10 |
| AutoMapper 14.x | .NET 10.0 | AutoMapper 14 supports .NET 8+ |

## Sources

- **Context7 (ASP.NET Core, EF Core):** Resolved library IDs `aspnetcore`, `efcore` — confirmed patterns for Minimal APIs, DbContext pooling, and OpenTelemetry integration (HIGH confidence)
- **APISIX openid-connect plugin docs:** https://apisix.apache.org/docs/apisix/plugins/openid-connect/ (HIGH confidence)
- **APISIX configuration reference:** https://apisix.apache.org/docs/apisix/architecture-design/plugin (HIGH confidence)
- **Keycloak 26.x server configuration:** https://www.keycloak.org/server/all-config (HIGH confidence)
- **Keycloak Docker image:** https://www.keycloak.org/server/containers (HIGH confidence)
- **Npgsql EF Core provider:** https://www.npgsql.org/efcore/index.html (HIGH confidence)
- **Npgsql connection string parameters:** https://www.npgsql.org/doc/connection-string-parameters.html (HIGH confidence)
- **.NET Aspire documentation:** https://learn.microsoft.com/en-us/dotnet/aspire/ (MEDIUM confidence — relies on official MS docs, version-specific features may vary)
- **PostgreSQL Docker image:** https://hub.docker.com/_/postgres (HIGH confidence)
- **OpenTelemetry .NET SDK:** https://opentelemetry.io/docs/languages/net/ (HIGH confidence)

---

*Stack research for: OpenCode .NET 10 + Aspire + Keycloak + APISIX + PostgreSQL stack*
*Researched: 2026-04-24*
