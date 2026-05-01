---
phase: 14-api-developer-portal-with-backstage-io-developers-register-v
plan: 02
subsystem: infra
tags: [backstage, oidc, keycloak, docker, postgresql]

requires:
  - phase: 14-01
    provides: Scaffolded Backstage app, Keycloak backstage-portal client, Portal PostgreSQL schema
provides:
  - Keycloak OIDC authentication provider for Backstage (auth.providers.oidc)
  - PostgreSQL database configuration (pg client, schema division mode, env var substitution)
  - OIDC auth provider module installed and registered in backend
  - Keycloak-branded sign-in page with OAuth2 OIDC flow
  - Production Dockerfile following Backstage official host-build pattern
  - .dockerignore with comprehensive exclusion patterns
affects: [14-03, 14-04]

tech-stack:
  added:
    - @backstage/plugin-auth-backend-module-oidc-provider@0.4.15
    - Docker host-build pattern (pre-build on host, yarn workspaces focus --production)
  patterns:
    - OIDC auth: ApiBlueprint with OAuth2.create, SignInPageBlueprint for sign-in
    - Environment variable substitution for all secrets (AUTH_OIDC_CLIENT_SECRET, POSTGRES_*)

key-files:
  created:
    - backstage/Dockerfile — Production Docker image (Node 22, yarn focus, HEALTHCHECK)
    - backstage/.dockerignore — Comprehensive exclusion patterns
  modified:
    - backstage/app-config.yaml — OIDC provider + PostgreSQL config + catalog locations
    - backstage/packages/backend/src/index.ts — OIDC provider module registration
    - backstage/packages/app/src/App.tsx — Keycloak sign-in page with OAuth2 OIDC flow
    - backstage/packages/backend/package.json — Added oidc-provider dependency
    - backstage/yarn.lock — Lock file with new OIDC provider and transitive deps
    - .gitignore — Negating exceptions for backstage/yarn.lock and backstage/.dockerignore

key-decisions:
  - "OIDC provider configured with emailMatchingUserEntityProfileEmail resolver — matches Keycloak users to Backstage User entities by email"
  - "Guest provider retained as fallback for local dev; disabled in production via env"
  - "PostgreSQL config uses env var substitution with defaults for local dev (POSTGRES_HOST:-localhost etc.)"
  - "SQLite config kept commented as fallback for dev without Docker"
  - "Dockerfile placed at backstage/Dockerfile (not packages/backend/Dockerfile) — builds the entire app, used by both Aspire and Docker Compose"
  - "HEALTHCHECK added to Dockerfile for container orchestration readiness probes"
  - "node:22-bookworm-slim chosen over trixie for better libc compatibility with node-gyp native modules"
  - "AUTH_OIDC_CLIENT_SECRET passed via env var at container runtime, never baked into image (T-14-04 mitigation)"

patterns-established:
  - "Backstage OIDC auth wiring: app-config.yaml provider -> backend OIDC module -> frontend SignInPageBlueprint + ApiBlueprint"
  - "Threat mitigation: secrets always via env vars (AUTH_OIDC_CLIENT_SECRET, POSTGRES_*), never in config files"

requirements-completed: [PORTAL-04, PORTAL-05]

duration: 5min
completed: 2026-05-01
---

# Phase 14 Plan 02: Backstage Configuration — OIDC, PostgreSQL, Dockerfile Summary

**Keycloak OIDC authentication, PostgreSQL database, and production Dockerfile configured for the Backstage developer portal — completing all core Backstage wiring needed for containerized deployment**

## Performance

- **Duration:** 5 min
- **Started:** 2026-05-01T04:15:00Z
- **Completed:** 2026-05-01T04:20:00Z
- **Tasks:** 3
- **Files modified:** 8

## Accomplishments

**Task 1 — Keycloak OIDC Auth + PostgreSQL in app-config.yaml:**
- Added OIDC provider under `auth.providers` with `metadataUrl` pointing to OpenCode Keycloak realm (`http://keycloak:8080/realms/opencode/.well-known/openid-configuration`)
- OIDC client ID set to `backstage-portal` with secret from `AUTH_OIDC_CLIENT_SECRET` env var
- Sign-in resolver configured as `emailMatchingUserEntityProfileEmail` — maps Keycloak email to Backstage User entity
- Guest provider retained as local dev fallback (disabled in production via env)
- Replaced SQLite database config with PostgreSQL (`client: pg`, `pluginDivisionMode: schema`)
- All connection params use env var substitution with sensible defaults (`${POSTGRES_HOST:-localhost}`)
- SQLite config kept commented as local dev fallback
- Updated CORS methods to standard CRUD verbs `[GET, POST, PUT, DELETE, PATCH]`
- Restructured catalog locations to point at entities.yaml, org.yaml, and catalog-info.yaml
- Added `Group` and `User` to catalog allow rules (needed for org.yaml)

**Task 2 — OIDC Provider Module + Sign-In Page:**
- Installed `@backstage/plugin-auth-backend-module-oidc-provider@0.4.15` (6 packages, 880 KiB)
- Registered the OIDC provider module in `packages/backend/src/index.ts` immediately after `plugin-auth-backend`
- Created Keycloak-branded sign-in page in `packages/app/src/App.tsx`:
  - `keycloakAuthApiRef` — typed API reference for OpenIdConnect
  - `keycloakAuthApi` — `ApiBlueprint.make()` factory using `OAuth2.create()` with OIDC provider
  - `signInPage` — `SignInPageBlueprint.make()` with Keycloak provider and sign-in message
  - Frontend module registered in `createApp()` features array
- TypeScript compilation passes with exit code 0
- Fixed root `.gitignore` to allow `backstage/yarn.lock` (blocked by `**/yarn.lock` pattern)

**Task 3 — Production Dockerfile:**
- Created `backstage/Dockerfile` (66 lines) using Backstage official host-build pattern:
  - Base image: `node:22-bookworm-slim`
  - System dependencies: python3, g++, build-essential, libsqlite3-dev
  - Cache-mount apt-get installs for layer efficiency
  - Non-root `USER node` for security
  - Copies `.yarn`, `.yarnrc.yml`, `backstage.json` for deterministic Yarn 4 installs
  - Copies `skeleton.tar.gz` for workspace structure, then `yarn workspaces focus --all --production`
  - Copies `bundle.tar.gz` with compiled JavaScript, then `app-config.yaml`
  - `HEALTHCHECK` on `/api/health` with 30s interval for container orchestration
  - `EXPOSE 7007` and `CMD ["node", "packages/backend", "--config", "app-config.yaml"]`
- Created `backstage/.dockerignore` with comprehensive exclusions (source, tests, cache, local config)
- Fixed root `.gitignore` to allow `backstage/.dockerignore` (blocked by `.dockerignore` pattern)
- Build steps documented in Dockerfile comments

## Task Commits

Each task was committed atomically:

| Task | Description | Commit |
|------|-------------|--------|
| 1 | Configure Keycloak OIDC auth and PostgreSQL database | `bea5b66` |
| 2 | Install OIDC provider module and configure Keycloak sign-in page | `b5101dc` |
| 3 | Create production Dockerfile for Backstage | `bf6cb5d` |

## Files Created/Modified

- `backstage/app-config.yaml` — OIDC provider, PostgreSQL database, catalog restructuring (29 lines added, 31 removed)
- `backstage/packages/backend/package.json` — Added `@backstage/plugin-auth-backend-module-oidc-provider` dependency
- `backstage/packages/backend/src/index.ts` — Added OIDC provider module import after auth-backend
- `backstage/packages/app/src/App.tsx` — Keycloak sign-in page with ApiBlueprint, SignInPageBlueprint, createFrontendModule (79 lines)
- `backstage/yarn.lock` — New lock file with OIDC provider and transitive dependencies (32,255 lines)
- `backstage/Dockerfile` — Production Docker image with host-build pattern (66 lines)
- `backstage/.dockerignore` — Comprehensive build context exclusion (22 lines)
- `.gitignore` — Added `!backstage/yarn.lock` and `!backstage/.dockerignore` negating exceptions

## Decisions Made

- **OIDC resolver choice:** Used `emailMatchingUserEntityProfileEmail` to match Keycloak user emails to Backstage User entities. This is the standard pattern for Keycloak integration and requires User entities in the catalog (provided by org.yaml).
- **PostgreSQL defaults:** Chose `-:-` syntax for env var defaults (`${POSTGRES_HOST:-localhost}`) rather than requiring all env vars to be set. This allows the config to work without modification in local Docker Compose where defaults match.
- **Dockerfile placement:** Placed at `backstage/Dockerfile` rather than `packages/backend/Dockerfile` per D-02 (custom image builds the entire app). The standard Backstage scaffold places it at the repo root, but our project root already has a docker-compose.yml, so `backstage/Dockerfile` is the correct path for the `backstage/` workspace.
- **Base image:** `node:22-bookworm-slim` chosen over `node:22-trixie-slim` because bookworm provides better compatibility for node-gyp native modules (sqlite3). Trixie is still very new and some packages don't have prebuilt binaries yet.
- **Root .gitignore fixes:** Two existing patterns (`**/yarn.lock` at line 110, `.dockerignore` at line 167) were blocking Backstage files. Both are standard .NET template patterns that need negating exceptions for the Backstage workspace.

## Deviations from Plan

**1. [Rule 3 - Blocking] Root .gitignore blocks `backstage/yarn.lock`**
- **Found during:** Task 2 (Commit)
- **Issue:** Root `.gitignore` has `**/yarn.lock` (line 110) which prevented staging the new lock file
- **Fix:** Added `!backstage/yarn.lock` negating exception
- **Files modified:** `.gitignore`
- **Committed in:** `b5101dc`

**2. [Rule 3 - Blocking] Root .gitignore blocks `backstage/.dockerignore`**
- **Found during:** Task 3 (Commit)
- **Issue:** Root `.gitignore` has `.dockerignore` (line 167) which prevented staging the new dockerignore file
- **Fix:** Added `!backstage/.dockerignore` negating exception
- **Files modified:** `.gitignore`
- **Committed in:** `bf6cb5d`

**3. [Rule 2 - Missing Critical] HEALTHCHECK not specified in plan**
- **Found during:** Task 3 (Dockerfile creation)
- **Issue:** The plan specified the Dockerfile structure but omitted a HEALTHCHECK, which is needed for container orchestration (Docker Compose depends_on condition, Aspire health checks)
- **Fix:** Added `HEALTHCHECK --interval=30s --timeout=3s --start-period=15s --retries=3 CMD ...` checking `/api/health`
- **Files modified:** `backstage/Dockerfile`
- **Committed in:** `bf6cb5d`

---

**Total deviations:** 3 (2 blocking, 1 missing critical)
**Impact on plan:** All non-scope-creeping fixes required for correct operation.

## Threat Model Compliance

| Threat ID | Category | Component | Disposition | Status |
|-----------|----------|-----------|-------------|--------|
| T-14-04 | S | OIDC client secret | mitigate | ✅ `AUTH_OIDC_CLIENT_SECRET` passed via env var in Docker, never in config file |
| T-14-05 | I | OIDC token validation | accept | ✅ Backstage delegates auth to Keycloak; Kong side validates tokens independently |
| T-14-06 | S | PostgreSQL credentials | mitigate | ✅ `POSTGRES_*` env vars set at container runtime, not baked into image |

## Known Stubs

- **Guest auth provider:** Still enabled as fallback in app-config.yaml. Intentionally retained for local development without Keycloak; disabled in production via env var.
- **Docker build dependencies:** The Dockerfile requires pre-built `skeleton.tar.gz` and `bundle.tar.gz` from `yarn build:backend`. This is standard Backstage host-build pattern — these files are generated during CI/CD, not committed.

## Threat Flags

None — all security-relevant surfaces (OIDC client secret, PostgreSQL credentials) are mitigated per the threat model.

## Next Phase Readiness

- Phase 14-03 (Catalog Entity YAML Files) — already complete per STATE.md. The catalog locations in app-config.yaml now point to the entities.yaml, org.yaml, and catalog-info.yaml files created in Plan 14-03.
- Phase 14-04 (Aspire + Docker Compose wiring) can proceed — the Docker image at backstage/Dockerfile uses the standard host-build pattern that both Aspire AppHost and docker-compose.yml reference. The OIDC provider and PostgreSQL config are wired. Both orchestrators need to mount environment variables for AUTH_OIDC_CLIENT_SECRET and POSTGRES_*.

## Self-Check: PASSED

| Check | Status |
|-------|--------|
| Dockerfile exists (≥30 lines) | ✅ PASS (66 lines) |
| Dockerfile `FROM node:22` | ✅ PASS |
| Dockerfile `yarn workspaces focus` | ✅ PASS |
| Dockerfile `CMD` | ✅ PASS |
| App.tsx contains `keycloakAuthApiRef` | ✅ PASS |
| App.tsx contains `SignInPageBlueprint` | ✅ PASS |
| Backend index.ts contains OIDC module | ✅ PASS |
| app-config.yaml has OIDC provider | ✅ PASS |
| app-config.yaml has PG client | ✅ PASS |
| app-config.yaml has AUTH_OIDC_CLIENT_SECRET | ✅ PASS |
| app-config.yaml has metadataUrl to Keycloak | ✅ PASS |
| TypeScript compilation passes | ✅ PASS |
| Commit `bea5b66` (Task 1) | ✅ FOUND |
| Commit `b5101dc` (Task 2) | ✅ FOUND |
| Commit `bf6cb5d` (Task 3) | ✅ FOUND |

---

*Phase: 14-api-developer-portal-with-backstage-io-developers-register-v*
*Completed: 2026-05-01*
