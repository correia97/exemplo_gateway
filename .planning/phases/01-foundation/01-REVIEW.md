---
phase: 01-foundation
reviewed: 2026-04-30T14:30:00Z
depth: standard
files_reviewed: 20
files_reviewed_list:
  - OpenCode.slnx
  - Directory.Packages.props
  - src/OpenCode.AppHost/OpenCode.AppHost.csproj
  - src/OpenCode.AppHost/Program.cs
  - src/OpenCode.AppHost/Properties/launchSettings.json
  - src/OpenCode.AppHost/appsettings.json
  - src/OpenCode.AppHost/appsettings.Development.json
  - src/OpenCode.ServiceDefaults/Extensions.cs
  - src/OpenCode.ServiceDefaults/OpenCode.ServiceDefaults.csproj
  - src/OpenCode.DragonBall.Api/OpenCode.DragonBall.Api.csproj
  - src/OpenCode.DragonBall.Api/Program.cs
  - src/OpenCode.DragonBall.Api/appsettings.json
  - src/OpenCode.DragonBall.Api/appsettings.Development.json
  - src/OpenCode.DragonBall.Api/Properties/launchSettings.json
  - src/OpenCode.Music.Api/OpenCode.Music.Api.csproj
  - src/OpenCode.Music.Api/Program.cs
  - src/OpenCode.Music.Api/appsettings.json
  - src/OpenCode.Music.Api/appsettings.Development.json
  - src/OpenCode.Music.Api/Properties/launchSettings.json
findings:
  critical: 2
  warning: 10
  info: 7
  total: 19
status: issues_found
---

# Phase 01: Code Review Report — Foundation & Solution Scaffolding

**Reviewed:** 2026-04-30T14:30:00Z
**Depth:** Standard
**Files Reviewed:** 20 (19 source files + 1 solution file)
**Status:** Issues Found

## Summary

This review covers the .NET 10 foundation scaffolding: solution file, centralized NuGet package management, Aspire AppHost orchestrating PostgreSQL + Jaeger + Keycloak + Kong + 2 frontends, ServiceDefaults (OpenTelemetry + Correlation ID middleware), and two API project stubs (DragonBall + Music).

**Key concerns:**
- The ServiceDefaults project will **not compile** — it uses ASP.NET Core types (`WebApplication`, `RequestDelegate`) but lacks the required `FrameworkReference` for the ASP.NET Core shared framework.
- **FluentValidation version incompatibility** — `FluentValidation.AspNetCore` 11.3.1 is mixed with `FluentValidation` 12.1.1, causing runtime type failures.
- **Hardcoded secrets** — Connection strings with plaintext credentials, Keycloak admin password `"admin"`, and JWT validation effectively disabled.
- **Container orchestration gaps** — Kong gateway can start before migrations complete; Jaeger/Keycloak use `latest` tags.

---

## Critical Issues

### CR-01: ServiceDefaults won't compile — missing `FrameworkReference` for ASP.NET Core

**File:** `src/OpenCode.ServiceDefaults/OpenCode.ServiceDefaults.csproj:1`
**Issue:** The project SDK is `Microsoft.NET.Sdk` (line 1), but `Extensions.cs` uses types from the ASP.NET Core shared framework (`WebApplication`, `RequestDelegate`, `HttpContext`, `IApplicationBuilder`, `WebApplicationBuilder`, etc.). These types reside in `Microsoft.AspNetCore.App`, which is only automatically referenced when using `Microsoft.NET.Sdk.Web`. Without either switching to the Web SDK or adding an explicit `<FrameworkReference Include="Microsoft.AspNetCore.App" />`, this project will **fail to compile**. The absence of any build artifacts in `src/OpenCode.ServiceDefaults/bin/` confirms the project has never been successfully built.

**Fix:** Add a FrameworkReference for the ASP.NET Core shared framework. In `OpenCode.ServiceDefaults.csproj`, add:

```xml
<ItemGroup>
  <FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>
```

Alternatively, change the SDK to `Microsoft.NET.Sdk.Web` if the project is intended to be a web library. The FrameworkReference approach is preferred for class libraries to avoid pulling in unnecessary web host defaults.

---

### CR-02: FluentValidation version incompatibility (v11 + v12 mixed)

**File:** `Directory.Packages.props:9`
**Issue:** `FluentValidation.AspNetCore` version **11.3.1** is declared (line 9) alongside `FluentValidation` **12.1.1** (line 7) and `FluentValidation.DependencyInjectionExtensions` **12.1.1** (line 10). The `FluentValidation.AspNetCore` package was deprecated at version 11 and is **not compatible** with FluentValidation v12 assemblies. When both v11 and v12 packages are resolved, the runtime will see two different versions of `FluentValidation.dll`, causing `TypeLoadException` or `MissingMethodException` when FluentValidation.AspNetCore tries to use v12 APIs.

**Fix:** Remove `FluentValidation.AspNetCore` 11.3.1 — its functionality was merged into `FluentValidation.DependencyInjectionExtensions` starting from FluentValidation v11. The API projects already call `AddValidatorsFromAssemblyContaining<Program>()` and `AddFluentValidationAutoValidation()`, which work with the DependencyInjectionExtensions package alone.

Delete line 9 from `Directory.Packages.props`:
```diff
-    <PackageVersion Include="FluentValidation.AspNetCore" Version="11.3.1" />
```

---

## Warnings

### WR-01: Npgsql OpenTelemetry not added to tracing configuration

**File:** `src/OpenCode.ServiceDefaults/Extensions.cs:38-42`
**Issue:** `AddNpgsqlInstrumentation()` is added to metrics (line 36) but **omitted** from the tracing block (lines 38-42). This means individual database call spans (SQL statements, query duration, success/failure per-call) will not appear in Jaeger traces. Only aggregate metrics (e.g., calls/sec) will be visible. This severely limits the observability value of the tracing pipeline.

**Fix:** Add Npgsql instrumentation to the tracing configuration:

```csharp
.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation()
           .AddHttpClientInstrumentation()
           .AddNpgsqlInstrumentation();  // ADD THIS
});
```

---

### WR-02: Hardcoded connection strings with plaintext credentials

**File:** `src/OpenCode.AppHost/Program.cs:25-27, 36-37`
**Issue:** Connection strings for DragonBall and Music APIs are hardcoded as environment variables with credentials in plaintext (`dragonball_user`/`dragonball_pass`, `music_user`/`music_pass`). This overrides the auto-generated connection string that would be injected by `WithReference(postgres)`. The result is:
1. Credentials duplicated in source code (also defined as `postgresUser`/`postgresPass` parameters on lines 4-5)
2. Usernames/passwords differ from the PostgreSQL parameter defaults (`postgres`/`postgres`)
3. No `secret: true` protection for the embedded values

**Fix:** Remove the manual connection strings and configure database credentials through Aspire's parameter system, then delegate to the auto-generated connection string. Alternatively, use `builder.AddConnectionString()` to define them in configuration:

```csharp
var dragonballUser = builder.AddParameter("dragonballUser", secret: true);
var dragonballPass = builder.AddParameter("dragonballPass", secret: true);

var postgres = builder.AddPostgres("postgres", username, password)
    .WithDataVolume()
    // ... other config
    .WithEnvironment("POSTGRES_MULTIPLE_DATABASES", "dragonball,music,keycloak,kong");

// Then let Aspire inject the connection string via WithReference:
var dragonballApi = builder.AddProject<Projects.OpenCode_DragonBall_Api>("dragonball-api")
    .WithReference(postgres);  // Aspire handles connection string
```

---

### WR-03: Hardcoded Keycloak admin password

**File:** `src/OpenCode.AppHost/Program.cs:53`
**Issue:** The Keycloak admin password is hardcoded as `"admin"` via a plain environment variable. Unlike the PostgreSQL credentials (which use `builder.AddParameter()` with `secret: true`), this follows no parameter pattern and exposes the admin password in plaintext.

**Fix:** Use `builder.AddParameter()` with `secret: true`:

```csharp
var keycloakAdminPassword = builder.AddParameter("keycloakAdminPassword", secret: true);

// Then use it:
.WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", keycloakAdminPassword)
```

---

### WR-04: Kong gateway starts without waiting for migrations

**File:** `src/OpenCode.AppHost/Program.cs:75-91`
**Issue:** The `gateway` container (line 75) does not call `.WaitFor(kongInit)`. The migration container (`gateway-init`) must complete `kong migrations bootstrap` before the gateway starts — if the gateway initializes first, it will fail because the Kong database schema doesn't exist yet.

**Fix:** Add `.WaitFor(kongInit)` to the gateway container chain:

```csharp
var kong = builder.AddContainer("gateway", "kong/kong", "3.9.1-ubuntu")
    // ... existing config ...
    .WithReference(postgres)
    .WaitFor(kongInit);  // ADD THIS
```

---

### WR-05: JWT validation effectively disabled

**Files:**
- `src/OpenCode.DragonBall.Api/Program.cs:56-58`
- `src/OpenCode.Music.Api/Program.cs:59-61`

**Issue:** Three JWT security settings are relaxed simultaneously:
1. `RequireHttpsMetadata = false` — metadata endpoint can be intercepted over HTTP
2. `ValidateAudience = false` — any audience accepted; a token issued for `music-api` could be used to call `dragonball-api`
3. `ValidateIssuer = false` — tokens from any issuer accepted

While acceptable for local development behind APISIX/Kong (which handles auth at the gateway), these settings mean the APIs have **zero JWT security** if accessed directly (e.g., via port-forwarded endpoints).

**Fix:** At minimum, enable audience and issuer validation at the API level since the APIs are independently addressable in the Aspire dev environment:

```csharp
options.TokenValidationParameters.ValidateAudience = true;
options.TokenValidationParameters.ValidateIssuer = true;
options.TokenValidationParameters.ValidIssuer = "http://localhost:8080/realms/OpenCode";
```

Or keep the relaxed settings behind a compile-time flag for development only.

---

### WR-06: `UseHttpsRedirection()` placed after endpoint mapping

**Files:**
- `src/OpenCode.DragonBall.Api/Program.cs:91`
- `src/OpenCode.Music.Api/Program.cs:92`

**Issue:** `UseHttpsRedirection()` is called AFTER both `MapScalarApiReference()` and `MapGroup().Map*Endpoints()`. In ASP.NET Core's middleware pipeline, middleware registered later executes later. With `UseHttpsRedirection()` positioned after endpoint mapping, HTTP requests can reach the endpoint routing infrastructure before the redirect middleware checks for HTTPS, causing unnecessary processing and potential information leakage. The redirect itself still works but is less efficient.

**Fix:** Move `UseHttpsRedirection()` before all `Map*()` calls:

```csharp
app.UseCorrelationId();
app.UseHttpsRedirection();  // MOVE HERE — before endpoint mapping
app.UseAuthentication();
app.UseAuthorization();
app.UseCors(corsPolicy);

app.MapOpenApi();
app.MapScalarApiReference(...);
// ... endpoint maps
```

---

### WR-07: CORS middleware ordering — runs after authentication

**Files:**
- `src/OpenCode.DragonBall.Api/Program.cs:73-76`
- `src/OpenCode.Music.Api/Program.cs:75-78`

**Issue:** `UseCors()` is placed after `UseAuthentication()` and `UseAuthorization()`. For CORS preflight requests (HTTP OPTIONS), the CORS middleware should execute before the authentication middleware to return the correct CORS headers (e.g., `Access-Control-Allow-Origin`) without requiring authentication. Preflight requests do not carry auth tokens by design.

**Fix:** Move `UseCors()` before `UseAuthentication()`:

```csharp
app.UseCorrelationId();
app.UseCors(corsPolicy);         // MOVE HERE
app.UseAuthentication();
app.UseAuthorization();
```

---

### WR-08: Container images use `latest` tags

**File:** `src/OpenCode.AppHost/Program.cs:15, 45`

**Issue:** Both Jaeger (`jaegertracing/all-in-one:latest`, line 15) and Keycloak (`quay.io/keycloak/keycloak:latest`, line 45) use the `:latest` tag. This produces non-reproducible deployments — a `docker compose up` today vs. next month may pull different image versions with incompatible behaviors.

**Fix:** Pin to specific versions:

```csharp
// Jaeger — use a specific minor version
var jaeger = builder.AddContainer("jaeger", "jaegertracing/all-in-one:1.66")

// Keycloak — use a specific version
var keycloak = builder.AddContainer("keycloak", "quay.io/keycloak/keycloak:26.2")
```

---

### WR-09: Both React and Angular frontends configured simultaneously

**File:** `src/OpenCode.AppHost/Program.cs:99-118`

**Issue:** Two frontend projects are configured: `OpenCode.Frontend` (React, port 5173) and `OpenCode.AngularFrontend` (Angular, port 4200). The project context (`AGENTS.md`) describes a single "React frontend". Starting both will:
1. Cause unnecessary resource consumption in the Aspire dashboard
2. Potentially cause port conflicts or hot-reload interference
3. Be confusing for developers ("which frontend should I use?")

**Fix:** Remove the unused frontend. If Angular is a future addition (Phase 09 per ROADMAP), move it to that phase.

---

### WR-10: No health checks on Jaeger, Keycloak, or Kong containers

**File:** `src/OpenCode.AppHost/Program.cs` (lines 15-21, 45-60, 75-91)

**Issue:** Only PostgreSQL has `WaitFor()` from consuming services. Jaeger, Keycloak, and Kong have no `WithHealthCheck()` configured. The OTLP exporter in the APIs references `jaeger:4317` (line 27, 38) but nothing verifies Jaeger is actually accepting connections before the APIs start exporting traces.

**Fix:** Add health checks to dependent containers:

```csharp
var jaeger = builder.AddContainer("jaeger", "jaegertracing/all-in-one:1.66")
    // ... existing config ...
    .WithHealthCheck("http", "/");  // Jaeger gRPC endpoint health

var keycloak = builder.AddContainer("keycloak", "quay.io/keycloak/keycloak:26.2")
    // ... existing config ...
    .WithHealthCheck("http", "/health/ready");
```

---

## Info

### IN-01: xunit version conflict — v2 and v3 both declared

**File:** `Directory.Packages.props:28, 30`
**Issue:** Both `xunit` 2.9.3 (xUnit v2 framework) and `xunit.v3` 3.2.2 (xUnit v3 framework) are versioned in the same central packages file. Test projects should standardize on one major version. Mixing v2 and v3 across different test projects leads to inconsistent testing patterns and potential confusion for developers.

**Fix:** Remove one version and standardize. xUnit v3 is recommended for new .NET 10 projects.

---

### IN-02: Inconsistent `<GenerateAssemblyInfo>` across API projects

**Files:**
- `src/OpenCode.DragonBall.Api/OpenCode.DragonBall.Api.csproj:8`
- `src/OpenCode.Music.Api/OpenCode.Music.Api.csproj` (missing)

**Issue:** The DragonBall API has `<GenerateAssemblyInfo>true</GenerateAssemblyInfo>` but the Music API does not. The default is already `true` for `Microsoft.NET.Sdk.Web`, so this tag in DragonBall is redundant, but the inconsistency suggests one project was customized while the other was not.

**Fix:** Remove the redundant tag from DragonBall's csproj, or add it to Music's csproj for consistency.

---

### IN-03: Correlation ID not exposed via `HttpContext.Items`

**File:** `src/OpenCode.ServiceDefaults/Extensions.cs:83`
**Issue:** The CorrelationIdMiddleware sets `context.TraceIdentifier` (line 83) and adds the ID to logging scopes, but it doesn't add it to `HttpContext.Items` for downstream middleware, controllers, or services to easily access. Currently, services would need to parse request headers or use `Activity.Current` to retrieve the correlation ID.

**Fix:** Add the correlation ID to `HttpContext.Items`:

```csharp
context.TraceIdentifier = correlationId;
context.Items["CorrelationId"] = correlationId;  // ADD THIS
```

---

### IN-04: Unused `using Microsoft.OpenApi` import

**Files:**
- `src/OpenCode.DragonBall.Api/Program.cs:6`
- `src/OpenCode.Music.Api/Program.cs:6`

**Issue:** Both API Program.cs files import `Microsoft.OpenApi` but never reference any types from this namespace directly. Scalar and ASP.NET Core OpenAPI work without this explicit import.

**Fix:** Remove the unused `using` directive:

```diff
- using Microsoft.OpenApi;
```

---

### IN-05: Redundant `AddOpenApi()` call

**Files:**
- `src/OpenCode.ServiceDefaults/Extensions.cs:15`
- `src/OpenCode.DragonBall.Api/Program.cs:29`
- `src/OpenCode.Music.Api/Program.cs:29`

**Issue:** `builder.Services.AddOpenApi()` is called both inside `AddServiceDefaults()` (in Extensions.cs line 15) and directly in each API's Program.cs. The call is idempotent — the OpenAPI document generator is registered once regardless of multiple calls — but the duplication is misleading about intent.

**Fix:** Remove the redundant call from the API Program.cs files since `AddServiceDefaults()` already handles it.

---

### IN-06: Extremely permissive CORS policy

**Files:**
- `src/OpenCode.DragonBall.Api/Program.cs:23-26`
- `src/OpenCode.Music.Api/Program.cs:23-26`

**Issue:** The CORS policy uses `AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()`, which allows requests from any domain to access these APIs. While acceptable for a local development PoC, this must be scoped down before any production or staging deployment.

**Fix:** Restrict to the allowed origins, particularly the frontend URLs:

```csharp
options.AddPolicy(corsPolicy, policy =>
{
    policy.WithOrigins("http://localhost:5173", "http://localhost:4200")
          .AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials();
});
```

---

### IN-07: Health endpoint unauthenticated / lacks correlation ID

**File:** `src/OpenCode.ServiceDefaults/Extensions.cs:49`
**Issue:** The `/health` endpoint returns `{ status: "healthy" }` without authentication. This is standard for health checks but worth noting that it leaks the application's existence. The health endpoint also doesn't receive the correlation ID through the middleware (since it runs AFTER `UseCorrelationId()` in the pipeline — correct placement), but if health checks are called externally, they won't carry a meaningful trace identifier.

**Fix (optional):** Add an `X-Correlation-Id` response header to the health endpoint:

```csharp
app.MapGet("/health", (HttpContext context) =>
{
    context.Response.Headers["X-Correlation-Id"] = 
        context.TraceIdentifier;
    return Results.Ok(new { status = "healthy" });
});
```

---

## Summary of Findings by Severity

| Severity | Count | Key Areas |
|----------|-------|-----------|
| Critical | 2 | Missing FrameworkReference (won't compile), FluentValidation version mismatch |
| Warning  | 10 | OTel tracing gaps, hardcoded secrets, middleware ordering, JWT validation, container orchestration |
| Info     | 7 | Redundant imports, version inconsistency, minor code quality suggestions |

**Top 3 actions:**
1. Add `<FrameworkReference Include="Microsoft.AspNetCore.App" />` to ServiceDefaults — the project cannot compile without it.
2. Remove `FluentValidation.AspNetCore` 11.3.1 from central packages — incompatible with FluentValidation 12.x.
3. Replace hardcoded connection strings with Aspire's auto-injection via `WithReference(postgres)` and use parameter secrets for all credentials.

---

_Reviewed: 2026-04-30T14:30:00Z_
_Reviewer: gsd-code-reviewer (standard depth)_
_Depth: standard_
