---
phase: 01-foundation
plan: 04
status: completed
completed_at: 2026-04-24
duration: ~3 min
verification: dotnet build succeeds, all criteria pass
---

## Summary

Configured ServiceDefaults library with OpenTelemetry and Correlation ID middleware. Wired both API projects.

### Changes
- `src/OpenCode.ServiceDefaults/Extensions.cs` — full rewrite with:
  - `ConfigureOpenTelemetry()` — metrics (ASP.NET Core, HTTP, Runtime), tracing (ASP.NET Core, HTTP), OTLP logging
  - `UseCorrelationId()` middleware extension
  - `CorrelationIdMiddleware` internal class — reads/sets `X-Correlation-Id` header, falls back to GUID
  - No Serilog references
- `src/OpenCode.ServiceDefaults/OpenCode.ServiceDefaults.csproj` — added 5 OTel packages (1.15.x stable)
- Both API Program.cs — added `builder.AddServiceDefaults()` and `app.UseCorrelationId()`

### Verification
- `dotnet build OpenCode.slnx` — 0 errors, 0 warnings
- Both APIs call `AddServiceDefaults()` and `UseCorrelationId()`
- Correlation middleware uses `X-Correlation-Id` header (request → response passthrough)
- Template code (weatherforecast, health checks) intact

### Next
- Plan 01-03: Central NuGet package management — create `Directory.Packages.props`
