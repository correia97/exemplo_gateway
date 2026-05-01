---
plan: 14-02
phase: 14
status: complete
completed: "2026-05-01"
tasks_completed: 1
tasks_total: 1
self_check: PASSED
key-files:
  created:
    - deploy/backstage/catalog-info.yaml
  modified: []
commits:
  - "feat(14-02): add Backstage catalog entity YAML with full hierarchy"
requirements_addressed: [PORTAL-04, PORTAL-05, PORTAL-06]
---

# Plan 14-02 Summary: Catalog Entity YAML

## What Was Built

Created `deploy/backstage/catalog-info.yaml` with the complete 7-entity hierarchy for the OpenCode platform:

### Entity Hierarchy
1. **Domain**: `opencode-platform` — top-level platform with embedded credentials how-to guide in description
2. **System**: `dragonball-system` — `spec.domain: opencode-platform`
3. **System**: `music-system` — `spec.domain: opencode-platform`
4. **Component**: `opencode-dragonball-service` — `spec.system: dragonball-system`, `spec.providesApis: [dragonball-api]`
5. **Component**: `opencode-music-service` — `spec.system: music-system`, `spec.providesApis: [music-api]`
6. **API**: `dragonball-api` — `spec.definition.$text: http://gateway:8000/api/v1/dragonball/openapi/v1.json`
7. **API**: `music-api` — `spec.definition.$text: http://gateway:8000/api/v1/music/openapi/v1.json`

### Credentials How-To Guide
Embedded as markdown in the Domain entity's `metadata.description`. Covers:
- Keycloak Admin Console login
- Client creation (client_credentials grant)
- Token request example (POST to token endpoint)
- Bearer token usage with Kong Gateway

### Design Decisions
- `$text` URLs use `http://gateway:8000/...` (Docker internal hostname) — `$text` does not support `$env{}` substitution
- `owner: guests` used consistently (default Backstage owner for PoC)
- `lifecycle: production` for Components and APIs (services are deployed)
- File location (`deploy/backstage/`) is outside Backstage source, mounted as volume in production

## Verification
- ✓ 1 Domain entity with name `opencode-platform`
- ✓ 2 System entities (`dragonball-system`, `music-system`) with `domain: opencode-platform`
- ✓ 2 Component entities with `providesApis` arrays
- ✓ 2 API entities with `$text` URLs pointing to gateway
- ✓ Domain description contains credentials how-to guide
- ✓ Total: 7 entities (grep -c "kind:" returns 7)

## Self-Check: PASSED
