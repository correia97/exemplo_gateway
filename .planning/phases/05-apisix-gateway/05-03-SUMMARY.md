# 05-03: APISIX Auth Plugin Integration — Summary

## What Was Done

1. **JWT Authentication plugin** configured on API routes:
   - `jwt-auth` plugin added to Dragon Ball and Music API routes
   - Plugin verifies JWT signature using Keycloak realm public key
   - Token extracted from `Authorization: Bearer <token>` header

2. **Keycloak OIDC plugin** (alternative approach):
   - `openid-connect` plugin configured on auth routes
   - Discovery endpoint: `http://keycloak:8080/realms/OpenCode/.well-known/openid-configuration`
   - Client ID and secret for frontend public client

3. **Public routes**:
   - `GET /api/auth/*` — no auth required (login, token refresh)
   - Root `/` — redirect to frontend

4. **Route-level auth enforcement**:
   - `jwt-auth` on read routes (validates token)
   - `jwt-auth + consumer-restriction` on write routes (validates role)
   - Consumer groups map Keycloak roles to APISIX consumers

## Verification

- Request without token → 401 from APISIX
- Request with valid token → proxied to backend
- Request with invalid token/signature → 401
- Request without required role → 403

## Key Findings

- APISIX JWT auth validates token before request reaches backend (early rejection)
- Consumer-restriction plugin maps to Keycloak roles for fine-grained access
- OIDC discovery endpoint simplifies configuration management
