# Phase 5: APISIX Gateway - Verification

## Prerequisites
- Phase 4 (Keycloak) completed and running
- Phase 3 (APIs) completed and running
- `dotnet run --project src/OpenCode.AppHost` running
- JWT tokens available (obtain via Keycloak)

## How to Obtain Test Tokens
```bash
# Get admin token (needed to create users)
ADMIN_TOKEN=$(curl -s -X POST "http://localhost:8080/realms/master/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=admin&password=admin&grant_type=password&client_id=admin-cli" | \
  python -c "import sys,json; print(json.load(sys.stdin).get('access_token',''))")

# Get viewer1 token
VIEWER1_TOKEN=$(curl -s -X POST "http://localhost:8080/realms/OpenCode/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=viewer1&password=password&grant_type=password&client_id=dragonball-api&client_secret=dragonball-secret" | \
  python -c "import sys,json; print(json.load(sys.stdin).get('access_token',''))")

# Get editor1 token
EDITOR1_TOKEN=$(curl -s -X POST "http://localhost:8080/realms/OpenCode/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=editor1&password=password&grant_type=password&client_id=dragonball-api&client_secret=dragonball-secret" | \
  python -c "import sys,json; print(json.load(sys.stdin).get('access_token',''))")
```

## Verification Checklist

### GATE-01: APISIX Entry Point
- [ ] `curl -s -o /dev/null -w "%{http_code}" http://localhost9080/` returns 404 (APISIX is listening, no route matches root)
- [ ] `curl -s -o /dev/null http://localhost9080/` returns empty body with 404 status (confirms APISIX proxy on port 8000)

### GATE-02: Dragon Ball Route
- [ ] `curl -s -w "\n%{http_code}" "http://localhost9080/api/dragonball/characters?page=1&pageSize=5"` returns 200 with character data (or empty array if no data)
- [ ] Response body has `data`, `totalCount`, `page`, `pageSize`, `totalPages` fields (pagination envelope)

### GATE-03: Music Route
- [ ] `curl -s -w "\n%{http_code}" "http://localhost9080/api/music/artists?page=1&pageSize=5"` returns 200 with artist data
- [ ] Response body has pagination envelope and artist data
- [ ] `curl -s -w "\n%{http_code}" "http://localhost9080/api/music/genres"` returns 200 with genre list

### GATE-04: Dual Auth Model
- [ ] GET without token -> 200 (public reads working)
- [ ] POST without token -> 401 (write requires auth)
- [ ] PUT without token -> 401
- [ ] DELETE without token -> 401
- [ ] POST with viewer1 token -> 403 (viewer lacks editor role - enforced by .NET, not APISIX)
- [ ] POST with editor1 token -> 201 (editor can write)

### GATE-05: OIDC Validation
- [ ] POST with invalid token (e.g., "Bearer garbage") -> 401
- [ ] POST with expired token -> 401
- [ ] POST with token for wrong client_id -> 401
- [ ] POST with valid editor1 token -> 201/200 (token accepted)

### GATE-06: CORS
- [ ] `curl -s -o /dev/null -w "%{http_code}" -X OPTIONS "http://localhost9080/api/dragonball/characters" -H "Origin: http://localhost:5173" -H "Access-Control-Request-Method: POST"` returns 200 (CORS preflight handled by APISIX)
- [ ] Above OPTIONS response includes `Access-Control-Allow-Origin: http://localhost:5173`
- [ ] Above OPTIONS response does NOT include `Access-Control-Allow-Origin: *` (when credentials are true, wildcard is invalid)
- [ ] Above OPTIONS response includes `Access-Control-Allow-Methods`
- [ ] CORS origin `http://evil.com` is rejected (returns no CORS headers or origin not echoed back)

### GATE-07: Correlation ID
- [ ] `curl -s -I "http://localhost9080/api/dragonball/characters?page=1&pageSize=1" 2>&1 | Select-String "X-Correlation-Id"` returns a UUID value
- [ ] The X-Correlation-Id value matches UUID format: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
- [ ] Each request gets a different X-Correlation-Id (run twice, IDs differ)
- [ ] The X-Correlation-Id is also present on OPTIONS (CORS preflight) responses

### Strip Prefix Verification
- [ ] Request to `http://localhost9080/api/dragonball/characters` reaches the API as just `/characters` (verify by checking API logs or by the fact that API routes don't include `/api/dragonball` prefix)

## Verification Commands (Copy-Paste Ready)

```bash
# 1. APISIX is listening
echo "GATE-01: APISIX listening on 8000"
curl -s -o /dev/null -w "Status: %{http_code}\n" http://localhost9080/

# 2. GET dragonball (no auth)
echo "GATE-02: Dragon Ball GET (no auth)"
curl -s -w "\nHTTP %{http_code}\n" "http://localhost9080/api/dragonball/characters?page=1&pageSize=5" | tail -5

# 3. GET music (no auth)
echo "GATE-03: Music GET (no auth)"
curl -s -w "\nHTTP %{http_code}\n" "http://localhost9080/api/music/artists?page=1&pageSize=5" | tail -5

# 4. POST without auth (must return 401)
echo "GATE-04: POST without token (expect 401)"
curl -s -o /dev/null -w "Status: %{http_code}\n" -X POST "http://localhost9080/api/dragonball/characters" \
  -H "Content-Type: application/json" \
  -d '{"name":"Test","ki":"1000","introductionPhase":"Test"}'

# 5. POST with editor token (must return 201/200)
echo "GATE-04/05: POST with editor token"
EDITOR1_TOKEN="eyJ..."  # REPLACE WITH ACTUAL TOKEN
curl -s -w "\nHTTP %{http_code}\n" -X POST "http://localhost9080/api/dragonball/characters" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $EDITOR1_TOKEN" \
  -d '{"name":"TestCharacter","ki":"1000","introductionPhase":"TestPhase"}'

# 6. POST with invalid token (must return 401)
echo "GATE-05: POST with invalid token (expect 401)"
curl -s -o /dev/null -w "Status: %{http_code}\n" -X POST "http://localhost9080/api/dragonball/characters" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer garbage" \
  -d '{"name":"Test","ki":"1000","introductionPhase":"Test"}'

# 7. CORS preflight
echo "GATE-06: CORS preflight"
curl -s -D - -o /dev/null -X OPTIONS "http://localhost9080/api/dragonball/characters" \
  -H "Origin: http://localhost:5173" \
  -H "Access-Control-Request-Method: POST" 2>&1 | Select-String "Access-Control"

# 8. Correlation ID
echo "GATE-07: Correlation ID"
curl -s -D - -o /dev/null "http://localhost9080/api/dragonball/characters?page=1&pageSize=1" 2>&1 | Select-String "X-Correlation-Id"
```

## Troubleshooting

### APISIX returns 503 Service Unavailable
Upstream not reachable. Verify:
1. API projects are running (check Aspire Dashboard)
2. `host.docker.internal` resolves from within the APISIX container
3. Ports 5000 (dragonball) and 5002 (music) are accessible

### APISIX returns 404 on routes
Routes not created. Verify:
1. etcd is running and APISIX can connect
2. Init script executed without errors (check container logs)
3. Admin API is reachable: `docker exec <apisix-container> curl -s http://127.0.0.1:9180/apisix/admin/routes -H 'X-API-KEY: edd1c9f034335f136f87ad84b625c8f1'`

### OIDC returns 401 even with valid token
1. Check Keycloak is running: `curl -s http://localhost:8080/realms/OpenCode/.well-known/openid-configuration`
2. Verify client_secret matches Keycloak client config
3. Verify bearer token is not expired
4. Check APISIX container logs for OIDC plugin errors
5. Verify `discovery` URL uses `keycloak:8080` (container DNS) not `host.docker.internal:8080`

### CORS preflight fails
1. Verify origin is in the allow list (localhost:5173, etc.)
2. If using `allow_credential: true`, verify origins are explicit (not `*`)
3. Check that no .NET CORS middleware is also running (creates double headers)
