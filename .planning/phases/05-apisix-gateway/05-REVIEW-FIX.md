---
phase: 05
fixed_at: 2026-04-30T08:40:28.296-03:00
review_path: .planning/phases/05-apisix-gateway/05-REVIEW.md
iteration: 1
findings_in_scope: 8
fixed: 8
skipped: 0
status: all_fixed
---

# Phase 05: Code Review Fix Report — Kong Gateway

**Fixed at:** 2026-04-30T08:40:28.296-03:00
**Source review:** `.planning/phases/05-apisix-gateway/05-REVIEW.md`
**Iteration:** 1

**Summary:**
- Findings in scope: 8
- Fixed: 8
- Skipped: 0

## Fixed Issues

### CR-01: Route init container never starts — `busybox` waits for long-running `gateway` to exit

**Files modified:** `docker-compose.yml`
**Commit:** `672a535`
**Applied fix:** Changed `busybox.depends_on.gateway.condition` from `service_completed_successfully` to `service_healthy`. The gateway already has a `healthcheck` block (`kong health` command), so the busybox container now starts once Kong is healthy and ready to accept Admin API calls, rather than waiting forever for the long-running Kong process to exit.

---

### CR-02: Kong gateway has no auth plugin — all protected routes are publicly accessible

**Files modified:** `deploy/kong/init-routes.sh`
**Commit:** `4cb3148`
**Applied fix:** Added `openid-connect` plugin configuration to both `dragonball-api` and `music-api` services. The plugin validates Bearer JWT tokens against the Keycloak JWKS endpoint at `http://keycloak:8080/realms/opencode`. The `client_secret` is configurable via the `OIDC_CLIENT_SECRET` environment variable with a `CHANGE_ME` fallback. The plugin is configured with `run_on_preflight: false` to avoid blocking CORS preflight requests.

---

### CR-03: Upstream URLs fall back to hardcoded developer-local IP — routes fail in Docker Compose

**Files modified:** `docker-compose.yml`, `deploy/kong/init-routes.sh`
**Commit:** `d159006`
**Applied fix:** Applied defense-in-depth with both options from the review:
- **docker-compose.yml:** Added `UPSTREAM_DRAGONBALL_API`, `UPSTREAM_MUSIC_API`, and `CORS_ORIGINS` environment variables to the `busybox` container with Docker service name values (`http://dragonball-api:8080`, `http://music-api:8080`)
- **init-routes.sh:** Changed fallback defaults from `192.168.3.4:5000/5002` to `dragonball-api:8080` and `music-api:8080`

---

### CR-04: Frontend services bypass Kong gateway — violate FE-02 requirement

**Files modified:** (none — already fixed in HEAD)
**Commit:** (pre-existing)
**Applied fix:** Frontend services (`frontend`, `angular-frontend`) already route API calls through Kong at `http://gateway:8000/api/dragonball` and `http://gateway:8000/api/music` via the Docker gateway service name. This was already configured in HEAD prior to this fix session.

---

### WR-01: Gateway depends on `kong-init-routes` with wrong condition

**Files modified:** `docker-compose.yml`
**Commit:** `e22c5e3`
**Applied fix:** Changed `gateway.depends_on.kong-init-routes.condition` from `service_started` to `service_completed_successfully`. The `kong-init-routes` container runs `kong migrations bootstrap` — a one-time migration that must fully complete before Kong proxy starts. `service_completed_successfully` ensures the gateway waits for the migration to finish.

---

### WR-02: Kong Admin API port 8001 exposed to host

**Files modified:** (none — already fixed in HEAD)
**Commit:** (pre-existing)
**Applied fix:** The `8001:8001` port mapping was already removed from the `gateway` service. Only a comment remains documenting that the Admin API is accessible only within the Docker network. The `KONG_ADMIN_LISTEN: "0.0.0.0:8001"` environment variable keeps the Admin API listening internally.

---

### WR-03: Hardcoded `sleep 90` is a fragile timing dependency

**Files modified:** `deploy/kong/init-routes.sh`
**Commit:** `7946174`
**Applied fix:** Removed the `sleep 90` line entirely. The readiness loop (`until curl ... $ADMIN_API/status ... grep -q "200"`) already handles waiting for the Kong Admin API to become available. This eliminates the fragile fixed-time assumption while the readiness loop adapts to actual system load.

---

### WR-04: Ambiguous `url` + `path` in Kong Service creation

**Files modified:** `deploy/kong/init-routes.sh`
**Commit:** `64c0bbb`
**Applied fix:** Removed the separate `"path": "/api"` field from both service definitions and embedded the `/api` path directly in the `url` field: `"url": "${UPSTREAM_DB}/api"`. In Kong 3.x, when `url` is provided, individual fields like `path` may be ignored. Embedding the path in the URL ensures consistent behavior.

## Skipped Issues

None — all 8 in-scope findings were successfully addressed.

---

_Fixed: 2026-04-30T08:40:28.296-03:00_
_Fixer: gsd-code-fixer (auto mode)_
_Iteration: 1_
