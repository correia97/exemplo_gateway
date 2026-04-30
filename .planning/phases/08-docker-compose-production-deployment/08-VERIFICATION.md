# Phase 8: Docker Compose & Production Deployment — Verification

## Success Criteria (from ROADMAP)

1. `docker compose up` starts all 7 services (PostgreSQL, Keycloak, Kong, DragonBall API, Music API, React frontend) successfully
2. The full stack works without Aspire: Kong routing, Keycloak auth, and both CRUD APIs respond correctly
3. All container images use `latest` stable official tags (no Bitnami images)
4. Health checks are configured for every service with proper restart policies

## Prerequisites

- Docker Engine 24+ with Compose V2
- No conflicting services on ports: 5432, 8080, 2379, 8000, 9180, 3000
- .NET 10 SDK (only needed if running tests outside Docker)
- Project root: `D:\projetos\opencodeDeepseek`

## Step 1: Image Audit — Verify No Bitnami Images

```bash
# Check all image references in docker-compose.yml
Select-String "image:" docker-compose.yml
```

Expected output:
```
image: postgres:17
image: quay.io/keycloak/keycloak:latest
image: quay.io/coreos/etcd:v3.5
image: apache/Kong:3.9.1-alpine
```

No line should contain "bitnami" (case-insensitive):
```bash
Select-String -Pattern "bitnami" -CaseSensitive:$false docker-compose.yml
```
Expected: no output (no matches).

---

## Step 2: Build Custom Images

```bash
# Build all custom images (may take 3-5 minutes first time)
docker compose build
```

Expected: All 3 builds succeed (dragonball-api, music-api, frontend). Each build:
- dragonball-api: multi-stage .NET 10 SDK → runtime with non-root `appuser`
- music-api: multi-stage .NET 10 SDK → runtime with non-root `appuser`
- frontend: multi-stage Node 20 → Nginx Alpine with non-root `nginx` user

Verify non-root users:
```bash
# Check DragonBall API user
docker run --rm opencode-dragonball-api:latest whoami
# Expected: appuser

# Check Music API user
docker run --rm opencode-music-api:latest whoami
# Expected: appuser

# Check Frontend user
docker run --rm opencode-frontend:latest whoami
# Expected: nginx
```

---

## Step 3: Startup All Services

```bash
# Start all services in detached mode
docker compose up -d

# Watch startup progress
docker compose logs -f
```

Wait 60-90 seconds for all services to become healthy (especially Keycloak on first run with realm import).

---

## Step 4: Health Check Verification

```bash
# Check status of all services
docker compose ps
```

Expected: all 7 services show `Up` and `(healthy)` status.

```bash
# Detailed health per service
docker compose ps --format "table {{.Name}}\t{{.Status}}"
```

Expected output pattern:
| Name | Status |
|------|--------|
| opencode-postgres | Up (healthy) |
| opencode-keycloak | Up (healthy) |
| opencode-etcd | Up (healthy) |
| opencode-Kong | Up (healthy) |
| opencode-dragonball-api | Up (healthy) |
| opencode-music-api | Up (healthy) |
| opencode-frontend | Up (healthy) |

If any service is not healthy, check logs:
```bash
docker compose logs <service-name>
```

---

## Step 5: Kong Gateway Verification

### 5a: Kong is listening on port 8000
```bash
curl -s -o /dev/null -w "%{http_code}" http://localhost9080/
```
Expected: `404` (Kong is listening, no route matches root — this is correct).

### 5b: Dragon Ball GET route (public, no auth)
```bash
curl -s -w "\nHTTP %{http_code}" "http://localhost9080/api/dragonball/characters?page=1&pageSize=5"
```
Expected: HTTP 200 with pagination envelope (may have empty `data` array if no characters exist).

### 5c: Music GET route (public, no auth)
```bash
curl -s -w "\nHTTP %{http_code}" "http://localhost9080/api/music/artists?page=1&pageSize=5"
```
Expected: HTTP 200 with pagination envelope.

### 5d: POST without auth (should return 401)
```bash
curl -s -o /dev/null -w "Status: %{http_code}\n" -X POST "http://localhost9080/api/dragonball/characters" \
  -H "Content-Type: application/json" \
  -d '{"name":"TestCharacter","isEarthling":true,"introductionPhase":"Saiyan Saga","pictureUrl":"http://example.com/pic.jpg"}'
```
Expected: `401` (write requires auth).

### 5e: POST invalid token (should return 401)
```bash
curl -s -o /dev/null -w "Status: %{http_code}\n" -X POST "http://localhost9080/api/dragonball/characters" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer garbage-token" \
  -d '{"name":"TestCharacter","isEarthling":true,"introductionPhase":"Saiyan Saga","pictureUrl":"http://example.com/pic.jpg"}'
```
Expected: `401`.

### 5f: CORS preflight
```bash
curl -s -D - -o /dev/null -X OPTIONS "http://localhost9080/api/dragonball/characters" \
  -H "Origin: http://localhost:3000" \
  -H "Access-Control-Request-Method: POST"
```
Expected: Response headers include `Access-Control-Allow-Origin: http://localhost:3000` and `Access-Control-Allow-Methods`.

### 5g: Correlation ID
```bash
curl -s -D - -o /dev/null "http://localhost9080/api/dragonball/characters?page=1&pageSize=1"
```
Expected: Response includes `X-Correlation-Id: <uuid>` header. Run twice — IDs should differ.

---

## Step 6: Keycloak Authentication

### 6a: Keycloak is reachable
```bash
curl -s -o /dev/null -w "%{http_code}" "http://localhost:8080/realms/OpenCode/.well-known/openid-configuration"
```
Expected: `200`.

### 6b: Get editor token
```bash
$EDITOR_TOKEN = curl -s -X POST "http://localhost:8080/realms/OpenCode/protocol/openid-connect/token" `
  -H "Content-Type: application/x-www-form-urlencoded" `
  -d "username=editor1&password=password&grant_type=password&client_id=dragonball-api&client_secret=dragonball-secret" | `
  python -c "import sys,json; print(json.load(sys.stdin).get('access_token',''))"

Write-Host "Token length: $($EDITOR_TOKEN.Length)"
```
Expected: Token length > 100 (valid JWT).

### 6c: POST with editor token (should succeed)
```bash
curl -s -w "\nHTTP %{http_code}" -X POST "http://localhost9080/api/dragonball/characters" `
  -H "Content-Type: application/json" `
  -H "Authorization: Bearer $EDITOR_TOKEN" `
  -d '{"name":"Goku","isEarthling":false,"introductionPhase":"Dragon Ball","pictureUrl":"http://example.com/goku.jpg"}'
```
Expected: HTTP 201 with the created character data.

### 6d: Verify created character via GET
```bash
curl -s -w "\nHTTP %{http_code}" "http://localhost9080/api/dragonball/characters?page=1&pageSize=10"
```
Expected: HTTP 200 — the `data` array should include "Goku".

---

## Step 7: Music API CRUD (Optional — Quick Smoke Test)

### 7a: Create a genre (with editor token)
```bash
curl -s -w "\nHTTP %{http_code}" -X POST "http://localhost9080/api/music/genres" `
  -H "Content-Type: application/json" `
  -H "Authorization: Bearer $EDITOR_TOKEN" `
  -d '{"name":"Rock","description":"Rock music genre"}'
```
Expected: HTTP 201.

### 7b: List genres (public)
```bash
curl -s -w "\nHTTP %{http_code}" "http://localhost9080/api/music/genres?page=1&pageSize=10"
```
Expected: HTTP 200 with genre data.

---

## Step 8: Frontend Verification

### 8a: Frontend serves content
```bash
curl -s -o /dev/null -w "%{http_code}" http://localhost:3000/
```
Expected: `200`.

### 8b: Frontend SPA routing works
```bash
curl -s -o /dev/null -w "%{http_code}" http://localhost:3000/dragonball
```
Expected: `200` (SPA fallback serves index.html for client-side routes).

---

## Step 9: Cleanup

```bash
# Stop all services
docker compose down

# Remove volumes (optional — destroys data)
docker compose down -v
```

---

## Full Verification Script (Copy-Paste)

Save the following as `verify-phase8.ps1` and run:

```powershell
# verify-phase8.ps1
Write-Host "=== Phase 8 Verification ===" -ForegroundColor Cyan

Write-Host "`n1. Image Audit" -ForegroundColor Yellow
$images = Select-String "image:" docker-compose.yml
Write-Host $images

Write-Host "`n2. Health Checks" -ForegroundColor Yellow
$hcs = Select-String "healthcheck:" docker-compose.yml
Write-Host "Health check blocks: $($hcs.Count)" (if ($hcs.Count -eq 7) { "✓" } else { "✗ Expected 7" })

Write-Host "`n3. Bitnami Check" -ForegroundColor Yellow
$bitnami = Select-String -Pattern "bitnami" -CaseSensitive:$false docker-compose.yml
if (-not $bitnami) { Write-Host "No Bitnami images: ✓" } else { Write-Host "Bitnami found: ✗" }

Write-Host "`n4. All 7 Services" -ForegroundColor Yellow
$svcs = docker compose config --services
Write-Host ($svcs -join ", ") (if ($svcs.Count -eq 7) { "✓" } else { "✗ Expected 7" })

Write-Host "`n5. API Health" -ForegroundColor Yellow
$result1 = curl -s -o /dev/null -w "%{http_code}" http://localhost9080/api/dragonball/characters
Write-Host "Dragon Ball GET: $result1" (if ($result1 -eq 200) { "✓" } else { "✗" })

$result2 = curl -s -o /dev/null -w "%{http_code}" http://localhost9080/api/music/artists
Write-Host "Music GET: $result2" (if ($result2 -eq 200) { "✓" } else { "✗" })

Write-Host "`n6. Frontend" -ForegroundColor Yellow
$fe = curl -s -o /dev/null -w "%{http_code}" http://localhost:3000/
Write-Host "Frontend: $fe" (if ($fe -eq 200) { "✓" } else { "✗" })

Write-Host "`n7. Correlation ID" -ForegroundColor Yellow
$corr = curl -s -D - -o /dev/null "http://localhost9080/api/dragonball/characters" | Select-String "X-Correlation-Id"
Write-Host "Correlation ID: $corr" (if ($corr) { "✓" } else { "✗" })
```

---

## Troubleshooting

### Service fails to start or stays unhealthy
Check logs:
```bash
docker compose logs <service-name>
```

Common issues:

| Symptom | Likely Cause | Fix |
|---------|-------------|-----|
| PostgreSQL unhealthy | Port 5432 already in use | Stop local PostgreSQL or change mapped port |
| Keycloak unhealthy | PostgreSQL not ready yet | Wait longer (start_period: 60s); check POSTGRES_DB env |
| Keycloak fails to import realm | Realm JSON path wrong | Verify volume mount path is absolute or relative correctly |
| etcd unhealthy | ALLOW_NONE_AUTHENTICATION not set | Check env vars in compose file |
| Kong unhealthy | etcd not ready | Kong depends on etcd; check etcd logs |
| Kong returns 503 | Upstream API not reachable | Check API containers are healthy; verify container DNS names |
| Kong returns 404 on route | Routes not initialized | Check init-routes.sh ran; check Kong entrypoint command |
| DragonBall API unhealthy | PostgreSQL not ready | depends_on uses service_healthy; check postgres is actually healthy |
| Music API unhealthy | PostgreSQL not ready | Same as DragonBall |
| Frontend unhealthy | Kong not ready | depends_on uses service_started; check Kong state |
| OIDC returns 401 | Keycloak realm or client secret mismatch | Verify `opencode-realm.json` has correct client secrets |
| docker build fails for .NET APIs | Missing Directory.Packages.props or solution file | Build context is project root; verify Dockerfile COPY paths |

### Full reset — rebuild and restart everything:
```bash
docker compose down -v
docker compose build --no-cache
docker compose up -d
```
