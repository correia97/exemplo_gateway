---
phase: 14-api-developer-portal-with-backstage-io-developers-register-v
plan: 01
subsystem: infra
tags: [backstage, keycloak, postgresql, oidc, developer-portal]

requires:
  - phase: 02-database-models
    provides: PostgreSQL init script pattern (schemas, users, grants)
  - phase: 04-keycloak-authentication-authorization
    provides: Keycloak realm JSON structure, OIDC client pattern
provides:
  - Portal PostgreSQL schema with dedicated portal_user and schema-scoped permissions
  - backstage-portal OIDC client in OpenCode Keycloak realm with audience mappers
  - Scaffolded Backstage 1.50.0 application with base configuration
affects: [14-02, 14-03, 14-04]

tech-stack:
  added:
    - Backstage 1.50.0 (create-app scaffold)
    - Yarn 4.4.1 (package manager)
  patterns:
    - Portal schema follows existing dragonball/music/keycloak schema isolation pattern
    - OIDC client follows frontend public client pattern for browser-based auth

key-files:
  created:
    - backstage/ — Full Backstage 1.50.0 app scaffold (50+ files)
    - backstage/app-config.yaml — Base configuration with guest auth, SQLite local dev
    - deploy/db/init.sql — Added portal schema, user, grants section
    - deploy/keycloak/OpenCode-realm.json — Added backstage-portal client
  modified:
    - deploy/db/init.sql — Added portal schema, user, permissions, updated header
    - deploy/keycloak/OpenCode-realm.json — Added backstage-portal OIDC client
    - .gitignore — Added !backstage/packages/ exception

key-decisions:
  - "Portal schema follows existing schema isolation pattern: CREATE SCHEMA portal, portal_user with schema-scoped GRANTs"
  - "backstage-portal client: publicClient=true, Authorization Code + PKCE (same as frontend), with audience mappers for backstage-portal, dragonball-api, and music-api"
  - "Backstage scaffolded with guest auth provider for initial testing; OIDC wired in Plan 14-02"
  - "SQLite in-memory for local dev; PostgreSQL config commented out for containerized deployment"

patterns-established:
  - "Portal infrastructure: init.sql schema + Keycloak OIDC client + Backstage scaffold = three prerequisites"

requirements-completed: [PORTAL-01, PORTAL-02, PORTAL-03]

duration: 15min
completed: 2026-05-01
---

# Phase 14 Plan 01: Infrastructure & Backstage Scaffold Summary

**Portal PostgreSQL schema, backstage-portal Keycloak OIDC client, and scaffolded Backstage 1.50.0 app — establishing all three infrastructure prerequisites for the developer portal**

## Performance

- **Duration:** 15 min
- **Started:** 2026-05-01T01:00:00Z
- **Completed:** 2026-05-01T01:15:00Z
- **Tasks:** 3
- **Files modified:** 50+ (3 committed changesets)

## Accomplishments

- Added `portal` PostgreSQL schema with `portal_user`, full schema-scoped grants, and CONNECT privilege — follows existing `dragonball`/`music`/`keycloak` pattern exactly
- Created `backstage-portal` OIDC client in OpenCode Keycloak realm with Authorization Code + PKCE flow, correct Backstage redirect URIs (port 7007), and audience mappers for `backstage-portal`, `dragonball-api`, and `music-api` audiences
- Scaffolded Backstage 1.50.0 application from official `@backstage/create-app` template with 48 files across packages/backend, packages/app, and root configuration
- Configured `app-config.yaml` with `OpenCode Developer Portal` title, port 7007, guest auth provider for initial testing, and commented-out PostgreSQL config for containerized deployment
- Fixed root `.gitignore` `packages/` pattern (from .NET template) conflicting with Backstage's `backstage/packages/` — added negating exception pattern

## Task Commits

Each task was committed atomically:

1. **Task 1: Add portal schema, user, and permissions to init.sql** - `0f5a04e` (feat)
2. **Task 2: Add backstage-portal OIDC client to Keycloak realm** - `8002e2c` (feat)
3. **Task 3: Scaffold Backstage app and create base configuration** - `8cffda1` (feat)

**Plan metadata:** (included in Task 3 commit)

## Files Created/Modified

- `deploy/db/init.sql` — Added CREATE SCHEMA portal, CREATE USER portal_user, CONNECT grant, full grants section (USAGE, ALL, ALTER DEFAULT); updated header comment
- `deploy/keycloak/OpenCode-realm.json` — Added backstage-portal client object with 3 audience mappers at end of clients array
- `.gitignore` — Added `!backstage/packages/` negating exception for Backstage packages directory
- `backstage/` — Full scaffolded application (48 files total):
  - `backstage/app-config.yaml` — Base configuration with custom title, port 7007, guest auth, SQLite local dev
  - `backstage/package.json` — Root workspace package with Backstage CLI 0.36.1
  - `backstage/catalog-info.yaml` — Component descriptor for the portal itself
  - `backstage/packages/backend/` — Backend package with standard plugins (auth, catalog, scaffolder, techdocs, search, kubernetes, notifications, mcp-actions)
  - `backstage/packages/app/` — Frontend package with React-based Backstage UI
  - `backstage/.yarn/releases/yarn-4.4.1.cjs` — Bundled Yarn 4.4.1 for reproducible installs

## Decisions Made

- **Portal schema pattern:** Followed the exact existing pattern from `dragonball`, `music`, and `keycloak` schemas — `CREATE SCHEMA IF NOT EXISTS`, user in the DO block, CONNECT grant, full grants section with ALTER DEFAULT PRIVILEGES. Deviations from plan: none.
- **backstage-portal client design:** Used `publicClient: true` (same as frontend client) since Backstage uses browser-based OIDC Authorization Code + PKCE flow. No service account or authorization services enabled. Three audience mappers ensure Backstage tokens are valid for Kong gateway.
- **Backstage scaffold:** Used latest `@backstage/create-app@latest` (v0.8.2) with `--skip-install` flag to get the structure, then ran `yarn install` to populate node_modules. The scaffold included standard plugins (auth, catalog, scaffolder, techdocs, kubernetes, notifications, signals, mcp-actions).
- **Root .gitignore fix:** The existing `packages/` exclusion (from .NET template) would have silently excluded `backstage/packages/` from git tracking. Added `!backstage/packages/` as a negating exception.

## Deviations from Plan

None - plan executed exactly as written.

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Root .gitignore `packages/` pattern conflicts with Backstage scaffold**
- **Found during:** Task 3 (Scaffold Backstage app)
- **Issue:** Root `.gitignore` has `packages/` (from .NET template, historically for NuGet package folders), which silently excludes the entire `backstage/packages/` directory from git tracking. The `git add` command succeeded but no packages files were staged.
- **Fix:** Added `!backstage/packages/` negating pattern to root `.gitignore` to exempt the Backstage packages directory from the `packages/` exclusion rule.
- **Files modified:** `.gitignore`
- **Verification:** `git add backstage/packages/` now stages all package files; `git check-ignore -v` confirmed no longer blocked.
- **Committed in:** `8cffda1` (Task 3 commit, amended)

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Essential fix — without it, all Backstage source files in packages/backend and packages/app would be git-ignored and lost. No scope creep.

## Issues Encountered

1. **Interactive prompt in `@backstage/create-app`:** The scaffold command uses `inquirer` and prompts for the app name. Piped `"backstage"` via stdin in PowerShell to handle non-interactively.
2. **Yarn install native build failures:** `cpu-features@npm:0.0.10` build failed on Windows (known issue, non-fatal — only affects SSH functionality not needed for local dev). Install completed with warnings, not errors.
3. **Root .gitignore conflict:** See auto-fix above.

## Known Stubs

- **`backstage/app-config.yaml` auth section:** Guest provider is enabled for initial testing; Keycloak OIDC provider will be wired in Plan 14-02. This is intentional per plan.
- **`backstage/app-config.yaml` database section:** SQLite in-memory for local dev; PostgreSQL config is commented out. The plan explicitly notes this is by design — PG config is activated by env vars in containerized deployment.
- **`backstage/catalog-info.yaml`:** Contains demo content. The real API hierarchy (Domain/System/Component/API entities) is added in Plan 14-03. This is intentional per plan.

## Threat Flags

None — all security-relevant surfaces (portal schema permissions, OIDC client configuration) are documented in the plan's threat model and verified present in the implementation.

## Next Phase Readiness

- Phase 14-02 (Backstage Configuration) can proceed — Keycloak OIDC provider, PostgreSQL database config, OIDC backend module, and production Dockerfile are all unblocked
- The `backstage-portal` client exists with correct redirect URIs for the Backstage OIDC handler (`http://localhost:7007/api/auth/oidc/handler/frame`)
- Backstage app is scaffolded with `packages/backend` and `packages/app` ready for plugin configuration

## Self-Check: PASSED

| Check | Status |
|-------|--------|
| Portal schema in init.sql | ✅ PASS |
| portal_user in init.sql | ✅ PASS |
| backstage-portal client in realm JSON | ✅ PASS |
| app-config.yaml exists | ✅ PASS |
| backend/src/index.ts exists | ✅ PASS |
| Commit 0f5a04e (Task 1) | ✅ FOUND |
| Commit 8002e2c (Task 2) | ✅ FOUND |
| Commit 8cffda1 (Task 3) | ✅ FOUND |
| Commit aee12d6 (final metadata) | ✅ FOUND |

---

*Phase: 14-api-developer-portal-with-backstage-io-developers-register-v*
*Completed: 2026-05-01*
