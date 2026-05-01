# Phase 14: API Developer Portal with Backstage.io - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-05-01
**Phase:** 14-api-developer-portal-with-backstage-io
**Areas discussed:** Phase scope, Catalog entity model, Keycloak OIDC config, Developer credentials

---

## Phase Scope

| Option | Description | Selected |
|--------|-------------|----------|
| Complete the portal config | Fix Keycloak OIDC auth, replace example catalog with real API entities, fix Aspire image + port config. No credential management. | ✓ |
| Config + developer credentials | Everything above PLUS implement credential flow from spike 006 (partial). | |
| Minimal — working portal only | Just make the portal start cleanly. No real catalog, no OIDC. | |

**User's choice:** Complete the portal config

---

### Phase Scope — Deployment targets

| Option | Description | Selected |
|--------|-------------|----------|
| Both Docker Compose + Aspire | Consistent with how all other services work. Aspire for local dev, Docker Compose for standalone. | ✓ |
| Docker Compose only | Simpler. Aspire AddContainer with built image is trickier. | |
| You decide | Match existing Aspire+Docker pattern for containers. | |

**User's choice:** Both Docker Compose + Aspire

---

### Phase Scope — Image strategy

| Option | Description | Selected |
|--------|-------------|----------|
| Build backstage:latest via yarn build-image | Matches spike 002 validation. Both Aspire and Docker Compose reference this image. | ✓ |
| Use official Backstage Docker Hub image | Simpler but no customization — no config/catalog. | |
| You decide | Whatever the existing Dockerfile in src/OpenCode.Backstage/ supports. | |

**User's choice:** Build backstage:latest image via yarn build-image

---

## Catalog Entity Model

### Hierarchy

| Option | Description | Selected |
|--------|-------------|----------|
| Domain → System → API | 3-level. Domain: opencode-platform, Systems: dragonball-system/music-system, APIs. | |
| Flat — two API entities | Just two API entities. Simpler, no grouping. | |
| Domain → System → Component → API | Full 4-level. Component = .NET service, API = REST interface. | ✓ |

**User's choice:** Domain → System → Component → API

---

### Catalog — API spec format

| Option | Description | Selected |
|--------|-------------|----------|
| OpenAPI from Scalar endpoints | Live spec URL via /openapi/v1.json. Developers see full spec inline. | ✓ |
| Static YAML files | Copy spec to static files bundled with Backstage. Can become stale. | |
| No spec — description only | Just name/description/owner. No OpenAPI embedded. | |

**User's choice:** OpenAPI from Scalar endpoints (live URLs)

---

### Catalog — File location

| Option | Description | Selected |
|--------|-------------|----------|
| Inside src/OpenCode.Backstage/ | Travels with the Backstage app, bundled into Docker image. | |
| At project root or deploy/ | Alongside other deploy configs (Kong, Keycloak). Mounted at runtime. | ✓ |

**User's choice:** deploy/ directory (alongside Kong/Keycloak configs)

---

### Catalog — Component/API semantics

| Option | Description | Selected |
|--------|-------------|----------|
| Component = .NET service, API = REST interface | opencode-dragonball-service (component) → dragonball-api (api). Standard Backstage semantics. | |
| Component = domain area, API = endpoint group | More granular. Over-engineered for a 2-API PoC. | |
| You decide | Follow Backstage catalog best practices. | ✓ |

**User's choice:** You decide (Claude's discretion)

---

### Catalog — Ownership/lifecycle

| Option | Description | Selected |
|--------|-------------|----------|
| owner: team-opencode, lifecycle: production | Simple, both APIs share one owner. | |
| owner per API, lifecycle: experimental | Separate teams per API, experimental lifecycle since PoC. | |
| You decide | Sensible Backstage defaults for a single-team PoC. | ✓ |

**User's choice:** You decide (Claude's discretion)

---

### Catalog — Spec URL format

| Option | Description | Selected |
|--------|-------------|----------|
| Static URL string in YAML | Simple. URL may differ between local and Docker deployment. | |
| Dynamic — environment variable substitution | $env substitution. Adapts per environment. | ✓ |
| Embedded static spec | Inline OpenAPI YAML. No live link, becomes stale. | |

**User's choice:** Dynamic env substitution

---

### Catalog — Loading mechanism

| Option | Description | Selected |
|--------|-------------|----------|
| app-config.yaml catalog.locations | Standard Backstage pattern. Files mounted at known container path. | ✓ |
| URL-based discovery from git | Fetches from GitHub/GitLab URL. Requires public/token access. | |
| You decide | Whatever app-config.yaml already supports. | |

**User's choice:** app-config.yaml catalog.locations (Recommended)

---

### Catalog — TechDocs

| Option | Description | Selected |
|--------|-------------|----------|
| No TechDocs — catalog entities + OpenAPI spec only | No mkdocs pipeline needed. Scalar already provides full API docs. | ✓ |
| Basic TechDocs with README | techdocs-ref: dir:. — renders README markdown in Backstage. | |
| You decide | Whatever adds most value without extra complexity. | |

**User's choice:** No TechDocs

---

## Keycloak OIDC Config

### Auth requirement

| Option | Description | Selected |
|--------|-------------|----------|
| Keycloak OIDC login required | Replace guest auth with backstage-portal Keycloak client. Consistent with React + Angular frontends. | ✓ |
| Guest access only | Keep auth.providers.guest: {}. Portal is public. | |
| Both — Keycloak + guest fallback | Guest for local dev, Keycloak for deployed. | |

**User's choice:** Keycloak OIDC login required

---

### Auth provider

| Option | Description | Selected |
|--------|-------------|----------|
| oidc provider | Built-in Backstage oidc provider with Keycloak OIDC discovery URL. Works with public clients. | ✓ |
| oauth2Proxy | Route behind oauth2-proxy sidecar. More infrastructure, overkill for PoC. | |
| You decide | Most compatible with existing public OIDC client setup. | |

**User's choice:** oidc provider

---

### Config file placement

| Option | Description | Selected |
|--------|-------------|----------|
| app-config.yaml + app-config.production.yaml | Dev config for local, production override for deployed. Consistent split. | ✓ |
| app-config.production.yaml only | Keep OIDC only in production. Dev uses guest. | |
| You decide | Split however makes sense for existing app-config structure. | |

**User's choice:** Both config files

---

## Developer Credentials

### Treatment

| Option | Description | Selected |
|--------|-------------|----------|
| Defer to future phase | Phase 14 delivers working catalog portal. Credentials = separate feature. | |
| Document the flow only | README/Backstage page explaining manual Keycloak credential flow. | |
| Implement simple credential page | Static page in portal with how-to guide. | ✓ |

**User's choice:** Implement simple credential page

---

### Credential page content

| Option | Description | Selected |
|--------|-------------|----------|
| How-to guide for Keycloak client setup | Steps: login to Keycloak admin → create client → use with Kong OIDC. | ✓ |
| Link to Keycloak admin console only | Minimal — user figures it out themselves. | |
| You decide | Whatever makes portal most useful for API access seekers. | |

**User's choice:** How-to guide for Keycloak client setup

---

## Claude's Discretion

- Component vs API entity naming conventions
- Ownership/lifecycle metadata (`owner`, `lifecycle` field values)
- Session secret configuration approach
- Sign-in resolver implementation
- Exact OIDC provider section format in app-config.yaml
- Port handling: backend-serves-frontend vs split-port pattern

## Deferred Ideas

- **Automated credential generation** — Full Keycloak client auto-creation flow (spike 006 was partial). Future phase if needed.
- **TechDocs pipeline** — mkdocs-based documentation per service.
- **GitHub/GitLab catalog source** — Loading catalog from git remote.
- **Backstage scaffolder templates** — Software templates for creating new APIs.
