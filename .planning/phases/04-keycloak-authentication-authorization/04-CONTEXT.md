# Phase 4: Keycloak Authentication & Authorization - Context

**Gathered:** 2026-04-25
**Status:** Ready for execution

<domain>
## Phase Boundary

Keycloak 26+ instance with dedicated PostgreSQL schema, realm `opencode`, OIDC clients, roles, test users, and JWT validation in .NET APIs.

</domain>

<decisions>
## Implementation Decisions

### PostgreSQL in Aspire
- Add PostgreSQL container to AppHost as Phase 4 prep (before Keycloak)
- Use `Aspire.Hosting.PostgreSQL` stable package (matches AppHost 13.2.3)
- Mount existing `src/OpenCode.Domain/Data/init.sql` to `/docker-entrypoint-initdb.d/`
- Wire connection strings for both APIs via Aspire resource references

### Keycloak container approach
- **Raw container** with env vars (no `Aspire.Hosting.Keycloak` preview package)
- Use `quay.io/keycloak/keycloak:latest` official image
- Environment: `KC_DB=postgres`, `KC_DB_URL`, `KC_DB_USERNAME=keycloak_user`, `KC_DB_PASSWORD=keycloak_pass`
- Link to PostgreSQL container via Aspire connection string reference

### Realm import mechanism
- **KC_IMPORT_REALM + JSON mount**: Mount `opencode-realm.json` via volume, set `KC_IMPORT_REALM=opencode-realm.json`
- Realm JSON is version-controlled in the phase directory

### JWT validation strategy
- **Double validation**: .NET middleware + APISIX OIDC (Phase 5)
- .NET validates JWT + role claims now; APISIX does gateway-level OIDC check in Phase 5

### Endpoint authorization model
- **Default deny + AllowAnonymous on GET**: Fallback policy requires authenticated user with `editor` role
- GET endpoints opt out via `.AllowAnonymous()` (public reads)
- POST/PUT/DELETE covered by fallback policy (requires `editor` role)

### Token configuration
- 5-minute access tokens
- Refresh tokens enabled
- `RequireHttpsMetadata = false` (development only)

### Keycloak client secrets
- Hardcoded in realm JSON (PoC scope)
- `dragonball-api`: secret `dragonball-secret`
- `music-api`: secret `music-secret`
- `frontend`: public client (PKCE)

### Roles
- `viewer`: read-only (covers AUTH-04)
- `editor`: full CRUD (covers AUTH-05)

### Test users
- `viewer1`/`viewer1`: `viewer` role only
- `editor1`/`editor1`: `viewer` + `editor` roles

### Claims transformation
- Custom `KeycloakRolesClaimsTransformation` flattens `realm_access.roles` into `ClaimTypes.Role`
- Standard pattern for Keycloak + .NET JWT integration

### NuGet packages
- `Microsoft.AspNetCore.Authentication.JwtBearer`: version matching runtime (10.0.x)
- `Aspire.Hosting.PostgreSQL`: stable, matches AppHost 13.2.3

### the agent's Discretion
- Exact Keycloak container resource naming
- Volume mount paths
- HttpHandler configuration for Aspire container waits
- Environment variable names for KC_DB_URL format

</decisions>

<canonical_refs>
## Canonical References

### Requirements
- `.planning/REQUIREMENTS.md` §Authentication & Authorization — AUTH-01 through AUTH-07

### Current infrastructure
- `src/OpenCode.AppHost/Program.cs` — Current AppHost with 2 API projects (no PG, no Keycloak)
- `src/OpenCode.Domain/Data/init.sql` — PostgreSQL init script (already has keycloak schema + user)
- `Directory.Packages.props` — Current package versions

### API endpoints (what we're protecting)
- `src/OpenCode.DragonBall.Api/Program.cs` — Dragon Ball API wiring
- `src/OpenCode.Music.Api/Program.cs` — Music API wiring
- `src/OpenCode.DragonBall.Api/Endpoints/Characters.cs` — Character endpoints
- `src/OpenCode.Music.Api/Endpoints/Genres.cs` — Genre endpoints
- `src/OpenCode.Music.Api/Endpoints/Artists.cs` — Artist endpoints
- `src/OpenCode.Music.Api/Endpoints/Albums.cs` — Album endpoints
- `src/OpenCode.Music.Api/Endpoints/Tracks.cs` — Track endpoints

### Service defaults
- `src/OpenCode.ServiceDefaults/Extensions.cs` — Contains `CorrelationIdMiddleware`, `AddServiceDefaults`, `ConfigureOpenTelemetry`

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `src/OpenCode.ServiceDefaults/Extensions.cs` — `UseCorrelationId()` middleware pattern can be followed for auth middleware
- `src/OpenCode.AppHost/Program.cs` — Current project references pattern for adding new services

### Established Patterns
- Minimal API with REPR pattern (endpoint extension methods)
- `FluentValidation` + `ProblemDetails` for error handling
- `Directory.Packages.props` for centralized version management

### Integration Points
- `AppHost/Program.cs` needs PG container + Keycloak container
- Both API `Program.cs` need `AddAuthentication()` + `AddAuthorization()` + middleware
- Endpoint files need `.AllowAnonymous()` on GET groups
- No changes needed to Domain project

</code_context>

<deferred>
## Deferred Ideas

- APISIX OIDC plugin integration — Phase 5
- Refresh token rotation in frontend — Phase 7
- Keycloak admin REST API for user management — Phase 7+
- Token revocation / logout endpoint — Phase 7+

</deferred>

---

*Phase: 04-keycloak-authentication-authorization*
*Context gathered: 2026-04-25*
