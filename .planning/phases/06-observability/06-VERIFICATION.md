# Phase 6: OpenTelemetry & Observability — Verification

## 6.01 — APISIX OTel Plugin + Jaeger + OTLP Wiring

**Status:** ✅ Complete

| Check | Verification |
|-------|-------------|
| `opentelemetry` plugin in config.yaml | Listed in plugins section |
| `init-routes-otel.sh` created | Patches opentelemetry plugin onto all 4 routes via PATCH |
| Jaeger all-in-one container in Program.cs | Ports 4317 (gRPC), 4318 (HTTP), 16686 (UI) |
| Jaeger env `COLLECTOR_OTLP_ENABLED=true` | Set in container definition |
| Jaeger env `COLLECTOR_OTLP_HTTP_ENABLED=true` | Set in container definition |
| `OTEL_EXPORTER_OTLP_ENDPOINT` on dragonball-api | `http://jaeger:4317` |
| `OTEL_EXPORTER_OTLP_ENDPOINT` on music-api | `http://jaeger:4317` |
| `JAEGER_ENDPOINT` env var on APISIX container | `http://jaeger:4318` |
| APISIX entrypoint chains init-routes-otel.sh | `chmod +x ... && init-routes.sh && init-routes-otel.sh && exec openresty` |

## 6.02 — Npgsql Instrumentation + ILogger Scope

**Status:** ✅ Complete

| Check | Verification |
|-------|-------------|
| `Npgsql.OpenTelemetry` version in Directory.Packages.props | `10.0.2` |
| PackageReference in ServiceDefaults csproj | Added `<PackageReference Include="Npgsql.OpenTelemetry" />` |
| `AppContext.SetSwitch("Npgsql.EnableTelemetry", true)` | In `ConfigureOpenTelemetry()` |
| `AddNpgsqlInstrumentation()` in metrics builder | In `.WithMetrics(...)` block |
| ILogger scope in CorrelationIdMiddleware | `BeginScope` wraps `_next(context)` with `X-Correlation-Id` |

## 6.03 — E2E Verification Scenarios

### Prerequisites
```powershell
# Start the solution (requires DCP workload)
dotnet run --project src/OpenCode.AppHost

# After all containers are healthy, verify routes are configured
docker exec <apisix-container-id> curl -s http://127.0.0.1:9180/apisix/admin/routes -H "X-API-KEY: edd1c9f034335f136f87ad84b625c8f1" | ConvertFrom-Json
```

### Scenario 1: Jaeger UI accessible
```powershell
# Open in browser
start http://localhost:16686
# Expected: Jaeger UI dashboard, service list shows "dragonball-api", "music-api", "apisix"
```

### Scenario 2: Trace generated on GET request
```powershell
$token = curl -s -X POST "http://localhost:8080/realms/OpenCode/protocol/openid-connect/token" `
  -H "Content-Type: application/x-www-form-urlencoded" `
  -d "client_id=dragonball-api&client_secret=dragonball-secret&grant_type=client_credentials" | ConvertFrom-Json | Select -ExpandProperty access_token

curl -s http://localhost9080/api/dragonball/characters -H "Authorization: Bearer $token" -H "X-Correlation-Id: test-trace-1"
```

Then in Jaeger UI: Search for `test-trace-1` — should show span from APISIX → dragonball-api → Npgsql query.

### Scenario 3: Trace generated on write request
```powershell
curl -s -X POST "http://localhost9080/api/dragonball/characters" `
  -H "Authorization: Bearer $token" `
  -H "Content-Type: application/json" `
  -d '{"name":"Goku","ki":10000,"description":"Saiyan"}' `
  -H "X-Correlation-Id: test-trace-2"
```

### Scenario 4: ILogger scope includes correlation ID
```powershell
# Check API container logs for structured logging with X-Correlation-Id
docker logs <dragonball-api-container-id> | Select-String "X-Correlation-Id"
# Expected: log entries include correlation ID in scope properties
```

### Scenario 5: OTLP exporter traces in AppHost output
```powershell
# When running `dotnet run --project src/OpenCode.AppHost`, look for:
#   OpenTelemetry information: OTLP exporter configured for http://jaeger:4317
```

### Scenario 6: Metrics include Npgsql
```powershell
# Expose metrics endpoint and verify Npgsql metrics
curl -s http://localhost:5000/metrics | Select-String "npgsql"
# Expected: metrics like npgsql_connections_opened_total, npgsql_commands_total
```

## Build Verification

```powershell
dotnet build
# Expected: 0 errors (1 pre-existing CS0436 warning is tolerated)
dotnet test
# Expected: 18 passed, 0 failed
```
