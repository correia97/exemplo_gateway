---
phase: 04-keycloak-authentication-authorization
reviewed: 2026-04-30T12:00:00Z
depth: standard
files_reviewed: 3
files_reviewed_list:
  - src/OpenCode.DragonBall.Api/Auth/KeycloakRolesClaimsTransformation.cs
  - src/OpenCode.Music.Api/Auth/KeycloakRolesClaimsTransformation.cs
  - deploy/keycloak/OpenCode-realm.json
findings:
  critical: 3
  warning: 8
  info: 3
  total: 14
status: issues_found
---

# Phase 04: Code Review Report — Keycloak Authentication & Authorization

**Reviewed:** 2026-04-30T12:00:00Z
**Depth:** standard
**Files Reviewed:** 3 (plus 5 related files for context: Program.cs, endpoint files)
**Status:** issues_found

## Summary

Reviewed the Keycloak authentication and authorization implementation for both Dragon Ball and Music APIs. Three primary files were analyzed: two identical `KeycloakRolesClaimsTransformation.cs` implementations and the `OpenCode-realm.json` Keycloak realm configuration (2883 lines). Additionally, `Program.cs` files and endpoint handlers were examined for authorization policy application.

**Key finding:** The `ApiPolicy` requiring the `editor` role is **defined in configuration but never applied to any endpoint**. All POST/PUT/DELETE endpoints rely on the default fallback authorization (authenticated user only), meaning any authenticated user — including self-registered users with no roles — can create, update, and delete resources. This is a critical authorization bypass.

Additional significant issues include disabled JWT audience/issuer validation, hardcoded client secrets in the realm export, and several problematic Keycloak configuration settings.

---

## Critical Issues

### CR-01: Authorization Policy Never Applied to Endpoints (Authorization Bypass)

**Files:**
- `src/OpenCode.DragonBall.Api/Program.cs:62-67`
- `src/OpenCode.Music.Api/Program.cs:64-69`
- `src/OpenCode.DragonBall.Api/Endpoints/Characters.cs:15-17`
- `src/OpenCode.Music.Api/Endpoints/Artists.cs:16-18`
- `src/OpenCode.Music.Api/Endpoints/Tracks.cs:15-17`
- `src/OpenCode.Music.Api/Endpoints/Albums.cs:16-18`
- `src/OpenCode.Music.Api/Endpoints/Genres.cs:15-17`

**Issue:** The `ApiPolicy` named policy is defined requiring both `RequireAuthenticatedUser()` and `RequireRole("editor")`, but it is **never applied to any endpoint**. Endpoints are registered with either `.AllowAnonymous()` (GET, Seed) or nothing at all (POST, PUT, DELETE). When no `.RequireAuthorization(...)` is specified, ASP.NET Core uses the **default authorization policy**, which only requires an authenticated user — not the `editor` role.

This means any user with a valid JWT (including self-registered users with no special roles) can:
- Create, update, and delete characters (Dragon Ball API)
- Create, update, and delete artists, albums, genres, and tracks (Music API)

The `ApiPolicy` policy exists only as dead configuration.

**Fix:**
```csharp
// In each endpoint file, apply the policy to mutating endpoints:
group.MapPost("/", CreateAsync).RequireAuthorization("ApiPolicy");
group.MapPut("/{id:int}", UpdateAsync).RequireAuthorization("ApiPolicy");
group.MapDelete("/{id:int}", DeleteAsync).RequireAuthorization("ApiPolicy");

// Alternatively, set ApiPolicy as the default fallback policy:
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireRole("editor")
        .Build())
    .AddPolicy("ApiPolicy", policy => { /* existing */ });
```

---

### CR-02: Audience Validation Disabled

**Files:**
- `src/OpenCode.DragonBall.Api/Program.cs:57`
- `src/OpenCode.Music.Api/Program.cs:60`

**Issue:** `TokenValidationParameters.ValidateAudience = false` means the JWT audience (`aud` claim) is never checked. A JWT issued for the Music API can be used to access the Dragon Ball API, and vice versa. While `RequireHttpsMetadata = false` and `ValidateIssuer = false` are acceptable for local development, audience validation is a fundamental security boundary between services.

**Fix:**
```csharp
options.TokenValidationParameters.ValidateAudience = true;
// The Audience property is already set (line 55/58), so validation will work:
// options.Audience = "dragonball-api";  // or "music-api"
```

---

### CR-03: Hardcoded Client Secrets in Realm Export

**File:** `deploy/keycloak/OpenCode-realm.json:777, 850, 952`

**Issue:** Three client secrets are hardcoded in plaintext in the realm JSON:
- `dragonball-api`: `z0ha560UlsBzE5jkufQU0xDO5Ske4ACb` (line 777)
- `frontend`: `b6ZmO3ykike5hwWYrl5tu3m4vAdSPajl` (line 850)
- `music-api`: `vN9206gfp08XJjyw5xjGqehsRNLgeVBv` (line 952)

These secrets are committed to source control and accessible to anyone with repository access. While Keycloak realm exports inherently include generated secrets, they should be managed through secure means (environment variables, secrets manager) and regenerated in production.

**Fix:**
- Replace secrets with placeholder values in the realm JSON (e.g., `"${DRAGONBALL_API_SECRET}"`) and inject real values via Keycloak environment variables or startup configuration
- Ensure the `.gitignore` or `.claudeignore` marks realm files that contain actual secrets, or use Keycloak's `--import` with secret variables
- Regenerate secrets before deploying to any non-development environment

---

## Warnings

### WR-01: Empty Catch Block Swallows JSON Exceptions

**Files:**
- `src/OpenCode.DragonBall.Api/Auth/KeycloakRolesClaimsTransformation.cs:33-35`
- `src/OpenCode.Music.Api/Auth/KeycloakRolesClaimsTransformation.cs:33-35`

**Issue:** The empty `catch (JsonException) {}` block silently discards JSON parsing errors. If the `realm_access` claim contains malformed JSON (e.g., due to token manipulation or Keycloak version differences), the exception is swallowed without logging. The method returns the cloned principal with no roles added. While this results in a deny for role-requiring policies, it makes debugging authentication failures extremely difficult.

**Fix:**
```csharp
catch (JsonException ex)
{
    // Log the exception for debugging
    // e.g., ILogger.LogWarning(ex, "Failed to parse realm_access claim");
    // Return the clone without roles
}
```

The claims transformation class should accept an `ILogger<KeycloakRolesClaimsTransformation>` via dependency injection.

---

### WR-02: Contradictory Client Configuration — Public Client with Secret

**File:** `deploy/keycloak/OpenCode-realm.json:850, 874`

**Issue:** The `frontend` client has `"publicClient": true` (line 874) but also has a `"secret"` property (line 850: `b6ZmO3ykike5hwWYrl5tu3m4vAdSPajl`). A public client (SPA) should not have a client secret — secrets in browser-accessible applications can be extracted by users. If the secret is unused (Keycloak ignores it for public clients), it creates confusion. If it is used, the frontend is a confidential client, and `publicClient` should be `false`.

**Fix:** Set `"publicClient": false` if the frontend truly uses the secret for authentication, or remove the `"secret"` field and set `"publicClient": true` (correct for a browser-based SPA using PKCE). For a React SPA with Keycloak, public client with PKCE is the recommended approach.

```json
{
  "clientId": "frontend",
  "publicClient": true,
  // Remove the "secret" property entirely when public
  // Ensure PKCE is configured (it is: "pkce.code.challenge.method": "S256")
}
```

---

### WR-03: Duplicate Code — Claims Transformation Identical in Two Projects

**Files:**
- `src/OpenCode.DragonBall.Api/Auth/KeycloakRolesClaimsTransformation.cs` (39 lines)
- `src/OpenCode.Music.Api/Auth/KeycloakRolesClaimsTransformation.cs` (39 lines)

**Issue:** The `KeycloakRolesClaimsTransformation` class is identical in both API projects. This is a maintenance risk — any change to role extraction logic must be applied in two places. Since both APIs share a Keycloak realm, the claims transformation logic is identical and should be shared.

**Fix:** Move the `KeycloakRolesClaimsTransformation` class into the shared `OpenCode.Domain` or a new shared class library project (`OpenCode.Shared.Auth`), and reference it from both API projects.

---

### WR-04: Full Scope Allowed on API and Frontend Clients

**File:** `deploy/keycloak/OpenCode-realm.json:811, 894, 986`

**Issue:** The `dragonball-api` (line 811), `frontend` (line 894), and `music-api` (line 986) clients all have `"fullScopeAllowed": true`. This means these clients can obtain access tokens containing all realm roles, not just the roles explicitly scoped to them. Combined with `ValidateAudience = false`, a compromised client could request tokens with elevated privileges (e.g., `realm-management` roles).

**Fix:** Set `"fullScopeAllowed": false` and explicitly define only the required client scopes for each client. For the API clients, only the `editor` and `viewer` roles are needed.

---

### WR-05: Service Accounts Missing `editor` Role

**File:** `deploy/keycloak/OpenCode-realm.json:510-551`

**Issue:** The service accounts for `dragonball-api` (line 512) and `music-api` (line 533) are assigned only `default-roles-opencode`, which grants `offline_access` and `uma_authorization`. Neither service account has the `editor` role. If these service accounts are intended to consume the APIs via client credentials grant, they will fail the `ApiPolicy` authorization check (which requires `RequireRole("editor")`).

If the service accounts are NOT meant to access the API endpoints directly (only for UMA protection), this is acceptable — but the intent should be documented.

**Fix:** Add the `editor` realm role to both service accounts if they need write access:
```json
"realmRoles": [
  "default-roles-opencode",
  "editor"
]
```
Or if the service accounts are not intended for API access, document this rationale.

---

### WR-06: Brute Force Protection Disabled

**File:** `deploy/keycloak/OpenCode-realm.json:38`

**Issue:** `"bruteForceProtected": false` (line 38) means there is no protection against brute-force login attempts. Combined with `"registrationAllowed": true` (line 30), an attacker can register an account and repeatedly attempt password guessing on other accounts without rate limiting.

**Fix:** Enable brute force protection:
```json
"bruteForceProtected": true,
"failureFactor": 5,
"maxFailureWaitSeconds": 900,
"waitIncrementSeconds": 60,
```

---

### WR-07: Claims Transformation Ignores Client-Level Roles

**Files:**
- `src/OpenCode.DragonBall.Api/Auth/KeycloakRolesClaimsTransformation.cs:11-31`
- `src/OpenCode.Music.Api/Auth/KeycloakRolesClaimsTransformation.cs:11-31`

**Issue:** The claims transformation only parses `realm_access.roles` (realm-level roles) but ignores `resource_access` (client-level roles). Keycloak stores client-specific roles in the `resource_access` claim (e.g., `resource_access.dragonball-api.roles`). If authorization policies ever need client-specific roles, the transformation will not pick them up.

Currently, the application uses realm-level `viewer` and `editor` roles, so this is not immediately blocking. However, it limits future flexibility.

**Fix:** Also extract client-level roles from `resource_access`:
```csharp
// Additional extraction for client roles
var resourceAccessClaim = principal.FindFirst("resource_access");
if (resourceAccessClaim is not null)
{
    var resourceAccess = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(resourceAccessClaim.Value);
    if (resourceAccess is not null)
    {
        foreach (var (clientId, clientData) in resourceAccess)
        {
            if (clientData.TryGetProperty("roles", out var clientRolesElement))
            {
                var clientRoles = JsonSerializer.Deserialize<List<string>>(clientRolesElement.GetRawText());
                if (clientRoles is not null)
                {
                    foreach (var role in clientRoles)
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }
        }
    }
}
```

---

### WR-08: User Self-Registration Enabled Without Email Verification

**File:** `deploy/keycloak/OpenCode-realm.json:30, 33`

**Issue:** `"registrationAllowed": true` (line 30) and `"verifyEmail": false` (line 33) allow anyone to register an account without email verification. While the `ApiPolicy` requires the `editor` role for write operations (and this policy is not yet applied — see CR-01), self-registered users with `default-roles-opencode` can still read all data. If the `ApiPolicy` is fixed (CR-01), self-registered users are limited to read access, which may be acceptable for a PoC.

**Fix for production:** Set `"registrationAllowed": false` or `"verifyEmail": true` to prevent anonymous account creation.

---

## Info

### IN-01: Null-Forgiving Operator on `clone.Identity`

**Files:**
- `src/OpenCode.DragonBall.Api/Auth/KeycloakRolesClaimsTransformation.cs:16`
- `src/OpenCode.Music.Api/Auth/KeycloakRolesClaimsTransformation.cs:16`

**Issue:** `var identity = (ClaimsIdentity)clone.Identity!;` uses the null-forgiving operator (`!`). While `ClaimsPrincipal.Clone()` should always produce a principal with an identity, a null value here would throw a `NullReferenceException` at the cast. The null-forgiving operator suppresses compiler warnings without adding runtime safety.

**Fix:**
```csharp
if (clone.Identity is not ClaimsIdentity identity)
    return Task.FromResult(clone);
```

---

### IN-02: Empty Authorization Services Configuration with ENFORCING Mode

**File:** `deploy/keycloak/OpenCode-realm.json:829-836, 1004-1011`

**Issue:** Both `dragonball-api` (line 829) and `music-api` (line 1004) have `"authorizationServicesEnabled": true` with `"policyEnforcementMode": "ENFORCING"` but empty `resources`, `policies`, and `scopes`. If a Keycloak Policy Enforcement Point (PEP) were used, this would deny all access. The .NET APIs use standard JWT Bearer authentication (not Keycloak PEP), so this configuration is currently dormant but misleading.

**Fix:** Either remove `authorizationSettings` entirely (or set it to `DISABLED`), or configure meaningful resources/policies if UMA protection is intended. Since the APIs use ASP.NET Core's built-in authorization, `authorizationServicesEnabled` should likely be `false`.

---

### IN-03: Hardcoded Localhost Authority URL

**Files:**
- `src/OpenCode.DragonBall.Api/Program.cs:54`
- `src/OpenCode.Music.Api/Program.cs:57`

**Issue:** `options.Authority = "http://localhost:8080/realms/OpenCode"` is hardcoded. While acceptable for development, this should be configurable via `appsettings.json` or environment variables for different environments (Docker Compose, production).

**Fix:**
```csharp
options.Authority = builder.Configuration["Keycloak:Authority"] 
    ?? "http://localhost:8080/realms/OpenCode";
```

Add to `appsettings.json`:
```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/OpenCode",
    "Audience": "dragonball-api"
  }
}
```

---

## Summary of Findings by Severity

| ID | Severity | File | Issue |
|----|----------|------|-------|
| CR-01 | Critical | Program.cs + Endpoints | Authorization policy never applied to endpoints — any authenticated user can write |
| CR-02 | Critical | Program.cs | Audience validation disabled — cross-service token reuse |
| CR-03 | Critical | OpenCode-realm.json | Hardcoded client secrets in source control |
| WR-01 | Warning | ClaimsTransformation.cs | Empty catch block swallows JSON errors |
| WR-02 | Warning | OpenCode-realm.json | Frontend client: publicClient + secret contradicts |
| WR-03 | Warning | ClaimsTransformation.cs | Duplicate code across two projects |
| WR-04 | Warning | OpenCode-realm.json | fullScopeAllowed: true grants excessive roles |
| WR-05 | Warning | OpenCode-realm.json | Service accounts missing editor role |
| WR-06 | Warning | OpenCode-realm.json | Brute force protection disabled |
| WR-07 | Warning | ClaimsTransformation.cs | Ignores client-level roles (resource_access) |
| WR-08 | Warning | OpenCode-realm.json | Self-registration without email verification |
| IN-01 | Info | ClaimsTransformation.cs | Null-forgiving operator on identity cast |
| IN-02 | Info | OpenCode-realm.json | Empty authorization settings with ENFORCING mode |
| IN-03 | Info | Program.cs | Hardcoded localhost authority URL |

---

_Reviewed: 2026-04-30T12:00:00Z_
_Reviewer: gsd-code-reviewer (standard depth)_
