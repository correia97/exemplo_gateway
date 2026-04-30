# Phase 6: OpenTelemetry & Observability — Design Context

## Objective

Wire end-to-end distributed tracing across Kong, both .NET APIs, and PostgreSQL. Enhance the existing Correlation ID middleware with structured logging integration. Configure the Kong OpenTelemetry plugin to generate gateway spans. Add Npgsql instrumentation for database query spans. Since the Aspire Dashboard workload (DCP) is not available on the dev machine, use a standalone Jaeger container as the OTLP collector and trace visualization UI.

## Key Decisions

### D-01: Use Jaeger as standalone OTLP collector instead of Aspire Dashboard

**Context:** The Aspire Dashboard requires the DCP workload which is not installed (AppHost is compile-only). The developer has previously opted to keep the environment lightweight.

**Decision:** Add a Jaeger `all-in-one` container to the AppHost that receives OTLP traces and provides a trace visualization UI at `http://localhost:16686`.

**Implications:**
- Jaeger replaces Aspire Dashboard for trace visualization (OTEL-07 adapted)
- OTLP endpoint for .NET APIs: `http://host.docker.internal:4318/` (env var override)
- OTLP endpoint for Kong: `http://jaeger:4318/v1/traces` (container DNS)
- Can be replaced with Aspire Dashboard later if DCP workload is installed

### D-02: Append `opentelemetry` plugin as global rule, not per-route

**Context:** Kong supports both global rules and per-route plugin configuration. ALL routes should generate OTel spans.

**Decision:** Configure the `opentelemetry` plugin as a **global rule** (like `request-id`). This ensures every request through Kong gets a gateway span without modifying individual route definitions.

**Implications:**
- Single Admin API call to create/update the global rule
- Plugin config: collector address, batch settings, custom attributes
- Works on both GET and POST/PUT/DELETE routes automatically

### D-03: Add `Npgsql.OpenTelemetry` to ServiceDefaults project

**Context:** The OTel SDK is configured in ServiceDefaults, but Npgsql instrumentation is missing.

**Decision:** Add the NuGet package reference to `OpenCode.ServiceDefaults.csproj` and add `.AddNpgsqlInstrumentation()` to the tracing builder in `Extensions.cs`.

**Implications:**
- `Npgsql.OpenTelemetry` version 10.0.2 matches the Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1 already in the project
- Database query spans automatically created for all EF Core queries
- SQL text captured in sanitized form (parameter values redacted)

### D-04: Inject CorrelationId into ILogger scopes via middleware enhancement

**Context:** The existing `CorrelationIdMiddleware` sets `context.TraceIdentifier` but does not push into `ILogger` scopes. Structured logs lack CorrelationId.

**Decision:** Modify `CorrelationIdMiddleware.InvokeAsync()` to create an `ILogger` scope containing `CorrelationId`. This ensures all log entries within a request automatically include the correlation identifier.

**Implications:**
- Minimal code change (~5 lines added to middleware)
- No changes needed in endpoint code or existing ILogger calls
- Scopes are automatically cleaned up by the `using` pattern

### D-05: Set explicit OTLP endpoint environment variables on API projects

**Context:** The .NET APIs auto-discover the OTLP endpoint via Aspire's env vars (`DOTNET_DASHBOARD_OTLP_ENDPOINT_URL`). Since the Aspire Dashboard isn't running, the endpoint must point to Jaeger.

**Decision:** Set `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable on both API projects in AppHost, pointing to `http://host.docker.internal:4318/`.

**Implications:**
- OTel exports go to Jaeger instead of the absent Aspire Dashboard
- No changes needed in ServiceDefaults code (env vars override defaults)
- Future: remove env vars if DCP workload is installed (Aspire auto-config takes over)

## Dependencies

- **Phase 1 (ServiceDefaults):** Existing OTel SDK setup, CorrelationIdMiddleware — enhanced in this phase
- **Phase 5 (Kong):** config.yaml and init-routes.sh — modified to add opentelemetry plugin
- **Phase 5 (Kong):** Kong must be verified operational before OTel plugin can be tested
- **Phase 3 (APIs):** Both APIs must be running to generate traces
- **Phase 5 VERIFICATION:** Should be executed before Phase 6 verification to confirm baseline routing works

## Scope

### In Scope (OTEL-02 through OTEL-07)

| Req | Description | Approach |
|-----|-------------|----------|
| OTEL-02 | Correlation ID on all requests/responses | Already done (Phase 1 + Phase 5). Verify existing implementation. |
| OTEL-03 | Correlation ID propagated and in structured logs | Enhance CorrelationIdMiddleware to push into ILogger scopes. |
| OTEL-04 | Distributed traces: browser → Kong → .NET API → PostgreSQL | W3C traceparent propagation via Kong OTel plugin + Npgsql instrumentation |
| OTEL-05 | Kong OTel plugin with OTLP collector | Add `opentelemetry` plugin to config.yaml whitelist + global rule in init-routes.sh |
| OTEL-06 | Npgsql instrumentation creates database spans | Add NuGet package + `.AddNpgsqlInstrumentation()` to ServiceDefaults |
| OTEL-07 | Aspire Dashboard shows distributed traces | **Adapted:** Use Jaeger UI for trace visualization (DCP not installed) |

### Out of Scope

- OpenTelemetry metrics dashboards (beyond traces)
- Custom span attributes for business logic
- Trace sampling strategies (always-sample for PoC)
- Alerting based on trace data
- Log aggregation pipeline (beyond OTel log export)
- Correlation ID for frontend error display (Phase 7 handles this — FE-09)

## Files Modified

| File | Change |
|------|--------|
| `.planning/phases/06-observability/init-routes.sh` | Add `opentelemetry` global rule + update existing wait logic |
| `.planning/phases/06-observability/init-routes-otel.sh` | New: separate script for OTel global rule (appended after init-routes.sh) |
| `src/OpenCode.AppHost/Program.cs` | Add Jaeger container, set OTLP endpoint env vars on APIs |
| `src/OpenCode.ServiceDefaults/Extensions.cs` | Add `.AddNpgsqlInstrumentation()`, enhance CorrelationIdMiddleware with ILogger scope |
| `src/OpenCode.ServiceDefaults/OpenCode.ServiceDefaults.csproj` | Add `Npgsql.OpenTelemetry` package reference |
| `Directory.Packages.props` | Pin `Npgsql.OpenTelemetry` version |
| `.planning/phases/06-observability/06-VERIFICATION.md` | Create verification document |

## Threat Model

| Threat | Category | Component | Mitigation |
|--------|----------|-----------|------------|
| OTLP endpoint DDoS | Denial of Service | Jaeger collector | Buffer in Kong OTel plugin (batch config), no external exposure |
| Trace data contains SQL | Information Disclosure | Npgsql instrumentation | `SetDbStatementForText=true` captures only sanitized SQL (no parameter values) |
| Correlation ID collision | Spoofing | CorrelationIdMiddleware | UUID v4 generation, no sequential IDs |
| Jaeger UI unauthenticated | Information Disclosure | Jaeger container | Not exposed to host network (Aspire inner-loop only). Accept for PoC. |

## Files Created

- `.planning/phases/06-observability/init-routes-otel.sh` — OTel plugin global rule via Admin API
- `.planning/phases/06-observability/06-VERIFICATION.md` — Verification checklist with copy-paste commands
