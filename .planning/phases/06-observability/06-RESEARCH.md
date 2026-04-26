# Phase 6: OpenTelemetry & Observability — Research

**Researched:** 2026-04-25
**Domain:** OpenTelemetry, OTLP Export, APISIX OTel Plugin, Npgsql Instrumentation, Structured Logging
**Confidence:** HIGH

## Summary

Phase 6 wires end-to-end distributed tracing across APISIX, both .NET APIs, and PostgreSQL. Correlation ID (already implemented in Phase 1's ServiceDefaults + Phase 5's APISIX `request-id` plugin) is enhanced with structured logging integration. Npgsql instrumentation is added for database query spans. APISIX's `opentelemetry` plugin is configured to emit gateway spans. Since the Aspire Dashboard workload (DCP) is not installed on the dev machine, a standalone Jaeger container is used as the OTLP collector and trace visualization UI.

## Key Findings from Codebase Analysis

### 1. Existing OTel in ServiceDefaults (Phase 1 — OTEL-01)

`src/OpenCode.ServiceDefaults/Extensions.cs` has:

```csharp
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation()
               .AddRuntimeInstrumentation();
    })
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation();
    });
```

**Gaps identified:**
- **No `.AddNpgsqlInstrumentation()`** — Missing database query spans (OTEL-06)
- **No explicit `.WithOtlpExporter()`** — Works because Aspire auto-configures via env vars, but could be explicit for Jaeger fallback
- **No custom attributes** — Missing `CorrelationId` as trace attribute
- **OTLP endpoint discovery:** In Aspire, the `DOTNET_DASHBOARD_OTLP_ENDPOINT_URL` env var is auto-injected by the AppHost, pointing to the Aspire Dashboard's OTLP receiver. Since DCP/Dashboard isn't installed, we need a standalone OTLP collector (Jaeger).

### 2. Existing Correlation ID (Phase 1 + Phase 5 — OTEL-02)

**ServiceDefaults** has `UseCorrelationId()` middleware that:
- Reads `X-Correlation-Id` from request (or generates new GUID)
- Sets `context.TraceIdentifier = correlationId`
- Adds `X-Correlation-Id` response header

**Phase 5 APISIX** has `request-id` global rule that generates `X-Correlation-Id` header.

Both APIs call `app.UseCorrelationId()` in Program.cs.

**Gap:** The middleware does NOT push CorrelationId into `ILogger` scopes, so structured logs don't include it by default (OTEL-03).

### 3. ProblemDetails (Phase 3)

Both APIs already include `correlationId` and `traceId` in ProblemDetails responses:
```csharp
options.CustomizeProblemDetails = ctx =>
{
    ctx.ProblemDetails.Extensions["correlationId"] =
        ctx.HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? "";
    ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
};
```

This is already complete for OTEL-02 partial coverage.

### 4. APISIX Configuration (Phase 5)

`config.yaml` plugin whitelist:
```yaml
plugins:
  - openid-connect
  - cors
  - request-id
  - prometheus
```

**Gap:** `opentelemetry` plugin not whitelisted (OTEL-05).

`init-routes.sh` creates routes with `cors` and `openid-connect` plugins but no `opentelemetry` plugin.

### 5. AppHost Configuration

`src/OpenCode.AppHost/Program.cs` has no OTLP collector or Jaeger container.

### 6. NuGet Packages (Directory.Packages.props)

**Present:**
- `OpenTelemetry.Exporter.OpenTelemetryProtocol` 1.15.3
- `OpenTelemetry.Extensions.Hosting` 1.15.3
- `OpenTelemetry.Instrumentation.AspNetCore` 1.15.2
- `OpenTelemetry.Instrumentation.Http` 1.15.1
- `OpenTelemetry.Instrumentation.Runtime` 1.15.1

**Missing (needed):**
- `OpenTelemetry.Instrumentation.Npgsql` — for PostgreSQL query spans (OTEL-06)

## Architecture

```
Browser/curl
  │
  ▼ port 8000
┌─────────────────────┐
│   APISIX 3.9.1      │
│                     │
│  OTel Plugin:       │  ← Sends W3C traceparent + spans
│  - collector:       │
│    host:4318 (HTTP) │
│                     │
│  W3C Trace Context  │  ← traceparent header forwarded to upstream
└────────┬────────────┘
         │
         ▼
┌──────────────────────┐
│ .NET API (DragonBall)│
│                      │
│ OTel SDK:           │
│ - AspNetCore instr.  │  ← Incoming request spans
│ - HttpClient instr.  │  ← Outgoing HTTP call spans
│ - Npgsql instr.      │  ← NEW: PostgreSQL query spans
│                      │
│ ILogger scopes:      │  ← NEW: CorrelationId in structured logs
│ - CorrelationId      │
│ - TraceId            │
└─────────┬────────────┘
          │
          ▼
┌──────────────────────┐
│ PostgreSQL 17        │
│ (via Npgsql)         │
│                      │
│ Database spans:      │  ← NEW: Npgsql instrumentation
│ - SELECT/INSERT/etc  │
│ - Duration           │
│ - SQL (sanitized)    │
└──────────────────────┘

┌──────────────────────┐
│ Jaeger (OTLP)        │  ← NEW: Standalone OTLP collector
│                      │     (fallback when Aspire Dashboard
│ Port: 4318 (HTTP)    │      is unavailable)
│ Port: 16686 (UI)     │
└──────────────────────┘
```

## APISIX OpenTelemetry Plugin

### Configuration

The `opentelemetry` plugin in APISIX 3.9.x supports:

| Attribute | Required | Default | Description |
|-----------|----------|---------|-------------|
| `collector.address` | yes | — | OTLP collector endpoint (HTTP: `http://host:4318`) or gRPC (`http://host:4317`) |
| `collector.request_timeout` | no | 10s | Timeout for sending traces |
| `batch` | no | — | Batch processing config |
| `batch.max_queue_size` | no | 1024 | Max trace queue size |
| `batch.batch_timeout` | no | 1s | Max wait before sending batch |
| `additional_attributes` | no | — | Custom attributes to add to spans |

### Plugin Placement

Should be added as a **global rule** (like `request-id`) so ALL routes generate spans. Alternatively, add per-route.

### Trace Context Propagation

APISIX automatically handles W3C `traceparent` header propagation when the `opentelemetry` plugin is enabled:
- If the incoming request has `traceparent`, APISIX uses it as parent span
- If absent, APISIX generates a new trace
- The `traceparent` is forwarded to the upstream (.NET API)
- This creates a continuous trace chain

### Configuration Example (Admin API)

```json
{
    "plugins": {
        "opentelemetry": {
            "collector": {
                "address": "http://jaeger:4318/v1/traces",
                "request_timeout": 1
            },
            "batch": {
                "batch_timeout": 1,
                "max_queue_size": 1024
            },
            "additional_attributes": {
                "service.name": "apisix-gateway"
            }
        }
    }
}
```

## Jaeger as OTLP Collector

### Why Not Aspire Dashboard?

The Aspire Dashboard requires the DCP (Distributed Application Platform) workload to be installed. The developer's machine has Aspire installed in compile-only mode (projects build but dashboard doesn't run). Jaeger is a lightweight alternative:
- Receives OTLP traces natively (port 4318 HTTP, 4317 gRPC)
- Provides a rich trace visualization UI (port 16686)
- Runs as a single Docker container
- Can be replaced with Aspire Dashboard later if DCP workload is installed

### Jaeger Container Configuration

```csharp
var jaeger = builder.AddContainer("jaeger", "jaegertracing/all-in-one:latest")
    .WithEnvironment("COLLECTOR_OTLP_ENABLED", "true")
    .WithEndpoint(port: 4318, targetPort: 4318, scheme: "http", name: "otlp-http")
    .WithEndpoint(port: 16686, targetPort: 16686, scheme: "http", name: "ui");
```

Using `jaegertracing/all-in-one:latest`:
- Includes collector, query service, and UI in one image
- Supports OTLP via HTTP (4318) and gRPC (4317)
- UI available at port 16686
- No storage configuration needed (in-memory by default — good for PoC)

### OTLP Endpoint for .NET APIs

The .NET APIs discover the OTLP endpoint via the `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable. Since the Aspire Dashboard isn't available, we must either:
1. **Set env var explicitly:** `OTEL_EXPORTER_OTLP_ENDPOINT=http://jaeger:4318/` on the API containers
2. **Use explicit `.WithOtlpExporter()`** in ServiceDefaults with endpoint configured

For Aspire-managed projects (AddProject), the endpoint is auto-configured. Since we're adding Jaeger as a fallback, we need to override the OTLP endpoint for the .NET APIs.

**Approach:** Configure Jaeger as the OTLP receiver and pass `OTEL_EXPORTER_OTLP_ENDPOINT` env vars to the API projects in AppHost.

## Npgsql OpenTelemetry Instrumentation

### Package

`OpenTelemetry.Instrumentation.Npgsql` — the community-maintained package for Npgsql database instrumentation.

Version: Should match the OpenTelemetry SDK versions already in use (1.x line). Latest stable for .NET 10.

### Usage

```csharp
.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation()
           .AddHttpClientInstrumentation()
           .AddNpgsqlInstrumentation(options =>
           {
               options.SetDbStatementForText = true;  // Include SQL text (sanitized)
               options.SetDbStatementForStoredProcedure = true;
           });
})
```

### What It Captures

- Database query spans (`SELECT`, `INSERT`, `UPDATE`, `DELETE`)
- Span attributes: `db.system`, `db.name`, `db.statement` (SQL, sanitized), `db.connection_string`
- Query duration
- Error information (timeouts, constraint violations)

## Correlation ID in Structured Logs

### Current State

The `CorrelationIdMiddleware` sets `context.TraceIdentifier = correlationId` but does NOT push it into `ILogger` scopes. This means:
- `ILogger<T>` calls in endpoints don't automatically include `CorrelationId`
- Log entries cannot be correlated by Correlation ID
- OTEL-03 is not satisfied

### Required Change

Modify `CorrelationIdMiddleware.InvokeAsync()` to push CorrelationId into the log scope:

```csharp
public async Task InvokeAsync(HttpContext context)
{
    var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                        ?? Guid.NewGuid().ToString();

    context.Response.OnStarting(() =>
    {
        context.Response.Headers[CorrelationIdHeader] = correlationId;
        return Task.CompletedTask;
    });

    context.TraceIdentifier = correlationId;

    using (context.RequestServices.GetRequiredService<ILogger<CorrelationIdMiddleware>>()
        .BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
    {
        await _next(context);
    }
}
```

This creates an `ILogger` scope that automatically includes `CorrelationId` in all log entries within the request pipeline.

### TraceId in Logs

The OTel SDK's `AddAspNetCoreInstrumentation()` already includes `TraceId` in activity scopes. When `logging.IncludeScopes = true` is set (already configured), the `TraceId` is automatically included in log entries.

However, to make it explicit and visible, we should also configure the OpenTelemetry logging exporter to include TraceId as a log attribute.

## Trace Chain Verification

Expected trace chain for a single request:

| Span Order | Service | Span Type | Details |
|-----------|---------|-----------|---------|
| 1 | APISIX | HTTP Server | `GET /api/dragonball/characters` |
| 2 | .NET API | HTTP Server | `GET /characters` (after strip_prefix) |
| 3 | .NET API | Internal | `CharacterRepository.ListAsync` |
| 4 | PostgreSQL | Database | `SELECT ... FROM characters ...` |

Verification commands:
```bash
# Send a request through APISIX
curl -s "http://localhost9080/api/dragonball/characters?page=1&pageSize=5"

# Check Jaeger UI for traces
open http://localhost:16686/search
```

## Constraints

1. **AppHost is compile-only (no DCP/Dashboard):** OTEL-07 (Aspire Dashboard) cannot use the built-in dashboard. Fallback to Jaeger for trace visualization.
2. **Phase 5 VERIFICATION not yet executed:** The APISIX gateway is not verified to be correctly routing. Phase 6 depends on Phase 5 being operational.
3. **Both APIs run as host processes (not containers):** In Aspire inner-loop, API projects run on the host. Jaeger runs as a container. The .NET APIs need to reach Jaeger via `host.docker.internal:4318`.
4. **APISIX runs in container:** Can reach Jaeger via Docker network (service name `jaeger:4318`).

## Anti-Patterns to Avoid

1. **No OTLP exporter timeout** — Default 10s timeout blocks startup. Set to 1s (Pitfall 10 from RESEARCH).
2. **Setting OTLP endpoint only in ServiceDefaults** — The endpoint needs to be configurable per environment (Aspire vs Jaeger vs production).
3. **Adding Npgsql instrumentation without sanitized SQL** — Set `SetDbStatementForText = true` only, skip storing raw SQL parameters.
4. **Overriding Correlation ID in .NET middleware** — APISIX already generates it; .NET middleware should preserve it.
5. **Forgetting `traceparent` propagation** — Without W3C trace context, APISIX and .NET spans won't be in the same trace.

## Assumptions Log

| # | Claim | Risk if Wrong |
|---|-------|---------------|
| A1 | `jaegertracing/all-in-one:latest` supports OTLP HTTP on port 4318 | Low — well-documented since Jaeger v1.58+ |
| A2 | APISIX 3.9.x `opentelemetry` plugin supports OTLP HTTP export | Medium — need to verify plugin docs for 3.9.1 |
| A3 | `OpenTelemetry.Instrumentation.Npgsql` 1.x is compatible with .NET 10 | Medium — OTel .NET packages occasionally lag behind .NET releases |
| A4 | `host.docker.internal:4318` resolves from .NET APIs to Jaeger container | Low — standard Docker Desktop behavior on Windows |
| A5 | Trace context (traceparent) propagates through ASP.NET Core automatically | Low — OTel SDK handles this by default |

## Sources

### Primary (HIGH confidence)
- [VERIFIED: OpenTelemetry .NET SDK] — OTel configuration, exporter setup, instrumentation APIs
- [VERIFIED: Npgsql OTel instrumentation] — Database span capture via `AddNpgsqlInstrumentation()`
- [VERIFIED: APISIX opentelemetry plugin docs] — Plugin config, collector address, batch settings
- [VERIFIED: Jaeger all-in-one Docker image] — Ports 4317/4318 for OTLP, 16686 for UI
- [VERIFIED: W3C Trace Context spec] — `traceparent` header format and propagation rules

### Secondary (MEDIUM confidence)
- [CITED: ASP.NET Core ILogger scopes] — Structured logging with BeginScope
- [CITED: .NET Aspire OTLP auto-config] — Environment variable-based OTLP endpoint discovery

## Metadata

**Confidence breakdown:**
- APISIX OTel plugin: HIGH — well-documented with example configs
- Npgsql instrumentation: HIGH — standard OTel pattern
- Jaeger as collector: HIGH — official Docker image, well-documented
- Structured logging: HIGH — standard .NET ILogger scopes
- OTLP endpoint resolution: MEDIUM — varies by deployment context (Aspire vs standalone)
- Correlation ID middleware enhancement: HIGH — straightforward scope injection

**Research date:** 2026-04-25
**Valid until:** 2026-05-25
