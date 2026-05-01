---
plan: 14-03
phase: 14
status: complete
completed: "2026-05-01"
tasks_completed: 2
tasks_total: 2
self_check: PASSED
key-files:
  created: []
  modified:
    - src/OpenCode.AppHost/Program.cs
    - docker-compose.yml
commits:
  - "feat(14-03): fix Backstage container config with OIDC env vars and catalog volume mount"
requirements_addressed: [PORTAL-07, PORTAL-08]
---

# Plan 14-03 Summary: Aspire AppHost + Docker Compose Backstage Wiring

## What Was Built

Fixed both deployment paths (Aspire and Docker Compose) to properly configure the Backstage container with OIDC env vars, catalog volume mount, and correct image reference.

### Task 1: Aspire AppHost (Program.cs)

Fixed `AddContainer("backstage", ...)`:
- **Image**: `"backstage:cli"` → `"backstage"` (matches `yarn build-image --tag backstage`)
- **Removed** extra frontend port 3000 endpoint — production image uses backend-serves-frontend on port 7007 only
- **Added** `KEYCLOAK_ISSUER: http://keycloak:8080/realms/OpenCode` — container-internal hostname
- **Added** `KEYCLOAK_CLIENT_ID: backstage-portal`
- **Fixed** `AUTH_OIDC_CLIENT_SECRET`: `"backstage-oidc-client-secret-dev"` → `"placeholder-not-used"` (public client)
- **Added** `.WithBindMount("../../deploy/backstage", "/app/catalog", isReadOnly: true)` — mounts catalog YAML

### Task 2: Docker Compose (docker-compose.yml)

Fixed `backstage:` service:
- **Added** `gateway` to `depends_on` with `condition: service_healthy` — Kong must be up before `$text` URL resolution runs
- **Added** `KEYCLOAK_ISSUER: http://keycloak:8080/realms/OpenCode`
- **Added** `KEYCLOAK_CLIENT_ID: backstage-portal`
- **Fixed** `AUTH_OIDC_CLIENT_SECRET`: `"backstage-oidc-client-secret-dev"` → `"placeholder-not-used"`
- **Added** `volumes: - ./deploy/backstage:/app/catalog:ro` — mounts catalog YAML to container

## Key Decisions
- Container-internal hostname `keycloak` (not `localhost`) for `KEYCLOAK_ISSUER` (Pitfall 1)
- `placeholder-not-used` for `AUTH_OIDC_CLIENT_SECRET` — public client, secret never sent (Pitfall 4)
- `gateway` dependency added to Docker Compose to prevent `$text` URL resolution failures on startup (Pitfall 3)
- Volume mount path `/app/catalog` matches production config `target: /app/catalog/catalog-info.yaml`

## Verification
- ✓ Program.cs uses `AddContainer("backstage", "backstage", "latest")` — no `backstage:cli`
- ✓ Program.cs has KEYCLOAK_ISSUER and KEYCLOAK_CLIENT_ID
- ✓ Program.cs has `/app/catalog` bind mount
- ✓ Program.cs has only port 7007 (no port 3000)
- ✓ docker-compose.yml backstage has KEYCLOAK_ISSUER and KEYCLOAK_CLIENT_ID
- ✓ docker-compose.yml backstage has `./deploy/backstage:/app/catalog:ro` volume
- ✓ docker-compose.yml backstage has `gateway` in depends_on

## Self-Check: PASSED
