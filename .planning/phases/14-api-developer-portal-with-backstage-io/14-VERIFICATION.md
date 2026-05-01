---
phase: 14-api-developer-portal-with-backstage-io
verified: 2026-05-01T00:00:00Z
status: human_needed
score: 10/10 must-haves verified
overrides_applied: 0
human_verification:
  - test: "Backstage OIDC login flow with Keycloak"
    expected: "Navigating to http://localhost:7007 redirects to Keycloak login page; after login, Backstage portal is accessible with the user's identity shown"
    why_human: "Cannot verify OIDC redirect and token exchange without a running Keycloak + Backstage stack"
  - test: "Catalog entity hierarchy visible in Backstage UI"
    expected: "Backstage catalog shows opencode-platform Domain, dragonball-system and music-system Systems, their Components, and dragonball-api and music-api API entities with live OpenAPI spec rendered"
    why_human: "Cannot verify $text URL resolution and Backstage catalog ingestion without running Docker Compose stack"
  - test: "Credentials how-to guide rendered in Domain entity About card"
    expected: "Clicking on opencode-platform Domain in the catalog shows the 'How to Get API Credentials' markdown guide including the token POST example and Kong Bearer usage"
    why_human: "Backstage markdown rendering of description field requires live portal"
gaps: []
---

# Phase 14: API Developer Portal with Backstage.io Verification Report

**Phase Goal:** Complete the Backstage.io developer portal with Keycloak OIDC authentication replacing guest auth, real API catalog entities (Domain->System->Component->API hierarchy) for DragonBall and Music APIs with live OpenAPI spec links, static credentials guide, and correct Aspire + Docker Compose wiring
**Verified:** 2026-05-01T00:00:00Z
**Status:** human_needed
**Re-verification:** No — initial verification

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | Backstage backend registers OIDC auth provider instead of guest provider | VERIFIED | `index.ts` line 29: `backend.add(import('@backstage/plugin-auth-backend-module-oidc-provider'))`. No `guest-provider` import in file. |
| 2 | app-config.yaml has full OIDC config under auth.providers.oidc.development | VERIFIED | Lines 91-107: `development:` key present, `metadataUrl`, `clientId`, `clientSecret`, `tokenEndpointAuthMethod: none`, `dangerouslyAllowSignInWithoutUserInCatalog: true` all present |
| 3 | app-config.production.yaml has full OIDC config under auth.providers.oidc.production with env var references and no defaults | VERIFIED | Lines 31-45: `production:` key present, all env vars use `${VAR}` syntax with no default fallbacks |
| 4 | Guest auth provider completely removed from both config files and backend code | VERIFIED | `grep guest` in app-config.yaml returns 0 matches; `grep guest-provider` in package.json and index.ts returns 0 matches |
| 5 | Both config files include Domain in catalog.rules allow list | VERIFIED | app-config.yaml line 117: `[Component, System, API, Resource, Location, Domain]`; app-config.production.yaml line 49: same list |
| 6 | Both config files point catalog.locations to real catalog entity files | VERIFIED | app-config.yaml: `target: ../../deploy/backstage/catalog-info.yaml`; app-config.production.yaml: `target: /app/catalog/catalog-info.yaml` |
| 7 | Catalog YAML contains correct 7-entity hierarchy (1 Domain, 2 Systems, 2 Components, 2 APIs) | VERIFIED | `deploy/backstage/catalog-info.yaml`: Domain `opencode-platform`, Systems `dragonball-system` + `music-system` (with `domain: opencode-platform`), Components `opencode-dragonball-service` + `opencode-music-service` (with `providesApis`), APIs `dragonball-api` + `music-api` (with `$text` URLs) |
| 8 | Domain entity description contains credentials how-to guide | VERIFIED | Lines 9-38 of catalog-info.yaml: `## How to Get API Credentials` section with step-by-step token request guide |
| 9 | Aspire AppHost backstage container uses correct image, single port 7007, OIDC env vars, and catalog volume mount | VERIFIED | Program.cs line 124: `AddContainer("backstage", "backstage", "latest")`; no `backstage:cli`; no port 3000; `KEYCLOAK_ISSUER` + `KEYCLOAK_CLIENT_ID` env vars; `.WithBindMount("../../deploy/backstage", "/app/catalog", isReadOnly: true)`; single `.WithEndpoint(port: 7007)` |
| 10 | Docker Compose backstage service has OIDC env vars, catalog volume mount, and gateway dependency | VERIFIED | docker-compose.yml lines 199-231: `KEYCLOAK_ISSUER: http://keycloak:8080/realms/OpenCode`, `KEYCLOAK_CLIENT_ID: backstage-portal`, `./deploy/backstage:/app/catalog:ro`, `gateway: condition: service_healthy` |

**Score:** 10/10 truths verified

### Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `src/OpenCode.Backstage/backstage/app-config.yaml` | Dev OIDC auth config + catalog rules with Domain + real catalog location | VERIFIED | OIDC under `development:`, `tokenEndpointAuthMethod: none`, `Domain` in rules, path `../../deploy/backstage/catalog-info.yaml` |
| `src/OpenCode.Backstage/backstage/app-config.production.yaml` | Production OIDC auth config + catalog rules with Domain + container path | VERIFIED | OIDC under `production:`, no env var defaults, `Domain` in rules, path `/app/catalog/catalog-info.yaml` |
| `src/OpenCode.Backstage/backstage/packages/backend/src/index.ts` | OIDC provider module registration | VERIFIED | Line 29: `backend.add(import('@backstage/plugin-auth-backend-module-oidc-provider'))` |
| `src/OpenCode.Backstage/backstage/packages/backend/package.json` | OIDC provider dependency | VERIFIED | Line 24: `"@backstage/plugin-auth-backend-module-oidc-provider": "^0.4.14"` |
| `deploy/backstage/catalog-info.yaml` | Complete catalog entity hierarchy: Domain -> Systems -> Components -> APIs with credentials guide | VERIFIED | 7 entities, correct hierarchy, `$text` URLs, credentials guide in Domain description |
| `src/OpenCode.AppHost/Program.cs` | Fixed Backstage container with correct image, single port 7007, OIDC env vars, catalog mount | VERIFIED | `"backstage", "latest"`, no cli tag, no port 3000, `KEYCLOAK_ISSUER`, `KEYCLOAK_CLIENT_ID`, `/app/catalog` bind mount |
| `docker-compose.yml` | Fixed Backstage service with OIDC env vars, catalog volume mount, gateway dependency | VERIFIED | `KEYCLOAK_ISSUER`, `KEYCLOAK_CLIENT_ID`, `./deploy/backstage:/app/catalog:ro`, gateway in depends_on |

### Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| `index.ts` | `app-config.yaml auth.providers.oidc` | `backend.add(import('...oidc-provider'))` registers module that reads OIDC config | WIRED | OIDC module registered in index.ts; `development:` OIDC config in app-config.yaml |
| `catalog-info.yaml Component.spec.providesApis` | `catalog-info.yaml API.metadata.name` | Backstage implicit providesApis relationship | WIRED | `providesApis: [dragonball-api]` matches `name: dragonball-api`; same for music-api |
| `catalog-info.yaml System.spec.domain` | `catalog-info.yaml Domain.metadata.name` | Backstage implicit partOf relationship | WIRED | Both systems have `domain: opencode-platform` matching Domain `name: opencode-platform` |
| `Program.cs backstage env vars` | `app-config.yaml ${KEYCLOAK_ISSUER}` | Env var substitution at runtime | WIRED | `KEYCLOAK_ISSUER` set in Program.cs; `${KEYCLOAK_ISSUER}` referenced in app-config.yaml line 97 |
| `docker-compose.yml backstage volumes` | `deploy/backstage/catalog-info.yaml` | Docker volume mount at /app/catalog | WIRED | `./deploy/backstage:/app/catalog:ro` mounts directory containing catalog-info.yaml; app-config.production.yaml targets `/app/catalog/catalog-info.yaml` |

### Data-Flow Trace (Level 4)

Not applicable. This phase produces configuration, catalog YAML, and infrastructure wiring — no dynamic data-rendering components.

The `$text` URL resolution in catalog-info.yaml (`http://gateway:8000/api/v1/dragonball/openapi/v1.json`) is a runtime fetch by Backstage backend from Kong. Cannot verify without a running stack — routed to human verification.

### Behavioral Spot-Checks

Step 7b: SKIPPED — Backstage requires Docker stack to start. No runnable entry points without `docker compose up`. All behavioral verification routed to human verification section.

### Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
|-------------|-------------|-------------|--------|---------|
| PORTAL-01 | 14-01-PLAN.md | Keycloak OIDC auth replaces guest auth in backend | SATISFIED | index.ts + package.json: oidc-provider registered, guest-provider removed |
| PORTAL-02 | 14-01-PLAN.md | app-config.yaml dev OIDC config | SATISFIED | app-config.yaml: full OIDC under `development:` with tokenEndpointAuthMethod, resolver |
| PORTAL-03 | 14-01-PLAN.md | app-config.production.yaml prod OIDC config + catalog rules | SATISFIED | app-config.production.yaml: `production:` OIDC, no defaults, Domain in catalog rules |
| PORTAL-04 | 14-02-PLAN.md | Domain entity in catalog | SATISFIED | catalog-info.yaml: `kind: Domain` with `name: opencode-platform` |
| PORTAL-05 | 14-02-PLAN.md | System entities for DragonBall and Music | SATISFIED | catalog-info.yaml: `dragonball-system` and `music-system` with `domain: opencode-platform` |
| PORTAL-06 | 14-02-PLAN.md | API entities with $text live OpenAPI links | SATISFIED | catalog-info.yaml: `dragonball-api` and `music-api` with `$text: http://gateway:8000/...` |
| PORTAL-07 | 14-03-PLAN.md | Aspire AppHost backstage container fixed | SATISFIED | Program.cs: correct image, single port 7007, OIDC env vars, /app/catalog mount |
| PORTAL-08 | 14-03-PLAN.md | Docker Compose backstage service fixed | SATISFIED | docker-compose.yml: OIDC env vars, catalog volume mount, gateway depends_on |

**IMPORTANT — Requirements coverage gap:** PORTAL-01 through PORTAL-08 are referenced in ROADMAP.md Phase 14 and in PLAN frontmatter, but are NOT defined in `.planning/REQUIREMENTS.md`. The traceability table in REQUIREMENTS.md has no Phase 14 entries. The requirement definitions (descriptions, acceptance conditions) exist only as inferred from the PLAN files. REQUIREMENTS.md should be updated to formally register these 8 PORTAL requirements and their Phase 14 traceability.

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|------|------|---------|----------|--------|
| `deploy/backstage/catalog-info.yaml` | 232 | `$text: http://gateway:8000/...` hardcoded Docker-internal URL | Info | URL only works in Docker Compose/Aspire container context; will fail in local dev (Backstage outside Docker). This is intentional per design decision documented in PLAN 14-02: `$text` does not support `$env{}` substitution. |
| `src/OpenCode.Backstage/backstage/app-config.yaml` | 1 | `app.baseUrl: http://localhost:3000` | Warning | baseUrl points to port 3000 but production image serves on port 7007. In dev (backend-serves-frontend), this is overridden by `app-config.production.yaml` `baseUrl: http://localhost:7007`. Verify the Backstage dev-mode startup path uses port 7007 correctly. |

No TODO/FIXME/PLACEHOLDER patterns found in phase artifacts. No empty implementations or stub handlers found.

### Human Verification Required

The following items require a running Docker Compose or Aspire stack to verify:

#### 1. Keycloak OIDC Login Flow

**Test:** Start the full Docker Compose stack (`docker compose up`). Navigate to `http://localhost:7007` in a browser.
**Expected:** Browser is redirected to Keycloak login page at `http://localhost:8080/realms/OpenCode/...`. After providing valid credentials, browser is redirected back to Backstage and the portal loads with the authenticated user's identity visible.
**Why human:** Cannot verify OIDC redirect URI handshake, token exchange, and session establishment without a live Keycloak + Backstage stack.

#### 2. Catalog Entity Hierarchy in Backstage UI

**Test:** With the stack running and logged in, navigate to the Backstage Catalog. Browse to Domain, System, Component, and API views.
**Expected:** `opencode-platform` Domain is visible. Under it, `dragonball-system` and `music-system` Systems are listed. Their Components (`opencode-dragonball-service`, `opencode-music-service`) are visible. The API entities (`dragonball-api`, `music-api`) show with live OpenAPI spec (the spec viewer should render the actual OpenAPI JSON fetched from Kong via `$text`).
**Why human:** Catalog ingestion of `catalog-info.yaml`, `$text` URL resolution from the container to `gateway:8000`, and Backstage UI rendering cannot be verified without a live stack.

#### 3. Credentials How-To Guide Rendered in Domain Entity

**Test:** Click on the `opencode-platform` Domain in the Backstage catalog.
**Expected:** The Domain entity "About" card shows the full credentials guide: "## How to Get API Credentials" heading, numbered steps for Keycloak client creation, the token POST example, and the Kong Bearer header usage pattern.
**Why human:** Backstage markdown rendering of `metadata.description` in the About card requires live portal.

---

### Gaps Summary

No automated-verifiable gaps found. All 10 must-have truths are VERIFIED in the codebase.

The phase delivers:
- OIDC auth fully wired in backend code and both config files with guest auth eliminated
- Complete 7-entity catalog hierarchy with credentials guide and live OpenAPI spec links
- Both Aspire and Docker Compose deployment paths correctly configured

One documentation gap exists: PORTAL-01 through PORTAL-08 are not registered in REQUIREMENTS.md (they exist only in ROADMAP.md and PLAN frontmatter). This does not block the phase goal — the requirements are satisfied — but REQUIREMENTS.md is incomplete for Phase 14.

Three human verification items remain to confirm runtime behavior (OIDC login, catalog ingestion, UI rendering). These cannot be verified programmatically.

---

_Verified: 2026-05-01T00:00:00Z_
_Verifier: Claude (gsd-verifier)_
