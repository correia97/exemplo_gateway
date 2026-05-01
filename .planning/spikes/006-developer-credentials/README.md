---
spike: 006
name: developer-credentials
type: standard
validates: "Given registered developer, when they select APIs to consume, then credentials are generated"
verdict: PENDING
related: [001-backstage-postgres, 002-backstage-docker, 003-backstage-aspire, 004-keycloak-integration, 005-api-catalog-model]
tags: [backstage, credentials, developer-portal]
---

# Spike 006: Developer Credentials

## What This Validates
Given registered developer, when they select APIs to consume, then credentials are generated

## Research

### Credential Generation Options
Backstage doesn't have built-in credential generation, but we can leverage:
- Keycloak client credentials flow
- API keys stored in database
- Integration with existing auth system

### Approach Comparison

| Approach | Tool/Library | Pros | Cons | Status |
|----------|-------------|------|------|--------|
| Keycloak client credentials | Keycloak OIDC | Native integration, secure | Requires Keycloak client | Recommended |
| Custom API key table | PostgreSQL | Full control | More implementation | Alternative |
| Backstage secrets plugin | Third-party plugin | Built for this | Extra dependency | Alternative |

### Chosen Approach
Use Keycloak client credentials flow. Developers register in Backstage → get Keycloak account → can request API access → credentials generated via Keycloak client.

### Key Configuration Points
- Developer registers via Backstage (user entity)
- Keycloak user created/linked
- API access requested via Backstage
- Keycloak client credentials issued for API access

## How to Run

The credential flow would be:
1. Developer signs in to Backstage via Keycloak
2. Developer browses API catalog
3. Developer requests access to specific APIs
4. Admin approves request
5. Keycloak client created for developer with API scopes
6. Developer gets client_id/client_secret or access token

## What to Expect
- User entity in Backstage catalog
- Keycloak user linked to Backstage user
- API access request workflow
- Credential generation on approval

## Investigation Trail

### Iteration 1: Credential flow design
- Designed the credential generation flow using Keycloak

## Results
Verdict: **PARTIAL ⚠**

Key findings:
- Backstage doesn't have built-in credential generation
- Keycloak client credentials flow is the recommended approach
- Flow: Developer signs in → browses API catalog → requests access → admin approves → Keycloak client created with API scopes

Design for credential flow:
1. Developer authenticates via Keycloak (OIDC from Spike 004)
2. Developer views API catalog (from Spike 005)
3. Developer clicks "Request Access" on desired APIs
4. Request stored in database (portal schema)
5. Admin reviews and approves requests
6. On approval, Keycloak client created for developer
7. Developer receives client_id/client_secret or access token

Surprises:
- Credential generation requires custom implementation
- Backstage is primarily a catalog/developer portal, not an API management tool
- Would need a custom plugin or external service for full credential management

Impact: Design complete. Full implementation would require additional work beyond the spike scope.