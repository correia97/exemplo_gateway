---
phase: 06-observability
reviewed: 2026-04-30T12:00:00Z
depth: standard
files_reviewed: 4
files_reviewed_list:
  - src/OpenCode.ServiceDefaults/Extensions.cs
  - src/OpenCode.DragonBall.Api/Program.cs
  - src/OpenCode.Music.Api/Program.cs
  - docker-compose.yml
findings:
  critical: 3
  warning: 3
  info: 5
  total: 11
status: issues_found
---

# Phase 06: Code Review Report — Observability

**Reviewed:** 2026-04-30T12:00:00Z
**Depth:** standard
**Files Reviewed:** 4
**Status:** issues_found

## Summary

Reviewed 4 files implementing Phase 06 observability (OpenTelemetry SDK, Jaeger OTLP export, Correlation ID middleware, health checks, structured logging). Three critical issues will prevent end-to-end distributed tracing from functioning: an OTLP protocol mismatch causes trace export to fail silently, both APIs lack OTEL_SERVICE_NAME making services unidentifiable in Jaeger, and the `/health` endpoint is defined but never registered. Additionally, the Npgsql instrumentation is wired for metrics only (not tracing), so database query spans will be absent from traces. The docker-compose configuration also has several inconsistencies with the AppHost design documented in the phase context.

---

## Critical Issues

### CR-01: OTLP Protocol Mismatch — Traces Will Not Export in Docker-Compose

**File:** `docker-compose.yml:140,165`
**Issue:** Both API containers set `OTEL_EXPORTER_OTLP_ENDPOINT=http://jaeger:4318/v1/traces`, which is the HTTP/protobuf endpoint. The OpenTelemetry .NET SDK defaults to the **gRPC protocol** (`OtlpExportProtocol.Grpc`). Without also setting `OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf`, the exporter will attempt gRPC on port 4318 (which expects HTTP/protobuf), causing trace export to **fail silently** with connection errors. This means no distributed traces will reach Jaeger.

This contradicts the AppHost configuration (`src/OpenCode.AppHost/Program.cs:27,38`) which correctly uses `http://jaeger:4317` (gRPC port), and also contradicts `06-VERIFICATION.md:14-15` which documents `http://jaeger:4317`.

**Fix (two options):**

**Option A** — Use gRPC (preferred, consistent with AppHost and default SDK behavior):
```yaml
# docker-compose.yml:141,166
OTEL_EXPORTER_OTLP_ENDPOINT: http://jaeger:4317
```
And expose Jaeger gRPC port 4317:
```yaml
# docker-compose.yml:183-185 (add port 4317)
ports:
  - "4317:4317"
  - "4318:4318"
  - "16686:16686"
```

**Option B** — Keep HTTP endpoint but set protocol explicitly:
```yaml
OTEL_EXPORTER_OTLP_ENDPOINT: http://jaeger:4318/v1/traces
OTEL_EXPORTER_OTLP_PROTOCOL: http/protobuf
```

---

### CR-02: Missing Service Name — APIs Unidentifiable in Jaeger

**File:** `docker-compose.yml:136-141, 161-166`
**Issue:** Neither API container sets `OTEL_SERVICE_NAME` in its environment. Without this, OpenTelemetry falls back to the assembly name or a default value. Both APIs (DragonBall and Music) would appear with the same or indistinguishable service names in Jaeger, making trace analysis impossible. The Aspire AppHost handles this automatically via project naming, but docker-compose does not.

**Fix:** Add `OTEL_SERVICE_NAME` to each API's environment:
```yaml
# dragonball-api (under environment:)
OTEL_SERVICE_NAME: dragonball-api
```
```yaml
# music-api (under environment:)
OTEL_SERVICE_NAME: music-api
```

---

### CR-03: MapDefaultEndpoints Never Called — /health Endpoint Not Registered

**File:** `src/OpenCode.DragonBall.Api/Program.cs` and `src/OpenCode.Music.Api/Program.cs`
**Issue:** `Extensions.cs:47-51` defines a `MapDefaultEndpoints()` method that registers a `/health` endpoint. Neither Program.cs calls this method. Both call `app.UseCorrelationId()` but omit `app.MapDefaultEndpoints()`. The health endpoint simply doesn't exist. This means:

1. Docker health checks (which curl `/` root, returning 404) provide no real health validation
2. The observability phase deliverable of a health check endpoint is missing
3. No readiness/liveness probes exist for any container orchestrator

The docker-compose health check curling `http://localhost:8080/` always passes because curl exits 0 on 404 — but this is an illusion of health.

**Fix:** Add `app.MapDefaultEndpoints();` after the middleware chain in both Program.cs files:
```csharp
// src/OpenCode.DragonBall.Api/Program.cs (after line 76)
app.UseCors(corsPolicy);
app.MapDefaultEndpoints();     // <-- add this

// src/OpenCode.Music.Api/Program.cs (after line 78)
app.UseCors(corsPolicy);
app.MapDefaultEndpoints();     // <-- add this
```

Then update docker-compose health checks to target `/health`:
```yaml
# docker-compose.yml:145,171
test: ["CMD", "curl", "-s", "-o", "/dev/null", "-w", "%{http_code}", "http://localhost:8080/health"]
```

---

## Warnings

### WR-01: Npgsql Instrumentation Missing from Tracing Pipeline

**File:** `src/OpenCode.ServiceDefaults/Extensions.cs:38-42`

**Issue:** `AddNpgsqlInstrumentation()` is only called in the `.WithMetrics()` block (line 36), but **not** in the `.WithTracing()` block (lines 38-42). Per the phase design document (D-03: "Add `.AddNpgsqlInstrumentation()` to the tracing builder"), this was an explicit requirement. Without it, EF Core database query spans (duration, SQL text) will not appear in distributed traces, breaking the trace chain `Gateway → API → PostgreSQL`. The Npgsql.OpenTelemetry NuGet package is correctly referenced in the csproj (line 13), but the tracing call is missing.

**Fix:**
```csharp
// src/OpenCode.ServiceDefaults/Extensions.cs:38-42
.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation()
           .AddHttpClientInstrumentation()
           .AddNpgsqlInstrumentation();  // <-- add this line
});
```

---

### WR-02: Docker Health Checks Target Root — No Meaningful Health Validation

**File:** `docker-compose.yml:145, 171`

**Issue:** Both API health checks curl `http://localhost:8080/` (root), which returns 404 since neither API defines a root route. Docker health checks only evaluate the **exit code** of the command — `curl` exits 0 even for 404 responses. This means containers are always reported as "healthy" regardless of actual API state (even a 500 error would pass). Health checks should target a dedicated health endpoint and verify the response.

**Fix:** After fixing CR-03 (registering `/health`), update health checks and improve validation:
```yaml
test: ["CMD-SHELL", "curl -s -o /dev/null -w '%{http_code}' http://localhost:8080/health | grep -q 200"]
```

---

### WR-03: Empty Correlation ID Header Passes Through Unchecked

**File:** `src/OpenCode.ServiceDefaults/Extensions.cs:74-75`

**Issue:** If the incoming `X-Correlation-Id` header is present but empty (e.g., `X-Correlation-Id: `), `context.Request.Headers[CorrelationIdHeader].FirstOrDefault()` returns `string.Empty` (not null), so the `??` fallback does NOT trigger. The middleware propagates an empty correlation ID through the request, setting it as the response header and logging scope value. This produces broken correlation IDs that pollute logs and downstream requests.

**Fix:** Use `string.IsNullOrWhiteSpace()` instead of null coalescing:
```csharp
var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();
if (string.IsNullOrWhiteSpace(correlationId))
    correlationId = Guid.NewGuid().ToString();
```

---

## Info

### IN-01: No OpenTelemetry Resource Attributes Configured

**File:** `src/OpenCode.ServiceDefaults/Extensions.cs:30`

**Issue:** The `ConfigureOpenTelemetry()` method calls `AddOpenTelemetry()` with `WithMetrics()` and `WithTracing()`, but does **not** call `.ConfigureResource()` to set service version, instance ID, deployment environment, or other resource attributes. While `OTEL_SERVICE_NAME` (env var) covers the service name, adding explicit resource configuration would improve trace fidelity, especially for distinguishing between deployments or tracking version changes.

**Fix:**
```csharp
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: builder.Environment.ApplicationName,
            serviceVersion: "1.0.0",
            autoGenerateServiceInstanceId: true))
    .WithMetrics(metrics => ...)
    .WithTracing(tracing => ...);
```

---

### IN-02: No Prometheus Metrics Endpoint

**File:** `src/OpenCode.ServiceDefaults/Extensions.cs:47-51`

**Issue:** The observability phase scope includes Prometheus metrics (06-VERIFICATION.md Scenario 6 expects `curl http://localhost:5000/metrics | Select-String "npgsql"`). However, `MapDefaultEndpoints()` only registers `/health`. No `/metrics` endpoint is exposed. The OpenTelemetry metrics pipeline is configured (runtime, ASP.NET Core, HTTP client, Npgsql metrics), but there's no way to scrape them.

**Fix:** Add `app.MapPrometheusScrapingEndpoint()` in `MapDefaultEndpoints()`:
```csharp
public static WebApplication MapDefaultEndpoints(this WebApplication app)
{
    app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
    
    return app;
}
```
This requires the `OpenTelemetry.Exporter.Prometheus.AspNetCore` package reference — or adopt the Aspire `WithMetrics` approach.

---

### IN-03: Busybox Container Depends on Gateway with Wrong Condition

**File:** `docker-compose.yml:113-124`

**Issue:** The `busybox` service uses `condition: service_completed_successfully` on the `gateway` service (line 119). Kong gateway runs indefinitely (it's a long-lived server), so it never "completes" with exit code 0. The `busybox` container will **never start** because it's waiting for a condition that will never be met. The correct dependency is `kong-init-routes` (which runs `kong migrations bootstrap` and exits).

**Fix:**
```yaml
busybox:
  depends_on:
    kong-init-routes:
      condition: service_completed_successfully
```

---

### IN-04: Commented-Out Restart Policies on Critical Services

**File:** `docker-compose.yml:17, 50, 79, 105, 143, 169, 188, 206, 230`

**Issue:** Postgres, Keycloak, Kong, Jaeger, and both APIs all have `# restart: unless-stopped` commented out. If any of these containers crash (e.g., Postgres restart, network blip, OOM), they will remain stopped until manually restarted. For a PoC this may be acceptable, but it significantly reduces reliability.

**Fix:** Uncomment or add restart policies inline:
```yaml
restart: unless-stopped
```
For `kong-init-routes` and `busybox` (which are one-shot init containers), keep no restart policy or use `restart: "no"`.

---

### IN-05: AddOpenApi Called Redundantly

**File:** `src/OpenCode.ServiceDefaults/Extensions.cs:15` and `src/OpenCode.DragonBall.Api/Program.cs:29` / `src/OpenCode.Music.Api/Program.cs:29`

**Issue:** `AddOpenApi()` is called inside `AddServiceDefaults()` (Extensions.cs:15), which is invoked by both APIs via `builder.AddServiceDefaults()` (DragonBall:18, Music:18). Both Program.cs files then call `builder.Services.AddOpenApi()` again on their respective line 29. While this is likely idempotent in .NET 10, it registers the service twice without need.

**Fix:** Remove the duplicate `builder.Services.AddOpenApi()` from both Program.cs files (lines 29), since `AddServiceDefaults()` already handles it.

---

_Reviewed: 2026-04-30T12:00:00Z_
_Reviewer: the agent (gsd-code-reviewer)_
_Depth: standard_
