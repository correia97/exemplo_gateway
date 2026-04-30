# Phase 11: Integration Tests with TestContainers - Context

**Gathered:** 2026-04-29
**Status:** Ready for planning (replan)

<domain>
## Phase Boundary

Integration tests using TestContainers for PostgreSQL that validate repositories against a real database, verify full API endpoint E2E flows through TestServer, confirm schema isolation, and validate correlation ID propagation. Covers the integration level of the test pyramid — complements Phase 10's unit tests.

</domain>

<decisions>
## Implementation Decisions

### Fixture & Container Lifecycle
- **D-01:** **Per-class container** — one PostgreSQL TestContainer per test class, starts before all tests in the class, stops after. Each class gets a fresh database with applied migrations.
- **D-02:** Database setup: apply EF Core migrations on container start + seed a shared baseline dataset (e.g., a few genres and a character), then each test adds its own data on top.

### Integration Test Scope
- **D-03:** **Full scope** — repository integration tests (5 repos × CRUD + filtering + pagination), API E2E tests (full CRUD cycles via TestServer), schema isolation tests, and correlation ID propagation tests. Approximately ~57 tests.
- **D-04:** Real DB repository tests complement (not replace) the Phase 10 in-memory EF Core tests — validates that actual PostgreSQL behavior matches in-memory behavior, especially for schema isolation, default values, and SQL-specific features.
- **D-05:** Test patterns from Phase 10 carry forward: per-class files (D-13), inline test data (D-14), AAA pattern (D-15), no shared state (D-16).

### Schema Isolation Testing
- **D-06:** **Combined approach** — raw SQL via Npgsql for negative tests (cross-schema queries MUST fail), EF Core for positive tests (schema-scoped queries work correctly).
- **D-07:** Raw SQL negative tests use separate `NpgsqlConnection` with different schema user credentials to verify `dragonball_user` cannot query `music` schema tables and vice versa.
- **D-08:** EF Core positive tests verify each DbContext's `HasDefaultSchema()` correctly scopes queries to its own schema.

### the agent's Discretion
- Exact TestContainers version selection (4.x series recommended)
- Container resource configuration (CPU/memory limits)
- Test timeout configuration for E2E tests
- Exact seed data content for the shared baseline dataset

</decisions>

<canonical_refs>
## Canonical References

### Requirements
- `.planning/REQUIREMENTS.md` — TEST-05 through TEST-08

### Prior Phase Context
- `.planning/phases/10-unit-tests/10-CONTEXT.md` — Phase 10 decisions (test patterns, coverage scope, D-12 deferral of real DB tests to this phase)
- `.planning/phases/02-database/02-CONTEXT.md` — Entity definitions, schema separation, `HasDefaultSchema()` config
- `.planning/phases/04-keycloak-authentication-authorization/04-CONTEXT.md` — Claims transformation pattern

### Existing Test Infrastructure
- `tests/OpenCode.Api.Tests/` — Phase 10 API test project (patterns to follow for integration tests)
- `tests/OpenCode.Api.Tests/Repositories/` — In-memory repository tests (will be complemented by real DB tests)
- `tests/OpenCode.Api.Tests/Services/CorrelationIdMiddlewareTests.cs` — TestHost pattern for middleware tests
- `src/OpenCode.Domain/Data/DragonBallContext.cs` — DragonBall schema with `HasDefaultSchema("dragonball")`
- `src/OpenCode.Domain/Data/MusicContext.cs` — Music schema with `HasDefaultSchema("music")`

### Existing Plans
- `.planning/phases/11-testcontainers-integration/11-01-PLAN.md` — Scaffold project + fixture + base class
- `.planning/phases/11-testcontainers-integration/11-02-PLAN.md` — Repository integration tests
- `.planning/phases/11-testcontainers-integration/11-03-PLAN.md` — API E2E + schema isolation + correlation ID

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `tests/OpenCode.Api.Tests/Repositories/` — 5 in-memory repository test files with CRUD + filtering test patterns (can be adapted for real DB by changing the context factory)
- `tests/OpenCode.Api.Tests/Services/CorrelationIdMiddlewareTests.cs` — TestServer pattern using `WebApplication.CreateBuilder() + UseTestServer()`
- `AutoValidationFilter<T>` in `tests/OpenCode.Api.Tests/Services/ValidationPipelineTests.cs` — `IEndpointFilter` pattern for endpoint filter tests

### Established Patterns
- Per-class test files, each test creates its own data, AAA pattern (Phase 10, D-13 through D-16)
- `DbContextOptionsBuilder<T>.UseInMemoryDatabase()` for unit tests (Phase 10)
- For integration: same pattern but with `UseNpgsql()` pointing to TestContainer's dynamic connection string
- TestServer via `WebApplication.CreateBuilder().WebHost.UseTestServer()` for HTTP-level tests

### Integration Points
- Requires `.csproj` with `TestContainers.PostgreSQL` and `Microsoft.AspNetCore.Mvc.Testing` packages
- Requires `Directory.Packages.props` entries for new packages
- Requires partial `Program` class (ApiMarker) in each API project for `WebApplicationFactory<TEntryPoint>`
- PostgresFixture implements `IAsyncLifetime` (xUnit v3 pattern) for container lifecycle
- Each test creates a fresh logical database via `CREATE DATABASE` or truncates tables between classes

</code_context>

<specifics>
## Specific Ideas

- "Use per-class container for isolation — faster than per-test, cleaner than global singleton"
- "Combine raw SQL negative tests with EF Core positive tests for schema isolation coverage"
- "Full E2E coverage: repos + endpoints + schema + correlation ID — trusted integration coverage"

</specifics>

<deferred>
## Deferred Ideas

- Performance/load testing — out of scope for PoC
- Multi-node TestContainers (e.g., Kong + Keycloak in containers for full stack E2E) — future phase
- Database migration rollback tests — not needed for PoC

</deferred>

---

*Phase: 11-testcontainers-integration*
*Context gathered: 2026-04-29*
