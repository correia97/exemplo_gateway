# Phase 5: APISIX Gateway — Research

**Researched:** 2026-04-25
**Domain:** Apache APISIX 3.9.x / etcd / Admin API / OIDC / CORS / request-id
**Confidence:** HIGH

## Summary

Phase 5 introduces Apache APISIX 3.9.1 as the single entry point for both APIs, replacing direct access to the .NET API projects. APISIX handles routing, authentication (via OIDC plugin with Keycloak), CORS, and correlation ID generation. Configuration is managed through APISIX's Admin API, which stores route/plugin definitions in etcd.

## Architecture

```
Client (browser/curl)
  │
  ▼ port 8000
┌─────────────────────┐
│   APISIX 3.9.1      │  ← container (AddContainer in AppHost)
│                     │
│  Plugins:           │
│  - request-id       │  ← global (all requests get correlation ID)
│  - cors             │  ← per-route (both APIs, all methods)
│  - openid-connect   │  ← per-route (POST/PUT/DELETE only)
│                     │
│  Routes:            │
│  /api/dragonball/*  │──→ Dragon Ball API (GET: public, writes: OIDC)
│  /api/music/*       │──→ Music API (GET: public, writes: OIDC)
└────────┬────────────┘
         │
         ▼
┌─────────────────────┐
│    etcd v3.5        │  ← config storage for APISIX
└─────────────────────┘
```

## APISIX Container Configuration

### Image & Version
- Image: `apache/apisix:3.9.1-alpine`
- Default proxy port: 9080 (maps to external port 8000)
- Admin API port: 9180 (internal, for init script)
- Config file location: `/usr/local/apisix/conf/config.yaml`

### config.yaml Structure

```yaml
apisix:
  node_listen: 9080
  enable_admin: true

deployment:
  admin:
    admin_key:
      - name: admin
        key: edd1c9f034335f136f87ad84b625c8f1
        role: admin
    allow_admin:
      - 0.0.0.0/0
    admin_listen:
      ip: 0.0.0.0
      port: 9180
  etcd:
    host:
      - http://etcd:2379

# Plugins to enable (whitelist)
apisix:
  plugins:
    - openid-connect
    - cors
    - request-id
    - prometheus
```

**Note:** In APISIX 3.x, plugins must be whitelisted in `config.yaml` under `apisix.plugins` (a list). Without whitelisting, the Admin API will reject plugin configuration with "invalid plugin name" errors.

## etcd Container

- Image: `quay.io/coreos/etcd:v3.5`
- Port: 2379 (client), 2380 (peer)
- No Bitnami images per project rules
- Must start before APISIX

Environment:
```
ALLOW_NONE_AUTHENTICATION=yes
ETCD_LISTEN_CLIENT_URLS=http://0.0.0.0:2379
ETCD_ADVERTISE_CLIENT_URLS=http://etcd:2379
```

## Admin API Route Creation

### Base URL
```
http://localhost:9180/apisix/admin/
```

### Auth Header
```
X-API-KEY: edd1c9f034335f136f87ad84b625c8f1
```

### Key Endpoints
| Method | Endpoint | Purpose |
|--------|----------|---------|
| PUT | `/routes/{id}` | Create/update route |
| PUT | `/upstreams/{id}` | Create/update upstream |
| PUT | `/global_rules/{id}` | Create/update global rule |

### Route Object Structure

```json
{
    "uri": "/api/dragonball/*",
    "methods": ["GET"],
    "strip_path": true,
    "plugins": {
        "cors": {
            "allow_origins": "http://localhost:5173,http://localhost:3000",
            "allow_methods": "GET,POST,PUT,DELETE,OPTIONS",
            "allow_headers": "*",
            "expose_headers": "X-Correlation-Id"
        }
    },
    "upstream_id": "dragonball-upstream"
}
```

**Key properties:**
- `uri` — path pattern with wildcard `*` for prefix matching
- `methods` — HTTP methods this route matches (empty = all methods, which is the default)
- `strip_path` — when `true`, removes the matched prefix before forwarding to upstream
  - Requests to `/api/dragonball/characters` → forwarded as `/characters`
- `plugins` — plugin configurations keyed by plugin name
- `upstream_id` — reference to an upstream definition
- `id` — route ID (used in the PUT URL path)

### Upstream Object Structure

```json
{
    "id": "dragonball-upstream",
    "type": "roundrobin",
    "nodes": {
        "127.0.0.1:5000": 1
    }
}
```

**Note:** For Aspire development, nodes should reference `host.docker.internal:{port}` since API projects run as host processes, not containers.

## openid-connect Plugin — Bearer-only Mode

### Configuration Attributes

| Name | Required | Description |
|------|----------|-------------|
| `client_id` | yes | Keycloak client ID (dragonball-api or music-api) |
| `client_secret` | yes | Keycloak client secret |
| `discovery` | yes | OIDC discovery URL |
| `bearer_only` | no (default false) | Set to `true` to skip redirect and validate Bearer token only |
| `ssl_verify` | no (default true) | Set to `false` for development |
| `realm` | no (default "apisix") | Realm in WWW-Authenticate header |

### Configuration Example

```json
{
    "openid-connect": {
        "client_id": "dragonball-api",
        "client_secret": "dragonball-secret",
        "discovery": "http://keycloak:8080/realms/opencode/.well-known/openid-configuration",
        "bearer_only": true,
        "ssl_verify": false,
        "realm": "opencode"
    }
}
```

### Bearer-only Flow
1. Client sends request with `Authorization: Bearer <jwt>` header
2. APISIX fetches JWKS from Keycloak discovery endpoint
3. APISIX validates JWT signature and claims
4. If valid → request passed to upstream
5. If invalid/missing → 401 Unauthorized

### Important: Client Secret Reference
Client secrets (`dragonball-secret`, `music-secret`) are defined in the Keycloak realm JSON from Phase 4. For security, they should be passed as environment variables to the APISIX container rather than hardcoded in the init script. APISIX supports `${ENV://VAR}` syntax for referencing environment variables in plugin configs.

Actually, APISIX 3.9 does NOT support `${ENV://VAR}` in plugin config directly for the openid-connect plugin. Instead, we use environment variables set on the container and reference them via `${{VAR}}` in the config.yaml admin_key, or we put the secrets in the init script directly since the init script is internal.

For the PoC, the simplest approach is: set `CLIENT_SECRET_DRAGONBALL` and `CLIENT_SECRET_MUSIC` as environment variables on the APISIX container, and use `${{ENV.CLIENT_SECRET_DRAGONBALL}}` in the init script. Actually, the Admin API request body is JSON, and APISIX does support a secret management system, but for simplicity in the init script, we construct the JSON using environment variable expansion in bash, e.g.:
```bash
CLIENT_SECRET_DRAGONBALL=${CLIENT_SECRET_DRAGONBALL:-dragonball-secret}
```

This lets the AppHost override the secrets via environment variables while providing sensible defaults.

## cors Plugin — Configuration

### Configuration Attributes

| Name | Required | Default | Description |
|------|----------|---------|-------------|
| `allow_origins` | no | `*` | Comma-separated allowed origins |
| `allow_methods` | no | `*` | Comma-separated allowed methods |
| `allow_headers` | no | `*` | Comma-separated allowed headers |
| `expose_headers` | no | — | Comma-separated exposed headers |
| `max_age` | no | 5 | Cache preflight in seconds |
| `allow_credential` | no | false | Allow credentials (cookies, auth headers) |

### Configuration Example

```json
{
    "cors": {
        "allow_origins": "http://localhost:5173,http://localhost:3000,http://localhost,http://localhost:5003",
        "allow_methods": "GET,POST,PUT,DELETE,OPTIONS",
        "allow_headers": "*",
        "expose_headers": "X-Correlation-Id",
        "allow_credential": true,
        "max_age": 86400
    }
}
```

**CORS Preflight Handling:** The `cors` plugin automatically handles OPTIONS preflight requests. When the plugin is enabled and an OPTIONS request comes in, APISIX returns CORS headers without forwarding to the upstream. This means the .NET APIs do NOT need their own CORS middleware (GATE-06 requires CORS at APISIX level only).

**Important:** When `allow_credential` is `true`, CORS spec requires that `allow_origins` cannot be `*`. Each origin must be explicitly listed.

## request-id Plugin — Correlation ID

### Configuration Attributes

| Name | Required | Default | Description |
|------|----------|---------|-------------|
| `header_name` | no | `X-Request-Id` | Header name for the correlation ID |
| `include_in_response` | no | true | Include in response headers |
| `algorithm` | no | `uuid` | ID generation algorithm |

### Configuration Example (as Global Rule)

```json
{
    "request-id": {
        "header_name": "X-Correlation-Id",
        "include_in_response": true,
        "algorithm": "uuid"
    }
}
```

**Behavior:**
- Every request gets a UUID-based correlation ID
- The ID is set in the request header forwarded to upstream, so .NET APIs receive `X-Correlation-Id`
- The ID is included in the response header for client-side debugging
- If the incoming request already has `X-Correlation-Id`, APISIX uses that value (does not overwrite)
- This should be configured as a **global rule** (not per-route) per CONTEXT.md

### Global Rules vs Per-Route

| Aspect | Global Rule | Per-Route |
|--------|-------------|-----------|
| Scope | All requests matching any route | Only requests matching specific route |
| Verb | `PUT /global_rules/{id}` | `PUT /routes/{id}` |
| Use case | request-id (every request needs ID) | cors, openid-connect (API-specific) |

**Decision:** `request-id` → global rule. `cors` and `openid-connect` → per-route.

## Upstream Resolution in Aspire

### The Challenge

During Aspire development:
- API projects run as host processes (not containers unless using docker.json config)
- APISIX runs as a container
- From container → host: use `host.docker.internal` (Docker Desktop) or gateway IP

### Solution: Well-Known Ports + host.docker.internal

The AppHost assigns well-known ports to API projects and passes upstream URLs as environment variables to APISIX:

```csharp
var dragonballApi = builder.AddProject<Projects.OpenCode_DragonBall_Api>("dragonball-api")
    .WithEndpoint("http", e => e.Port = 5000)
    .WithEndpoint("https", e => e.Port = 5001);

var apisix = builder.AddContainer("apisix", "apache/apisix:3.9.1-alpine")
    .WithEnvironment("UPSTREAM_DRAGONBALL_API", "http://host.docker.internal:5000")
    .WithEnvironment("UPSTREAM_MUSIC_API", "http://host.docker.internal:5001")
    .WithEnvironment("KEYCLOAK_URL", "http://host.docker.internal:8080");
```

The init-routes.sh script reads these env vars to construct the Admin API calls.

### Alternative: Docker Network

If using Aspire's Docker execution mode (all resources as containers), the projects would also be containers and would be reachable by their service name (`dragonball-api`, `music-api`). This is future-proof for Phase 8 Docker Compose mode.

For now, the init script uses environment variables and falls back to sensible defaults:

```bash
UPSTREAM_DRAGONBALL_API=${UPSTREAM_DRAGONBALL_API:-http://host.docker.internal:5000}
```

## Init Script Dependencies on Keycloak

The APISIX OIDC plugin requires Keycloak to be reachable at the OIDC discovery URL. The init script should:
1. Wait for APISIX Admin API to be ready (port 9180 responds)
2. Create upstreams and routes (can be done before Keycloak is ready)
3. OIDC plugin config references the Keycloak URL — validation happens at request time, not config time

So the init script can fully configure routes before Keycloak is available. OIDC validation only kicks in when a POST/PUT/DELETE request arrives.

## Reference: Admin API Init Script Pattern

```bash
#!/bin/bash
set -e

ADMIN_API="http://127.0.0.1:9180/apisix/admin"
ADMIN_KEY="edd1c9f034335f136f87ad84b625c8f1"

# Wait for Admin API
until curl -s -o /dev/null -w "%{http_code}" "$ADMIN_API/routes" -H "X-API-KEY: $ADMIN_KEY"; do
    echo "Waiting for Admin API..."
    sleep 1
done

# Create upstreams
curl -s -X PUT "$ADMIN_API/upstreams/dragonball" \
    -H "X-API-KEY: $ADMIN_KEY" \
    -H "Content-Type: application/json" \
    -d '{"type":"roundrobin","nodes":{"'"$UPSTREAM_DRAGONBALL_API"'":1}}'

# Create routes
curl -s -X PUT "$ADMIN_API/routes/dragonball-get" \
    -H "X-API-KEY: $ADMIN_KEY" \
    -H "Content-Type: application/json" \
    -d '{
        "uri": "/api/dragonball/*",
        "methods": ["GET"],
        "strip_path": true,
        "upstream_id": "dragonball",
        "plugins": {
            "cors": {
                "allow_origins": "'"$CORS_ORIGINS"'",
                "allow_methods": "GET,POST,PUT,DELETE,OPTIONS",
                "allow_headers": "*",
                "expose_headers": "X-Correlation-Id"
            }
        }
    }'
```

## Anti-Patterns to Avoid

1. **Hardcoding secrets in init-routes.sh** — Use environment variables with defaults
2. **Missing plugin whitelist in config.yaml** — APISIX rejects unknown plugins with 400
3. **CORS in both APISIX AND .NET APIs** — Double CORS headers cause browser issues. Remove .NET CORS middleware when APISIX handles it.
4. **Setting `allow_origins: "*"` with `allow_credential: true`** — Browsers reject this per CORS spec
5. **Incorrect strip_path behavior** — `strip_path: true` removes the entire matched prefix. `/api/dragonball/characters` becomes just `/characters` to upstream
6. **Forgetting etcd must start first** — APISIX crashes on startup if etcd is unreachable
7. **Using `host.docker.internal` on Linux without `--add-host`** — Need `docker run --add-host host.docker.internal:host-gateway` on Linux

## Assumptions Log

| # | Claim | Risk if Wrong |
|---|-------|---------------|
| A1 | APISIX 3.9.1-alpine config format matches 3.x docs | Low — format is stable across 3.x |
| A2 | `WithEndpoint(port: ...)` on AddProject assigns that port | Medium — Aspire may use a proxy; need to verify |
| A3 | `host.docker.internal` works on developer's machine | Low — standard Docker Desktop behavior on Win/Mac |
| A4 | OIDC plugin validates JWT at request time, not config time | Low — bearer_only mode is well-documented |
| A5 | Plugin whitelist is required in config.yaml for 3.x | High — this is a common pitfall; must verify |

## Sources

### Primary (HIGH confidence)
- [VERIFIED: apisix.apache.org/docs/apisix/admin-api/] — Admin API route, upstream, and global rules endpoints
- [VERIFIED: apisix.apache.org/docs/apisix/plugins/openid-connect/] — OIDC plugin with bearer_only mode
- [VERIFIED: apisix.apache.org/docs/apisix/plugins/cors/] — CORS plugin attributes and behavior
- [VERIFIED: apisix.apache.org/docs/apisix/plugins/request-id/] — request-id plugin with header_name config
- [VERIFIED: quay.io/coreos/etcd] — etcd container image availability

### Secondary (MEDIUM confidence)
- [CITED: apisix.apache.org/docs/apisix/architecture-design/apisix/] — APISIX architecture and plugin lifecycle
- [CITED: learn.microsoft.com/en-us/dotnet/aspire/] — Aspire container orchestration with AddContainer, WithEndpoint, WithEnvironment

## Metadata

**Confidence breakdown:**
- APISIX Admin API: HIGH — well-documented, stable API
- OIDC plugin: HIGH — bearer_only mode documented with examples
- CORS plugin: HIGH — straightforward configuration
- request-id plugin: HIGH — simple global rule
- Aspire integration: MEDIUM — port binding with AddContainer vs AddProject needs verification
- etcd setup: MEDIUM — need to confirm quay.io/coreos/etcd:v3.5 image works as APISIX backend

**Research date:** 2026-04-25
**Valid until:** 2026-05-25
