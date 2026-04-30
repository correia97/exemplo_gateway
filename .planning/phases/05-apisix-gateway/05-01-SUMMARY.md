# 05-01: Kong Configuration File — Summary

## What Was Done

1. **Kong config** at `deploy/kong/config.yaml`:
   - `apix: node_listen: 9080`, `enable_admin: true`
   - Admin API key: `edd1c9f034335f136f87ad84b625c8f1`
   - Plugin list includes: `jwt-auth`, `key-auth`, `limit-req`, `limit-count`, `cors`, `prometheus`, `opentelemetry`, `proxy-rewrite`, `response-rewrite`
   - Deployment mode: traditional (single node)

2. **Docker Compose** service:
   - Image `apache/apisix:3.9.1-alpine`
   - Ports: 9080 (HTTP), 9180 (Admin API), 9443 (HTTPS)
   - Config mounted from `./deploy/kong/config.yaml`
   - Network `opencode-net`

3. **`init-routes.sh`** lifecycle:
   - Runs after Kong starts via health check polling
   - Creates upstreams and routes via Admin API

## Verification

- Kong starts and Admin API responds on port 9180
- `GET /apisix/admin/routes` returns configured routes
- Config YAML validates without errors

## Key Findings

- Plugin list must include all plugins used in routes; missing plugins cause silent failures
- Config file approach preferred over Admin API for base config (admin key stored in env var in production)
- `traditional` deployment mode sufficient for single-node development
