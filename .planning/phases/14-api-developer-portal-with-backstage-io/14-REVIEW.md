---
phase: 14-api-developer-portal-with-backstage-io
reviewed: 2026-05-01T00:00:00Z
depth: standard
files_reviewed: 7
files_reviewed_list:
  - deploy/backstage/catalog-info.yaml
  - docker-compose.yml
  - src/OpenCode.AppHost/Program.cs
  - src/OpenCode.Backstage/backstage/app-config.yaml
  - src/OpenCode.Backstage/backstage/app-config.production.yaml
  - src/OpenCode.Backstage/backstage/packages/backend/package.json
  - src/OpenCode.Backstage/backstage/packages/backend/src/index.ts
findings:
  critical: 5
  warning: 6
  info: 4
  total: 15
status: issues_found
---

# Phase 14: Code Review Report

**Reviewed:** 2026-05-01T00:00:00Z
**Depth:** standard
**Files Reviewed:** 7
**Status:** issues_found

## Summary

This phase adds Backstage as a developer portal and wires it into the existing Docker Compose stack and .NET Aspire AppHost. The catalog YAML, Backstage configuration files, backend TypeScript entry-point, and infrastructure manifests were reviewed.

Key concerns are: hardcoded plain-text credentials scattered across multiple files (docker-compose.yml, Program.cs), an intentionally non-functional OIDC client secret carried verbatim into production config, dangerously permissive auth sign-in settings, the Kong Admin API exposed on a public port, a broken dependency ordering in Program.cs (busybox waits for Keycloak but not Kong), and catalog entities referencing internal Docker hostnames that are unreachable from the Backstage backend in some deployment topologies.

---

## Critical Issues

### CR-01: Hardcoded credentials in docker-compose.yml and Program.cs

**File:** `docker-compose.yml:8`, `docker-compose.yml:36-38`, `docker-compose.yml:70-71`, `docker-compose.yml:91-92`, `docker-compose.yml:215-216`, `docker-compose.yml:219`, `src/OpenCode.AppHost/Program.cs:4-6`, `src/OpenCode.AppHost/Program.cs:31`, `src/OpenCode.AppHost/Program.cs:55`, `src/OpenCode.AppHost/Program.cs:68`

**Issue:** Database passwords, Keycloak admin credentials, Kong database passwords, and the Backstage database password are all hardcoded in plain text in both `docker-compose.yml` and the Aspire `Program.cs`. Credential values include `postgres`/`postgres`, `admin`/`admin`, `keycloak_pass`, `kong_pass`, `portal_pass`, and `dragonball_pass`/`music_pass`. Even for a proof-of-concept these values end up committed to version control and leak in CI logs. The `AddParameter` calls in Program.cs use hardcoded default values that make the secret overriding mechanism ineffective — `builder.AddParameter("postgresPass", "postgres", secret: true)` stores `"postgres"` as the fallback, so running `dotnet run` without override silently uses the insecure default.

**Fix:** Move all secrets to environment-variable references only — no defaults for secrets. For docker-compose use a `.env` file (git-ignored) or Docker secrets. For Aspire, pass parameters without inline defaults and require an `appsettings.Development.json` or user-secrets source:
```csharp
// Do NOT provide a default for a secret parameter
var password = builder.AddParameter("postgresPass", secret: true);
```
Add `.env` to `.gitignore` and document that operators must supply a `.env` before first run.

---

### CR-02: AUTH_OIDC_CLIENT_SECRET set to a placeholder in both development and production config

**File:** `docker-compose.yml:220`, `src/OpenCode.AppHost/Program.cs:133`, `src/OpenCode.Backstage/backstage/app-config.yaml:99`

**Issue:** The OIDC client secret is set to the string `"placeholder-not-used"` in both Compose and Aspire. `app-config.yaml` also defaults to `placeholder-not-used` via `${AUTH_OIDC_CLIENT_SECRET:placeholder-not-used}`. The comment `tokenEndpointAuthMethod: none` suggests a public (PKCE) client is intended, but the secret variable still flows into `app-config.production.yaml` line 39 where it is passed to the `clientSecret` field without any fallback. If Keycloak is ever configured as a confidential client, this placeholder is used silently. More critically, the presence of a non-empty string for `clientSecret` alongside `tokenEndpointAuthMethod: none` creates an ambiguous configuration that may confuse the OIDC library and skip validation.

**Fix:** For a truly public PKCE client, omit `clientSecret` entirely from the config rather than supplying a placeholder. If confidential, require a real secret via a separate env var with no fallback:
```yaml
# app-config.production.yaml — confidential client variant
clientSecret: ${AUTH_OIDC_CLIENT_SECRET}   # no fallback; fail-fast if unset
```
Or remove `clientSecret` for a public client and switch `tokenEndpointAuthMethod` to `none` deliberately.

---

### CR-03: Kong Admin API exposed on host port 8001 (unauthenticated)

**File:** `docker-compose.yml:101-102`, `src/OpenCode.AppHost/Program.cs:76`

**Issue:** Kong's Admin API (port 8001) and GUI (port 8002) are bound to `0.0.0.0` on the host. The Admin API requires no authentication by default in Kong OSS. Any process on the host — or any machine that can reach the host's IP — can read, modify, or delete all Kong routes, plugins, and credentials. The comment on line 104 of docker-compose.yml says "Admin API accessible only within the Docker network" but the `ports` mapping on lines 101-102 explicitly publishes it to the host.

**Fix:** Remove the host port bindings for 8001 and 8002. Kong containers in the same Docker network can still reach the Admin API internally. If local development requires GUI access, bind to localhost only:
```yaml
ports:
  - "8000:8000"
  - "127.0.0.1:8001:8001"   # localhost only
  - "127.0.0.1:8002:8002"   # localhost only
```

---

### CR-04: `dangerouslyAllowSignInWithoutUserInCatalog: true` in production OIDC config

**File:** `src/OpenCode.Backstage/backstage/app-config.yaml:105`, `src/OpenCode.Backstage/backstage/app-config.production.yaml:45`

**Issue:** `dangerouslyAllowSignInWithoutUserInCatalog: true` is present in both the development and production OIDC resolver configuration. This Backstage setting explicitly bypasses the requirement that a signing-in user must be represented as a `User` entity in the software catalog. The result is that any OIDC user valid at Keycloak level — including misconfigured or compromised accounts — can log in to Backstage and be granted whatever permissions the allow-all policy permits. This is particularly risky because the backend also registers `plugin-permission-backend-module-allow-all-policy` (index.ts line 44), making every authenticated user an unrestricted actor.

**Fix:** For production, remove `dangerouslyAllowSignInWithoutUserInCatalog: true` and add explicit `User` entities to the catalog for all permitted users. For development, leave the flag only in `app-config.yaml`, not in `app-config.production.yaml`.

---

### CR-05: Busybox (Kong route initializer) waits for Keycloak but not for the Kong gateway

**File:** `src/OpenCode.AppHost/Program.cs:43-47`

**Issue:** The `busybox` container runs `init-routes.sh` to configure Kong routes. In Program.cs it only waits for `keycloak` (line 47: `.WaitFor(keycloak)`). There is no `.WaitFor(kong)` or `.WaitFor(kongInit)`. This is a race condition: `init-routes.sh` tries to hit the Kong Admin API before Kong is guaranteed to be ready. In docker-compose.yml the equivalent service has `depends_on: gateway: condition: service_healthy` (line 126), which is correct. The Aspire host is missing this dependency entirely, so running via `dotnet run` will intermittently fail to register routes.

**Fix:**
```csharp
var busybox = builder.AddContainer("busybox", "rootpublic/curl", "bookworm-slim_rootio")
    .WithBindMount("../../deploy/kong/init-routes.sh", "/init-routes.sh")
    .WithEntrypoint("/bin/sh")
    .WithArgs("/init-routes.sh")
    .WaitFor(kong);          // Kong must be healthy before running init-routes.sh
```

---

## Warnings

### WR-01: API definition URLs in catalog-info.yaml use internal Docker hostnames

**File:** `deploy/backstage/catalog-info.yaml:124`, `deploy/backstage/catalog-info.yaml:140`

**Issue:** The OpenAPI `$text` definition URLs point to `http://gateway:8000/api/v1/dragonball/openapi/v1.json` and `http://gateway:8000/api/v1/music/openapi/v1.json`. The hostname `gateway` is only resolvable within the Docker network. If Backstage fetches these definitions from within the Docker network (as in docker-compose), this works. However, if `$text` causes the Backstage frontend to attempt a browser-side fetch, the hostname is unreachable and the API page in the portal will be broken. Additionally, any CI pipeline or external catalog import tool running outside the Docker network will also fail to resolve `gateway`.

**Fix:** Either expose the OpenAPI documents on a path reachable via the public `localhost:8000` URL and reference that, or use a proxy endpoint in Backstage (`proxy:` config) to forward the request through the backend, ensuring resolution is server-side only.

---

### WR-02: `app.baseUrl` mismatch between app-config.yaml and app-config.production.yaml

**File:** `src/OpenCode.Backstage/backstage/app-config.yaml:3`, `src/OpenCode.Backstage/backstage/app-config.production.yaml:3`

**Issue:** `app-config.yaml` sets `app.baseUrl: http://localhost:3000` (line 3) while `app-config.production.yaml` sets it to `http://localhost:7007` (line 3). When both files are merged at startup (CMD in Dockerfile: `--config app-config.yaml --config app-config.production.yaml`), the production value wins. The value `http://localhost:7007` equals the backend URL, which means the Backstage frontend will send auth callbacks and API requests to the backend port directly — this is only correct if the app-backend plugin serves the frontend from the same port. If a separate frontend is deployed, cookie domain and CORS will break. The variable `backend.cors.origin` in `app-config.yaml` line 40 (`http://localhost:3000`) is not overridden in production config, creating a mismatch between the CORS allowed origin and the production app baseUrl.

**Fix:** Set `app.baseUrl` via an environment variable (`${APP_BASE_URL}`) in both configs or ensure the production value explicitly matches the real publicly reachable URL and the CORS origin is updated correspondingly.

---

### WR-03: `organization.name` left as scaffolded placeholder

**File:** `src/OpenCode.Backstage/backstage/app-config.yaml:21`

**Issue:** `organization.name: My Company` is the default scaffolded value from the Backstage CLI. This string appears in the Backstage UI header and in notifications. For a project-specific portal this should reflect the actual organization name.

**Fix:** Update to the real organization name or make it configurable via an env var: `name: ${ORG_NAME:OpenCode}`.

---

### WR-04: Keycloak realm import disabled in Aspire AppHost

**File:** `src/OpenCode.AppHost/Program.cs:26`

**Issue:** Line 26 comments out `"--import-realm"` from the Keycloak container arguments. The environment variable `KC_IMPORT_REALM` (line 35) is set but that is not a recognised Keycloak environment variable — realm import is controlled via the `--import-realm` CLI flag combined with the file placed in `/opt/keycloak/data/import/`. Without the flag, the Keycloak realm configuration (`OpenCode-realm.json`) is never imported on first startup in the Aspire environment. The Backstage OIDC client (`backstage-portal`) therefore does not exist in Keycloak, causing all auth flows to fail.

**Fix:** Re-enable the flag and remove the non-functional environment variable:
```csharp
.WithArgs("start-dev", "--import-realm", "--verbose")
// Remove: .WithEnvironment("KC_IMPORT_REALM", "opencode-realm.json")
```

---

### WR-05: `better-sqlite3` is a production dependency but the production image uses PostgreSQL

**File:** `src/OpenCode.Backstage/backstage/packages/backend/package.json:48`

**Issue:** `better-sqlite3` is listed in `dependencies` (not `devDependencies`), which means it is compiled during the Docker build (requiring `libsqlite3-dev` and native build tools). The production configuration uses PostgreSQL exclusively (`app-config.production.yaml` database client is `pg`). Carrying SQLite as a production dependency increases image size, build time, and attack surface unnecessarily.

**Fix:** Move `better-sqlite3` and `node-gyp` to `devDependencies`. If SQLite is only needed for local development without a PostgreSQL instance, document this clearly and ensure the Docker build does not install `libsqlite3-dev` in production images.

---

### WR-06: `backstage` image tag is `latest` in both docker-compose.yml and Aspire AppHost

**File:** `docker-compose.yml:200`, `src/OpenCode.AppHost/Program.cs:124`

**Issue:** Both deployment manifests pull `backstage:latest`. Because this is a locally-built image (no registry prefix), `latest` will silently use whatever was last built on the developer's machine. There is no guarantee the image is current or matches the source in the repository. If a new developer runs `docker-compose up` without building first, Docker will look for and fail to find `backstage:latest` or — worse — use a stale image from a previous build.

**Fix:** Pin to a content-addressable tag or use `docker-compose build` / `build:` block to always build from source:
```yaml
backstage:
  build:
    context: src/OpenCode.Backstage/backstage
    dockerfile: ../../Dockerfile
```

---

## Info

### IN-01: `github-provider` auth module registered but GitHub integration is optional

**File:** `src/OpenCode.Backstage/backstage/packages/backend/src/index.ts:18`, `src/OpenCode.Backstage/backstage/app-config.yaml:63-66`

**Issue:** `plugin-auth-backend-module-github-provider` and `plugin-scaffolder-backend-module-github` are loaded unconditionally at startup. The GitHub token is configured as `${GITHUB_TOKEN}` with no fallback. If `GITHUB_TOKEN` is not set, Backstage will fail to register the GitHub integration and may emit noisy startup errors or fail entirely. For a portal where GitHub is optional, this is unnecessary startup risk.

**Fix:** Document that `GITHUB_TOKEN` must be set or remove the GitHub plugins from `index.ts` if they are not used in this project.

---

### IN-02: `kubernetes` config section present but empty

**File:** `src/OpenCode.Backstage/backstage/app-config.yaml:122-123`

**Issue:** The `kubernetes:` key exists in `app-config.yaml` with no child configuration. The `plugin-kubernetes-backend` is also loaded in `index.ts` (line 59). An empty `kubernetes:` block with a loaded plugin will cause Backstage to start in a degraded state with Kubernetes features enabled but no clusters configured, generating periodic reconciliation errors in logs.

**Fix:** Either remove the `kubernetes:` section and the plugin import if Kubernetes is not in scope, or add a minimal cluster configuration.

---

### IN-03: `KC_HOSTNAME` set to `localhost` in docker-compose.yml for Keycloak

**File:** `docker-compose.yml:39`

**Issue:** `KC_HOSTNAME: localhost` tells Keycloak that its canonical hostname is `localhost`. This means the `iss` (issuer) claim in tokens will be `http://localhost:8080/realms/OpenCode`. The Backstage OIDC provider is configured with `metadataUrl: ${KEYCLOAK_ISSUER}/.well-known/openid-configuration` where `KEYCLOAK_ISSUER` in docker-compose is `http://keycloak:8080/realms/OpenCode`. The issuer in the OIDC metadata document will be `http://localhost:8080/realms/OpenCode`, but the metadata URL itself is `http://keycloak:8080/...`. Strict OIDC implementations reject this issuer mismatch. Backstage's OIDC provider may fail token validation or discovery.

**Fix:** Set `KEYCLOAK_ISSUER` to `http://localhost:8080/realms/OpenCode` (matching `KC_HOSTNAME`) and ensure Keycloak is also reachable from the Backstage container on that hostname (requires network routing or a shared alias), or change `KC_HOSTNAME` to `keycloak` to keep internal resolution consistent.

---

### IN-04: CORS in app-config.yaml allows all HTTP and HTTPS origins

**File:** `src/OpenCode.Backstage/backstage/app-config.yaml:36`

**Issue:** The CSP `connect-src` directive is set to `["'self'", 'http:', 'https:']`. Allowing the bare `http:` and `https:` schemes permits connections to any origin over those protocols — this is equivalent to `*` for `connect-src`. Combined with `credentials: true` on the CORS config this is broadly permissive.

**Fix:** Restrict `connect-src` to the specific origins needed:
```yaml
csp:
  connect-src: ["'self'", 'http://localhost:8080', 'http://gateway:8000']
```

---

_Reviewed: 2026-05-01T00:00:00Z_
_Reviewer: Claude (gsd-code-reviewer)_
_Depth: standard_
