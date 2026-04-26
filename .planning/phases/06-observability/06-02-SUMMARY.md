# 06-02: Aspire Dashboard & OTLP Configuration — Summary

## What Was Done

1. **Aspire Dashboard** configured in `docker-compose.yml`:
   - Image: `mcr.microsoft.com/dotnet/aspire-dashboard:9.0`
   - Ports: 18888 (OTLP gRPC), 18889 (OTLP HTTP), 18890 (Dashboard UI)
   - Environment: `DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true`
   - OTLP endpoint: `http://localhost:18889/` (HTTP) / `http://localhost:18888/` (gRPC)

2. **OTLP exporter** configured in ServiceDefaults:
   - Endpoint reads from `OTEL_EXPORTER_OTLP_ENDPOINT` env var
   - Protocol: `Grpc` by default, falls back to `HttpProtobuf`
   - Headers: `X-Sequence=0` (required by Aspire Dashboard)

3. **Docker Compose networking**:
   - Aspire Dashboard on `opencode-net` network
   - Each API sets `OTEL_EXPORTER_OTLP_ENDPOINT=http://aspire-dashboard:18889/`
   - Frontend sets `OTEL_EXPORTER_OTLP_ENDPOINT` for client-side telemetry

4. **OTEL env vars** in each API service:
   - `OTEL_EXPORTER_OTLP_ENDPOINT`
   - `OTEL_SERVICE_NAME` (overrides default)
   - `OTEL_RESOURCE_ATTRIBUTES` (deployment environment)

## Verification

- Aspire Dashboard UI available at port 18890
- Traces appear in Dashboard when API receives requests
- Span details visible: duration, status, attributes, events
- Logs correlated with traces via trace ID

## Key Findings

- Aspire Dashboard requires `DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true` for dev
- OTLP gRPC endpoint (18888) preferred for .NET; HTTP (18889) for non-.NET components
- Trace correlation works across Dragon Ball API, Music API, and Frontend
- Dashboard shows live trace stream with sub-second latency
