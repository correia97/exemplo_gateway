---
phase: 06-observability
fixed_at: 2026-04-30T12:35:00Z
review_path: .planning/phases/06-observability/06-REVIEW.md
iteration: 1
findings_in_scope: 6
fixed: 6
skipped: 0
status: all_fixed
---

# Phase 06: Code Review Fix Report — Observability

**Fixed at:** 2026-04-30T12:35:00Z
**Source review:** .planning/phases/06-observability/06-REVIEW.md
**Iteration:** 1

**Summary:**
- Findings in scope: 6 (3 critical, 3 warning)
- Fixed: 6
- Skipped: 0

## Fixed Issues

### CR-01: OTLP Protocol Mismatch — Traces Will Not Export in Docker-Compose

**Files modified:** `docker-compose.yml`
**Commit:** f51af23
**Applied fix:** Switched `OTEL_EXPORTER_OTLP_ENDPOINT` from `http://jaeger:4318/v1/traces` (HTTP) to `http://jaeger:4317` (gRPC) for both dragonball-api and music-api services. Added Jaeger gRPC port `4317:4317` to port mappings. This matches the default OTLP gRPC protocol used by the .NET OpenTelemetry SDK and is consistent with the AppHost configuration.

### CR-02: Missing Service Name — APIs Unidentifiable in Jaeger

**Files modified:** `docker-compose.yml`
**Commit:** f51af23 (included alongside CR-01 changes)
**Applied fix:** Added `OTEL_SERVICE_NAME: dragonball-api` to the dragonball-api environment and `OTEL_SERVICE_NAME: music-api` to the music-api environment. This ensures each API is identifiable by its service name in Jaeger traces.

### CR-03: MapDefaultEndpoints Never Called — /health Endpoint Not Registered

**Files modified:** `src/OpenCode.DragonBall.Api/Program.cs`, `src/OpenCode.Music.Api/Program.cs`, `docker-compose.yml`
**Commit:** e9f1870
**Applied fix:** Added `app.MapDefaultEndpoints()` call in both Program.cs files after the middleware chain (after `UseCorrelationId` / `UseCors`). Updated docker-compose health checks to target `http://localhost:8080/health` instead of root `/`. The `MapDefaultEndpoints()` extension method (defined in `Extensions.cs:47-51`) registers the `/health` endpoint.

### WR-01: Npgsql Instrumentation Missing from Tracing Pipeline

**Files modified:** `src/OpenCode.ServiceDefaults/Extensions.cs`
**Commit:** 9d7d7d6
**Applied fix:** Added `.AddNpgsql()` to the `.WithTracing()` builder chain (line 42). Used `.AddNpgsql()` instead of `.AddNpgsqlInstrumentation()` (as suggested by the review) because Npgsql 10.x uses `.AddNpgsql()` on `TracerProviderBuilder` — `.AddNpgsqlInstrumentation()` is only available on `MeterProviderBuilder` (metrics). This enables EF Core database query spans (duration, SQL text) in distributed traces.

### WR-02: Docker Health Checks Target Root — No Meaningful Health Validation

**Files modified:** `docker-compose.yml`
**Commit:** eb1fed5
**Applied fix:** Changed both API health checks from `CMD` format to `CMD-SHELL` format with HTTP 200 validation: `curl -s -o /dev/null -w '%{http_code}' http://localhost:8080/health | grep -q 200`. This ensures containers are only reported as healthy when the `/health` endpoint returns HTTP 200, not just any response.

### WR-03: Empty Correlation ID Header Passes Through Unchecked

**Files modified:** `src/OpenCode.ServiceDefaults/Extensions.cs`
**Commit:** 6686746
**Applied fix:** Replaced the null-coalescing operator (`??`) with `string.IsNullOrWhiteSpace()` check. The previous code `FirstOrDefault() ?? Guid.NewGuid().ToString()` would allow empty string correlation IDs (e.g., `X-Correlation-Id: `) to pass through because `FirstOrDefault()` returns `string.Empty` (not null) for empty headers. The fix ensures empty or whitespace-only correlation IDs are replaced with a new GUID.

---

_Fixed: 2026-04-30T12:35:00Z_
_Fixer: the agent (gsd-code-fixer)_
_Iteration: 1_
