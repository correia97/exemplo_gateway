---
phase: 03
slug: api-endpoints-dragon-ball-music-crud
status: draft
nyquist_compliant: false
wave_0_complete: true
created: 2026-04-24
---

# Phase 3 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit (existing in solution) |
| **Config file** | Existing `tests/OpenCode.Domain.Tests/` |
| **Quick run command** | `dotnet test --filter "Category=Unit"` |
| **Full suite command** | `dotnet test` |
| **Estimated runtime** | ~30 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet build` (compile check)
- **After every plan wave:** Run `dotnet test`
- **Before `/gsd-verify-work`:** Full suite must be green
- **Max feedback latency:** 60 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| 03-01-01 | 01 | 1 | DBALL-10, DBALL-11 | T-03-01-01 | DTOs prevent over-posting | compile | `dotnet build` | ❌ W0 | ⬜ pending |
| 03-01-02 | 01 | 1 | DBALL-10, MUSIC-15 | T-03-01-03 | Page size limited to 100 | compile | `dotnet build` | ❌ W0 | ⬜ pending |
| 03-01-03 | 01 | 1 | DBALL-10, DBALL-11 | T-03-01-02 | ProblemDetails hides stack traces | compile | `dotnet build` | ❌ W0 | ⬜ pending |
| 03-02-01 | 02 | 2 | DBALL-05, DBALL-06 | T-03-02-01 | DTOs prevent over-posting | compile | `dotnet build src/OpenCode.DragonBall.Api` | ❌ W0 | ⬜ pending |
| 03-02-02 | 02 | 2 | DBALL-05, DBALL-06, DBALL-07 | T-03-02-03 | Page size capped at 100 | compile | `dotnet build src/OpenCode.DragonBall.Api` | ❌ W0 | ⬜ pending |
| 03-02-03 | 02 | 2 | DBALL-10 | T-03-02-02 | FluentValidation validates input | compile | `dotnet build src/OpenCode.DragonBall.Api` | ❌ W0 | ⬜ pending |
| 03-03-01 | 03 | 2 | MUSIC-09, MUSIC-11 | T-03-03-01 | DTOs prevent over-posting | compile | `dotnet build src/OpenCode.Music.Api` | ❌ W0 | ⬜ pending |
| 03-03-02 | 03 | 2 | MUSIC-09, MUSIC-10, MUSIC-11, MUSIC-12 | T-03-03-02 | Page size capped at 100 | compile | `dotnet build src/OpenCode.Music.Api` | ❌ W0 | ⬜ pending |
| 03-03-03 | 03 | 2 | MUSIC-15 | T-03-03-03 | FluentValidation validates input | compile | `dotnet build src/OpenCode.Music.Api` | ❌ W0 | ⬜ pending |
| 03-04-01 | 04 | 3 | DBALL-08, DBALL-09, MUSIC-13, MUSIC-14 | T-03-04-01 | OpenAPI doc exposed in dev only | compile | `dotnet build` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] Existing infrastructure covers all phase requirements (xUnit + build checks)
- [ ] No new test project needed — compile verification is sufficient for this phase

*Wave 0 complete — no new test infrastructure required.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| GET /api/characters returns paginated data | DBALL-06 | Requires running database | Start solution, curl endpoint |
| POST invalid data returns 400 with ProblemDetails | DBALL-10, DBALL-11 | Requires running application | Start solution, POST junk data |
| Scalar UI renders at /scalar | DBALL-08, MUSIC-13 | Browser visual check | Navigate to /scalar in browser |
| Nested route artists/{id}/albums works | MUSIC-10 | Requires running database | Start solution, curl nested route |

---

## Validation Sign-Off

- [x] All tasks have `<automated>` verify or Wave 0 dependencies
- [x] Sampling continuity: no 3 consecutive tasks without automated verify
- [x] Wave 0 covers all MISSING references
- [x] No watch-mode flags
- [x] Feedback latency < 60s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
