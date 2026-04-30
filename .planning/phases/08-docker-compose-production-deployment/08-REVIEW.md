---
phase: 08-docker-compose-production-deployment
reviewed: 2026-04-30T12:00:00Z
depth: standard
files_reviewed: 5
files_reviewed_list:
  - docker-compose.yml
  - src/OpenCode.DragonBall.Api/Dockerfile
  - src/OpenCode.Music.Api/Dockerfile
  - src/OpenCode.Frontend/Dockerfile
  - src/OpenCode.Frontend/nginx.conf
findings:
  critical: 3
  warning: 5
  info: 6
  total: 14
status: issues_found
---

# Phase 08: Code Review Report — Docker Compose Production Deployment

**Reviewed:** 2026-04-30T12:00:00Z
**Depth:** standard
**Files Reviewed:** 5
**Status:** issues_found

## Summary

Reviewed the Docker Compose production deployment configuration including `docker-compose.yml`, three Dockerfiles (DragonBall API, Music API, and Frontend), and the Nginx SPA configuration. The setup follows a reasonable multi-service architecture (Postgres, Keycloak, Kong gateway, two .NET APIs, Jaeger tracing, and two frontend apps) but contains several production-critical issues:

1. **The Kong route initialization (`busybox` service) will never run** due to an incorrect `depends_on` condition — this blocks all API routing.
2. **API health checks are broken** — they target the root URL `/` instead of the correct `/health` endpoint, and neither API actually registers the health endpoint.
3. **Frontend containers reference `localhost`** for API/Keycloak URLs instead of Docker service names, causing connection failures inside the container network.
4. **No non-root user** in the .NET API Dockerfiles, violating production security hardening.
5. **Unpinned image tags** (`:latest`) for Keycloak and Jaeger.
6. Several other warnings around secret handling, missing security headers, and commented-out restart policies.

---

## Critical Issues

### CR-01: Kong route initialization service never starts (blocking bug)

**File:** `docker-compose.yml:118-120`

**Issue:** The `busybox` service has `depends_on: { gateway: { condition: service_completed_successfully } }`. Since `gateway` runs the Kong proxy — a long-lived process that never exits — `service_completed_successfully` will **never** be satisfied. The `init-routes.sh` script that configures Kong routes and CORS plugins will therefore never execute. This renders the entire Kong gateway misconfigured: no services, no routes, and no CORS plugins will be registered at startup.

**Fix:** Change to `service_healthy` so the init container runs once the Kong gateway is healthy and ready to accept Admin API calls:

```yaml
    depends_on:
      gateway:
        condition: service_healthy
```

---

### CR-02: API container health checks target wrong endpoint and the health endpoint is not registered

**Files:**
- `docker-compose.yml:145,171`
- `src/OpenCode.DragonBall.Api/Program.cs` (missing `MapDefaultEndpoints()` call)
- `src/OpenCode.Music.Api/Program.cs` (missing `MapDefaultEndpoints()` call)

**Issue:** Two separate problems compound:

1. The health check command runs `curl http://localhost:8080/` (root path), but neither API has a route registered at `/`. The ASP.NET Core default for unmatched routes is **404 Not Found**, so `curl -w "%{http_code}"` returns `404`, and the health check always fails.

2. The health endpoint is defined in `Extensions.cs:49` as `app.MapGet("/health", ...)` inside `MapDefaultEndpoints()`, but **neither `Program.cs` calls `app.MapDefaultEndpoints()`**. So even if the compose file were changed to target `/health`, the endpoint wouldn't exist.

**Fix:**

In both `Program.cs` files, add the `MapDefaultEndpoints()` call (typically after `app.Build()`):

```csharp
var app = builder.Build();

app.UseCorrelationId();
app.MapDefaultEndpoints();  // <-- Add this
app.UseAuthentication();
// ... rest of middleware
```

Then fix the health check in `docker-compose.yml` to target the correct path:

```yaml
    healthcheck:
      test: ["CMD", "curl", "-s", "-o", "/dev/null", "-w", "%{http_code}", "http://localhost:8080/health"]
      interval: 15s
      timeout: 5s
      retries: 3
      start_period: 30s
```

---

### CR-03: Frontend environment URLs use `localhost` inside containers (connection failure)

**File:** `docker-compose.yml:200-202,224-226`

**Issue:** Both `frontend` and `angular-frontend` services set environment variables using `http://localhost:5000`, `http://localhost:5002`, and `http://localhost:8080`. Inside a Docker container, `localhost` refers to the **container itself**, not the host machine or other containers. These will be rendered into `env-config.js` by the `docker-entrypoint.sh` script, and the frontend will attempt to reach APIs on its own loopback interface — which will fail.

**Fix:** Use Docker Compose service names for inter-container communication:

```yaml
    environment:
      DRAGONBALL_API_URL: http://gateway:8000/api/dragonball
      MUSIC_API_URL: http://gateway:8000/api/music
      KEYCLOAK_URL: http://keycloak:8080
```

(And the same for `angular-frontend`.)

---

## Warnings

### WR-01: Unpinned container image tags (`:latest`)

**File:** `docker-compose.yml:26,178`

**Issue:** Keycloak (`quay.io/keycloak/keycloak:latest`) and Jaeger (`jaegertracing/all-in-one:latest`) use the `:latest` tag. In a production deployment context, `:latest` is non-deterministic — the exact image can change with any rebuild, leading to untested version upgrades, unexpected breaking changes, and inability to reproduce a known-good state.

**Fix:** Pin to specific versions:
```yaml
    image: quay.io/keycloak/keycloak:26.1.7    # or current stable
    ...
    image: jaegertracing/all-in-one:1.64.0     # or current stable
```

---

### WR-02: No non-root user in .NET API runtime images

**Files:**
- `src/OpenCode.DragonBall.Api/Dockerfile:19-28`
- `src/OpenCode.Music.Api/Dockerfile:16-25`

**Issue:** Both runtime stages use the `mcr.microsoft.com/dotnet/aspnet:10.0` base image and run as **root**. Running container processes as root is a security risk — if the application is compromised, an attacker gains full root access within the container. Production containers should run with the least privilege necessary.

**Fix:** Create and switch to a non-root user in the runtime stage:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser

COPY --from=build /app .
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "OpenCode.DragonBall.Api.dll"]
```

---

### WR-03: Kong Admin API port exposed to host

**File:** `docker-compose.yml:102`

**Issue:** Port `8001` (Kong Admin API) is mapped to the host in `ports`. The Admin API provides full control over Kong's configuration — services, routes, plugins, certificates — with no authentication by default. Exposing it to the host network creates a significant security risk, especially in production.

**Fix:** Remove the host port mapping for the Admin API. Inter-container discovery via the `opencode-net` network is sufficient:

```yaml
    ports:
      - "8000:8000"
      # Remove "8001:8001" — Admin API accessible only within the Docker network
```

If external admin access is needed, bind to a specific IP or add Kong's authentication plugin.

---

### WR-04: Postgres port exposed to host

**File:** `docker-compose.yml:14`

**Issue:** Port `5432` is exposed to the host with `"5432:5432"`. In production, the database should not be directly accessible from outside the Docker network. Direct database access bypasses the API layer, authentication, and auditing.

**Fix:** Remove the port mapping or restrict to a specific management IP:
```yaml
    # ports:
    #   - "5432:5432"    # Remove or bind to internal IP only
```

---

### WR-05: Missing security headers in nginx

**File:** `src/OpenCode.Frontend/nginx.conf:22-24`

**Issue:** The configuration includes `X-Content-Type-Options`, `X-Frame-Options`, and `X-XSS-Protection` but is missing several important security headers for a production SPA:

- **`Strict-Transport-Security`** (HSTS) — enforces HTTPS connections
- **`Content-Security-Policy`** — mitigates XSS and data injection
- **`Referrer-Policy`** — controls referrer information leakage
- **`Permissions-Policy`** — restricts browser API access

**Fix:** Add the following headers:

```nginx
add_header Strict-Transport-Security "max-age=31536000; includeSubDomains; preload" always;
add_header Content-Security-Policy "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; connect-src 'self' http://localhost:5000 http://localhost:5002 http://localhost:8080; img-src 'self' data:; font-src 'self'" always;
add_header Referrer-Policy "strict-origin-when-cross-origin" always;
add_header Permissions-Policy "camera=(), microphone=(), geolocation=()" always;
```

(Update the CSP `connect-src` with actual allowed origins when URLs are corrected per CR-03.)

---

## Info

### IN-01: All `restart` policies are commented out

**File:** `docker-compose.yml` (lines 17, 50, 79, 105, 143, 169, 188, 206, 230)

**Issue:** Every `restart` directive is commented out (e.g., `# restart: unless-stopped`). In production, services should automatically restart on failure to ensure uptime. Without a restart policy, a crashed container stays stopped until manually restarted.

**Fix:** Uncomment the appropriate restart policies. Recommended:
- Stateful services (postgres, keycloak): `restart: unless-stopped`
- Stateless APIs (dragonball-api, music-api, frontend): `restart: unless-stopped`
- Init containers (kong-init-routes, busybox): `restart: "no"` (already correct)

---

### IN-02: `.NET API containers missing timezone environment variable`

**File:** `docker-compose.yml:140` (dragonball-api)

**Issue:** The `dragonball-api` service doesn't set `TZ` while `music-api` (line 166) and other services do. This inconsistency won't cause a functional bug but should be consistent for log timestamp alignment.

**Fix:** Add to dragonball-api:

```yaml
    environment:
      # ... existing variables
      TZ: America/Sao_Paulo
```

---

### IN-03: `Useless UseHttpsRedirection()` in container environment

**Files:**
- `src/OpenCode.DragonBall.Api/Program.cs:91`
- `src/OpenCode.Music.Api/Program.cs:92`

**Issue:** Both APIs call `app.UseHttpsRedirection()` while listening on HTTP only (`ASPNETCORE_URLS=http://+:8080`). This middleware redirects HTTP requests to HTTPS, but since no HTTPS endpoint is configured, the redirect target (`https://localhost:8080/...`) will fail. This is a no-op that adds confusion. TLS termination should be handled by the gateway (Kong), not the application.

**Fix:** Remove both `app.UseHttpsRedirection()` calls from `Program.cs` files, or wrap in an `if (!app.Environment.IsProduction())` guard.

---

### IN-04: `.NET API Dockerfiles set `ASPNETCORE_ENVIRONMENT` twice`

**Files:**
- `src/OpenCode.DragonBall.Api/Dockerfile:26`
- `src/OpenCode.Music.Api/Dockerfile:23`
- `docker-compose.yml:138,163`

**Issue:** `ASPNETCORE_ENVIRONMENT=Production` is set both in the Dockerfile as an `ENV` and in the Compose file as an `environment` variable. The Compose value wins at runtime, making the Dockerfile setting redundant. Similarly, `ASPNETCORE_URLS` is set in both places.

**Fix:** Remove the duplicate `ENV` lines from the Dockerfiles (environment-specific configuration belongs in the Compose/Orchestration layer, not the image):

```dockerfile
# Remove these from Dockerfile:
# ENV ASPNETCORE_URLS=http://+:8080
# ENV ASPNETCORE_ENVIRONMENT=Production
```

---

### IN-05: Frontend nginx:alpine image tag not pinned

**File:** `src/OpenCode.Frontend/Dockerfile:10`

**Issue:** `FROM nginx:alpine` uses the `alpine` floating tag which tracks the latest stable nginx release. While less risky than `:latest` (alpine is an alias for the latest stable), it's still non-deterministic across rebuilds.

**Fix:** Pin to a specific version:
```dockerfile
FROM nginx:1.27-alpine AS runtime
```

---

### IN-06: `rootpublic/curl` image is not an official image — security concern

**File:** `docker-compose.yml:114`

**Issue:** The `busybox` service uses `rootpublic/curl:bookworm-slim_rootio`, which is a community-maintained image on Docker Hub. Using unverified third-party images introduces supply chain risk — the image could be outdated, contain vulnerabilities, or (in a worst case) be malicious.

**Fix:** Use an official base image instead, such as `alpine:3.21` with curl installed via `apk`, or `curlimages/curl:latest`:

```yaml
  kong-init-routes:
    image: alpine:3.21
    container_name: kong-init-routes
    entrypoint: ["/bin/sh"]
    command: ["/init-routes.sh"]
    volumes:
      - ./deploy/kong/init-routes.sh:/init-routes.sh:ro
```

(The `init-routes.sh` script would need a `RUN apk add --no-cache curl` in a custom Dockerfile or the base image must include curl.)

---

_Reviewed: 2026-04-30T12:00:00Z_
_Reviewer: gsd-code-reviewer_
_Depth: standard_
