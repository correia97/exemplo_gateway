---
phase: 04
fixed_at: 2026-04-30T12:30:00Z
review_path: .planning/phases/04-keycloak-authentication-authorization/04-REVIEW.md
iteration: 1
findings_in_scope: 11
fixed: 10
skipped: 1
status: partial
---

# Phase 04: Code Review Fix Report — Keycloak Authentication & Authorization

**Fixed at:** 2026-04-30T12:30:00Z
**Source review:** `.planning/phases/04-keycloak-authentication-authorization/04-REVIEW.md`
**Iteration:** 1

**Summary:**
- Findings in scope: 11 (3 critical, 8 warning)
- Fixed: 10
- Skipped: 1

## Fixed Issues

### CR-01: Authorization Policy Never Applied to Endpoints (Authorization Bypass)

**Files modified:** `src/OpenCode.DragonBall.Api/Endpoints/Characters.cs`, `src/OpenCode.Music.Api/Endpoints/Artists.cs`, `src/OpenCode.Music.Api/Endpoints/Albums.cs`, `src/OpenCode.Music.Api/Endpoints/Genres.cs`, `src/OpenCode.Music.Api/Endpoints/Tracks.cs`
**Commit:** `1c6f185`
**Applied fix:** Added `.RequireAuthorization("ApiPolicy")` to all POST, PUT, and DELETE endpoints across all 5 endpoint files. GET endpoints remain `.AllowAnonymous()` for public read access. This ensures only users with the `editor` realm role can create, update, or delete resources.

### CR-02: Audience Validation Disabled

**Files modified:** `src/OpenCode.DragonBall.Api/Program.cs`, `src/OpenCode.Music.Api/Program.cs`
**Commit:** `4dbf1f9`
**Applied fix:** Changed `TokenValidationParameters.ValidateAudience = false` to `true` in both `Program.cs` files. Each API already has `options.Audience` set to its respective client ID (`dragonball-api` and `music-api`), so audience validation now enforces that tokens are used only for the intended API.

### CR-03: Hardcoded Client Secrets in Realm Export

**Files modified:** `deploy/keycloak/OpenCode-realm.json`
**Commit:** `2d59b32`
**Applied fix:** Replaced the three hardcoded client secrets with placeholder environment variable references:
- `dragonball-api`: `z0ha560UlsBzE5jkufQU0xDO5Ske4ACb` → `${DRAGONBALL_API_SECRET}`
- `frontend`: `b6ZmO3ykike5hwWYrl5tu3m4vAdSPajl` → `${FRONTEND_SECRET}`
- `music-api`: `vN9206gfp08XJjyw5xjGqehsRNLgeVBv` → `${MUSIC_API_SECRET}`

These secrets should be regenerated before any non-development deployment and injected via Keycloak environment variables or Kubernetes secrets.

### WR-01: Empty Catch Block Swallows JSON Exceptions

**Files modified:** `src/OpenCode.DragonBall.Api/Auth/KeycloakRolesClaimsTransformation.cs`, `src/OpenCode.Music.Api/Auth/KeycloakRolesClaimsTransformation.cs`
**Commit:** `aba7d03`
**Applied fix:** Added `ILogger<KeycloakRolesClaimsTransformation>` dependency injection via constructor. Replaced the empty `catch (JsonException) {}` with `catch (JsonException ex) { _logger.LogWarning(ex, "Failed to parse realm_access claim from JWT token"); }`. The logger is already auto-registered by the ASP.NET Core hosting framework.

### WR-02: Contradictory Client Configuration — Public Client with Secret

**Files modified:** `deploy/keycloak/OpenCode-realm.json`
**Commit:** `90f5e7c`
**Applied fix:** Removed the `"secret"` property from the `frontend` client. The client is correctly configured as `"publicClient": true` with PKCE (`"pkce.code.challenge.method": "S256"`), which is the recommended approach for a browser-based SPA. Public clients should not have secrets since they cannot be kept confidential in browser-accessible applications.

### WR-04: Full Scope Allowed on API and Frontend Clients

**Files modified:** `deploy/keycloak/OpenCode-realm.json`
**Commit:** `aa2ae4e`
**Applied fix:** Set `"fullScopeAllowed": false` for `dragonball-api`, `frontend`, and `music-api` clients. This restricts each client to only the roles explicitly scoped to them, preventing token requests with elevated privileges (e.g., `realm-management` roles).

### WR-05: Service Accounts Missing `editor` Role

**Files modified:** `deploy/keycloak/OpenCode-realm.json`
**Commit:** `e6da9fe`
**Applied fix:** Added `"editor"` to the `realmRoles` of both `service-account-dragonball-api` and `service-account-music-api`. Previously they only had `default-roles-opencode` (which grants `offline_access` and `uma_authorization`). The `editor` role is required by the `ApiPolicy` authorization policy for write operations.

### WR-06: Brute Force Protection Disabled

**Files modified:** `deploy/keycloak/OpenCode-realm.json`
**Commit:** `09b3c5b`
**Applied fix:** Enabled brute force protection by setting `"bruteForceProtected": true` and reduced `"failureFactor"` from 30 to 5 (fewer failed attempts before lockout). The existing `"maxFailureWaitSeconds": 900` (15 min) and `"waitIncrementSeconds": 60` (1 min increment) remain, providing reasonable rate limiting against password guessing.

### WR-07: Claims Transformation Ignores Client-Level Roles

**Files modified:** `src/OpenCode.DragonBall.Api/Auth/KeycloakRolesClaimsTransformation.cs`, `src/OpenCode.Music.Api/Auth/KeycloakRolesClaimsTransformation.cs`
**Commit:** `eba08e3`
**Applied fix:** Added parsing of the `resource_access` JWT claim to extract client-level roles in addition to the existing `realm_access` parsing. The new code iterates over all clients in the `resource_access` dictionary and adds their roles as `ClaimTypes.Role` claims. Both extractions are wrapped in try/catch blocks with logging for graceful failure on malformed claims.

### WR-08: User Self-Registration Enabled Without Email Verification

**Files modified:** `deploy/keycloak/OpenCode-realm.json`
**Commit:** `972adaf`
**Applied fix:** Set `"verifyEmail": true` to require email verification before self-registered accounts are activated. Registration remains enabled (`"registrationAllowed": true`) so users can still sign up, but they must verify their email address before accessing the APIs.

## Skipped Issues

### WR-03: Duplicate Code — Claims Transformation Identical in Two Projects

**File:** `src/OpenCode.DragonBall.Api/Auth/KeycloakRolesClaimsTransformation.cs`, `src/OpenCode.Music.Api/Auth/KeycloakRolesClaimsTransformation.cs`
**Reason:** Skipped — requires structural project change
**Original issue:** The `KeycloakRolesClaimsTransformation` class is identical in both API projects, creating a maintenance risk.

**Why skipped:** Extracting the shared code requires either:
1. Adding `Microsoft.AspNetCore.Authentication.Abstractions` (via `FrameworkReference`) to the `OpenCode.Domain` project, which would introduce ASP.NET Core dependencies into the domain layer, breaking clean architecture separation
2. Creating a new `OpenCode.Shared.Auth` class library project with shared auth abstractions

Both options are valid but require project-level changes (csproj modifications, solution file updates, potentially new project creation) that go beyond the scope of targeted code fixes. This should be addressed as a separate refactoring task when the shared infrastructure library is established.

---

_Fixed: 2026-04-30T12:30:00Z_
_Fixer: gsd-code-fixer (intelligent)_
_Iteration: 1_
