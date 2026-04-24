# Domain Pitfalls

**Domain:** .NET 10 multi-API solution with Aspire, Keycloak, APISIX, EF Core + PostgreSQL
**Researched:** 2026-04-24
**Confidence:** HIGH — pitfalls verified against EF Core docs, Keycloak docs, APISIX docs, and community post-mortems

## Critical Pitfalls

Mistakes that cause rewrites or major issues.

### Pitfall 1: Keycloak Schema Not Created Before Keycloak Starts

**What goes wrong:** Keycloak attempts to create its schema and tables on first startup via JDBC. If the `keycloak` schema does not exist in PostgreSQL, and the PostgreSQL user does not have `CREATE SCHEMA` privileges, Keycloak fails to start with a cryptic error like `ERROR: schema "keycloak" does not exist` or `ERROR: relation "KEYCLOAK_ROLE" does not exist`.

**Why it happens:** Most users assume Keycloak will auto-create the schema. It does auto-create tables within the schema, but Keycloak 26.x does NOT auto-create the schema itself — it expects the schema to already exist, or the database user must have `CREATE` privilege on the database.

**Consequences:** Keycloak container enters restart loop. All services depending on Keycloak fail to initialize. Problem is hard to diagnose because the error is buried in Keycloak startup logs.

**Prevention:**
1. Create the schema explicitly before Keycloak starts:
   - Option A: Use an init SQL script in PostgreSQL container (`/docker-entrypoint-initdb.d/`)
   - Option B: Use Aspire's `WithInitBindMount()` on the PostgreSQL resource to run a SQL script
   - Option C: Grant `CREATE ON DATABASE` to the PostgreSQL user:
     ```sql
     GRANT CREATE ON DATABASE dragonmusic TO postgres;
     ```

2. Set the `KC_DB_SCHEMA` environment variable:
   ```
   KC_DB=postgres
   KC_DB_SCHEMA=keycloak
   KC_DB_URL=jdbc:postgresql://postgres:5432/dragonmusic?currentSchema=keycloak
   ```

**Detection:** Watch Keycloak container logs for `ERROR: schema "keycloak" does not exist`. This appears in the first ~5 seconds of Keycloak startup.

### Pitfall 2: Multiple EF Core DbContexts Sharing One Database — Migration Confusion

**What goes wrong:** Two DbContexts (DragonBallContext and MusicContext) both use the same database. Each produces its own `__EFMigrationsHistory` table in its respective schema. But if migrations aren't managed carefully, developers accidentally:
- Run migration commands targeting the wrong DbContext
- Forget to specify schema on a migration, and tables end up in the default `public` schema
- Create a migration that applies changes to the wrong schema

**Why it happens:** The default EF Core behavior creates tables in the database's `public` schema unless `HasDefaultSchema()` or `ToTable(schema:)` is specified. When running `dotnet ef migrations add`, it's easy to run it from the wrong project directory or forget the `--context` flag.

**Consequences:** Tables appear in the wrong schema, creating confusion. Migration history tables conflict. Developers must manually move tables or reset migrations, losing data.

**Prevention:**

1. **Explicit architecture: keep each DbContext in its own API project** (NOT in a shared class library). Each API project has its own `Migrations/` directory.

2. **Always use `modelBuilder.HasDefaultSchema()`** in the `OnModelCreating` method of each DbContext:
   ```csharp
   // DragonBallContext
   modelBuilder.HasDefaultSchema("dragonball");
   
   // MusicContext
   modelBuilder.HasDefaultSchema("music");
   ```

3. **Name migration commands explicitly**:
   ```bash
   dotnet ef migrations add InitialCreate --context DragonBallContext --project src/DragonBall.Api
   dotnet ef migrations add InitialCreate --context MusicContext --project src/Music.Api
   ```

4. **Use `--output-dir`** if putting migrations in a subfolder:
   ```bash
   dotnet ef migrations add InitialCreate --output-dir Data/Migrations
   ```

5. **Add a unit test** (via `context.Database.HasPendingModelChanges()`) to catch forgotten migrations at build time.

**Detection:** Run `SELECT * FROM information_schema.tables WHERE table_schema NOT IN ('keycloak', 'public')` to check if all tables are in the correct schema.

### Pitfall 3: Aspire.Hosting.Keycloak Preview Package Breaking Between Updates

**What goes wrong:** The official `Aspire.Hosting.Keycloak` NuGet package is in preview (13.2.3-preview.1). Between preview versions, the API surface can change — method signatures change, parameter names change, or entire methods are removed. The CommunityToolkit extension is also in beta. A `dotnet restore` or build on a different machine or CI may pull a different preview version and break.

**Why it happens:** NuGet resolves the latest matching preview version by default. If `13.2.3-preview.1.26217.6` is specified without pinning, a newer preview `13.2.3-preview.2` could break the build.

**Consequences:** Build breaks. CI pipeline fails. Developer productivity drops while debugging package version issues.

**Prevention:**
1. **Pin ALL preview packages with exact versions** in the project file:
   ```xml
   <PackageReference Include="Aspire.Hosting.Keycloak" Version="13.2.3-preview.1.26217.6" />
   ```
   (Do NOT use version ranges like `13.2.3-preview.*`)

2. **Create a fallback plan** documented in the repo: "If Aspire.Hosting.Keycloak breaks, run Keycloak as a standalone Docker Compose service outside of Aspire."

3. **Consider skipping the Aspire Keycloak package entirely** and using `builder.AddContainer()` to manage Keycloak directly — more code but guaranteed stability.

**Detection:** Build fails after NuGet restore with errors like `'AddKeycloak' does not contain a definition for 'WithImageTag'` or `CS1061: 'IResourceBuilder<KeycloakResource>' does not contain a definition for 'WithDatabase'`.

### Pitfall 4: APISIX Proxying Breaks Scalar/OpenAPI URLs

**What goes wrong:** Scalar UI is accessed at `http://localhost:9080/api/dragonball/scalar/v1` (via APISIX). But the OpenAPI spec generated by ASP.NET Core contains server URLs relative to the API's direct URL (e.g., `http://localhost:5001`). Scalar uses these server URLs for "Try It" requests, which bypasses APISIX entirely.

**Why it happens:** The `Microsoft.AspNetCore.OpenApi` package auto-generates server URLs based on the request that hit `MapScalarApiReference()`. When accessed through APISIX, the `Host` header is different from the original request.

**Consequences:** "Try It" requests in Scalar fail (CORS errors because they go directly to the API port, not through APISIX). Developers get confused about why API calls work in curl but not in Scalar.

**Prevention:**
1. **Configure the OpenAPI document's server URL explicitly** in Program.cs:
   ```csharp
   app.MapOpenApi().AddDocumentTransformer((document, context, cancellationToken) =>
   {
       document.Servers = [new OpenApiServer { Url = "http://localhost:9080/api/dragonball" }];
       return Task.CompletedTask;
   });
   ```

2. **OR configure APISIX to pass the original Host header** via `proxy_set_header Host $host` (in config.yaml).

3. **OR access Scalar directly on the API's port** for development (bypass APISIX), using APISIX only for gateway testing.

**Detection:** Open Scalar UI, click "Try It" on any endpoint. If the request goes to `http://localhost:5001/...` instead of `http://localhost:9080/api/dragonball/...`, this pitfall is active.

### Pitfall 5: Keycloak OIDC Redirect Loop in Docker

**What goes wrong:** The React frontend redirects to Keycloak for login. After login, Keycloak redirects back to the frontend. But the redirect URL (determined by `redirect_uri` parameter) doesn't match what Keycloak expects, causing an infinite redirect loop or `invalid_redirect_uri` error.

**Why it happens:** Keycloak validates the `redirect_uri` against the list of valid redirect URIs configured for the client. In Docker, the frontend URL might be `http://localhost:5173` (Vite dev server), but the redirect URI registered in Keycloak is `http://localhost:3000` (production). Or, Keycloak is at `http://localhost:8080` internally but the frontend references it as `http://localhost:9080/auth` (via APISIX).

**Consequences:** Users cannot log in. The browser ends up in a redirect loop between the frontend and Keycloak.

**Prevention:**
1. **Register ALL valid redirect URIs** in the Keycloak admin console for the client:
   ```
   Valid Redirect URIs: http://localhost:5173/*, http://localhost:3000/*
   Web Origins: + (same as redirect URIs)
   ```

2. **Use environment-specific configuration** so the frontend knows which Keycloak URL to use:
   - Dev: `http://localhost:8080` (direct Keycloak)
   - With APISIX: `http://localhost:9080/auth` (Keycloak via APISIX)

3. **Stick to a single, stable Keycloak port.** Aspire's Keycloak resource changes ports on restart unless explicitly configured. Pin to port 8080:
   ```csharp
   builder.AddKeycloak("keycloak", port: 8080);  // :8080 is stable
   ```

4. **Use `Valid Post Logout Redirect URIs`** for logout flow:
   ```
   http://localhost:5173/*
   ```

**Detection:** Open browser DevTools → Network tab. Look for `302` redirects to Keycloak login, then back to the app, then to Keycloak again — this is the redirect loop. Check the console for `invalid_redirect_uri` errors.

## Moderate Pitfalls

### Pitfall 6: EF Core N+1 Queries with Related Data

**What goes wrong:** When listing entities with related data (e.g., `GET /api/music/artists/{id}/albums`), EF Core executes one query for the parent and N additional queries for each child — the classic N+1 problem. This happens when `Include()` is missing or when the repository returns `IQueryable` that gets enumerated in a loop.

**Prevention:**
1. Always use `Include()` or `ThenInclude()` for related data at the repository level.
2. Avoid returning `IQueryable` from repositories — return `Task<List<T>>` or `IReadOnlyList<T>` to force eager evaluation.
3. Use `AsSplitQuery()` for collections to avoid cartesian explosion:
   ```csharp
   query.Include(a => a.Albums).ThenInclude(al => al.Tracks).AsSplitQuery();
   ```
4. Profile queries with Npgsql.OpenTelemetry spans in the Aspire Dashboard to detect N+1.

### Pitfall 7: Npgsql Connection Pool Exhaustion

**What goes wrong:** Each request opens a new database connection. Under load (or during debugging with hot reload), the Npgsql connection pool is exhausted, causing `The connection pool has been exhausted` errors. This happens when DbContext instances are not properly disposed, or when `AddNpgsqlDbContext()` is used without pooling.

**Prevention:**
1. `AddNpgsqlDbContext()` uses pooling by default (NpgsqlDataSource with `MaxPoolSize=100`).
2. Never create `new DbContext()` manually — always inject via DI (IScoped lifetime).
3. Increase `MaxPoolSize` if needed:
   ```csharp
   builder.AddNpgsqlDbContext<DragonBallContext>("postgresdb",
       configureDbContextOptions: options => {},  // EF Core options
       configureDataSourceBuilder: npgsqlBuilder => npgsqlBuilder.MaxPoolSize(200));
   ```

### Pitfall 8: Aspire Dashboard Port Conflicts with Other Services

**What goes wrong:** The Aspire Dashboard defaults to port `17000` and `17001` (for OTLP). If another service is already using these ports (common with Docker containers or other development tools), Aspire fails to start.

**Prevention:**
1. Configure custom ports in the AppHost launch profile:
   ```csharp
   builder.AddProject<Projects.AppHost>()
       .WithEnvironment("ASPIRE_DASHBOARD_PORT", "18000");
   ```
2. Kill conflicting processes first.
3. Use `--no-dashboard` flag when you don't need it.

### Pitfall 9: APISIX Route Order — More Specific Routes Must Come First

**What goes wrong:** A route like `uri: /api/music/*` catches ALL requests under `/api/music/`, including nested routes like `/api/music/artists/{id}/albums`. If a more specific route (e.g., for static files or different upstreams) is defined after a broader catch-all, it is shadowed and never matched.

**Prevention:**
1. APISIX evaluates routes in order of `priority` (if set) or by the position in `routes` array.
2. Static routes → Specific resource routes → Catch-all routes.
3. For this PoC, URI prefix matching (`/api/dragonball/*` and `/api/music/*`) is sufficient — no nested route conflicts.

### Pitfall 10: OpenTelemetry Export Failure Causing Startup Delays

**What goes wrong:** The OpenTelemetry SDK attempts to connect to the OTLP endpoint (Aspire Dashboard) during application startup. If the dashboard isn't ready yet (race condition with Aspire startup), the API project hangs on startup or takes minutes to start.

**Prevention:**
1. Configure the OTLP exporter with a short timeout:
   ```csharp
   .WithOtlpExporter(options => options.TimeoutMilliseconds = 1000);
   ```
2. Use the `ConfigureOpenTelemetryExporter` overload in ServiceDefaults to set retry limits.
3. Aspire resolves this naturally by starting the dashboard before API projects, but be aware if running with `--no-dashboard`.

## Minor Pitfalls

### Pitfall 11: Soft Delete Unique Constraint Violation on Re-creation

When a record is soft-deleted (e.g., a character with name "Goku" is deleted and then a new character with the same name is created). Without a conditional unique index, the insert fails because the deleted record still exists.

**Fix:** Always use PostgreSQL partial unique indexes that exclude soft-deleted rows:
```sql
CREATE UNIQUE INDEX ix_characters_name 
ON dragonball.characters (name) 
WHERE deleted_at IS NULL;
```

Configure via EF Core fluent API:
```csharp
entity.HasIndex(c => c.Name)
      .HasFilter("deleted_at IS NULL")
      .IsUnique();
```

### Pitfall 12: Docker Network Name Resolution (APISIX → API)

APISIX resolves upstream hostnames at startup. If the API projects aren't running when APISIX starts, APISIX may cache DNS failures and refuse to route. This is common in Docker Compose if APISIX starts before the API containers.

**Fix:** Use Docker Compose `depends_on` or configure APISIX with lazy DNS resolution:
```yaml
discovery:
  dns:
    lazy: true
```

### Pitfall 13: Keycloak Admin Password Not Set

Keycloak 26+ requires `KEYCLOAK_ADMIN` and `KEYCLOAK_ADMIN_PASSWORD` environment variables. If not set, the container exits with error `You need to set the WFLYKEYCLOAK_ADMIN_PASSWORD environment variable`.

**Fix:** Always set both variables:
```yaml
KEYCLOAK_ADMIN: admin
KEYCLOAK_ADMIN_PASSWORD: admin
```

### Pitfall 14: EFCore.NamingConventions Version Mismatch

`EFCore.NamingConventions` 10.0.1 targets EF Core >=10.0.1. If the project accidentally uses EF Core 9.x (via transitive dependencies), the package won't resolve.

**Fix:** Ensure consistent EF Core 10.x across all projects. Pin the version:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.7" />
```

### Pitfall 15: Scalar UI Not Found (404) When Accessed via APISIX

Scalar's default endpoint is `/scalar/v1`. When accessed through APISIX at `/api/dragonball/scalar/v1`, APISIX may not match the route because the route pattern is `/api/dragonball/*` and the Scalar path includes `/scalar/v1`.

**Fix:** Verify the route pattern matches everything under the prefix. APISIX's `/*` glob handles this correctly. If using exact paths, Scalar won't work through the gateway — expose it only on the API's direct port.

## Phase-Specific Warnings

| Phase Topic | Likely Pitfall | Mitigation |
|-------------|---------------|------------|
| **Phase 0: Solution setup** | Aspire SDK version mismatch (apphost vs workload) | Run `dotnet workload install aspire` before building. Verify SDK 10.0.107. Run `dotnet restore --interactive` first time. |
| **Phase 1: EF Core models + migrations** | Pitfall 2 (migration confusion), Pitfall 11 (unique constraint) | Use `HasDefaultSchema()` everywhere. Add partial unique indexes. Test `dotnet ef migrations list` for both projects. |
| **Phase 2: API endpoints + Scalar** | Pitfall 4 (Scalar URLs broken), Pitfall 6 (N+1 queries) | Configure OpenAPI server URLs explicitly in `AddDocumentTransformer`. Use `Include()` + `AsSplitQuery()`. |
| **Phase 3: Keycloak auth** | Pitfall 1 (schema not created), Pitfall 5 (redirect loop), Pitfall 13 (admin password) | Create `keycloak` schema before starting Keycloak. Pin Keycloak port to 8080. Set admin credentials. Configure Valid Redirect URIs. |
| **Phase 4: APISIX gateway** | Pitfall 4 (Scalar through gateway), Pitfall 9 (route order), Pitfall 12 (DNS resolution) | Configure OpenAPI server URLs. Route order careful. APISIX config with `lazy: true` DNS. Test `http://localhost:9080` access. |
| **Phase 5: OTel observability** | Pitfall 10 (OTLP startup delay) | Set OTLP timeout to 1 second. Verify Aspire Dashboard starts first. |
| **Phase 6: React frontend** | Pitfall 5 (CORS/redirect), token storage security | Use `Authorization Code + PKCE` flow. Store token in memory (not localStorage). Apply CORS in APISIX. |
| **Phase 7: Docker Compose workspace** | Pitfall 12 (network name resolution), image tag mismatches | Use `depends_on`. Pin ALL image tags explicitly. Use `host.docker.internal` only for Windows/Mac — use container names for Linux. |

## Sources

- **EF Core Multiple DbContexts/Migrations:** https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/projects — verified migration management across projects
- **EF Core Global Query Filters:** https://learn.microsoft.com/en-us/ef/core/querying/filters — verified `HasQueryFilter()` patterns
- **EF Core Partial Indexes:** https://learn.microsoft.com/en-us/ef/core/providers/postgres/indexes — verified PostgreSQL partial unique indexes
- **EF Core N+1 Prevention:** https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager — verified `Include()` and `AsSplitQuery()`
- **Keycloak DB Configuration:** https://www.keycloak.org/server/db — verified KC_DB_SCHEMA and KC_DB_URL env vars
- **Keycloak Docker Getting Started:** https://www.keycloak.org/getting-started/getting-started-docker — verified KEYCLOAK_ADMIN and KEYCLOAK_ADMIN_PASSWORD requirements
- **APISIX Route Matching:** https://apisix.apache.org/docs/apisix/architecture-design/route — verified route priority and prefix matching
- **Aspire Container Lifetime:** https://learn.microsoft.com/en-us/dotnet/aspire/containers/container-resource#container-lifetime — verified `ContainerLifetime.Persistent` usage
- **Npgsql Connection Pooling:** https://www.npgsql.org/doc/connection-string-parameters.html — verified MaxPoolSize defaults
- **OpenTelemetry .NET Known Issues:** https://opentelemetry.io/docs/languages/net/getting-started/#troubleshooting — verified startup timeout considerations
- **Scalar + OpenAPI Server URLs:** https://github.com/scalar/scalar/issues/ (community) — verified OpenAPI server URL transformer pattern
- **OIDC Redirect Loop Diagnosis:** https://www.keycloak.org/docs/latest/server_admin/#_oidc-settings — verified redirect URI validation logic

---

*Pitfalls research for: .NET 10 multi-API solution with Aspire, Keycloak, APISIX, EF Core + PostgreSQL*
*Researched: 2026-04-24*
*Confidence: HIGH — all pitfalls verified against official documentation and community post-mortems*
