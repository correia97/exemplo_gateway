---
phase: 07-react-frontend
fixed_at: 2026-04-30T12:15:00Z
review_path: .planning/phases/07-react-frontend/07-REVIEW.md
iteration: 1
findings_in_scope: 9
fixed: 8
skipped: 1
status: partial
---

# Phase 7: Code Review Fix Report

**Fixed at:** 2026-04-30T12:15:00Z
**Source review:** .planning/phases/07-react-frontend/07-REVIEW.md
**Iteration:** 1

**Summary:**
- Findings in scope: 9 (5 critical + 4 warning)
- Fixed: 8
- Skipped: 1

## Fixed Issues

### CR-02: API Paths Don't Match Kong Routing Configuration

**Files modified:** `src/OpenCode.Frontend/src/api/client.ts`, `src/OpenCode.Frontend/src/api/dragonball.ts`, `src/OpenCode.Frontend/src/api/music.ts`
**Commit:** e2eac68
**Applied fix:**
- Replaced `DRAGONBALL_API_URL` and `MUSIC_API_URL` exports with a single `Kong_URL` export defaulting to `http://localhost:9080`
- Modified `apiFetch` to prepend `Kong_URL` for relative URLs (URLs that don't start with `http`)
- Updated `dragonball.ts` to use relative paths prefixed with `/api/dragonball/`
- Updated `music.ts` to use relative paths prefixed with `/api/music/`

---

### CR-03: WriteGuard Defaults to Non-Existent Role `'write'`

**Files modified:** `src/OpenCode.Frontend/src/auth/WriteGuard.tsx`
**Commit:** f3b3c0b
**Applied fix:**
- Changed default `role` prop from `'write'` (which doesn't exist in Keycloak realm) to `'editor'` (the actual Keycloak write role per AUTH-03)

---

### CR-04: MusicPage Errors Swallowed — Toasts Never Rendered

**Files modified:** `src/OpenCode.Frontend/src/pages/music/MusicPage.tsx`
**Commit:** ff1e0ed
**Applied fix:**
- Destructured `toasts` and `dismissToast` from `useToast()` alongside existing `handleError`
- Added toast rendering JSX (matching DragonBallPage pattern) — fixed top-right notification area with auto-dismiss, correlation ID display, and dismiss buttons

---

### CR-05: No Correlation ID Header Sent on Outgoing Requests

**Files modified:** `src/OpenCode.Frontend/src/api/client.ts`
**Commit:** e1ba2e5
**Applied fix:**
- Added `generateCorrelationId()` helper using `crypto.randomUUID()` with fallback
- Generates a UUID per-request and sends it as `X-Correlation-Id` header
- Renamed variable to `requestCorrelationId` to avoid shadowing the response correlation ID
- Falls back to `requestCorrelationId` if server doesn't echo it back

---

### WR-01: Debug `console.log` Left in Production Code

**Files modified:** `src/OpenCode.Frontend/src/pages/Callback.tsx`
**Commit:** f3c40ae
**Applied fix:**
- Removed `console.log(user)` debug artifact
- Removed unused `user` parameter from `.then()` callback

---

### WR-02: `selectedTrack` Never Updated — `edit-track` View Unreachable

**Files modified:** `src/OpenCode.Frontend/src/pages/music/MusicPage.tsx`
**Commit:** f5e3221
**Applied fix:**
- Removed dead `const [selectedTrack] = useState<Track | null>(null)` state (no setter, never updated)
- Removed unreachable `edit-track` view branch from JSX
- Cleaned up unused `Track` type import
- Note: The `'edit-track'` type variant in the `View` union and its reference in `handleBack` remain for future implementation; the dead code path is eliminated

---

### WR-03: Errors Silently Swallowed in CharacterList and ArtistList

**Files modified:** `src/OpenCode.Frontend/src/pages/dragonball/CharacterList.tsx`, `src/OpenCode.Frontend/src/pages/music/ArtistList.tsx`
**Commit:** b5a0c16
**Applied fix:**
- Added `error` state and `retryCount` state to both components
- Updated catch handlers to set descriptive error messages
- Imported and rendered `ErrorDisplay` component with retry button
- Added `retryCount` to `useEffect` dependency array to trigger re-fetch on retry
- Error display shown before the empty/table state; regular content hidden when in error state

---

### WR-04: Silent Failure on OIDC Token Renewal

**Files modified:** `src/OpenCode.Frontend/src/auth/AuthProvider.tsx`
**Commit:** 3a86b1b
**Applied fix:**
- Added `userManager.signinRedirect()` fallback inside the silent renewal error handler
- If the user's Keycloak session has expired, they are redirected to login instead of remaining in a broken authenticated state

## Skipped Issues

### CR-01: API URL Defaults Bypass Kong (Violates FE-02)

**File:** `src/OpenCode.Frontend/src/api/client.ts:17-27`
**Reason:** Already applied by CR-02 fix — CR-02 replaced `DRAGONBALL_API_URL` and `MUSIC_API_URL` with a single `Kong_URL` defaulting to `http://localhost:9080`, and `apiFetch` now prepends `Kong_URL` for relative URLs. This fully addresses the CR-01 concern about defaults bypassing Kong.
**Original issue:** The fallback URLs for `DRAGONBALL_API_URL` and `MUSIC_API_URL` defaulted to direct API ports (`http://localhost:5000`, `http://localhost:5002`) instead of the Kong proxy (`http://localhost:9080`), violating FE-02.

---

_Fixed: 2026-04-30T12:15:00Z_
_Fixer: the agent (gsd-code-fixer)_
_Iteration: 1_
