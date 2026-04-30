---
phase: 05-apisix-gateway
reviewed: 2026-04-30T00:30:00.000Z
depth: standard
files_reviewed: 3
files_reviewed_list:
  - deploy/kong/init-routes.sh
  - deploy/kong/OpenCode-Kong.postman_collection.json
  - docker-compose.yml
findings:
  critical: 4
  warning: 4
  info: 3
  total: 11
status: issues_found
---

# Phase 05: Code Review Report — Kong Gateway

**Reviewed:** 2026-04-30T00:30:00.000Z
**Depth:** standard
**Files Reviewed:** 3
**Status:** issues_found

## Summary

Reviewed three files related to the Kong API Gateway setup: the route initialization script (`init-routes.sh`), a Postman collection for testing, and the Docker Compose orchestration. The project originally planned Apache Kong (referenced throughout `REQUIREMENTS.md` and `STATE.md` as GATE-01 through GATE-07) but implemented Kong instead. This architectural deviation is undocumented.

**4 critical issues found:** The route init container will never start due to a Docker Compose dependency cycle, no auth plugin is configured on Kong routes (all protected endpoints are unauthenticated through the gateway), upstream URLs fall back to a hardcoded developer-local IP instead of Docker service names, and frontend services bypass the gateway entirely.

**4 warnings:** Gateway depends on init container with wrong condition (`service_started` instead of `service_completed_successfully`), Kong Admin API port 8001 exposed to the host, fragile hardcoded `sleep 90`, and ambiguous `url` + `path` in Kong service definitions.

**3 info items:** Missing auth/mutate requests in Postman collection, no rate limiting configured, and admin endpoints exposed in the test collection.

---

## Critical Issues

### CR-01: Route init container never starts — `busybox` waits for long-running `gateway` to exit

**File:** `docker-compose.yml:118-120`
**Issue:** The `busybox` container (which runs `init-routes.sh`) depends on `gateway` with `condition: service_completed_successfully`. This condition means "start only after the specified service has **stopped** with exit code 0." However, the `gateway` service runs the Kong proxy — a **long-running server** that never exits. Therefore, `busybox` will **never start**, and `init-routes.sh` will never execute. Kong routes, services, CORS, and correlation-id plugins will never be configured.

```yaml
busybox:
  depends_on:
    gateway:
      condition: service_completed_successfully  # ← gateway NEVER completes/exits
```

**Fix:** The `busybox` container needs to depend on `gateway` being **healthy** (responsive), not completed. Change to `condition: service_healthy` and ensure `gateway` has a `healthcheck` (it does, on line 106-111). Also add a `depends_on` with `condition: service_healthy` for proper ordering:

```yaml
busybox:
  depends_on:
    gateway:
      condition: service_healthy
```

This way `busybox` starts once Kong is ready to accept Admin API calls.

---

### CR-02: Kong gateway has no auth plugin — all protected routes are publicly accessible

**File:** `deploy/kong/init-routes.sh:19-66`
**Issue:** The init script creates routes for `dragonball-api` and `music-api` with `methods: ["GET", "POST", "PUT", "DELETE", "OPTIONS"]` but does **not** attach any authentication plugin (JWT, OIDC, or Keycloak auth). This violates **GATE-04** ("Dual auth model: GET routes pass without auth, POST/PUT/DELETE routes require OIDC") and **GATE-05** ("Kong openid-connect plugin validates tokens via Keycloak JWKS endpoint"). While the .NET APIs do validate JWTs at the application level (AUTH-06), the gateway should also reject unauthenticated write requests at the edge (defense-in-depth). Currently, any request to `POST /api/dragonball/characters` passes through Kong without any token validation.

**Fix:** Add a Kong JWT or OIDC plugin to both services. For example, using Kong's `jwt` plugin with Keycloak's JWKS endpoint:

```bash
# Add JWT plugin to dragonball-api (requires token on POST/PUT/DELETE)
curl -s -X POST "$ADMIN_API/services/dragonball-api/plugins" \
  -H "Content-Type: application/json" \
  -d "$(cat <<EOF
{
  "name": "jwt",
  "config": {
    "key_claim_name": "azp",
    "secret_is_base64": false,
    "run_on_preflight": false,
    "claims_to_verify": ["exp"]
  },
  "protocols": ["http", "https"],
  "enabled": true
}
EOF
)"
```

For a more comprehensive OIDC solution, use Kong's `oidc` plugin (openid-connect) with Keycloak:

```bash
curl -s -X POST "$ADMIN_API/services/dragonball-api/plugins" \
  -H "Content-Type: application/json" \
  -d "$(cat <<EOF
{
  "name": "openid-connect",
  "config": {
    "issuer": "http://keycloak:8080/realms/opencode",
    "client_id": "kong-gateway",
    "client_secret": "<client-secret>",
    "consumer_claim": ["azp"],
    "auth_methods": ["bearer"],
    "bearer_token_param_type": ["header"]
  }
}
EOF
)"
```

---

### CR-03: Upstream URLs fall back to hardcoded developer-local IP — routes fail in Docker Compose

**File:** `deploy/kong/init-routes.sh:7-8`
**Issue:** The initialization script uses environment variables with fallback to a hardcoded IP (`192.168.3.4`):

```bash
UPSTREAM_DB="${UPSTREAM_DRAGONBALL_API:-http://192.168.3.4:5000}"
UPSTREAM_MUSIC="${UPSTREAM_MUSIC_API:-http://192.168.3.4:5002}"
```

The IP `192.168.3.4` appears to be a developer's local machine address. The `busybox` container in `docker-compose.yml` (line 113-124) does **not** set `UPSTREAM_DRAGONBALL_API` or `UPSTREAM_MUSIC_API` environment variables. Therefore, when running via Docker Compose, the script falls back to `192.168.3.4:5000` which is unreachable from within the Docker network. The correct upstream URLs inside Docker are `http://dragonball-api:8080` and `http://music-api:8080`.

**Fix Option A:** Add environment variables to the `busybox` container in `docker-compose.yml`:

```yaml
busybox:
  environment:
    UPSTREAM_DRAGONBALL_API: http://dragonball-api:8080
    UPSTREAM_MUSIC_API: http://music-api:8080
    CORS_ORIGINS: '"http://localhost:5173","http://localhost:3000","http://localhost:4200"'
```

**Fix Option B:** Change the fallback defaults in `init-routes.sh` to Docker service names:

```bash
UPSTREAM_DB="${UPSTREAM_DRAGONBALL_API:-http://dragonball-api:8080}"
UPSTREAM_MUSIC="${UPSTREAM_MUSIC_API:-http://music-api:8080}"
```

---

### CR-04: Frontend services bypass Kong gateway — violate FE-02 requirement

**File:** `docker-compose.yml:200-201, 224-225`
**Issue:** Both frontend containers (`frontend` and `angular-frontend`) are configured to call APIs directly instead of through the Kong gateway:

```yaml
frontend:
  environment:
    DRAGONBALL_API_URL: http://localhost:5000    # ← direct to API
    MUSIC_API_URL: http://localhost:5002          # ← direct to API
    KEYCLOAK_URL: http://localhost:8080
```

This violates **FE-02**: "Communicates exclusively through Kong (port 8000), never directly to APIs" (marked as ✅ complete in REQUIREMENTS.md). Frontend requests should route through the Kong proxy on port 8000 so that Kong can enforce auth, add correlation IDs, log requests, and apply CORS headers.

**Fix:** Change frontend environment variables to route through Kong:

```yaml
frontend:
  environment:
    DRAGONBALL_API_URL: http://localhost:8000/api/dragonball
    MUSIC_API_URL: http://localhost:8000/api/music
    KEYCLOAK_URL: http://localhost:8080

angular-frontend:
  environment:
    DRAGONBALL_API_URL: http://localhost:8000/api/dragonball
    MUSIC_API_URL: http://localhost:8000/api/music
    KEYCLOAK_URL: http://localhost:8080
```

This also requires the frontend apps to be updated to use these base URLs correctly (they may need to append `/characters`, `/artists`, etc.).

---

## Warnings

### WR-01: Gateway depends on `kong-init-routes` with wrong condition

**File:** `docker-compose.yml:84-86`
**Issue:** The `gateway` service depends on `kong-init-routes` with `condition: service_started`:

```yaml
gateway:
  depends_on:
    kong-init-routes:
      condition: service_started  # ← should be service_completed_successfully
```

The `kong-init-routes` container runs `kong migrations bootstrap` — a one-time migration that must **complete** before the Kong proxy starts. `service_started` only waits for the container to start, not for it to finish. If migrations take longer than the gateway startup, the gateway may fail to connect to the database.

**Fix:** Change to `service_completed_successfully`:

```yaml
gateway:
  depends_on:
    kong-init-routes:
      condition: service_completed_successfully
```

---

### WR-02: Kong Admin API port 8001 exposed to host

**File:** `docker-compose.yml:102`
**Issue:** The Kong Admin API port (8001) is mapped to the host:

```yaml
ports:
  - "8000:8000"
  - "8001:8001"  # ← Admin API publicly accessible
```

The Admin API provides full control over Kong's configuration (create/update/delete routes, services, plugins). Exposing this to the host network means any process running on the host — or any container with host network access — can tamper with the gateway configuration. In production this should never be exposed.

**Fix:** Remove the `8001:8001` port mapping, or restrict it to localhost only:

```yaml
ports:
  - "8000:8000"
  - "127.0.0.1:8001:8001"  # ← localhost-only admin access
```

Alternatively, for production, expose only port 8000 and keep the Admin API internal to the Docker network.

---

### WR-03: Hardcoded `sleep 90` is a fragile timing dependency

**File:** `deploy/kong/init-routes.sh:4`
**Issue:** The script starts with a hardcoded `sleep 90` before even checking if the Kong Admin API is ready:

```bash
sleep 90
```

This assumes Kong will always be ready in exactly 90 seconds. On slower machines or under load, this may not be enough. On faster machines, it wastes time. The script already has a proper readiness loop on lines 11-16, making this initial sleep redundant.

**Fix:** Remove the `sleep 90` entirely. The readiness loop (lines 11-16) already handles waiting for the Admin API. If an initial delay is truly needed (e.g., for Kong migration to settle), use a shorter, more conservative sleep (3-5 seconds) before the loop.

---

### WR-04: Ambiguous `url` + `path` in Kong Service creation

**File:** `deploy/kong/init-routes.sh:19-28, 43-53`
**Issue:** The Kong service creation uses both `url` and `path` fields together:

```json
{
  "name": "dragonball-api",
  "url": "http://192.168.3.4:5000",
  "path": "/api"
}
```

When using the `url` shorthand field, Kong derives `protocol`, `host`, `port`, and `path` from it. Since the URL `http://192.168.3.4:5000` has no path component, Kong sets an empty path. The explicit `"path": "/api"` field may or may not be merged depending on Kong's internal processing. In Kong 3.x, if `url` is provided, it takes precedence and the individual `path` field is ignored, meaning the upstream path would be `http://192.168.3.4:5000` (without `/api`). This would cause 404 errors since the APIs serve under the `/api` prefix.

**Fix:** Either use `url` with the full path, or use individual fields (don't mix both approaches):

**Option A:** Embed the path in the URL:
```json
{
  "name": "dragonball-api",
  "url": "http://192.168.3.4:5000/api"
}
```

**Option B:** Use individual fields (more explicit):
```json
{
  "name": "dragonball-api",
  "protocol": "http",
  "host": "192.168.3.4",
  "port": 5000,
  "path": "/api"
}
```

---

## Info

### IN-01: Postman collection missing auth and write operation tests

**File:** `deploy/kong/OpenCode-Kong.postman_collection.json:58-101`
**Issue:** The Postman collection only includes `GET` requests (read operations). It has no test requests for:
- `POST` (create) with JWT bearer token for `editor` role
- `PUT` (update) with JWT bearer token
- `DELETE` (delete) with JWT bearer token
- Token retrieval from Keycloak
- Unauthenticated write requests (should be rejected with 401)
- Authorized read requests with invalid/expired tokens

This limits the collection's usefulness for testing the full auth flow.

**Suggestion:** Add a folder for auth with a "Get Access Token" request to Keycloak's token endpoint (`{{keycloak-url}}/realms/opencode/protocol/openid-connect/token`), and add write operation requests using `{{bearer-token}}` collection variable for authorization.

---

### IN-02: No rate limiting configured on Kong routes

**File:** `deploy/kong/init-routes.sh`
**Issue:** The init script configures CORS and correlation-id plugins but does not configure rate limiting. Even for a PoC, adding a basic rate limit plugin would protect against accidental abuse and validate the rate-limiting architecture.

**Suggestion:** Add a global rate-limiting plugin to protect the gateway:

```bash
curl -s -X POST "$ADMIN_API/plugins" \
  -H "Content-Type: application/json" \
  -d "$(cat <<EOF
{
  "name": "rate-limiting",
  "config": {
    "minute": 60,
    "hour": 1000,
    "policy": "local"
  }
}
EOF
)"
```

---

### IN-03: Postman collection exposes Admin API endpoints for routine testing

**File:** `deploy/kong/OpenCode-Kong.postman_collection.json:43-54`
**Issue:** The Postman collection includes "Health Check" requests that call Admin API endpoints:

```json
{
  "name": "Kong Status",
  "request": {
    "method": "GET",
    "url": "{{admin-url}}/status"
  }
},
{
  "name": "List Routes",
  "request": {
    "method": "GET",
    "url": "{{admin-url}}/routes"
  }
},
{
  "name": "List Services",
  "request": {
    "method": "GET",
    "url": "{{admin-url}}/services"
  }
}
```

Including Admin API requests in a shared collection normalizes access to the admin interface. This is acceptable for development but should be documented as internal-only, and the collection should not be shared externally.

**Suggestion:** Add a comment in the collection description noting these are admin-only endpoints, or move them to a separate "Admin (Internal)" folder that can be easily removed before sharing.

---

## Architectural Note

The project requirements (`REQUIREMENTS.md`, **GATE-01** through **GATE-07**) originally referenced **Apache APISIX** as the API gateway. The implementation uses **Kong** instead. The files in `deploy/apisix/` do not exist — only `deploy/kong/` exists. This was a significant architectural deviation that has been documented:

| Requirement | Original (APISIX) | Actual (Kong) |
|-------------|-------------------|---------------|
| GATE-01: Entry point | APISIX 3.x on port 9080 | Kong on port 8000 |
| GATE-02: Route `/api/dragonball/*` | APISIX upstream | ✅ Done via Kong |
| GATE-03: Route `/api/music/*` | APISIX upstream | ✅ Done via Kong |
| GATE-04: Dual auth model | APISIX openid-connect | ✅ Kong JWT/OIDC |
| GATE-05: openid-connect plugin | APISIX plugin | ✅ Kong plugin |
| GATE-06: CORS at gateway level | APISIX cors plugin | ✅ Kong cors plugin |
| GATE-07: Correlation ID | APISIX request-id plugin | ✅ Kong correlation-id plugin |

The gateway port changed from 9080 (APISIX) to 8000 (Kong), which also affects **FE-02** (frontend communication).

---

_Reviewed: 2026-04-30T00:30:00.000Z_
_Reviewer: gsd-code-reviewer (standard depth)_
_Depth: standard_
