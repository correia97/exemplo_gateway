# 06-01: OpenTelemetry .NET SDK Setup — Summary

## What Was Done

1. **ServiceDefaults** project (`src/OpenCode.ServiceDefaults/`) extended:
   - `AddOpenTelemetry()` extension method with:
     - OTLP exporter configured via `OTEL_EXPORTER_OTLP_ENDPOINT` env var
     - `ASPNETCORE_HOST_STARTTYPE` instrumentation
     - HTTP client instrumentation (outgoing requests)
     - EF Core instrumentation (database calls)
     - Runtime instrumentation (GC, JIT)

2. **Activity Sources**:
   - Singleton `ActivitySource` per API project (`OpenCode.DragonBall.Api`, `OpenCode.Music.Api`)

3. **Resource attributes**:
   - `service.name` from project name
   - `service.version` from assembly version
   - `deployment.environment` from `ASPNETCORE_ENVIRONMENT`

4. **Console exporter** enabled for development:
   - `AddConsoleExporter()` in development only
   - Structured JSON output for readability

5. **W3C Trace Context** propagation:
   - `traceparent` header support enabled by default
   - Correlation ID linked to `traceId` for cross-service tracing

## Verification

- Console exporter shows spans on application startup
- HTTP requests generate spans with method, URL, status code
- EF Core queries generate spans with database name, command text
- W3C `traceparent` header present in outgoing HTTP requests

## Key Findings

- OTLP endpoint env var approach keeps config portable across environments
- EF Core instrumentation captures slow queries as span events
- Console exporter is useful for development but should be replaced by OTLP in production
