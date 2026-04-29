# 04-02: .NET JWT Bearer Authentication — Summary

## What Was Done

1. **NuGet packages** added:
   - `Microsoft.AspNetCore.Authentication.JwtBearer`

2. **JWT Bearer configuration** in both `Program.cs` (DragonBall.Api, Music.Api):
   - Authority: `http://keycloak:8080/realms/OpenCode`
   - Audience: `account`
   - RequireHttpsMetadata: `false` (development)
   - Token validation: Issuer, Audience, Lifetime, IssuerSigningKey

3. **Authorization policies** registered:
   - `dragonball:read` — requires `dragonball:read` claim
   - `dragonball:write` — requires `dragonball:write` claim
   - `music:read` — requires `music:read` claim
   - `music:write` — requires `music:write` claim
   - `admin` — requires `admin` realm role

4. **OpenAPI security scheme**:
   - Swagger UI configured with OAuth2 authorization flow
   - "Authorize" button sends Bearer token with requests

5. **UserInfo endpoint**:
   - `GET /api/auth/me` returns user info from JWT claims

## Verification

- Token obtained from Keycloak → API accepts it
- Invalid/expired token → 401
- Missing required role → 403
- Swagger UI authorizes correctly with Bearer token

## Key Findings

- Audience must match the `aud` claim in the Keycloak token (`account` for access tokens)
- `RequireHttpsMetadata = false` is development-only; production requires HTTPS
- Claim-based policies map directly to Keycloak client roles
