# Phase 13: Version Endpoints — Context

**Gathered:** 2026-04-30
**Status:** Ready for planning

<domain>
## Phase Boundary

Implement .NET 10 API versioning across both DragonBall and Music APIs using `Asp.Versioning` v10 with URL path versioning (`/api/v1/*`). Migrate all existing endpoints from `/api/*` to `/api/v1/*`, update Scalar documentation for versioned OpenAPI docs, update Kong is automatic (existing routes handle new paths), and update both frontend API clients and test suites.

</domain>

<decisions>
## Implementation Decisions

### Versioning Strategy
- **D-01: URL path versioning** — `/api/v1/characters`, `/api/v1/genres`, etc. Using `UrlSegmentApiVersionReader`. Most explicit and discoverable approach.
- **D-02: Full migration** — All existing endpoints moved from `/api/*` to `/api/v1/*`. Clean break — no backward-compat unversioned routes kept.
- **D-03: Both APIs** — Both DragonBall API and Music API get versioned infrastructure.

### Technology
- **D-04: Asp.Versioning v10** — Use `Asp.Versioning.Http` (Minimal APIs), `Asp.Versioning.Mvc.ApiExplorer`, and `Asp.Versioning.OpenApi` packages.
- **D-05: Versioned OpenAPI** — Use `WithDocumentPerVersion()` to generate separate OpenAPI docs per version (`/openapi/v1.json`). Scalar UI shows version dropdown.
- **D-06: NuGet packages** — `Asp.Versioning.Http@10.0.0`, `Asp.Versioning.Mvc.ApiExplorer@10.0.0`, `Asp.Versioning.OpenApi@10.0.0-rc.1`

### Endpoint Structure
- **D-07: Versioned route groups** — Use `app.NewVersionedApi("Characters").MapGroup("api/v1/characters").HasApiVersion("1.0")` pattern for each entity group.
- **D-08: Keep MapGroup pattern** — Existing endpoint extension methods (`MapCharacterEndpoints()`, `MapGenreEndpoints()`) registered on versioned groups instead of unversioned ones.

### Kong
- **D-09: No Kong config changes needed** — Existing routes (`/api/dragonball/*` with `strip_path: true`) automatically forward `/api/dragonball/v1/characters` → upstream `/api/v1/characters`. The `/api/v1/` segment passes through naturally.

### Frontend Updates
- **D-10: Update both React and Angular** — Update API client base URLs and all endpoint paths from `/api/characters` to `/api/v1/characters` in both frontends.

### Test Updates
- **D-11: Update existing tests + add versioning tests** — Update path references in all 162 unit tests and 44 integration tests. Add new tests: verify versioned request resolves correctly, verify unversioned fallback to default version.

### Scalar
- **D-12: Versioned Scalar docs** — Configure `app.MapOpenApi().WithDocumentPerVersion()` and `app.MapScalarApiReference()` with documents from `app.DescribeApiVersions()`.

### the agent's Discretion
- Exact version format string in `GroupNameFormat` (default: `"'v'VVV"`)
- Default API version setup (`DefaultApiVersion = new ApiVersion(1, 0)`)
- Endpoint registration helper pattern (extension methods vs inline in Program.cs)
- Existing Program.cs refactoring approach

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Reference Article
- `https://devblogs.microsoft.com/dotnet/api-versioning-in-dotnet-10-applications/` — Official Microsoft guide on Asp.Versioning v10 with OpenAPI in .NET 10. Covers controller and Minimal API setup, URL/query/header versioning, Scalar/SwaggerUI integration, and migration from v8.

### Phase Requirements
- `.planning/REQUIREMENTS.md` — DBALL-14, MUSIC-19 (version endpoint requirements — scope expanded to full API versioning)

### Existing API Code
- `src/OpenCode.DragonBall.Api/Program.cs` — Existing endpoint registration patterns (MapGroup, MapCharacterEndpoints)
- `src/OpenCode.Music.Api/Program.cs` — Same pattern for Music endpoints
- `src/OpenCode.DragonBall.Api/Endpoints/` — Endpoint extension method definitions
- `src/OpenCode.Music.Api/Endpoints/` — Music endpoint definitions

### Kong
- `deploy/kong/init-routes.sh` — Existing Kong routes (no changes needed)

### Frontend API Clients
- `src/OpenCode.Frontend/src/api/` — React API client functions (need path updates)
- `src/OpenCode.AngularFrontend/src/api/` — Angular API client services

### Tests
- `tests/OpenCode.Api.Tests/` — 162 unit tests (need path updates)
- `tests/OpenCode.Integration.Tests/` — 44 integration tests (need path updates)

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `src/OpenCode.DragonBall.Api/Program.cs` — MapGroup endpoint registration pattern (refactor for versioned groups)
- `src/OpenCode.Music.Api/Program.cs` — Same pattern
- `deploy/kong/init-routes.sh` — Existing Kong routes handle `/api/v1/*` without changes

### Established Patterns
- **Minimal API extensions**: `Map{Entity}Endpoints()` extension methods on `RouteGroupBuilder` — keep this pattern, just register on versioned groups
- **ServiceDefaults**: Shared infrastructure in `OpenCode.ServiceDefaults` — Asp.Versioning config will go in each API's Program.cs (not shared, since each API needs independent versioning setup)

### Integration Points
- Both API Program.cs files need: `AddApiVersioning()` → `AddApiExplorer()` → `AddOpenApi()` → `MapOpenApi().WithDocumentPerVersion()`
- Frontend API client paths: `DRAGONBALL_API_URL/api/characters` → `DRAGONBALL_API_URL/api/v1/characters`
- Kong: no changes needed — `/api/dragonball/v1/characters` → upstream `/api/v1/characters` automatically

</code_context>

<specifics>
## Specific Ideas

- "Use the reference article's pattern exactly: NewVersionedApi → MapGroup → HasApiVersion for Minimal APIs"
- "Asp.Versioning v10 release candidate may still have quirks — test versioned OpenAPI document generation early"
- "Frontend updates are mechanical path changes — search-replace /api/ → /api/v1/ in API client URLs"
- "Test server integration tests may need explicit version in request URLs or default version config"

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope.

</deferred>

---

*Phase: 13-version-endpoints*
*Context gathered: 2026-04-30*
