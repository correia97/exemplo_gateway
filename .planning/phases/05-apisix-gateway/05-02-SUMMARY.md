# 05-02: Kong Route Initialization — Summary

## What Was Done

1. **Route init script** at `deploy/kong/init-routes.sh`:
   - Creates upstream `dragonball-api` pointing to `opencode-dragonball-api:8080`
   - Creates upstream `music-api` pointing to `opencode-music-api:8080`
   - Creates upstream `keycloak` pointing to `keycloak:8080`

2. **Routes defined** via Admin API calls:
   - `GET /api/dragonball/*` → upstream `dragonball-api`, methods: GET
   - `POST/PUT/DELETE /api/dragonball/*` → upstream `dragonball-api`, methods: POST, PUT, DELETE
   - `GET /api/music/*` → upstream `music-api`, methods: GET
   - `POST/PUT/DELETE /api/music/*` → upstream `music-api`, methods: POST, PUT, DELETE
   - `GET /api/auth/*` → upstream `keycloak` (public endpoints)
   - Root `/` redirect to frontend

3. **Route matching**:
   - Host matching: routes match any host (development)
   - Priority: `0` (default)
   - Strip prefix: `/api` prefix stripped before proxying

## Verification

- All routes created and listed via Admin API `GET /apisix/admin/routes`
- Requests to Kong port 9080 route to correct upstream
- Path rewriting works correctly (`/api/dragonball/characters` → `/characters`)

## Key Findings

- Route ordering matters: more specific routes should be created first
- `strip_path: true` removes the matched prefix from the upstream request
- Upstream health checks verify backend availability before proxying
