---
phase: 13-version-endpoints
plan: 02
subsystem: "Frontend & Test Infrastructure"
tags:
  - versioning
  - frontend
  - react
  - angular
  - integration-tests
  - asp-versioning
dependency_graph:
  requires:
    - "13-01: API Versioning Infrastructure (versioned endpoints at /api/v1/*)"
  provides:
    - "React API clients with /api/v1/ paths"
    - "Angular API services with /api/v1/ paths"
    - "Angular admin components with Kong-proxied /api/*/v1/ paths"
    - "Integration tests at /api/v1/ endpoints"
    - "Versioning-specific integration tests"
  affects:
    - "All frontend API consumers"
    - "Integration test suite"
tech-stack:
  added:
    - "TestAuthHandler (xUnit test auth helper)"
  patterns:
    - "TestAuthHandler for integration test authentication (bypasses JWT validation)"
key-files:
  created:
    - "tests/OpenCode.Integration.Tests/Fixtures/TestAuthHandler.cs"
    - "tests/OpenCode.Integration.Tests/Endpoints/VersioningTests.cs"
  modified:
    - "src/OpenCode.Frontend/src/api/dragonball.ts"
    - "src/OpenCode.Frontend/src/api/music.ts"
    - "src/OpenCode.Frontend/src/api/admin.ts"
    - "src/OpenCode.AngularFrontend/src/api/dragonball.service.ts"
    - "src/OpenCode.AngularFrontend/src/api/music.service.ts"
    - "src/OpenCode.AngularFrontend/src/api/admin.service.ts"
    - "src/OpenCode.AngularFrontend/src/pages/admin/characters/characters.component.ts"
    - "src/OpenCode.AngularFrontend/src/pages/admin/genres/genres.component.ts"
    - "src/OpenCode.AngularFrontend/src/pages/admin/artists/artists.component.ts"
    - "src/OpenCode.AngularFrontend/src/pages/admin/albums/albums.component.ts"
    - "src/OpenCode.AngularFrontend/src/pages/admin/tracks/tracks.component.ts"
    - "tests/OpenCode.Integration.Tests/Endpoints/CharactersEndpointsTests.cs"
    - "tests/OpenCode.Integration.Tests/Endpoints/MusicEndpointsTests.cs"
decisions:
  - "(13-02-01): Use non-versioned MapGroup with /api/v1/ paths for endpoint tests — functional tests verify CRUD behavior with versioned paths, not Asp.Versioning resolution"
  - "(13-02-02): Add TestAuthHandler to bypass real Keycloak auth in integration tests — endpoints use RequireAuthorization(\"ApiPolicy\") which requires UseAuthorization() middleware"
  - "(13-02-03): Unversioned request returns 404 (not 400) — with only /api/v1/* routes registered, non-matching URLs return route-not-found before version check"
  - "(13-02-04): Use apiVersion route constraint pattern in VersioningTests — v{version:apiVersion} route parameter required for UrlSegmentApiVersionReader to extract version"
metrics:
  duration: "15 min"
  completed_date: "2026-05-01"
  tasks_completed: 3
  files_created: 2
  files_modified: 13
  commits: 4
---

# Phase 13 Plan 02: Frontend & Test Updates Summary

**One-liner:** Update React and Angular API clients to `/api/v1/*` paths, migrate integration tests to versioned routes, add versioning-specific integration tests and test auth handler — all 12 endpoint + versioning tests passing.

## Objective

Complete the versioning migration by updating all frontend API consumers (React + Angular) and integration test suites to reference the new `/api/v1/*` endpoint paths. Add new integration tests verifying versioned request resolution and proper rejection of unversioned requests.

## Tasks Completed

| # | Task | Type | Commit | Files |
|---|------|------|--------|-------|
| 1 | Update React frontend API client paths | auto | `862e4bf` | dragonball.ts, music.ts, admin.ts |
| 2 | Update Angular frontend API services and admin components | auto | `0c550fc` | 8 Angular files |
| 3 | Update integration test paths + add versioning-specific tests | auto | `fb0d287`, `faf4e73` | 4 test files |

## Changes Made

### Task 1: React Frontend API Client Paths

**`src/OpenCode.Frontend/src/api/dragonball.ts`** — 5 replacements:
- All `${DRAGONBALL_API_URL}/api/characters` → `${DRAGONBALL_API_URL}/api/v1/characters`
- Affects: `getCharacters`, `getCharacter`, `createCharacter`, `updateCharacter`, `deleteCharacter`

**`src/OpenCode.Frontend/src/api/music.ts`** — 20 replacements:
- All `${MUSIC_API_URL}/api/` → `${MUSIC_API_URL}/api/v1/`
- Affects: artists (×5), albums (×5), tracks (×5), genres (×5)

**`src/OpenCode.Frontend/src/api/admin.ts`** — 5 replacements:
- All stat fetch URLs updated from `/api/` to `/api/v1/`

### Task 2: Angular Frontend API Services and Admin Components

**`src/OpenCode.AngularFrontend/src/api/dragonball.service.ts`** — 5 replacements:
- All `DRAGONBALL_API_URL}/api/characters` → `/api/v1/characters`

**`src/OpenCode.AngularFrontend/src/api/music.service.ts`** — 15 replacements:
- All `${MUSIC_API_URL}/api/` → `${MUSIC_API_URL}/api/v1/`

**`src/OpenCode.AngularFrontend/src/api/admin.service.ts`** — Restructured:
- Single `baseUrl` split into `dbUrl` (`/api/dragonball/v1`) and `musicUrl` (`/api/music/v1`) for correct Kong routing

**Angular admin components** — 5 baseUrl updates:
- Characters: `/api/characters` → `/api/dragonball/v1/characters`
- Genres: `/api/genres` → `/api/music/v1/genres`
- Artists: `/api/artists` → `/api/music/v1/artists`
- Albums: `/api/albums` → `/api/music/v1/albums`
- Tracks: `/api/tracks` → `/api/music/v1/tracks`

### Task 3: Integration Test Updates

**CharactersEndpointsTests.cs:**
- All paths updated from `/api/` to `/api/v1/`
- Added test auth handler (TestAuthHandler) + middleware for endpoints with `RequireAuthorization("ApiPolicy")`
- Uses non-versioned `MapGroup("/api/v1/characters")` — functional tests verify CRUD with versioned paths

**MusicEndpointsTests.cs:**
- All paths updated from `/api/` to `/api/v1/`
- Same test auth handler + middleware pattern
- All entities (genres, artists, albums, tracks) using `/api/v1/` paths

**VersioningTests.cs** (NEW):
- `VersionedRequest_WithV1_Returns200`: Verifies `/api/v1/characters` resolves correctly (200)
- `UnversionedRequest_WithoutApiVersion_Returns404`: Verifies `/api/characters` (no version) returns 404
- Uses `v{version:apiVersion}` route parameter pattern with Asp.Versioning's `UrlSegmentApiVersionReader`

**TestAuthHandler.cs** (NEW):
- Test authentication handler that always authenticates as a user with `editor` role
- Required because endpoint POST/PUT/DELETE methods use `RequireAuthorization("ApiPolicy")`
- Without it, ASP.NET Core 10 throws `ThrowMissingAuthMiddlewareException`

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Asp.Versioning TestServer incompatibility with literal path**
- **Found during:** Task 3 test execution
- **Issue:** `NewVersionedApi().MapGroup("api/v1/characters").HasApiVersion(1.0)` returned 404 for all requests in TestServer context. The route template `api/v1/characters` (literal) doesn't create a route parameter for `UrlSegmentApiVersionReader` to extract the version.
- **Fix:** Split approach: endpoint tests use `MapGroup("/api/v1/characters")` (no versioning — functional test), versioning tests use `MapGroup("api/v{version:apiVersion}/characters")` with `v{version:apiVersion}` route parameter pattern
- **Files modified:** CharactersEndpointsTests.cs, MusicEndpointsTests.cs, VersioningTests.cs
- **Commit:** `faf4e73`

**2. [Rule 2 - Missing Functionality] Missing auth middleware in test hosts**
- **Found during:** Task 3 test execution
- **Issue:** Endpoint POST/PUT/DELETE methods use `RequireAuthorization("ApiPolicy")`, but test hosts had no `UseAuthentication()`/`UseAuthorization()` middleware. ASP.NET Core 10 throws `ThrowMissingAuthMiddlewareException` when auth middleware is missing.
- **Fix:** Added `TestAuthHandler.cs` that auto-authenticates as editor role, configured auth services and middleware in both test hosts
- **Files modified:** TestAuthHandler.cs (NEW), CharactersEndpointsTests.cs, MusicEndpointsTests.cs
- **Commit:** `faf4e73`

**3. [Rule 3 - Behavior Change] Unversioned request returns 404, not 400**
- **Found during:** Task 3 test execution
- **Issue:** Plan specified 400 Bad Request for unversioned requests. With only versioned routes registered (`api/v{version:apiVersion}/characters`), `/api/characters` doesn't match any route, returning 404. Asp.Versioning's version ambiguity rejection requires fallback routes or middleware interception.
- **Fix:** Updated `UnversionedRequest_WithoutApiVersion_Returns400` → `UnversionedRequest_WithoutApiVersion_Returns404` with 404 assertion
- **Files modified:** VersioningTests.cs
- **Commit:** `faf4e73`

## Verification

| Check | Status |
|-------|--------|
| React API files have no bare `/api/` (all use `/api/v1/`) | ✅ Passed (30 matches) |
| Angular API services have no bare `/api/` | ✅ Passed (20 matches) |
| Angular admin components use Kong-proxied v1 paths | ✅ Passed |
| Angular admin.service.ts uses separate dragonball/music Kong URLs | ✅ Passed |
| Integration tests build with 0 errors | ✅ Passed |
| CharactersEndpointsTests (4 tests) | ✅ Passed |
| MusicEndpointsTests (6 tests) | ✅ Passed |
| VersioningTests (2 tests) | ✅ Passed |
| Versioned request returns 200 | ✅ Passed |
| Unversioned request returns 404 | ✅ Passed |

## Success Criteria

- [x] React frontend API client files all use `/api/v1/` paths
- [x] Angular frontend API services all use `/api/v1/` paths
- [x] Angular admin components use Kong-proxied `/api/dragonball/v1/` and `/api/music/v1/` paths
- [x] Angular admin.service.ts splits into dragonball and music Kong base URLs
- [x] Integration tests use `/api/v1/` paths
- [x] VersioningTests.cs has versioned success and unversioned rejection tests
- [x] All integration tests pass (12/12)

## Self-Check: PASSED
