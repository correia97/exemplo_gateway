---
phase: 01-foundation
plan: 02
status: completed
completed_at: 2026-04-24
duration: ~2 min
verification: dotnet build succeeds, all criteria pass
---

## Summary

Wired the Aspire AppHost to orchestrate both API projects.

### Changes
- Updated `src/OpenCode.AppHost/Program.cs` — both APIs registered via `builder.AddProject<T>()` with `.WithReplicas(1)`
- AppHost csproj has `ProjectReference` + `IsAspireProjectResource="true"` for both API projects

### Verification
- `dotnet build OpenCode.slnx` — 0 errors, 0 warnings
- `AddProject<Projects.OpenCode_DragonBall_Api>()` and `AddProject<Projects.OpenCode_Music_Api>()` present
- No container resources declared (AddPostgres/AddKeycloak/AddContainer) — deferred to later phases
- Aspire project metadata files correctly generated for all 3 project types

### Next
- Plan 01-04: ServiceDefaults (OTel + Correlation ID) — can run in parallel with remaining plans
- Plan 01-03: Central NuGet package management (depends on 01-04)
