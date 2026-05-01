---
phase: 14
slug: api-developer-portal-with-backstage-io
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-05-01
---

# Phase 14 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | Manual smoke tests (no automated test suite for Backstage portal) |
| **Config file** | none — portal validation is runtime/integration-based |
| **Quick run command** | `curl -sf http://localhost:7007/api/health` |
| **Full suite command** | `curl -sf http://localhost:7007/api/catalog/entities` |
| **Estimated runtime** | ~10 seconds |

---

## Sampling Rate

- **After every task commit:** Run `curl -sf http://localhost:7007/api/health`
- **After every plan wave:** Run full catalog + auth smoke test
- **Before `/gsd-verify-work`:** All manual verifications must pass
- **Max feedback latency:** 60 seconds (Backstage startup time)

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| 14-01-01 | 01 | 1 | D-01 | — | Image builds without error | manual | `docker images backstage:latest` | ❌ W0 | ⬜ pending |
| 14-01-02 | 01 | 1 | D-09 | — | OIDC login redirects to Keycloak | manual | `curl -sf http://localhost:7007/api/auth/oidc/start` | ❌ W0 | ⬜ pending |
| 14-02-01 | 02 | 1 | D-04 | — | Catalog entities loaded | manual | `curl -sf http://localhost:7007/api/catalog/entities` | ❌ W0 | ⬜ pending |
| 14-02-02 | 02 | 1 | D-06 | — | catalog-info.yaml accessible in container | manual | `docker exec backstage ls /backstage/catalog/` | ❌ W0 | ⬜ pending |
| 14-03-01 | 03 | 2 | D-02 | — | Aspire starts backstage container on port 7007 | manual | `curl -sf http://localhost:7007/api/health` | ❌ W0 | ⬜ pending |
| 14-03-02 | 03 | 2 | D-02 | — | Docker Compose backstage healthcheck passes | manual | `docker inspect backstage --format={{.State.Health.Status}}` | ❌ W0 | ⬜ pending |
| 14-04-01 | 04 | 2 | D-12 | — | Credentials how-to page visible in portal | manual | Visit http://localhost:7007 and navigate to the static page | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- No automated test framework install needed — Backstage is a pre-existing Node.js app
- Validation is primarily manual smoke tests and Docker health checks
- The `yarn build-image` script must succeed as a prerequisite (Wave 1)

*Existing infrastructure covers automated unit testing; this phase is integration/runtime validation only.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Keycloak OIDC login flow | D-09, D-10, D-11 | Requires browser interaction with Keycloak UI | Open http://localhost:7007, verify redirect to Keycloak login page, login with test user, verify redirect back to Backstage portal |
| Catalog entities visible | D-04, D-05, D-06, D-07 | Requires Backstage UI to render catalog | After login, navigate to Catalog in Backstage UI, verify DragonBall and Music API entities appear with OpenAPI spec viewer |
| OpenAPI spec renders in portal | D-05 | Requires live API running and accessible from Backstage | Click on dragonball-api or music-api entity, verify OpenAPI spec loads in the portal's API viewer |
| Credentials how-to page | D-12 | UI page content | Navigate to credentials guide page, verify it contains step-by-step Keycloak client creation instructions |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 60s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
