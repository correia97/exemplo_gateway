# 08-02 Summary: Dockerfiles, Health Checks, and Non-Root Users

## Completed: 2026-04-25

## Files Created
- `src/OpenCode.DragonBall.Api/Dockerfile` — multi-stage .NET 10 SDK→runtime, non-root appuser
- `src/OpenCode.Music.Api/Dockerfile` — same pattern as DragonBall
- `src/OpenCode.Frontend/Dockerfile` — multi-stage Node 20 Alpine→Nginx Alpine, non-root nginx user
- `src/OpenCode.Frontend/nginx.conf` — SPA fallback routing, security headers, gzip, asset caching

## Dockerfile Patterns
- **DragonBall/Music APIs**: Restore before source copy (Docker layer caching), `--no-restore` on publish, Ubuntu-based `addgroup`/`adduser` for non-root user
- **Frontend**: `npm ci` for deterministic builds, multi-stage (build artifacts only in runtime), `USER nginx` (built-in nginx:alpine user)

## Health Checks Added
| Service | Test | Interval | Start Period |
|---------|------|----------|-------------|
| PostgreSQL | pg_isready -U postgres -d opencode | 10s | 30s |
| Keycloak | TCP socket test on :8080 | 15s | 60s |
| etcd | etcdctl endpoint health | 15s | 10s |
| Kong | curl Admin API :9180/routes | 10s | 15s |
| DragonBall API | curl :8080/ | 15s | 30s |
| Music API | curl :8080/ | 15s | 30s |
| Frontend | curl :80/ | 15s | 15s |

## Environment
- `depends_on` uses `condition: service_healthy` for postgres-dependent services (keycloak, both APIs, Kong)
- `OTEL_EXPORTER_OTLP_ENDPOINT: http://jaeger:4317` on .NET APIs for Jaeger tracing
- `OTEL_SERVICE_NAME` set on each .NET API for trace identification
