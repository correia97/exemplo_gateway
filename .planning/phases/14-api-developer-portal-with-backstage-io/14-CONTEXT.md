# Phase 14: API Developer Portal with Backstage.io - Context

**Gathered:** 2026-05-01
**Status:** Ready for planning

<domain>
## Phase Boundary

Complete the Backstage.io API Developer Portal configuration so it runs cleanly with the existing stack. The Backstage app is already scaffolded in `src/OpenCode.Backstage/`, Docker Compose and Aspire are wired, and the Keycloak `backstage-portal` client exists — but the configuration has gaps: guest auth instead of Keycloak OIDC, catalog pointing to example files, wrong image tag in Aspire, and missing real API entity definitions.

This phase delivers:
1. A properly built Backstage Docker image (`backstage:latest` via `yarn build-image`)
2. Keycloak OIDC authentication replacing guest auth in both dev and production configs
3. Real API catalog entities (Domain→System→Component→API hierarchy) for DragonBall and Music APIs with live OpenAPI spec links, stored in `deploy/backstage/` and loaded via `app-config.yaml`
4. A static "How to get credentials" page in the portal explaining Keycloak client setup for API consumers

**Not in scope:** Automated credential generation, TechDocs build pipeline, Kubernetes deployment, new Keycloak roles.

</domain>

<decisions>
## Implementation Decisions

### Phase Scope
- **D-01: Complete portal configuration** — Fix Keycloak OIDC auth, replace example catalog with real API entities, fix Aspire image reference, fix port/baseUrl config. Portal is functional and shows DragonBall + Music APIs to authenticated users.
- **D-02: Both Docker Compose + Aspire** — Both deployment paths work. Consistent with how all other services in this project are handled (Aspire for local dev, Docker Compose for standalone deployment).
- **D-03: Build backstage:latest image** — Run `yarn build-image` inside `src/OpenCode.Backstage/backstage/` to produce `backstage:latest`. Both Aspire AppHost and Docker Compose reference this image. Matches spike 002 validation.

### Catalog Entity Model
- **D-04: Domain→System→Component→API hierarchy** — Four-level entity hierarchy:
  - Domain: `opencode-platform` (the overall platform)
  - Systems: `dragonball-system`, `music-system`
  - Components: `opencode-dragonball-service`, `opencode-music-service` (the .NET Minimal API projects)
  - APIs: `dragonball-api`, `music-api` (the REST interfaces served by the components)
- **D-05: Live OpenAPI spec via dynamic env substitution** — API entity spec uses `$env` substitution for the spec URL (e.g., `$env{DRAGONBALL_API_URL}/openapi/v1.json`). Adapts per environment (local vs Docker). No static copy.
- **D-06: Catalog YAML files in deploy/backstage/** — Catalog entity files live at `deploy/backstage/catalog-info.yaml` (alongside Kong and Keycloak configs). Not bundled inside the Docker image — mounted at runtime or referenced via file path.
- **D-07: Loaded via app-config.yaml catalog.locations** — `app-config.yaml` references catalog files via `type: file, target:` entries. Files mounted into container at a known path.
- **D-08: No TechDocs** — No mkdocs pipeline. Scalar already provides full API documentation. Portal shows catalog metadata + OpenAPI spec viewer only.

### Keycloak OIDC Authentication
- **D-09: Keycloak OIDC login required** — Replace `auth.providers.guest: {}` with the `oidc` provider configured for the existing `backstage-portal` Keycloak client. Developers must log in with Keycloak credentials to access the portal.
- **D-10: Use `oidc` auth provider** — Backstage's built-in `oidc` provider with Keycloak realm OIDC discovery URL (`http://<keycloak-host>/realms/opencode/.well-known/openid-configuration`). Works with public OIDC clients (no client secret needed for public client).
- **D-11: Both config files** — OIDC configuration in both `app-config.yaml` (pointing to local Keycloak container) and `app-config.production.yaml` (for Docker Compose deployment). Dev uses container-internal URLs; production overrides to deployed URLs.

### Developer Credentials
- **D-12: Static how-to page in Backstage** — Add a static markdown page (or catalog description annotation) in the portal explaining how API consumers get credentials: 1) Login to Keycloak admin console, 2) Create a new client with `client_credentials` grant type, 3) Use `client_id` + `client_secret` with Kong's OIDC plugin. Practical guide, no automation.

### Claude's Discretion
- Component vs API entity naming conventions (use standard Backstage naming patterns)
- Ownership/lifecycle metadata (`owner`, `lifecycle` fields in catalog YAML)
- Session secret configuration (use env var `AUTH_SESSION_SECRET` already in Docker Compose/Aspire)
- Sign-in resolver implementation (use default Backstage OIDC sign-in resolver)
- Exact `app-config.yaml` OIDC provider section format (follow Backstage docs for `oidc` provider)
- Port handling: frontend on 3000 vs backend on 7007 — use backend-serves-frontend pattern (port 7007 only exposed externally)

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Existing Backstage Configuration
- `src/OpenCode.Backstage/backstage/app-config.yaml` — Current dev Backstage config (database, CORS, catalog locations — needs OIDC added)
- `src/OpenCode.Backstage/backstage/app-config.production.yaml` — Current production config (needs OIDC replacing guest auth, catalog locations for real entities)
- `src/OpenCode.Backstage/backstage/package.json` — Backstage version and scripts including `build-image`

### Existing Infrastructure Integration
- `docker-compose.yml` — Backstage service definition (lines ~199-220: image, env vars, ports, networks)
- `src/OpenCode.AppHost/Program.cs` — Aspire Backstage container resource (lines ~123-137)
- `deploy/keycloak/OpenCode-realm.json` — Keycloak realm config including `backstage-portal` client definition

### Project Standards
- `.planning/STATE.md` — Architecture decisions and project context
- `.planning/REQUIREMENTS.md` — Project requirements (reference for what APIs exist)

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `src/OpenCode.Backstage/backstage/` — Full scaffolded Backstage 1.x app (packages/app, packages/backend, yarn.lock, Dockerfile)
- `src/OpenCode.Backstage/backstage/examples/` — Example catalog YAML files (structure reference for real catalog entities)
- `deploy/keycloak/OpenCode-realm.json` — Contains `backstage-portal` client config (redirect URIs, audience mappers already set)

### Established Patterns
- All services use `POSTGRES_HOST`, `POSTGRES_PORT`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB` env vars for PostgreSQL connection
- Aspire uses `builder.AddContainer()` for non-.NET services; Docker Compose uses `image:` + `depends_on:`
- Keycloak OIDC: both React and Angular use Authorization Code + PKCE flow — Backstage `oidc` provider follows same pattern
- Service URLs in env vars: pattern is `SERVICE_URL` or `SERVICE_HOST`/`SERVICE_PORT`

### Integration Points
- Backstage connects to the existing PostgreSQL container (user: `portal_user`, db: `opencode`) — no new schema needed (Backstage creates its own tables)
- Keycloak realm is already configured with `backstage-portal` public OIDC client including correct redirect URIs
- Kong serves DragonBall API at `:8000/api/dragonball/*` and Music API at `:8000/api/music/*` — these are the upstream URLs for OpenAPI spec links
- The OpenAPI spec endpoints: DragonBall at `/openapi/v1.json`, Music at `/openapi/v1.json` (via versioned Scalar from Phase 13)

</code_context>

<specifics>
## Specific Ideas

- `backstage:latest` image built with `yarn build-image` from `src/OpenCode.Backstage/backstage/`
- Catalog YAML in `deploy/backstage/catalog-info.yaml` — single file with all entities (Domain, Systems, Components, APIs)
- API spec URLs use env vars: `$env{DRAGONBALL_API_SPEC_URL}` → `http://localhost:8000/api/dragonball/openapi/v1.json` (local), Docker Compose/Aspire pass the env var
- Credentials how-to: either as a Backstage "About" annotation on the catalog domain entity, or as a standalone TechDocs-free markdown page linked from the catalog

</specifics>

<deferred>
## Deferred Ideas

- **Automated developer credential generation** — Full Keycloak client auto-creation flow (spike 006 partial validation). Would require a Backstage plugin or custom action. Deferred to future phase if needed.
- **TechDocs pipeline** — mkdocs-based API documentation per service. Deferred — Scalar covers this for the PoC scope.
- **GitHub/GitLab catalog source** — Loading catalog entities from a git remote instead of mounted files. Deferred — file-based is sufficient for PoC.
- **Backstage scaffolder templates** — Software templates for creating new APIs. Out of scope for this phase.

</deferred>

---

*Phase: 14-api-developer-portal-with-backstage-io*
*Context gathered: 2026-05-01*
