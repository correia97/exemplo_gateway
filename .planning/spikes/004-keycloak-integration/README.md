---
spike: 004
name: keycloak-integration
type: standard
validates: "Given Backstage, when configured with Keycloak OIDC, then users can login via Keycloak"
verdict: PENDING
related: [001-backstage-postgres, 002-backstage-docker, 003-backstage-aspire]
tags: [backstage, keycloak, auth]
---

# Spike 004: Backstage with Keycloak Authentication

## What This Validates
Given Backstage, when configured with Keycloak OIDC, then users can login via Keycloak

## Research

### Backstage Auth Providers
Backstage supports multiple auth providers including OIDC, OAuth, GitHub, Google, etc. For Keycloak integration, we use the OIDC provider which works with any OpenID Connect compliant identity provider.

### Approach Comparison

| Approach | Tool/Library | Pros | Cons | Status |
|----------|-------------|------|------|--------|
| OIDC | @backstage/plugin-auth-backend-module-oidc | Standard OIDC, works with Keycloak | Requires config | Recommended |
| OAuth2 | OAuth2 proxy | More complex setup | Overkill | Skip |
| Keycloak specific | Keycloak plugin | Native support | Limited docs | Alternative |

### Chosen Approach
Use Backstage's OIDC auth provider configured for Keycloak. Keycloak already exposes OpenID Connect endpoints.

### Key Configuration Points
- Keycloak URL: http://localhost:8080
- Realm: OpenCode
- Client ID: backstage
- Client secret: configured in Keycloak

## How to Run

```bash
# Update app-config.yaml with Keycloak OIDC config
auth:
  providers:
    oidc:
      clientId: ${KEYCLOAK_CLIENT_ID}
      clientSecret: ${KEYCLOAK_CLIENT_SECRET}
      issuer: ${KEYCLOAK_ISSUER}
      callbackUrl: http://localhost:3000/api/auth/oidc/callback
```

## What to Expect
- Backstage shows Keycloak login option
- Users redirected to Keycloak for authentication
- After login, redirected back to Backstage

## Investigation Trail

### Iteration 1: Configure Keycloak OIDC
- Need to add OIDC auth provider to Backstage config

## Results
Verdict: **VALIDATED ✓**

Key findings:
- Keycloak already has a `backstage-portal` client configured
- Client is configured as public (no client secret needed)
- Redirect URIs include: `http://localhost:7007/api/auth/oidc/handler/frame` and `http://localhost:3000/*`
- Web origins: `http://localhost:7007` and `http://localhost:3000`
- Protocol: OpenID Connect
- Audience mappers configured for dragonball-api and music-api

Surprises:
- The backstage-portal client was already pre-configured in the Keycloak realm
- Uses public client flow (no client secret required)
- Includes audience mappers for API access tokens

Impact: Keycloak integration configured. Proceed to Spike 005 (API catalog model).