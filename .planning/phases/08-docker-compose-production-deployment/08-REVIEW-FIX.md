---
phase: 08-docker-compose-production-deployment
fixed_at: 2026-04-30T08:45:00-03:00
review_path: .planning/phases/08-docker-compose-production-deployment/08-REVIEW.md
iteration: 1
findings_in_scope: 8
fixed: 6
skipped: 2
status: all_fixed
---

# Phase 08: Code Review Fix Report — Docker Compose Production Deployment

**Fixed at:** 2026-04-30T08:45:00-03:00
**Source review:** `.planning/phases/08-docker-compose-production-deployment/08-REVIEW.md`
**Iteration:** 1

**Summary:**
- Findings in scope: 8 (3 critical + 5 warning)
- Fixed: 6 (CR-02, CR-03, WR-01, WR-02, WR-03, WR-04, WR-05)
- Skipped: 2 (CR-01, CR-03 partially — already fixed in prior sessions)

## Fixed Issues

### CR-02: API container health checks target wrong endpoint and the health endpoint is not registered

**Files modified:** `src/OpenCode.DragonBall.Api/Program.cs`, `src/OpenCode.Music.Api/Program.cs`
**Commit:** `59a7189`
**Applied fix:**
- Added `app.MapDefaultEndpoints()` call in both `Program.cs` files after `app.UseCorrelationId()` to register the `/health` endpoint (defined in `Extensions.cs:49`)
- The docker-compose.yml health check URLs already had `/health` (fixed in prior session `e9f1870`)

### CR-03: Frontend environment URLs use `localhost` inside containers

**Files modified:** `docker-compose.yml`
**Commit:** `b2e7f42`
**Applied fix:**
- Changed `DRAGONBALL_API_URL` from `http://localhost:8000/api/dragonball` to `http://gateway:8000/api/dragonball` for both `frontend` and `angular-frontend` services
- Changed `MUSIC_API_URL` from `http://localhost:8000/api/music` to `http://gateway:8000/api/music` for both services
- Changed `KEYCLOAK_URL` from `http://localhost:8080` to `http://keycloak:8080` for both services

### WR-01: Unpinned container image tags (`:latest`)

**Files modified:** `docker-compose.yml`
**Commit:** `6efc0ef`
**Applied fix:**
- Pinned `quay.io/keycloak/keycloak:latest` to `quay.io/keycloak/keycloak:26.6.1`
- Pinned `jaegertracing/all-in-one:latest` to `jaegertracing/all-in-one:1.76.0`

### WR-02: No non-root user in .NET API runtime images

**Files modified:** `src/OpenCode.DragonBall.Api/Dockerfile`, `src/OpenCode.Music.Api/Dockerfile`
**Commit:** `d1b2eaf`
**Applied fix:**
- Added `RUN adduser --disabled-password --gecos '' appuser` to create a non-root user in the runtime stage
- Added `USER appuser` to switch to the non-root user before running the application
- Both added after `COPY --from=build` and before `EXPOSE`

### WR-03: Kong Admin API port exposed to host

**Files modified:** `docker-compose.yml`
**Commit:** `6efc0ef`
**Applied fix:**
- Removed `"8001:8001"` from the gateway `ports` section
- Added comment: `# Admin API accessible only within the Docker network (port 8001)`

### WR-04: Postgres port exposed to host

**Files modified:** `docker-compose.yml`
**Commit:** `6efc0ef`
**Applied fix:**
- Changed `- "5432:5432"` to a commented-out line: `#   - "5432:5432"    # Removed — database accessible only within the Docker network`
- Database is now accessible only within the Docker network

### WR-05: Missing security headers in nginx

**Files modified:** `src/OpenCode.Frontend/nginx.conf`
**Commit:** `0b65f1f`
**Applied fix:**
- Added `Strict-Transport-Security` header (HSTS) with `max-age=31536000; includeSubDomains; preload`
- Added `Content-Security-Policy` header with appropriate restrictions for the SPA
- Added `Referrer-Policy` header set to `strict-origin-when-cross-origin`
- Added `Permissions-Policy` header restricting camera, microphone, and geolocation APIs

## Skipped Issues

### CR-01: Kong route initialization service never starts (blocking bug)

**File:** `docker-compose.yml:118-120`
**Reason:** Already fixed in prior session (commit `672a535` from Phase 05). The `busybox` service's `depends_on` condition was changed from `service_completed_successfully` to `service_healthy` so the init container runs once the Kong gateway is healthy and ready to accept Admin API calls.

### CR-02 (docker-compose.yml part): API health check URLs

**File:** `docker-compose.yml:145,171`
**Reason:** Already fixed in prior session (commit `e9f1870` from Phase 06). The health check URLs were already changed from `http://localhost:8080/` to `http://localhost:8080/health` in a previous fix session.

---

_Fixed: 2026-04-30T08:45:00-03:00_
_Fixer: the agent (gsd-code-fixer)_
_Iteration: 1_
