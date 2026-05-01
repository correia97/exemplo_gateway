---
phase: 14-api-developer-portal-with-backstage-io-developers-register-v
plan: 03
subsystem: backstage
tags: [backstage, catalog, entities, api-discovery, yaml]

requires:
  - phase: 14-api-developer-portal-with-backstage-io-developers-register-v
    plan: 01
    provides: Scaffolded Backstage 1.50.0 app with base configuration

provides:
  - Complete Backstage catalog entity hierarchy (Domain, System, Component, API) for API discovery
  - Root catalog Location entity linking to the API hierarchy
  - Developer group and user entities for Keycloak SSO matching

affects: [14-04]

tech-stack:
  added:
    - Backstage catalog entity YAML format (backstage.io/v1alpha1)
    - OpenAPI 3.0 inline definitions embedded in API entities
  patterns:
    - Product = System, Context = Domain, Sub-context = Component, API = API entity

key-files:
  created:
    - backstage/examples/entities.yaml — 15 entities across 4 Backstage kinds with full relationship wiring
  modified:
    - backstage/catalog-info.yaml — Changed from demo Component to Location entity referencing entities.yaml
    - backstage/examples/org.yaml — Replaced guest demo with api-developers Group and 2 User entities
    - backstage/app-config.yaml — Added Domain to catalog allowed rules

key-decisions:
  - "Entity relationships use spec.domain/spec.system implicit links (not explicit partOf/hasPart) — Backstage's catalog processor infers partOf from these references"
  - "Owner set to api-developers group for all entities, matching the org.yaml Group for Keycloak emailMatchingUserEntityProfileEmail resolver"
  - "API entities include both spec.system (linking to parent System) and spec.apiProvidedBy (linking to providing Component) for bidirectional relationship wiring"
  - "OpenAPI 3.0 definitions embedded inline in each API entity with Kong proxy server URLs (http://localhost:8000) and representative CRUD paths"

requirements-completed: [PORTAL-06]

duration: 5min
completed: 2026-05-01
---

# Phase 14 Plan 03: Backstage Catalog Entity YAML Files Summary

**Complete Product → Context → Sub-context → API hierarchy modeled as Backstage Domain → System → Component → API entities with full OpenAPI definitions and entity relationship wiring**

## Performance

- **Duration:** 5 min
- **Started:** 2026-05-01
- **Completed:** 2026-05-01
- **Tasks:** 2
- **Files modified:** 4 (2 created, 2 modified, 1 amended)
- **Total entities:** 15 across 4 Backstage kinds

## Accomplishments

### Task 1: Created the full API hierarchy catalog YAML file

`backstage/examples/entities.yaml` now contains 15 entities in a single multi-document YAML file:

| Entity Kind | Count | Entities |
|-------------|-------|----------|
| Domain | 2 | `dragon-ball`, `music` |
| System | 2 | `dragon-ball-platform`, `music-platform` |
| Component | 4 | `dragonball-character-service`, `dragonball-planet-service`, `dragonball-transformation-service`, `music-catalog-service` |
| API | 7 | `characters-api-v1`, `planets-api-v1`, `transformations-api-v1`, `genres-api-v1`, `artists-api-v1`, `albums-api-v1`, `tracks-api-v1` |

**Entity relationships:**
- System → Domain via `spec.domain` (implicit `partOf`)
- Component → System via `spec.system` (implicit `partOf`)
- Component → API via `providesApis`
- API → Component via `apiProvidedBy`
- API → System via `spec.system`

**OpenAPI definitions:** Each of the 7 API entities has a complete inline OpenAPI 3.0 definition including:
- `info.title` and `info.version`
- `servers[0].url` pointing to the Kong proxy URL (`http://localhost:8000/api/dragonball/v1` or `http://localhost:8000/api/music/v1`)
- List (GET) and create (POST) endpoint paths
- Per-ID CRUD endpoints (GET/{id}, PUT/{id}, DELETE/{id})
- `components/schemas` with the main entity type and create request DTO

### Task 2: Updated root catalog-info.yaml and org.yaml

- **`backstage/catalog-info.yaml`**: Changed from demo Component entity (`backstage`, type: website, owner: john@example.com) to a Location entity (`opencode-api-hierarchy`) targeting `./examples/entities.yaml`
- **`backstage/examples/org.yaml`**: Replaced demo `guest` User and `guests` Group with real entities:
  - `Group:api-developers` — Team group for portal users
  - `User:admin` — Admin user (email: admin@opencode.local) for Keycloak SSO matching
  - `User:developer1` — Developer One (email: developer1@opencode.local) for Keycloak SSO matching
- **`backstage/app-config.yaml`** (Rule 2 fix): Added `Domain` to the `catalog.rules.allow` list so Backstage will ingest Domain entities from the catalog

## Task Commits

Each task was committed atomically:

| Task | Description | Commit |
|------|-------------|--------|
| 1 | Create catalog entity YAML with full API hierarchy | `fb709c2` |
| 2 | Update root catalog-info.yaml and org.yaml with API hierarchy and developer groups | `300b49f` |

**Plan metadata:** Committed in Task 2 commit

## Files Created/Modified

- **`backstage/examples/entities.yaml`** (CREATED) — 897 lines, 15 entities, complete API hierarchy with OpenAPI definitions
- **`backstage/catalog-info.yaml`** (MODIFIED) — Changed from demo Component to Location entity referencing entities.yaml
- **`backstage/examples/org.yaml`** (MODIFIED) — Guest demo replaced with api-developers Group and 2 Users
- **`backstage/app-config.yaml`** (MODIFIED) — Added Domain to catalog allowed rules

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Domain entity kind not in Backstage catalog allowed rules**
- **Found during:** Task 2 (post-verification)
- **Issue:** `backstage/app-config.yaml` line 122 has `catalog.rules.allow: [Component, System, API, Resource, Location]` which does not include `Domain`. Without Domain in the allow list, Backstage's catalog processor would reject all Domain entities (dragon-ball, music) during ingestion, breaking the entire hierarchy.
- **Fix:** Added `Domain` to the allow list: `[Component, System, API, Resource, Location, Domain]`
- **Files modified:** `backstage/app-config.yaml`
- **Verification:** `grep -c "Domain" backstage/app-config.yaml` returns 2 (one in rule, one in techdocs comments)
- **Committed in:** `300b49f` (Task 2 commit)

**2. [Rule 3 - Blocking] Plan template key_links pattern inconsistency**
- **Found during:** Verification
- **Issue:** The plan's `must_haves.key_links` section specifies `pattern: "partOf|hasPart"` for Domain↔System and System↔Component relationships, but the task action explicitly says "Domain entities DON'T set spec.parent (they are top-level)" and "System entities set spec.domain to link to parent Domain" — using `spec.domain`/`spec.system` instead of explicit `partOf`/`hasPart` fields.
- **Resolution:** Not actually a bug — Backstage's catalog processor infers `partOf`/`hasPart` relationships automatically from `spec.domain` and `spec.system` references. The entity relationships ARE correctly wired, just through Backstage's implicit mechanism rather than explicit `partOf`/`hasPart` YAML keys. No code change needed.
- **Action:** None — relationships are correct at runtime.

---

**Total deviations:** 2 (1 auto-fixed, 1 plan inconsistency — no-op)

## Issues Encountered

1. **Python3 yaml module not available** for the automated verification script in the plan. Used PowerShell `Select-String` for entity counting instead, which confirmed all counts meet requirements.

## Known Stubs

- **org.yaml User entities:** `admin` and `developer1` are placeholder users for Keycloak OIDC emailMatchingUserEntityProfileEmail resolver testing. These are intentionally lightweight — Keycloak is the source of truth for user data.
- **OpenAPI definitions:** Inline definitions are minimal (representative endpoints and schemas) rather than full generated specs. The canonical OpenAPI docs are served by each API's `/openapi/v1.json` endpoint.

## Threat Flags

| Flag | File | Description |
|------|------|-------------|
| threat_flag: info | backstage/examples/entities.yaml | OpenAPI definitions contain localhost Kong proxy URLs (http://localhost:8000) — informational per T-14-08 (accepted risk) |
| threat_flag: info | backstage/examples/entities.yaml | API definitions are for developer discovery, no code generation — informational per T-14-07 (accepted risk) |

## Next Phase Readiness

- Phase 14-04 (Aspire/Docker Compose wiring) can proceed — catalog entities are defined and ready for Backstage ingestion
- Root catalog-info.yaml Location entity points to entities.yaml for automatic discovery
- Domain entities are now in the catalog allowed rules
- Developer group and user entities are defined for Keycloak SSO matching

## Self-Check: PASSED

| Check | Status |
|-------|--------|
| entities.yaml exists with >= 9 entities | ✅ PASS (15) |
| entities.yaml has Domain, System, Component, API kinds | ✅ PASS (all 4) |
| Domain count >= 2 | ✅ PASS (2) |
| API count >= 4 | ✅ PASS (7) |
| providesApis count >= 2 | ✅ PASS (4) |
| entities.yaml >= 80 lines | ✅ PASS (897 lines) |
| catalog-info.yaml is kind: Location | ✅ PASS |
| catalog-info.yaml references entities.yaml | ✅ PASS |
| org.yaml has api-developers group | ✅ PASS |
| org.yaml has User entities | ✅ PASS (2) |
| app-config.yaml allows Domain in catalog rules | ✅ PASS |
| All API entities have OpenAPI definitions | ✅ PASS (7/7) |
| Kong proxy URLs in API definitions | ✅ PASS (7 URLs) |
| Commit fb709c2 (Task 1) | ✅ FOUND |
| Commit 300b49f (Task 2) | ✅ FOUND |

---

*Phase: 14-api-developer-portal-with-backstage-io-developers-register-v*
*Plan: 03 — Backstage Catalog Entity YAML Files*
*Completed: 2026-05-01*
