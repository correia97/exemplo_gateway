// Rebuilds kong-apis.yaml with OpenAPI specs inlined as YAML literal blocks.
// Avoids Backstage's $text placeholder URL resolution issues on Windows / file://.
import { readFileSync, writeFileSync } from 'node:fs';
import { resolve } from 'node:path';

const here = new URL('.', import.meta.url).pathname.replace(/^\/([A-Za-z]:)/, '$1');
const root = resolve(here);

const dragonballSpec = readFileSync(resolve(root, 'openapi/dragonball-openapi.json'), 'utf8');
const musicSpec = readFileSync(resolve(root, 'openapi/music-openapi.json'), 'utf8');

const indent = (text, spaces) => {
  const pad = ' '.repeat(spaces);
  return text.split(/\r?\n/).map(line => (line.length ? pad + line : line)).join('\n');
};

const yaml = `---
# OpenCode APIs exposed via Kong API Gateway
# See deploy/kong/init-routes.sh for the upstream route definitions.
# OpenAPI specs are inlined (regenerate via examples/inline-kong-apis.mjs).
apiVersion: backstage.io/v1alpha1
kind: System
metadata:
  name: opencode-platform
  description: OpenCode backend services exposed through the Kong API Gateway.
  tags:
    - opencode
    - kong
  links:
    - url: http://localhost:8002
      title: Kong Manager
    - url: http://localhost:8001
      title: Kong Admin API
spec:
  owner: guests
---
apiVersion: backstage.io/v1alpha1
kind: Resource
metadata:
  name: kong-gateway
  description: |
    Kong API Gateway (3.9.1) — fronts the OpenCode backend APIs.
    Routes /api/dragonball and /api/music with OIDC (Keycloak) + CORS plugins.
  tags:
    - gateway
    - kong
  links:
    - url: http://localhost:8000
      title: Proxy (public)
    - url: http://localhost:8001
      title: Admin API
    - url: http://localhost:8002
      title: Kong Manager UI
spec:
  type: api-gateway
  owner: guests
  system: opencode-platform
---
apiVersion: backstage.io/v1alpha1
kind: Component
metadata:
  name: dragonball-service
  description: .NET 8 service that powers the DragonBall characters catalog.
  tags:
    - dotnet
    - postgres
  annotations:
    backstage.io/source-location: url:https://github.com/opencode/opencodeDeepseek/tree/main/src/OpenCode.DragonBall.Api
  links:
    - url: http://localhost:5000/openapi/v1.json
      title: OpenAPI spec (direct)
    - url: http://localhost:8000/api/dragonball
      title: Public endpoint (via Kong)
spec:
  type: service
  lifecycle: production
  owner: guests
  system: opencode-platform
  providesApis:
    - dragonball-api
  dependsOn:
    - resource:kong-gateway
---
apiVersion: backstage.io/v1alpha1
kind: Component
metadata:
  name: music-service
  description: .NET 8 service that powers the Music catalog (genres, artists, albums).
  tags:
    - dotnet
    - postgres
  annotations:
    backstage.io/source-location: url:https://github.com/opencode/opencodeDeepseek/tree/main/src/OpenCode.Music.Api
  links:
    - url: http://localhost:5002/openapi/v1.json
      title: OpenAPI spec (direct)
    - url: http://localhost:8000/api/music
      title: Public endpoint (via Kong)
spec:
  type: service
  lifecycle: production
  owner: guests
  system: opencode-platform
  providesApis:
    - music-api
  dependsOn:
    - resource:kong-gateway
---
apiVersion: backstage.io/v1alpha1
kind: API
metadata:
  name: dragonball-api
  description: |
    DragonBall Characters API — exposed publicly via Kong at /api/dragonball.
    Authentication: Bearer token (Keycloak realm 'opencode', client 'kong-gateway').
  tags:
    - rest
    - openapi
    - kong
  links:
    - url: http://localhost:8000/api/dragonball
      title: Public endpoint (via Kong)
    - url: http://localhost:5000/openapi/v1.json
      title: Raw OpenAPI JSON
spec:
  type: openapi
  lifecycle: production
  owner: guests
  system: opencode-platform
  definition: |
${indent(dragonballSpec, 4)}
---
apiVersion: backstage.io/v1alpha1
kind: API
metadata:
  name: music-api
  description: |
    Music catalog API — exposed publicly via Kong at /api/music.
    Authentication: Bearer token (Keycloak realm 'opencode', client 'kong-gateway').
  tags:
    - rest
    - openapi
    - kong
  links:
    - url: http://localhost:8000/api/music
      title: Public endpoint (via Kong)
    - url: http://localhost:5002/openapi/v1.json
      title: Raw OpenAPI JSON
spec:
  type: openapi
  lifecycle: production
  owner: guests
  system: opencode-platform
  definition: |
${indent(musicSpec, 4)}
`;

writeFileSync(resolve(root, 'kong-apis.yaml'), yaml, 'utf8');
console.log(`Wrote kong-apis.yaml (${yaml.length} bytes)`);
