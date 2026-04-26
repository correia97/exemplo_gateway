# 06-03: Health Checks, Metrics & Structured Logging — Summary

## What Was Done

1. **Health checks** in both APIs:
   - `/health/ready` — readiness probe (database connectivity)
   - `/health/live` — liveness probe (process health)
   - Database health check via EF Core `IDbContextFactory<>`

2. **Prometheus metrics** via `OpenTelemetry.Exporter.Prometheus.AspNetCore`:
   - `/metrics` endpoint exposed on both APIs
   - Metrics: request count, request duration, active requests, DB query duration, memory usage
   - Custom meters for business metrics (character count, song count)

3. **Structured logging** via `Microsoft.Extensions.Logging` + OpenTelemetry:
   - `ILogger<T>` messages emitted as structured logs
   - Logs enriched with: `traceId`, `spanId`, `correlationId`, `serviceName`
   - JSON console formatting for machine readability

4. **Docker health checks** in `docker-compose.yml`:
   - Each API service has `healthcheck` directive polling `/health/ready`

5. **Correlation ID middleware** in ServiceDefaults:
   - Reads `X-Correlation-ID` from request header or generates new GUID
   - Adds correlation ID to `HttpContext.Items` and response headers
   - Included in all log messages via `BeginScope`

## Verification

- `GET /health/ready` returns 200 when DB is connected, 503 when not
- `GET /metrics` returns Prometheus-formatted metrics
- Logs show correlation IDs and trace IDs on every entry
- Docker health check status updates (healthy/unhealthy)

## Key Findings

- Health checks enable container orchestration (Docker restart on failure)
- Business metrics (entity counts) require custom `Meter` instrumentation
- Correlation ID in logs enables cross-service debugging workflows
- Prometheus scrape endpoint compatible with APISIX prometheus plugin
