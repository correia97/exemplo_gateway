# 04-01: Keycloak Realm & Client Setup — Summary

## What Was Done

1. **Realm configuration** in `deploy/keycloak/setup.sh`:
   - Realm `opencode` created with `--set enabled=true`
   - Login theme set, registration enabled

2. **Client configuration**:
   - Client `dragonball-api` of type `bearer-only` for M2M auth
   - Client `music-api` of type `bearer-only`
   - Client `frontend` of type `public` with standard flow and redirect URIs
   - Client `aspire-dashboard` of type `confidential` with service account

3. **Roles** defined:
   - `realm-default`: `user`, `admin`
   - `dragonball-api`: `dragonball:read`, `dragonball:write`
   - `music-api`: `music:read`, `music:write`

4. **Test users** created:
   - `admin@opencode.local` with `admin` realm role + all client roles
   - `user@opencode.local` with `user` realm role + read-only client roles
   - Passwords set via `setup.sh --set-password`

5. **Docker Compose** integration:
   - Keycloak service with PostgreSQL store
   - Health check on `/health/ready`
   - Volume for data persistence

## Verification

- Realm exported and re-imported successfully
- Users can log in via admin console
- Clients return valid tokens via password grant and client credentials grant
- `setup.sh` idempotent (safe to re-run)

## Key Findings

- Bearer-only client type prevents Keycloak from initiating login (API-appropriate)
- Public client type needed for frontend SPA (no client secret)
- Role naming convention `resource:action` simplifies policy evaluation
