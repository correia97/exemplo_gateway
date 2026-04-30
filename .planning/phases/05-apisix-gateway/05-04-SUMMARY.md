# 05-04: Kong Rate Limiting, CORS & Correlation ID — Summary

## What Was Done

1. **Rate limiting** via `limit-req` plugin:
   - Dragon Ball API: 100 req/min per consumer
   - Music API: 200 req/min per consumer
   - Burst: 20 requests above limit
   - Response code: 429 when exceeded

2. **CORS** via `cors` plugin:
   - Allow origins: `*` (development)
   - Allow methods: GET, POST, PUT, DELETE, OPTIONS
   - Allow headers: Authorization, Content-Type, X-Correlation-ID
   - Expose headers: X-Correlation-ID, X-Request-Id

3. **Correlation ID** via `proxy-rewrite` + `response-rewrite`:
   - Proxy rewrite adds `X-Correlation-ID` header from request (or generates UUID)
   - Response rewrite copies `X-Correlation-ID` from upstream response
   - Frontend route passes through correlation ID

4. **Prometheus metrics** exposed on `/apisix/prometheus/metrics`

5. **Logging**:
   - Access logs include correlation ID, client IP, method, path, status, latency
   - Error logs include full request context

## Verification

- Rate limit exceeded → 429 response with `Retry-After` header
- CORS preflight (OPTIONS) returns correct headers
- Correlation ID flows from request → Kong → backend → response
- Prometheus metrics endpoint returns valid metrics

## Key Findings

- Rate limiting is per-consumer (per authenticated user), not global
- CORS must handle OPTIONS preflight before auth plugins (route priority)
- Correlation ID enables end-to-end request tracing across services
