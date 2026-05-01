# Phase 14: API Developer Portal with Backstage.io - Research

**Researched:** 2026-05-01
**Domain:** Backstage.io configuration, OIDC authentication, catalog entity modeling
**Confidence:** HIGH

## Summary

Phase 14 completes the already-scaffolded Backstage.io developer portal by fixing authentication (replacing guest auth with Keycloak OIDC), creating real API catalog entities for the DragonBall and Music APIs, and wiring everything into both Aspire AppHost and Docker Compose. The Backstage app is scaffolded at `src/OpenCode.Backstage/backstage/` using the new backend system (Backstage 1.x with `@backstage/backend-defaults`), already has PostgreSQL configured, and already has a Dockerfile that builds a backend-serves-frontend image on port 7007.

The three main technical challenges are: (1) configuring the OIDC provider correctly with a public Keycloak client (requires `tokenEndpointAuthMethod: none` workaround since `clientSecret` is technically required in Backstage's schema), (2) creating catalog entity YAML with `$text` URL references for live OpenAPI specs (which only works with HTTP URLs, not local files), and (3) fixing several misconfigurations in the existing Aspire and Docker Compose definitions (wrong image tag, wrong ports, missing env vars, missing catalog rules for `Domain` kind).

**Primary recommendation:** Fix the OIDC auth config, create catalog YAML in `deploy/backstage/`, mount it into the container, add `Domain` to catalog rules, and fix the Aspire container to use `backstage:latest` with port 7007 only.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** Fix Keycloak OIDC auth, replace example catalog with real API entities, fix Aspire image reference, fix port/baseUrl config
- **D-02:** Both Docker Compose + Aspire deployment paths must work
- **D-03:** Build backstage:latest image via `yarn build-image` in `src/OpenCode.Backstage/backstage/`
- **D-04:** Four-level entity hierarchy: Domain (opencode-platform) -> Systems (dragonball-system, music-system) -> Components (opencode-dragonball-service, opencode-music-service) -> APIs (dragonball-api, music-api)
- **D-05:** Live OpenAPI spec via `$env` substitution for the spec URL
- **D-06:** Catalog YAML files live at `deploy/backstage/catalog-info.yaml`
- **D-07:** Loaded via `app-config.yaml` `catalog.locations` with `type: file, target:` entries, files mounted into container
- **D-08:** No TechDocs -- Scalar already provides full API documentation
- **D-09:** Keycloak OIDC login required, replace guest auth
- **D-10:** Use `oidc` auth provider with Keycloak realm OIDC discovery URL
- **D-11:** OIDC configuration in both `app-config.yaml` and `app-config.production.yaml`
- **D-12:** Static how-to page as catalog description annotation

### Claude's Discretion
- Component vs API entity naming conventions
- Ownership/lifecycle metadata (owner, lifecycle fields)
- Session secret configuration
- Sign-in resolver implementation
- Exact app-config.yaml OIDC provider section format
- Port handling: backend-serves-frontend pattern (port 7007 only)

### Deferred Ideas (OUT OF SCOPE)
- Automated developer credential generation
- TechDocs pipeline
- GitHub/GitLab catalog source
- Backstage scaffolder templates
</user_constraints>

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|-------------|----------------|-----------|
| OIDC authentication | Backstage Backend (Node.js) | Keycloak | Backend handles OAuth flow via `@backstage/plugin-auth-backend-module-oidc-provider`; Keycloak is the IdP |
| Catalog entity ingestion | Backstage Backend | Filesystem | Backend reads YAML from mounted volume, processes `$text` URL substitutions |
| API spec rendering | Backstage Frontend (React) | Backend proxy | `@backstage/plugin-api-docs` renders OpenAPI specs fetched via backend proxy |
| Static credentials page | Catalog entity metadata | -- | Description field in Domain entity, rendered as markdown by Backstage UI |
| Container orchestration | Docker Compose / Aspire | -- | Both paths serve the same `backstage:latest` image with env var configuration |

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| `@backstage/backend-defaults` | ^0.17.0 | Backend framework | Already installed, new backend system [VERIFIED: packages/backend/package.json] |
| `@backstage/plugin-auth-backend-module-oidc-provider` | ^0.4.14 | OIDC auth module | Official Backstage OIDC backend module [VERIFIED: npm registry via web search] |
| `@backstage/plugin-auth-backend` | ^0.28.0 | Auth plugin | Already installed [VERIFIED: packages/backend/package.json] |
| `@backstage/plugin-api-docs` | ^0.14.0 | API documentation viewer | Already installed in frontend [VERIFIED: packages/app/package.json] |
| `@backstage/plugin-catalog-backend` | ^3.6.0 | Catalog processing | Already installed [VERIFIED: packages/backend/package.json] |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| `@backstage/plugin-auth-backend-module-guest-provider` | ^0.2.18 | Guest auth | REMOVE -- being replaced by OIDC [VERIFIED: packages/backend/package.json] |
| `pg` | ^8.11.3 | PostgreSQL client | Already installed for database [VERIFIED: packages/backend/package.json] |

**Installation (OIDC module only -- everything else is already installed):**
```bash
cd src/OpenCode.Backstage/backstage
yarn --cwd packages/backend add @backstage/plugin-auth-backend-module-oidc-provider
```

## Architecture Patterns

### System Architecture Diagram

```
Browser (Developer)
    |
    v  (port 7007)
[Backstage Backend + Frontend]  <-- Single Docker container, backend-serves-frontend
    |           |           |
    v           v           v
[Keycloak]  [PostgreSQL]  [API Services via $text URL]
(OIDC IdP)  (portal schema) (DragonBall/Music OpenAPI specs)
    |
    v
[/realms/OpenCode/.well-known/openid-configuration]
```

**Data flow for authentication:**
1. User hits Backstage at port 7007
2. Backstage frontend redirects to Keycloak login
3. Keycloak authenticates, returns code to `/api/auth/oidc/handler/frame`
4. Backstage backend exchanges code for tokens (no client_secret sent due to `tokenEndpointAuthMethod: none`)
5. User session established with `AUTH_SESSION_SECRET`

**Data flow for catalog:**
1. Backstage backend reads `catalog.locations` from app-config
2. Processes local YAML files (mounted at `/app/catalog/`)
3. For API entities, resolves `$text: http://...` URLs to fetch OpenAPI specs
4. Renders catalog in frontend with API docs viewer

### Recommended Project Structure
```
deploy/backstage/
    catalog-info.yaml          # All catalog entities (Domain, Systems, Components, APIs)

src/OpenCode.Backstage/backstage/
    app-config.yaml            # Dev config (OIDC + catalog pointing to ../../deploy/backstage/)
    app-config.production.yaml # Production config (OIDC + catalog pointing to /app/catalog/)
    packages/
        backend/
            src/index.ts       # Add OIDC module, remove guest module
            package.json       # Add @backstage/plugin-auth-backend-module-oidc-provider
        app/
            ...                # No changes needed (api-docs plugin already installed)
```

### Pattern 1: OIDC Provider Configuration with Public Keycloak Client

**What:** Configure Backstage OIDC auth with a public Keycloak client (no real client secret).

**Critical finding:** Backstage's OIDC config schema requires `clientSecret` as a mandatory field, but the Keycloak `backstage-portal` client is configured as `publicClient: true` (no secret). The workaround is to provide a placeholder value for `clientSecret` and set `tokenEndpointAuthMethod: none` so the secret is never sent to the token endpoint. [CITED: github.com/backstage/backstage/blob/master/plugins/auth-backend-module-oidc-provider/config.d.ts]

**app-config.yaml OIDC section:**
```yaml
auth:
  session:
    secret: ${AUTH_SESSION_SECRET}
  providers:
    oidc:
      development:
        metadataUrl: ${KEYCLOAK_ISSUER:http://localhost:8080/realms/OpenCode}/.well-known/openid-configuration
        clientId: ${KEYCLOAK_CLIENT_ID:backstage-portal}
        clientSecret: ${AUTH_OIDC_CLIENT_SECRET:placeholder-not-used}
        tokenEndpointAuthMethod: none
        prompt: auto
        signIn:
          resolvers:
            - resolver: emailLocalPartMatchingUserEntityName
              dangerouslyAllowSignInWithoutUserInCatalog: true
```
[CITED: backstage.io/docs/auth/oidc/, backstage.io/docs/auth/identity-resolver/]

**Why `emailLocalPartMatchingUserEntityName` with `dangerouslyAllowSignInWithoutUserInCatalog: true`:** No User entities exist in the catalog (this is a PoC, not an enterprise with an LDAP sync). This resolver extracts the local part of the email and creates a Backstage identity without requiring a matching User entity. [CITED: backstage.io/docs/auth/identity-resolver/]

**Why `prompt: auto`:** Without this, if no Keycloak session exists, Backstage will fail to authenticate silently and show an error. [ASSUMED]

### Pattern 2: Catalog Entity Hierarchy

**What:** Domain -> System -> Component -> API hierarchy using implicit relationships via `spec.domain`, `spec.system`, and `spec.providesApis`.

**Implicit relationships:** Backstage's catalog processor automatically infers `partOf` relationships from `spec.domain` and `spec.system` references. No need for explicit `partOf`/`hasPart` fields. [VERIFIED: STATE.md decision 14-03-01]

### Pattern 3: API Spec via $text URL

**What:** Reference live OpenAPI specs from API catalog entities using `$text: <url>`.

**Critical finding:** `$text` substitution in catalog YAML supports HTTP/HTTPS URLs only -- NOT local file paths. Environment variable substitution (`$env{VAR}`) is NOT available inside `$text` values in catalog YAML. Env var substitution (`${VAR}`) works only in `app-config.yaml`, not in catalog entity files. [CITED: backstage.io/docs/features/software-catalog/descriptor-format/, github.com/backstage/backstage/issues/14372]

**Implication for D-05 (live OpenAPI spec via dynamic env substitution):** The `$text` placeholder in catalog YAML does NOT support `$env{}` substitution. The URL must be hardcoded or use a proxy. For the PoC, the simplest approach is to hardcode the URLs for each environment (dev catalog vs production catalog via separate catalog locations in each app-config file), OR use a single catalog file with a Backstage proxy endpoint that forwards to the API services.

**Recommended approach:** Use hardcoded URLs in the catalog YAML pointing to the container-internal service names (for Docker Compose/production) and configure app-config.yaml catalog locations differently per environment. In development mode, `$text` URLs can point to `http://localhost:8000/api/v1/dragonball/openapi/v1.json` (via Kong), while in production mode (Docker), they point to container names.

**Alternative (simpler for single-environment):** Inline a placeholder OpenAPI spec or use `$text` pointing to the Kong gateway URL since Backstage in Docker is on the same network.

### Pattern 4: Backend-Serves-Frontend (Production Image)

**What:** The `yarn build-image` command produces a Docker image where the backend serves the compiled frontend. Only port 7007 is needed. [VERIFIED: packages/backend/Dockerfile CMD line]

**The Dockerfile CMD:** `CMD ["node", "packages/backend", "--config", "app-config.yaml", "--config", "app-config.production.yaml"]`

This loads both config files, with production overriding dev values. The `app-config.production.yaml` sets `app.baseUrl: http://localhost:7007` and `backend.baseUrl: http://localhost:7007` -- confirming backend-serves-frontend on port 7007 only. [VERIFIED: app-config.production.yaml]

### Anti-Patterns to Avoid
- **Exposing port 3000 in production:** The production image does NOT run a separate frontend server. Port 3000 is for `yarn start` dev mode only. Exposing both 3000 and 7007 in Aspire/Docker Compose for production is wrong. [VERIFIED: Dockerfile uses backend-only CMD]
- **Using `backstage:cli` image tag:** The image should be `backstage:latest` (built by `yarn build-image`). The `backstage:cli` tag does not exist -- it was a mistake in the Aspire config. [VERIFIED: Dockerfile `--tag backstage`, docker-compose.yml uses `backstage:latest`]
- **Leaving guest auth alongside OIDC:** Having both `guest: {}` and `oidc:` providers means anyone can bypass OIDC login. Guest must be removed entirely. [VERIFIED: app-config.yaml lines 95-99]

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| OIDC auth flow | Custom OAuth middleware | `@backstage/plugin-auth-backend-module-oidc-provider` | Handles PKCE, token exchange, session management [CITED: backstage.io/docs/auth/oidc/] |
| API spec rendering | Custom OpenAPI viewer | `@backstage/plugin-api-docs` (already installed) | Renders OpenAPI, AsyncAPI, GraphQL, gRPC specs [VERIFIED: packages/app/package.json] |
| Catalog processing | Custom YAML parser | Backstage catalog backend | Handles `$text` substitution, entity validation, relationship inference [CITED: backstage.io/docs/features/software-catalog/descriptor-format/] |

## Common Pitfalls

### Pitfall 1: Keycloak Realm Name Case Sensitivity
**What goes wrong:** OIDC discovery fails with "issuer mismatch" or 404 errors.
**Why it happens:** The realm JSON file has `"realm": "OpenCode"` (capital O, capital C) but URLs are case-sensitive. If the `KEYCLOAK_ISSUER` env var uses `opencode` (lowercase), the OIDC discovery URL will 404.
**How to avoid:** Use `http://keycloak:8080/realms/OpenCode` (matching the JSON exactly). The current `app-config.yaml` default `http://localhost:8080/realms/OpenCode` is correct for local dev. For Docker Compose, set `KEYCLOAK_ISSUER=http://keycloak:8080/realms/OpenCode`.
**Warning signs:** 404 on `/.well-known/openid-configuration`, "issuer mismatch" errors in logs.
[VERIFIED: deploy/keycloak/OpenCode-realm.json line 2 shows "realm": "OpenCode"]

### Pitfall 2: Missing Domain Kind in Catalog Rules
**What goes wrong:** Domain entities are silently ignored by the catalog processor.
**Why it happens:** The current `catalog.rules` in `app-config.yaml` only allows `[Component, System, API, Resource, Location]`. `Domain` is NOT in this list. Backstage's default rules do not include `Domain`.
**How to avoid:** Add `Domain` to the catalog rules: `rules: - allow: [Component, System, API, Resource, Location, Domain]`
**Warning signs:** Domain entity loads without error but never appears in the catalog UI.
[VERIFIED: app-config.yaml line 121, CITED: backstage.io/docs/features/software-catalog/configuration/]

### Pitfall 3: $text URL Resolution Timing
**What goes wrong:** API entities show "No definition provided" because $text URLs cannot reach the API services.
**Why it happens:** Backstage fetches `$text` URLs during catalog processing, which happens at startup and periodically. If the API services are not yet running or not reachable from the Backstage container, the fetch fails silently.
**How to avoid:** Ensure API services start before Backstage (use `depends_on` in Docker Compose). Use container-internal URLs (e.g., `http://dragonball-api:8080`) rather than `localhost` URLs from inside Docker.
**Warning signs:** API entities exist in catalog but show empty spec. Check Backstage logs for fetch errors.
[CITED: backstage.io/docs/features/software-catalog/descriptor-format/]

### Pitfall 4: clientSecret Required Even for Public Clients
**What goes wrong:** Backstage fails to start with config validation error about missing `clientSecret`.
**Why it happens:** The OIDC provider config schema marks `clientSecret` as required in TypeScript types, even though Keycloak public clients don't need one.
**How to avoid:** Provide a placeholder value (e.g., `placeholder-not-used`) and set `tokenEndpointAuthMethod: none` so the secret is never sent to Keycloak.
**Warning signs:** Config validation error on startup mentioning `clientSecret`.
[CITED: github.com/backstage/backstage/blob/master/plugins/auth-backend-module-oidc-provider/config.d.ts]

### Pitfall 5: CORS Between Backstage and Keycloak
**What goes wrong:** OIDC login popup shows a CORS error or blank frame.
**Why it happens:** The Backstage backend makes server-side calls to Keycloak (not browser CORS), but the redirect URI handler (`/api/auth/oidc/handler/frame`) must be in the Keycloak client's `redirectUris`.
**How to avoid:** Verify `redirectUris` in Keycloak client includes the correct Backstage backend URL. Currently set to `http://localhost:7007/api/auth/oidc/handler/frame` -- this works for local dev. For Docker Compose, add `http://backstage:7007/api/auth/oidc/handler/frame` OR ensure the redirect uses the external URL.
**Warning signs:** Blank popup, CORS error in browser console.
[VERIFIED: deploy/keycloak/OpenCode-realm.json line 1138]

### Pitfall 6: Missing Environment Variables in Docker Compose / Aspire
**What goes wrong:** Backstage starts but OIDC fails or catalog locations are wrong.
**Why it happens:** Neither Docker Compose nor Aspire currently passes `KEYCLOAK_ISSUER` or `KEYCLOAK_CLIENT_ID` env vars.
**How to avoid:** Add `KEYCLOAK_ISSUER`, `KEYCLOAK_CLIENT_ID` to both Docker Compose and Aspire environment configurations.
**Warning signs:** OIDC defaults to `localhost` URLs from inside a container, which cannot reach Keycloak.
[VERIFIED: docker-compose.yml lines 199-225, Program.cs lines 123-137]

## Code Examples

### Catalog Entity YAML (deploy/backstage/catalog-info.yaml)

```yaml
# Source: backstage.io/docs/features/software-catalog/descriptor-format/
---
apiVersion: backstage.io/v1alpha1
kind: Domain
metadata:
  name: opencode-platform
  description: |
    OpenCode API Platform - A proof-of-concept demonstrating .NET 10 + Aspire + Keycloak + Kong + OpenTelemetry.

    ## How to Get API Credentials

    API consumers authenticate via Keycloak OIDC. To obtain credentials:

    1. **Login to Keycloak Admin Console** at http://localhost:8080 (admin/admin)
    2. **Navigate to** Clients > Create Client
    3. **Set Client Type** to "OpenID Connect"
    4. **Enable** "Client authentication" (confidential client)
    5. **Set Access Type** to "Service Account" (client_credentials grant)
    6. **Save** and copy the Client ID and Client Secret from the Credentials tab
    7. **Request a token:**
       ```
       POST http://localhost:8080/realms/OpenCode/protocol/openid-connect/token
       Content-Type: application/x-www-form-urlencoded

       grant_type=client_credentials&client_id=YOUR_CLIENT_ID&client_secret=YOUR_SECRET
       ```
    8. **Use the token** with Kong Gateway at http://localhost:8000:
       ```
       Authorization: Bearer <access_token>
       ```
  tags:
    - dotnet
    - rest
    - poc
spec:
  owner: guests
---
apiVersion: backstage.io/v1alpha1
kind: System
metadata:
  name: dragonball-system
  description: Dragon Ball character management API and service
  tags:
    - dotnet
    - rest
spec:
  owner: guests
  domain: opencode-platform
---
apiVersion: backstage.io/v1alpha1
kind: System
metadata:
  name: music-system
  description: Music catalog management API and service (genres, artists, albums, tracks)
  tags:
    - dotnet
    - rest
spec:
  owner: guests
  domain: opencode-platform
---
apiVersion: backstage.io/v1alpha1
kind: Component
metadata:
  name: opencode-dragonball-service
  description: .NET 10 Minimal API serving Dragon Ball character CRUD operations
  tags:
    - dotnet
    - minimal-api
  links:
    - url: http://localhost:5000/scalar
      title: Scalar API Docs
      icon: docs
spec:
  type: service
  lifecycle: production
  owner: guests
  system: dragonball-system
  providesApis:
    - dragonball-api
---
apiVersion: backstage.io/v1alpha1
kind: Component
metadata:
  name: opencode-music-service
  description: .NET 10 Minimal API serving Music catalog CRUD operations
  tags:
    - dotnet
    - minimal-api
  links:
    - url: http://localhost:5002/scalar
      title: Scalar API Docs
      icon: docs
spec:
  type: service
  lifecycle: production
  owner: guests
  system: music-system
  providesApis:
    - music-api
---
apiVersion: backstage.io/v1alpha1
kind: API
metadata:
  name: dragonball-api
  description: Dragon Ball Character REST API - CRUD operations for characters with pagination and filtering
  tags:
    - rest
    - openapi
spec:
  type: openapi
  lifecycle: production
  owner: guests
  system: dragonball-system
  definition:
    $text: http://localhost:8000/api/v1/dragonball/openapi/v1.json
---
apiVersion: backstage.io/v1alpha1
kind: API
metadata:
  name: music-api
  description: Music Catalog REST API - CRUD operations for genres, artists, albums, and tracks
  tags:
    - rest
    - openapi
spec:
  type: openapi
  lifecycle: production
  owner: guests
  system: music-system
  definition:
    $text: http://localhost:8000/api/v1/music/openapi/v1.json
```
[CITED: backstage.io/docs/features/software-catalog/descriptor-format/]

### Backend index.ts Changes

```typescript
// Source: backstage.io/docs/auth/oidc/
// REMOVE this line:
// backend.add(import('@backstage/plugin-auth-backend-module-guest-provider'));

// ADD this line:
backend.add(import('@backstage/plugin-auth-backend-module-oidc-provider'));
```

### app-config.yaml Auth Section (Development)

```yaml
# Source: backstage.io/docs/auth/oidc/
auth:
  session:
    secret: ${AUTH_SESSION_SECRET}
  providers:
    oidc:
      development:
        metadataUrl: ${KEYCLOAK_ISSUER:http://localhost:8080/realms/OpenCode}/.well-known/openid-configuration
        clientId: ${KEYCLOAK_CLIENT_ID:backstage-portal}
        clientSecret: ${AUTH_OIDC_CLIENT_SECRET:placeholder-not-used}
        tokenEndpointAuthMethod: none
        prompt: auto
        signIn:
          resolvers:
            - resolver: emailLocalPartMatchingUserEntityName
              dangerouslyAllowSignInWithoutUserInCatalog: true
```

### app-config.production.yaml Auth Section

```yaml
# Source: backstage.io/docs/auth/oidc/
auth:
  session:
    secret: ${AUTH_SESSION_SECRET}
  providers:
    oidc:
      production:
        metadataUrl: ${KEYCLOAK_ISSUER}/.well-known/openid-configuration
        clientId: ${KEYCLOAK_CLIENT_ID}
        clientSecret: ${AUTH_OIDC_CLIENT_SECRET}
        tokenEndpointAuthMethod: none
        prompt: auto
        signIn:
          resolvers:
            - resolver: emailLocalPartMatchingUserEntityName
              dangerouslyAllowSignInWithoutUserInCatalog: true
```

**Note on `auth.environment`:** Backstage determines the auth environment from the `NODE_ENV` environment variable. In the Dockerfile, `NODE_ENV=production` is set, so the `production:` key is used. In dev mode, `development:` is used. Both config blocks need the OIDC config under the appropriate environment key.

### Catalog Configuration in app-config.yaml

```yaml
catalog:
  rules:
    - allow: [Component, System, API, Resource, Location, Domain]
  locations:
    # Real catalog entities
    - type: file
      target: ../../deploy/backstage/catalog-info.yaml
```

### Catalog Configuration in app-config.production.yaml

```yaml
catalog:
  rules:
    - allow: [Component, System, API, Resource, Location, Domain]
  locations:
    - type: file
      target: /app/catalog/catalog-info.yaml
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Backstage old backend system | New backend system (`createBackend()`) | Backstage 1.x | This project already uses new backend system [VERIFIED: index.ts] |
| `app-backend` plugin for frontend serving | `@backstage/plugin-app-backend` | Current | Already installed, serves compiled frontend from backend [VERIFIED: packages/backend/package.json] |
| Separate auth environment config | `auth.providers.oidc.development/production` | Current | Auth env matches NODE_ENV [CITED: backstage.io/docs/auth/oidc/] |

## $text URL Resolution and D-05 Constraint

**D-05 specifies:** "API entity spec uses `$env` substitution for the spec URL."

**Research finding:** Catalog entity YAML files do NOT support `$env{}` or `${}` variable substitution. Only `app-config.yaml` supports `${ENV_VAR}` substitution. The `$text`, `$json`, and `$yaml` placeholders in catalog entities are for referencing external content, not for variable interpolation. [CITED: backstage.io/docs/features/software-catalog/descriptor-format/, backstage.io/docs/conf/writing/]

**Practical resolution:** Use two different catalog location strategies:
1. **Development (app-config.yaml):** Point to `../../deploy/backstage/catalog-info.yaml` which has `$text: http://localhost:8000/api/v1/.../openapi/v1.json` (Kong on host)
2. **Production (app-config.production.yaml):** Point to `/app/catalog/catalog-info.yaml` which has the same URLs but these work because Backstage in Docker can reach Kong at `gateway:8000` via Docker network

Since both dev and Docker Compose access the APIs through Kong on port 8000, and the `$text` URL `http://localhost:8000/...` works from the host, the simpler approach is:
- Use a single `catalog-info.yaml` with `$text: http://gateway:8000/api/v1/.../openapi/v1.json` for Docker
- Override/supplement with a dev catalog location that uses `localhost:8000` for local development

**Simplest approach for PoC:** Use `$text` with container-internal URLs for Docker Compose (the primary deployment target), and note that local dev may need API services running to resolve specs.

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | `tokenEndpointAuthMethod: none` allows skipping real client secret for public Keycloak clients | Pattern 1 | OIDC login will fail; would need to switch Keycloak client to confidential |
| A2 | `prompt: auto` is needed to avoid silent auth failures when no Keycloak session exists | Pattern 1 | Login may still work but with degraded UX (extra click or error) |
| A3 | Backstage's `$text` in catalog YAML can fetch from Docker-internal URLs like `http://gateway:8000/...` | $text URL Resolution | API spec will not render; would need to inline spec or use proxy |
| A4 | The `emailLocalPartMatchingUserEntityName` resolver works with Keycloak email claims | Pattern 1 | Sign-in fails; would need `emailMatchingUserEntityProfileEmail` instead |

## Open Questions

1. **$text URL reachability from Docker container**
   - What we know: `$text` fetches URLs server-side from the Backstage backend. In Docker Compose, Backstage is on `opencode-net` network with access to `gateway` container.
   - What's unclear: Whether Backstage's catalog processor can resolve Docker service names (e.g., `http://gateway:8000/...`) in `$text` URLs, or whether it only uses configured integrations.
   - Recommendation: Test with container-internal URL first. Fallback: use Backstage proxy backend to proxy API spec requests, or inline a static spec.

2. **Keycloak redirect URI for Docker Compose**
   - What we know: Current redirect URIs include `http://localhost:7007/api/auth/oidc/handler/frame`. When Backstage runs in Docker, the browser still accesses it at `localhost:7007`.
   - What's unclear: Whether the OIDC callback URL seen by the browser matches `localhost:7007` when running in Docker Compose (it should, since the user's browser hits `localhost:7007` which Docker maps to the container).
   - Recommendation: The current redirect URI should work since the browser-facing URL is still `localhost:7007`. No change needed.

## File Inventory

### Files to Create
| File | Purpose |
|------|---------|
| `deploy/backstage/catalog-info.yaml` | All catalog entities (Domain, Systems, Components, APIs) |

### Files to Modify
| File | Changes |
|------|---------|
| `src/OpenCode.Backstage/backstage/packages/backend/package.json` | Add `@backstage/plugin-auth-backend-module-oidc-provider`, remove `@backstage/plugin-auth-backend-module-guest-provider` |
| `src/OpenCode.Backstage/backstage/packages/backend/src/index.ts` | Replace guest provider import with OIDC provider import |
| `src/OpenCode.Backstage/backstage/app-config.yaml` | Replace guest auth with OIDC config, update catalog rules (add Domain), update catalog locations to real entities |
| `src/OpenCode.Backstage/backstage/app-config.production.yaml` | Replace guest auth with OIDC config, add catalog rules (add Domain), update catalog locations for container paths |
| `docker-compose.yml` | Add `KEYCLOAK_ISSUER`, `KEYCLOAK_CLIENT_ID` env vars, add volume mount for catalog YAML, add `depends_on` for gateway |
| `src/OpenCode.AppHost/Program.cs` | Fix image tag from `backstage:cli` to `backstage`, remove port 3000 endpoint (keep only 7007), add `KEYCLOAK_ISSUER`, `KEYCLOAK_CLIENT_ID` env vars, add catalog volume mount |

### Files NOT to Modify (already correct)
| File | Why |
|------|-----|
| `deploy/keycloak/OpenCode-realm.json` | `backstage-portal` client already configured correctly (public, correct redirect URIs) |
| `deploy/db/init.sql` | `portal` schema and `portal_user` already exist |
| `src/OpenCode.Backstage/backstage/packages/backend/Dockerfile` | Already correct (backend-serves-frontend, port 7007) |
| `src/OpenCode.Backstage/backstage/packages/app/package.json` | `@backstage/plugin-api-docs` already installed |

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | Manual validation (no automated tests for Backstage config) |
| Quick run command | `curl -s http://localhost:7007/api/health` |
| Full suite command | Manual checklist below |

### Phase Requirements -> Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| D-01 | Portal shows real APIs with OIDC login | manual | `curl http://localhost:7007/api/health` | N/A |
| D-03 | Docker image builds | smoke | `cd src/OpenCode.Backstage/backstage && yarn build-image` | N/A |
| D-04 | Catalog shows Domain/System/Component/API hierarchy | manual | `curl http://localhost:7007/api/catalog/entities?filter=kind=Domain` | N/A |
| D-09 | OIDC login works | manual | Browser test at http://localhost:7007 | N/A |

### Sampling Rate
- **Per task commit:** `curl -s http://localhost:7007/api/health` (after services are up)
- **Per wave merge:** Full manual validation of OIDC flow + catalog entities
- **Phase gate:** All catalog entities visible, OIDC login succeeds, API specs render

### Wave 0 Gaps
- None -- this phase produces configuration files, not testable code. Validation is via runtime smoke tests.

## Security Domain

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|-----------------|
| V2 Authentication | yes | Keycloak OIDC via `@backstage/plugin-auth-backend-module-oidc-provider` |
| V3 Session Management | yes | `AUTH_SESSION_SECRET` env var for Backstage session cookies |
| V4 Access Control | no | Backstage uses allow-all permission policy (PoC scope) |
| V5 Input Validation | no | No custom input handling (catalog YAML is static) |
| V6 Cryptography | no | No custom crypto (OIDC handled by library) |

### Known Threat Patterns

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| Guest auth bypass | Elevation of Privilege | Remove `guest: {}` provider entirely [VERIFIED: current config has both guest and oidc] |
| Session secret exposure | Information Disclosure | Use env var `AUTH_SESSION_SECRET`, not hardcoded value |
| OIDC token replay | Spoofing | Short token lifetime (300s in Keycloak realm config) [VERIFIED: OpenCode-realm.json `accessTokenLifespan: 300`] |

## Sources

### Primary (HIGH confidence)
- [backstage.io/docs/auth/oidc/](https://backstage.io/docs/auth/oidc/) - OIDC provider configuration
- [backstage.io/docs/auth/identity-resolver/](https://backstage.io/docs/auth/identity-resolver/) - Sign-in resolvers, `dangerouslyAllowSignInWithoutUserInCatalog`
- [backstage.io/docs/features/software-catalog/descriptor-format/](https://backstage.io/docs/features/software-catalog/descriptor-format/) - Catalog entity YAML format for all kinds
- [backstage.io/docs/features/software-catalog/configuration/](https://backstage.io/docs/features/software-catalog/configuration/) - Catalog rules configuration
- [github.com/backstage/backstage config.d.ts](https://github.com/backstage/backstage/blob/master/plugins/auth-backend-module-oidc-provider/config.d.ts) - OIDC config TypeScript interface
- Codebase files: app-config.yaml, app-config.production.yaml, package.json files, index.ts, Dockerfile, docker-compose.yml, Program.cs, OpenCode-realm.json

### Secondary (MEDIUM confidence)
- [@backstage/plugin-auth-backend-module-oidc-provider on npm](https://www.npmjs.com/package/@backstage/plugin-auth-backend-module-oidc-provider) - Package version verification

### Tertiary (LOW confidence)
- Web search results about `tokenEndpointAuthMethod: none` for public clients - not verified with official Backstage docs specifically confirming this works

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All packages verified in existing package.json or npm registry
- Architecture: HIGH - Dockerfile, config files, and Backstage docs all confirm backend-serves-frontend on port 7007
- OIDC configuration: MEDIUM - Config schema verified, but `tokenEndpointAuthMethod: none` with public client is assumption A1
- Catalog entities: HIGH - Format verified from official docs, $text URL behavior confirmed
- Pitfalls: HIGH - All identified from direct codebase inspection + official docs

**Research date:** 2026-05-01
**Valid until:** 2026-05-31 (Backstage releases frequently but core config format is stable)
