---
plan: 14-01
phase: 14
status: complete
completed: "2026-05-01"
tasks_completed: 2
tasks_total: 2
self_check: PASSED
key-files:
  created:
    - src/OpenCode.Backstage/backstage/packages/backend/package.json
    - src/OpenCode.Backstage/backstage/packages/backend/src/index.ts
    - src/OpenCode.Backstage/backstage/app-config.yaml
    - src/OpenCode.Backstage/backstage/app-config.production.yaml
  modified: []
commits:
  - "feat(14-01): replace guest auth with Keycloak OIDC in Backstage backend and config"
requirements_addressed: [PORTAL-01, PORTAL-02, PORTAL-03]
---

# Plan 14-01 Summary: Keycloak OIDC Auth Configuration

## What Was Built

Replaced Backstage guest authentication with Keycloak OIDC across all backend and config files:

### Task 1: Backend OIDC Module Swap
- **`packages/backend/package.json`**: Replaced `plugin-auth-backend-module-guest-provider@^0.2.18` with `plugin-auth-backend-module-oidc-provider@^0.4.14`
- **`packages/backend/src/index.ts`**: Replaced `backend.add(import('...guest-provider'))` with `backend.add(import('...oidc-provider'))` and updated comment to reference OIDC docs

### Task 2: Config File Updates
- **`app-config.yaml`** (dev):
  - Auth section: Full OIDC config under `auth.providers.oidc.development` with `metadataUrl`, `clientId`, `clientSecret` (placeholder), `tokenEndpointAuthMethod: none`, and `dangerouslyAllowSignInWithoutUserInCatalog: true`
  - Session secret via `${AUTH_SESSION_SECRET}`
  - Catalog rules: Added `Domain` to allow list
  - Catalog locations: Single entry pointing to `../../deploy/backstage/catalog-info.yaml`
- **`app-config.production.yaml`** (prod):
  - Auth section: Full OIDC config under `auth.providers.oidc.production` (no default values — env vars required)
  - Catalog rules: Added `Domain` to allow list
  - Catalog locations: Single entry pointing to `/app/catalog/catalog-info.yaml` (container mount)

## Key Decisions
- `tokenEndpointAuthMethod: none` required because `backstage-portal` Keycloak client is public (no client secret)
- `dangerouslyAllowSignInWithoutUserInCatalog: true` required because no User entities in catalog (PoC)
- Guest auth completely removed — OIDC is the only auth path (T-14-01 mitigation)
- Session secret from env var, never hardcoded (T-14-02 mitigation)

## Verification
- ✓ `plugin-auth-backend-module-guest-provider` removed from package.json
- ✓ `plugin-auth-backend-module-oidc-provider` added to package.json
- ✓ `guest-provider` removed from index.ts
- ✓ `oidc-provider` registered in index.ts
- ✓ `tokenEndpointAuthMethod: none` in both config files
- ✓ `Domain` in catalog.rules allow list in both config files
- ✓ Catalog locations point to real entity files (dev: relative path, prod: container path)
- ✓ No `guest: {}` provider in either config file

## Self-Check: PASSED
