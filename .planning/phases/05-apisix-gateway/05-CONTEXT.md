# Phase 5: Kong Gateway — Design Context

## Deployment

- Kong `apache/apisix:3.9.1-alpine` as raw container via `AddContainer` in AppHost
- etcd container for Kong data persistence (route definitions, plugin configs)
- Routes created via Admin API at startup through an init script
- Config files stored in `.planning/phases/05-apisix-gateway/`

## Route Definitions

| Prefix | Upstream | Strip Prefix | Auth |
|--------|----------|-------------|------|
| `/api/dragonball/*` | Dragon Ball API | Yes | OIDC on write routes |
| `/api/music/*` | Music API | Yes | OIDC on write routes |

- `strip_prefix: true` — backend APIs see `/characters`, `/genres` without `/api/dragonball` prefix
- GET routes pass through without authentication
- POST/PUT/DELETE routes protected by Kong `openid-connect` plugin

## Dual Auth Model

- **GET routes**: No auth plugin — public read access
- **POST/PUT/DELETE routes**: `openid-connect` plugin in `bearer_only` mode
  - Validates JWT bearer token using Keycloak JWKS endpoint
  - No authorization code redirect (Kong is not the OIDC initiator)
  - Reuses existing Keycloak clients (`dragonball-api`, `music-api`) for plugin config
- Token validation at gateway level is in ADDITION to .NET-level JWT validation (defense in depth)

## OIDC Plugin Configuration

- Bearer-only mode (no redirect to Keycloak)
- OIDC discovery URL: `http://keycloak:8080/realms/OpenCode/.well-known/openid-configuration`
- JWKS URI: fetched from discovery document
- `ssl_verify: false` (development)
- Client ID: `dragonball-api` or `music-api` (matching the upstream API)
- Client secret: from Keycloak client config

## CORS

- Configured via Kong `cors` plugin at route level
- Allowed origins: `http://localhost:5173`, `http://localhost:3000`, `http://localhost`, `http://localhost:5003`
- Allow all methods (GET, POST, PUT, DELETE, OPTIONS)
- Allow all headers
- Expose Correlation ID header

## Correlation ID

- Kong `request-id` plugin generates UUID-based correlation IDs
- Header name: `X-Correlation-Id` (consistent with .NET `UseCorrelationId()` middleware)
- Generated for all requests at entry point
- Passed through to upstream APIs

## Upstream Discovery

- Upstream URLs resolved via Aspire service discovery environment variables
- Kong config references variables injected by AppHost
- Ports dynamically assigned by Aspire

## Plugins Enabled

| Plugin | Scope | Purpose |
|--------|-------|---------|
| `openid-connect` | Per-route (write routes) | JWT token validation |
| `cors` | Per-route | CORS headers |
| `request-id` | Global | Correlation ID generation |
| `prometheus` | Global | Metrics (optional, for observability) |

## Dependencies

- Phase 3 (APIs running with working endpoints)
- Phase 4 (Keycloak running with realm and clients configured)
- etcd container must start before Kong
- Kong must start before routes are configured via Admin API
- Routes must exist before frontend can send requests through gateway

## Files

- `.planning/phases/05-apisix-gateway/config.yaml` — Kong main configuration
- `.planning/phases/05-apisix-gateway/init-routes.sh` — Admin API script to create routes
- `.planning/phases/05-apisix-gateway/etcd-config.sh` — etcd startup (minimal, default config)

## Requirements Coverage (GATE-01 → GATE-07)

| Req | Description | Approach |
|-----|-------------|----------|
| GATE-01 | Kong entry point on port 8000 | Kong container with `WithEndpoint(port: 8000, targetPort: 9080)` |
| GATE-02 | Route `/api/dragonball/*` | Route with upstream to dragonball-api, strip prefix |
| GATE-03 | Route `/api/music/*` | Route with upstream to music-api, strip prefix |
| GATE-04 | Dual auth model | OIDC plugin on write routes, no plugin on GET routes |
| GATE-05 | OIDC JWKS validation | openid-connect plugin in bearer_only mode | 
| GATE-06 | CORS at Kong level | cors plugin on all routes |
| GATE-07 | Correlation ID | request-id plugin globally |
