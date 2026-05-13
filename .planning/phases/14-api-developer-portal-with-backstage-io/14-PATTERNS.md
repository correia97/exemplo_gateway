# Phase 14: API Developer Portal with Backstage.io - Pattern Map

**Mapped:** 2026-05-01
**Files analyzed:** 7 (1 create, 6 modify)
**Analogs found:** 7 / 7

## File Classification

| New/Modified File | Role | Data Flow | Closest Analog | Match Quality |
|-------------------|------|-----------|----------------|---------------|
| `deploy/backstage/catalog-info.yaml` | config | static-data | `src/OpenCode.Backstage/examples/entities.yaml` | exact |
| `src/OpenCode.Backstage/app-config.yaml` | config | request-response | self (modify in place) | exact |
| `src/OpenCode.Backstage/app-config.production.yaml` | config | request-response | self (modify in place) | exact |
| `src/OpenCode.Backstage/packages/backend/src/index.ts` | config | event-driven | self (modify in place) | exact |
| `src/OpenCode.Backstage/packages/backend/package.json` | config | N/A | self (modify in place) | exact |
| `src/OpenCode.AppHost/Program.cs` | config | request-response | self -- keycloak container block (lines 24-41) | exact |
| `docker-compose.yml` | config | request-response | self -- backstage service block (lines 199-225) | exact |

## Pattern Assignments

### `deploy/backstage/catalog-info.yaml` (config, static-data) -- CREATE

**Analog:** `src/OpenCode.Backstage/examples/entities.yaml`

**Entity structure pattern** (full file, lines 1-42):
```yaml
---
# https://backstage.io/docs/features/software-catalog/descriptor-format#kind-system
apiVersion: backstage.io/v1alpha1
kind: System
metadata:
  name: examples
spec:
  owner: guests
---
# https://backstage.io/docs/features/software-catalog/descriptor-format#kind-component
apiVersion: backstage.io/v1alpha1
kind: Component
metadata:
  name: example-website
spec:
  type: website
  lifecycle: experimental
  owner: guests
  system: examples
  providesApis: [example-grpc-api]
---
# https://backstage.io/docs/features/software-catalog/descriptor-format#kind-api
apiVersion: backstage.io/v1alpha1
kind: API
metadata:
  name: example-grpc-api
spec:
  type: grpc
  lifecycle: experimental
  owner: guests
  system: examples
  definition: |
    syntax = "proto3";
    ...
```

**Key conventions from analog:**
- Multi-document YAML separated by `---`
- `apiVersion: backstage.io/v1alpha1` on every entity
- `spec.owner: guests` as default owner
- `spec.system:` references system by name (implicit `partOf` relationship)
- `spec.providesApis:` on Component links to API entities
- API `spec.definition:` can be inline or `$text: <url>`

**New entity kinds not in analog (Domain):** Follow same structure, adding `kind: Domain` with `spec.owner`. Domain has no `spec.system` (it IS the top level). Systems reference Domain via `spec.domain: <domain-name>`.

---

### `src/OpenCode.Backstage/app-config.yaml` (config, request-response) -- MODIFY

**Analog:** Self (current file)

**Auth section to REPLACE** (lines 91-111):
```yaml
auth:
  # see https://backstage.io/docs/auth/ to learn about auth providers
  providers:
    # See https://backstage.io/docs/auth/guest/provider
    guest: {}
    # Keycloak OIDC authentication
    oidc:
      clientId: ${KEYCLOAK_CLIENT_ID:backstage-portal}
      issuer: ${KEYCLOAK_ISSUER:http://localhost:8080/realms/OpenCode}
```

**Env var substitution pattern** (lines 48-52, database section -- shows `${VAR:default}` convention):
```yaml
  database:
    client: pg
    connection:
      host: ${POSTGRES_HOST:10.60.100.10}
      port: ${POSTGRES_PORT:5432}
      user: ${POSTGRES_USER:portal_user}
      password: ${POSTGRES_PASSWORD:portal_pass}
      database: ${POSTGRES_DB:opencode}
```

**Catalog section to REPLACE** (lines 116-147):
```yaml
catalog:
  import:
    entityFilename: catalog-info.yaml
    pullRequestBranchName: backstage-integration
  rules:
    - allow: [Component, System, API, Resource, Location]
  locations:
    # Local example data, file locations are relative to the backend process, typically `packages/backend`
    - type: file
      target: ../../examples/entities.yaml
    - type: file
      target: ../../examples/template/template.yaml
      rules:
        - allow: [Template]
    - type: file
      target: ../../examples/org.yaml
      rules:
        - allow: [User, Group]
```

**Key changes needed:**
1. Remove `guest: {}` from `auth.providers`
2. Replace incomplete `oidc:` block with full OIDC config including `development:` environment key, `metadataUrl`, `clientSecret`, `tokenEndpointAuthMethod: none`, `prompt: auto`, and `signIn.resolvers`
3. Add `auth.session.secret: ${AUTH_SESSION_SECRET}`
4. Add `Domain` to `catalog.rules` allow list
5. Replace example catalog locations with real `deploy/backstage/catalog-info.yaml` path (relative: `../../deploy/backstage/catalog-info.yaml` from `packages/backend`)

---

### `src/OpenCode.Backstage/app-config.production.yaml` (config, request-response) -- MODIFY

**Analog:** Self (current file)

**Auth section to REPLACE** (lines 31-33):
```yaml
auth:
  providers:
    guest: {}
```

**Catalog section to REPLACE** (lines 35-55):
```yaml
catalog:
  # Overrides the default list locations from app-config.yaml as these contain example data.
  locations:
    - type: file
      target: ./examples/entities.yaml
    - type: file
      target: ./examples/template/template.yaml
      rules:
        - allow: [Template]
    - type: file
      target: ./examples/org.yaml
      rules:
        - allow: [User, Group]
```

**Key changes needed:**
1. Remove `guest: {}` entirely
2. Add full OIDC config under `auth.providers.oidc.production:` key (production environment matches NODE_ENV=production in Docker)
3. Add `auth.session.secret: ${AUTH_SESSION_SECRET}`
4. Add `catalog.rules` with `Domain` included
5. Replace example locations with `/app/catalog/catalog-info.yaml` (container mount path)

---

### `src/OpenCode.Backstage/packages/backend/src/index.ts` (config, event-driven) -- MODIFY

**Analog:** Self (current file)

**Auth plugin section to modify** (lines 26-30):
```typescript
// auth plugin
backend.add(import('@backstage/plugin-auth-backend'));
// See https://backstage.io/docs/backend-system/building-backends/migrating#the-auth-plugin
backend.add(import('@backstage/plugin-auth-backend-module-guest-provider'));
// See https://backstage.io/docs/auth/guest/provider
```

**Plugin registration pattern** (lines 13-14, shows `backend.add(import(...))` convention):
```typescript
backend.add(import('@backstage/plugin-app-backend'));
backend.add(import('@backstage/plugin-proxy-backend'));
```

**Key changes needed:**
1. Remove line 29: `backend.add(import('@backstage/plugin-auth-backend-module-guest-provider'));`
2. Add: `backend.add(import('@backstage/plugin-auth-backend-module-oidc-provider'));`
3. Keep line 27: `backend.add(import('@backstage/plugin-auth-backend'));` (required base auth plugin)

---

### `src/OpenCode.AppHost/Program.cs` (config, request-response) -- MODIFY

**Analog:** Self -- keycloak container registration block

**Container registration pattern with env vars, bind mounts, endpoints** (lines 24-41):
```csharp
var keycloak = builder.AddContainer("keycloak", "quay.io/keycloak/keycloak", "26.6.1")
    .WithArgs("start-dev",
    //"--import-realm",
    "--verbose")
    .WithEnvironment("KC_DB", "postgres")
    .WithEnvironment("KC_DB_URL", "jdbc:postgresql://postgres:5432/opencode")
    .WithEnvironment("KC_DB_USERNAME", "keycloak_user")
    .WithEnvironment("KC_DB_PASSWORD", "keycloak_pass")
    .WithEnvironment("KC_DB_SCHEMA", "keycloak")
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", keycloakAdminPassword)
    .WithEnvironment("KC_IMPORT_REALM", "opencode-realm.json")
    .WithEnvironment("KC_HOSTNAME_URL", "http://localhost:8080")
    .WithEnvironment("TZ", "America/Sao_Paulo")
    .WithBindMount("../../deploy/keycloak/OpenCode-realm.json", "/opt/keycloak/data/import/OpenCode-realm.json", isReadOnly: false)
    .WithEndpoint(port: 8080, targetPort: 8080, scheme: "http", name: "http")
    .WithReference(postgres)
    .WaitFor(postgres);
```

**Current backstage block to fix** (lines 123-137):
```csharp
// Backstage Developer Portal -- self-service API discovery
var backstage = builder.AddContainer("backstage", "backstage:cli", "latest")
    .WithEnvironment("POSTGRES_HOST", "postgres")
    .WithEnvironment("POSTGRES_PORT", "5432")
    .WithEnvironment("POSTGRES_USER", "portal_user")
    .WithEnvironment("POSTGRES_PASSWORD", "portal_pass")
    .WithEnvironment("POSTGRES_DB", "opencode")
    .WithEnvironment("AUTH_SESSION_SECRET", "backstage-session-secret-dev")
    .WithEnvironment("AUTH_OIDC_CLIENT_SECRET", "backstage-oidc-client-secret-dev")
    .WithEnvironment("TZ", "America/Sao_Paulo")
    .WithEndpoint(port: 3000, targetPort: 3000, scheme: "http", name: "frontend")
    .WithEndpoint(port: 7007, targetPort: 7007, scheme: "http", name: "backend")
    .WithReference(postgres)
    .WaitFor(postgres)
    .WaitFor(keycloak);
```

**Key changes needed:**
1. Fix image: `"backstage:cli", "latest"` -> `"backstage", "latest"` (image name is `backstage`, tag is `latest`; `backstage:cli` is not a valid image)
2. Remove port 3000 endpoint (production image only serves on 7007)
3. Add `KEYCLOAK_ISSUER` env var: `.WithEnvironment("KEYCLOAK_ISSUER", "http://keycloak:8080/realms/OpenCode")`
4. Add `KEYCLOAK_CLIENT_ID` env var: `.WithEnvironment("KEYCLOAK_CLIENT_ID", "backstage-portal")`
5. Add bind mount for catalog: `.WithBindMount("../../deploy/backstage", "/app/catalog", isReadOnly: true)`

---

### `docker-compose.yml` (config, request-response) -- MODIFY

**Analog:** Self -- backstage service block AND keycloak service block (for volume mount pattern)

**Current backstage service** (lines 199-225):
```yaml
  backstage:
    image: backstage:latest
    container_name: backstage
    depends_on:
      postgres:
        condition: service_healthy
      keycloak:
        condition: service_started
    ports:
      - "7007:7007"
    environment:
      POSTGRES_HOST: postgres
      POSTGRES_PORT: "5432"
      POSTGRES_USER: portal_user
      POSTGRES_PASSWORD: portal_pass
      POSTGRES_DB: opencode
      AUTH_SESSION_SECRET: backstage-session-secret-dev
      AUTH_OIDC_CLIENT_SECRET: backstage-oidc-client-secret-dev
      TZ: America/Sao_Paulo
    networks:
      - opencode-net
```

**Volume mount pattern from keycloak service** (lines 44-45):
```yaml
    volumes:
      - ./deploy/keycloak/OpenCode-realm.json:/opt/keycloak/data/import/OpenCode-realm.json:ro
```

**Environment variable pattern from keycloak service** (lines 32-40):
```yaml
    environment:
      KC_DB: postgres
      KC_DB_URL: jdbc:postgresql://postgres:5432/opencode
      KC_DB_SCHEMA: keycloak
      KC_DB_USERNAME: keycloak_user
      KC_DB_PASSWORD: keycloak_pass
      KEYCLOAK_ADMIN: admin
      KEYCLOAK_ADMIN_PASSWORD: admin
      KC_HOSTNAME: localhost
      TZ: America/Sao_Paulo
```

**depends_on with condition pattern from busybox service** (lines 124-126):
```yaml
    depends_on:
      gateway:
        condition: service_healthy
```

**Key changes needed:**
1. Add `KEYCLOAK_ISSUER: http://keycloak:8080/realms/OpenCode` to environment (container-internal URL)
2. Add `KEYCLOAK_CLIENT_ID: backstage-portal` to environment
3. Add volume mount: `./deploy/backstage:/app/catalog:ro`
4. Add `gateway` to `depends_on` (so Kong is up before Backstage tries `$text` URL resolution)

---

## Shared Patterns

### Environment Variable Substitution in Backstage Config
**Source:** `src/OpenCode.Backstage/app-config.yaml` lines 48-52
**Apply to:** Both `app-config.yaml` and `app-config.production.yaml`
```yaml
# Pattern: ${ENV_VAR:default_value} for dev, ${ENV_VAR} (no default) for production
host: ${POSTGRES_HOST:10.60.100.10}
port: ${POSTGRES_PORT:5432}
```

### Aspire Container Registration
**Source:** `src/OpenCode.AppHost/Program.cs` lines 24-41
**Apply to:** Backstage container block (lines 123-137)
```csharp
// Pattern: builder.AddContainer("name", "image", "tag")
//   .WithEnvironment("KEY", "value")
//   .WithBindMount("host/path", "/container/path", isReadOnly: true)
//   .WithEndpoint(port: NNNN, targetPort: NNNN, scheme: "http", name: "name")
//   .WithReference(dependency)
//   .WaitFor(dependency);
```

### Docker Compose Service Definition
**Source:** `docker-compose.yml` lines 199-225 (backstage) and lines 25-56 (keycloak)
**Apply to:** Backstage service block
```yaml
# Pattern: service with image, depends_on with condition, ports, environment, volumes, networks, healthcheck
  service-name:
    image: image:tag
    container_name: name
    depends_on:
      dependency:
        condition: service_healthy
    ports:
      - "host:container"
    environment:
      KEY: value
    volumes:
      - ./host/path:/container/path:ro
    networks:
      - opencode-net
    healthcheck:
      test: [...]
```

### Backstage Plugin Registration
**Source:** `src/OpenCode.Backstage/packages/backend/src/index.ts` lines 13-14
**Apply to:** Backend index.ts auth plugin swap
```typescript
// Pattern: backend.add(import('@backstage/plugin-<name>'));
backend.add(import('@backstage/plugin-app-backend'));
```

### Catalog Entity YAML Structure
**Source:** `src/OpenCode.Backstage/examples/entities.yaml` lines 1-20
**Apply to:** `deploy/backstage/catalog-info.yaml`
```yaml
# Pattern: multi-document YAML, each entity has apiVersion + kind + metadata + spec
---
apiVersion: backstage.io/v1alpha1
kind: <Kind>
metadata:
  name: <kebab-case-name>
  description: <text>
  tags:
    - <tag>
spec:
  type: <type>
  lifecycle: <lifecycle>
  owner: guests
  system: <system-name>
```

## No Analog Found

| File | Role | Data Flow | Reason |
|------|------|-----------|--------|
| (none) | -- | -- | All files have analogs in the existing codebase |

Note: The OIDC auth configuration block format comes from RESEARCH.md (Backstage docs), not from an existing codebase analog, since no OIDC provider is currently configured. The research provides the exact YAML structure to use.

## Metadata

**Analog search scope:** `src/OpenCode.Backstage/`, `src/OpenCode.AppHost/`, `docker-compose.yml`, `deploy/`
**Files scanned:** 7 source files read
**Pattern extraction date:** 2026-05-01
