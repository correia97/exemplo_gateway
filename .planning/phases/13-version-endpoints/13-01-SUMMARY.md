---
phase: 13-version-endpoints
plan: 01
subsystem: "API Infrastructure"
tags:
  - versioning
  - openapi
  - minimal-api
  - dotnet-10
  - packages
dependency_graph:
  requires: []
  provides:
    - "Versioned endpoint infrastructure for all CRUD endpoints"
    - "Version endpoint at /api/v1/version (DBALL-14, MUSIC-19)"
    - "Versioned OpenAPI docs at /openapi/v1.json"
  affects:
    - "src/OpenCode.DragonBall.Api/* (versioned routing)"
    - "src/OpenCode.Music.Api/* (versioned routing)"
    - "Frontend API clients (need /api/v1/* paths)"
tech-stack:
  added:
    - "Asp.Versioning.Http@10.0.0"
    - "Asp.Versioning.Mvc.ApiExplorer@10.0.0"
    - "Asp.Versioning.OpenApi@10.0.0-rc.1"
  patterns:
    - "NewVersionedApi → MapGroup → HasApiVersion(double) for versioned Minimal API routing"
    - "UrlSegmentApiVersionReader for /api/v1/* path versioning"
    - "WithDocumentPerVersion() for multi-document OpenAPI generation"
    - "DescribeApiVersions() + AddDocument() for Scalar version dropdown"
key-files:
  created:
    - "src/OpenCode.DragonBall.Api/Endpoints/Version.cs"
    - "src/OpenCode.Music.Api/Endpoints/Version.cs"
  modified:
    - "Directory.Packages.props"
    - "src/OpenCode.DragonBall.Api/OpenCode.DragonBall.Api.csproj"
    - "src/OpenCode.Music.Api/OpenCode.Music.Api.csproj"
    - "src/OpenCode.DragonBall.Api/Program.cs"
    - "src/OpenCode.Music.Api/Program.cs"
    - "src/OpenCode.DragonBall.Api/Endpoints/Characters.cs"
    - "src/OpenCode.Music.Api/Endpoints/Genres.cs"
    - "src/OpenCode.Music.Api/Endpoints/Artists.cs"
    - "src/OpenCode.Music.Api/Endpoints/Albums.cs"
    - "src/OpenCode.Music.Api/Endpoints/Tracks.cs"
decisions:
  - "(13-01-01): Use HasApiVersion(1.0) double literal instead of \"1.0\" string — Asp.Versioning.Http@10.0.0 HasApiVersion overloads take double, int, or ApiVersion, not string"
  - "(13-01-02): Use AddDocument(name, title) without isDefault parameter — Scalar.AspNetCore 2.14.6 method signature"
metrics:
  duration: "15 min"
  completed_date: "2026-04-30"
  tasks_completed: 3
  files_created: 2
  files_modified: 10
  commits: 1
---

# Phase 13 Plan 01: API Versioning Infrastructure Summary

**One-liner:** Implement Asp.Versioning v10 with URL path versioning (`/api/v1/*`), versioned OpenAPI docs, version metadata endpoints, and Scalar version dropdown across both DragonBall and Music Minimal APIs.

## Objective

Migrate all existing unversioned endpoints (`/api/characters`, `/api/genres`, etc.) to versioned `/api/v1/*` paths using Asp.Versioning v10 with `UrlSegmentApiVersionReader`. Add version metadata endpoints per DBALL-14 and MUSIC-19. Configure versioned OpenAPI documentation via `WithDocumentPerVersion()` and Scalar UI with version dropdown via `DescribeApiVersions()`.

## Tasks Completed

| # | Task | Type | Files |
|---|------|------|-------|
| 1 | Add Asp.Versioning NuGet packages + update both API Program.cs | auto | Directory.Packages.props, both csproj, both Program.cs |
| 2 | Update Created() paths + create Version.cs endpoints | auto | 5 endpoint files, 2 new Version.cs files |
| 3 | Full build verification | auto | Both API projects rebuilt successfully |

## Changes Made

### NuGet Package Changes (`Directory.Packages.props` + both .csproj)

Added 3 package version pins and references:
- `Asp.Versioning.Http@10.0.0`
- `Asp.Versioning.Mvc.ApiExplorer@10.0.0`
- `Asp.Versioning.OpenApi@10.0.0-rc.1`

### Configuration Changes (both Program.cs)

**DragonBall API (`src/OpenCode.DragonBall.Api/Program.cs`):**
- Replaced `using Microsoft.OpenApi;` with `using Asp.Versioning;`
- Replaced `builder.Services.AddOpenApi()` with AddApiVersioning chain:
  - `AddApiVersioning` with `UrlSegmentApiVersionReader`
  - `AddApiExplorer` with `GroupNameFormat = "'v'VVV"`
  - `.AddOpenApi()` (Asp.Versioning variant)
- Replaced `app.MapOpenApi()` with `app.MapOpenApi().WithDocumentPerVersion()`
- Replaced endpoint registrations with versioned groups:
  - `app.NewVersionedApi("Characters").MapGroup("api/v1/characters").HasApiVersion(1.0)`
  - `app.NewVersionedApi("Seed").MapGroup("api/v1").HasApiVersion(1.0)`
  - `app.NewVersionedApi("Version").MapGroup("api/v1").HasApiVersion(1.0)`
- Updated Scalar config with `app.DescribeApiVersions()` loop for version dropdown

**Music API (`src/OpenCode.Music.Api/Program.cs`):**
- Same changes as DragonBall with Music-specific entity groups (Genres, Artists, Albums, Tracks, Seed, Version)

### Endpoint Created() Path Updates

Updated `TypedResults.Created()` paths from `/api/` to `/api/v1/`:
- `Characters.cs`: `$"/api/v1/characters/{created.Id}"`
- `Genres.cs`: `$"/api/v1/genres/{created.Id}"`
- `Artists.cs`: `$"/api/v1/artists/{created.Id}"`
- `Albums.cs`: `$"/api/v1/albums/{created.Id}"`
- `Tracks.cs`: `$"/api/v1/tracks/{created.Id}"`

### New Version Endpoint Files

**DragonBall API (`src/OpenCode.DragonBall.Api/Endpoints/Version.cs`):**
- Public GET at `/api/v1/version` (anonymous access)
- Returns: ApiName, AssemblyVersion, RuntimeVersion, OsDescription, ProcessArchitecture, EnvironmentName, Status

**Music API (`src/OpenCode.Music.Api/Endpoints/Version.cs`):**
- Same structure with ApiName = "Music API"

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Use `HasApiVersion(1.0)` double literal instead of `HasApiVersion("1.0")` string**
- **Found during:** Task 1 build verification
- **Issue:** The Asp.Versioning.Http@10.0.0 `HasApiVersion` extension method does not accept a `string` parameter. The available overloads accept `double`, `int`, `DateOnly`, or `ApiVersion`. The plan's code used `HasApiVersion("1.0")` which fails with CS1503: "cannot convert from 'string' to 'int'".
- **Fix:** Changed all `HasApiVersion("1.0")` to `HasApiVersion(1.0)` (double literal) in both Program.cs files.
- **Files modified:** Both Program.cs files
- **Commit:** `6e26883`

**2. [Rule 2 - API Compatibility] Updated Scalar `AddDocument` call**
- **Found during:** Implementation
- **Issue:** The plan used `options.AddDocument(name, title, isDefault)` with `isDefault` parameter, but Scalar.AspNetCore 2.14.6's `AddDocument` signature is `AddDocument(string name, string? title, string? routePattern)` — no `isDefault` parameter.
- **Fix:** Changed to `options.AddDocument(description.GroupName, description.GroupName)` which sets name and title for the version document.
- **Files modified:** Both Program.cs files
- **Commit:** `6e26883`

**3. [Rule 3 - Task Dependency] Merged Tasks 1 and 2 into single commit**
- **Found during:** Execution ordering
- **Issue:** Task 1 (Program.cs changes) depends on Task 2 (Version.cs files) because `Program.cs` references `MapVersionEndpoints()` which only exists in Version.cs. These cannot be committed as separate buildable units.
- **Fix:** Combined all changes into one commit after both tasks completed.
- **Commit:** `6e26883`

## Verification

| Check | Status |
|-------|--------|
| `dotnet build src/OpenCode.DragonBall.Api` | ✅ Passed (0 errors, 0 warnings) |
| `dotnet build src/OpenCode.Music.Api` | ✅ Passed (0 errors, 0 warnings) |
| Directory.Packages.props has Asp.Versioning pins | ✅ Passed |
| Both csproj have 3 new package references | ✅ Passed |
| Both Program.cs use NewVersionedApi + HasApiVersion(1.0) | ✅ Passed |
| Both Program.cs use WithDocumentPerVersion() | ✅ Passed |
| Scalar config uses DescribeApiVersions() | ✅ Passed |
| All Created() paths use /api/v1/ | ✅ Passed |
| Version.cs files exist in both endpoints dirs | ✅ Passed |
| Version metadata endpoint compiles | ✅ Passed |

## Success Criteria

- [x] All 5 endpoint files have `/api/v1/` paths in Created() responses
- [x] Both API Program.cs files compile with Asp.Versioning chain
- [x] Scalar config uses DescribeApiVersions() for version dropdown
- [x] OpenAPI is mapped with WithDocumentPerVersion()
- [x] Version.cs endpoint files exist in both API Endpoints folders
- [x] Solution builds with zero errors

## Self-Check: PASSED
