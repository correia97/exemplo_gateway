---
phase: 07-react-frontend
reviewed: 2026-04-30T12:00:00Z
depth: standard
files_reviewed: 33
files_reviewed_list:
  - src/OpenCode.Frontend/src/App.tsx
  - src/OpenCode.Frontend/src/main.tsx
  - src/OpenCode.Frontend/src/index.css
  - src/OpenCode.Frontend/src/api/client.ts
  - src/OpenCode.Frontend/src/api/dragonball.ts
  - src/OpenCode.Frontend/src/api/music.ts
  - src/OpenCode.Frontend/src/api/types.ts
  - src/OpenCode.Frontend/src/auth/AuthProvider.tsx
  - src/OpenCode.Frontend/src/auth/AuthGuard.tsx
  - src/OpenCode.Frontend/src/auth/ProtectedRoute.tsx
  - src/OpenCode.Frontend/src/auth/WriteGuard.tsx
  - src/OpenCode.Frontend/src/auth/config.ts
  - src/OpenCode.Frontend/src/components/Layout.tsx
  - src/OpenCode.Frontend/src/components/DataTable.tsx
  - src/OpenCode.Frontend/src/components/Pagination.tsx
  - src/OpenCode.Frontend/src/components/EmptyState.tsx
  - src/OpenCode.Frontend/src/components/ErrorBoundary.tsx
  - src/OpenCode.Frontend/src/components/ErrorDisplay.tsx
  - src/OpenCode.Frontend/src/hooks/useApiError.ts
  - src/OpenCode.Frontend/src/pages/Login.tsx
  - src/OpenCode.Frontend/src/pages/Callback.tsx
  - src/OpenCode.Frontend/src/pages/dashboard/Dashboard.tsx
  - src/OpenCode.Frontend/src/pages/dragonball/DragonBallPage.tsx
  - src/OpenCode.Frontend/src/pages/dragonball/CharacterList.tsx
  - src/OpenCode.Frontend/src/pages/dragonball/CharacterDetail.tsx
  - src/OpenCode.Frontend/src/pages/dragonball/CharacterForm.tsx
  - src/OpenCode.Frontend/src/pages/music/MusicPage.tsx
  - src/OpenCode.Frontend/src/pages/music/ArtistList.tsx
  - src/OpenCode.Frontend/src/pages/music/ArtistDetail.tsx
  - src/OpenCode.Frontend/src/pages/music/AlbumDetail.tsx
  - src/OpenCode.Frontend/src/pages/music/MusicForm.tsx
  - src/OpenCode.Frontend/vite.config.ts
  - src/OpenCode.Frontend/tsconfig.app.json
  - src/OpenCode.Frontend/package.json
findings:
  critical: 5
  warning: 4
  info: 7
  total: 16
status: issues_found
---

# Phase 7: Code Review Report — React Frontend

**Reviewed:** 2026-04-30T12:00:00Z
**Depth:** standard
**Files Reviewed:** 33
**Status:** issues_found

## Summary

Reviewed the React 19 + Vite + TypeScript SPA frontend (33 source files) covering API client, OIDC authentication, reusable components, Dragon Ball and Music CRUD pages, form handling, and error management. The codebase is well-structured with clear separation of concerns, proper TypeScript usage, and good use of Tailwind CSS for styling. However, several **critical issues** were found:

1. **API URL defaults bypass Kong** — violating the architectural requirement (FE-02) that the frontend must communicate exclusively through Kong.
2. **WriteGuard defaults to non-existent role `'write'`** — Keycloak realm defines `viewer` and `editor` roles, so all write-action buttons (Edit, Delete, New) are invisible to all users.
3. **MusicPage errors are swallowed** — toasts are stored in state but never rendered, making write failures invisible.
4. **No correlation ID header is sent** on outgoing requests, despite being required by the context document.
5. **API paths don't match Kong routing** — if configured to use Kong, the routes `/api/characters` won't match the defined Kong route `/api/dragonball/*`.

Additional warnings include a `console.log` debug artifact left in Callback.tsx, unreachable `edit-track` view due to never-updated state, and silenced error handling in list components. Several planned architectural patterns (Zod validation, React Query, Toast notification system) are missing or only partially implemented.

---

## Critical Issues

### CR-01: API URL Defaults Bypass Kong (Violates FE-02)

**Files:**
- `src/OpenCode.Frontend/src/api/client.ts:17-27`

**Issue:** The fallback URLs for `DRAGONBALL_API_URL` and `MUSIC_API_URL` default to direct API ports (`http://localhost:5000`, `http://localhost:5002`) instead of the Kong proxy (`http://localhost:9080`). This violates requirement **FE-02**: "Communicates exclusively through Kong (port 8000), never directly to APIs." If environment variables are not set (common in local dev), requests bypass Kong entirely — skipping CORS, auth, and correlation ID features configured at the gateway level.

**Fix:** Change defaults to the Kong proxy URL and add an `Kong_BASE_URL` env var:

```ts
// client.ts
const env = (window as any).__ENV__ || {}

export const Kong_URL = import.meta.env.VITE_Kong_URL as string | undefined
  ?? env.APISIX_URL
  ?? 'http://localhost:9080'

// Remove DRAGONBALL_API_URL and MUSIC_API_URL — use Kong_URL + path routing instead
```

---

### CR-02: API Paths Don't Match Kong Routing Configuration

**Files:**
- `src/OpenCode.Frontend/src/api/dragonball.ts:13` (and all URLs in file)
- `src/OpenCode.Frontend/src/api/music.ts:10` (and all URLs in file)

**Issue:** Kong routes (per GATE-02/GATE-03) are:
- `/api/dragonball/*` → Dragon Ball API
- `/api/music/*` → Music API

But the frontend constructs paths like `${DRAGONBALL_API_URL}/api/characters`, which resolves to `/api/characters` — this **does not match** the Kong route `/api/dragonball/*`. The frontend paths must include the domain prefix expected by Kong.

**Fix:** Update all API paths to include the Kong domain routing prefix:

```ts
// dragonball.ts line 13
const BASE = '/api/dragonball'
return apiFetch<PaginatedResponse<Character>>(`${BASE}/characters${qs ? `?${qs}` : ''}`)

// music.ts line 10
const BASE = '/api/music'
return apiFetch<PaginatedResponse<Artist>>(`${BASE}/artists${qs ? `?${qs}` : ''}`)
```

And in `client.ts`, use a single `Kong_BASE_URL` (e.g., `http://localhost:9080`) as the base:

```ts
// client.ts — base URL for all API calls
const BASE_URL = import.meta.env.VITE_Kong_URL ?? env.APISIX_URL ?? 'http://localhost:9080'
```

---

### CR-03: WriteGuard Defaults to Non-Existent Role `'write'`

**Files:**
- `src/OpenCode.Frontend/src/auth/WriteGuard.tsx:10`

**Issue:** `WriteGuard` defaults to `role = 'write'`, but Keycloak realm `opencode` defines roles `viewer` and `editor` (per AUTH-03). The role `'write'` does not exist in the realm, so **every instance of `<WriteGuard>` without an explicit role prop** will always hide its children from ALL users — including editors. This affects: "New Character" button (CharacterList.tsx:85), "Edit"/"Delete" buttons (CharacterDetail.tsx:20), "New Artist" button (ArtistList.tsx:50), and all write buttons in the Music domain.

**Fix:** Change the default role to match the Keycloak realm role:

```tsx
// WriteGuard.tsx line 10
export default function WriteGuard({ children, role = 'editor', fallback = null }: WriteGuardProps) {
```

---

### CR-04: MusicPage Errors Swallowed — Toasts Never Rendered

**Files:**
- `src/OpenCode.Frontend/src/pages/music/MusicPage.tsx:17` (uses `useToast()`)

**Issue:** `MusicPage` uses `useToast()` (line 17) and calls `handleError(e)` on failures, which adds toasts to internal state. However, **the `toasts` array is never rendered** in the JSX — none of the toast rendering logic that exists in `DragonBallPage.tsx` (lines 63-76) is present in `MusicPage.tsx`. All write error notifications are invisible to the user.

**Fix:** Add toast rendering to the MusicPage JSX (same pattern as DragonBallPage):

```tsx
// MusicPage.tsx — add near the return statement
const { toasts, handleError, dismissToast, success } = useToast()

return (
  <div>
    {toasts.length > 0 && (
      <div className="fixed top-4 right-4 z-50 flex flex-col gap-2">
        {toasts.map(t => (
          <div key={t.id} className="px-4 py-3 rounded shadow-lg text-white bg-red-600 flex items-center gap-3 min-w-[300px]">
            <span className="flex-1 text-sm">{t.message}</span>
            {t.correlationId && <span className="text-xs text-red-200 font-mono">{t.correlationId}</span>}
            <button onClick={() => dismissToast(t.id)} className="text-white/80 hover:text-white text-lg leading-none">&times;</button>
          </div>
        ))}
      </div>
    )}
    ...
  </div>
)
```

---

### CR-05: No Correlation ID Header Sent on Outgoing Requests

**Files:**
- `src/OpenCode.Frontend/src/api/client.ts:29-51`

**Issue:** The context document states: "Custom `X-Correlation-Id` header on every request (generated per-request UUID)". However, the `apiFetch` function does not generate or attach a correlation ID to outgoing requests. It only reads `X-Correlation-Id` from the response (line 41). Without sending a correlation ID, the distributed tracing flow (OTEL-04) cannot correlate browser requests with server-side spans.

**Fix:** Generate and attach a UUID correlation ID header on every request:

```ts
// client.ts — add at top
function generateCorrelationId(): string {
  return crypto.randomUUID?.() ?? `${Date.now()}-${Math.random().toString(36).slice(2, 10)}`
}

// Inside apiFetch, before making the request:
const correlationId = generateCorrelationId()
const headers: Record<string, string> = {
  'Content-Type': 'application/json',
  'X-Correlation-Id': correlationId,
  ...(options.headers as Record<string, string>),
}
```

---

## Warnings

### WR-01: Debug `console.log` Left in Production Code

**File:** `src/OpenCode.Frontend/src/pages/Callback.tsx:13`

**Issue:** `console.log(user)` is left in the OIDC callback handler. This leaks user/token information to the browser console in production.

**Fix:** Remove the `console.log` call:

```tsx
// Callback.tsx lines 11-14 — change from:
userManager.signinRedirectCallback()
  .then((user) => {
    console.log(user);
    navigate('/', { replace: true })})
// To:
userManager.signinRedirectCallback()
  .then(() => navigate('/', { replace: true }))
```

---

### WR-02: `selectedTrack` Never Updated — `edit-track` View Unreachable

**File:** `src/OpenCode.Frontend/src/pages/music/MusicPage.tsx:16`

**Issue:** The state `selectedTrack` is initialized to `null` on line 16 without a setter function (`const [selectedTrack] = useState<Track | null>(null)`). The variable is never updated anywhere in the file. The `edit-track` view at line 118-119 requires `selectedTrack` to be truthy, so this view branch can never render. Combined with the fact that no code sets view to `'edit-track'`, this is a dead code path.

**Fix:** Either implement track selection (similar to `handleSelectAlbum`/`handleSelectArtist`) or remove the `edit-track` view and `selectedTrack` state:

```tsx
// Either add a handleSelectTrack callback:
const handleSelectTrack = useCallback((track: Track) => {
  setSelectedTrack(track)
  setView('edit-track')
}, [])

// Or remove the dead code if edit-track is not needed:
const [selectedTrack] = useState<Track | null>(null)  // <-- remove this line
// And remove the edit-track view section (lines 118-120)
```

---

### WR-03: Errors Silently Swallowed in CharacterList and ArtistList

**Files:**
- `src/OpenCode.Frontend/src/pages/dragonball/CharacterList.tsx:39-43`
- `src/OpenCode.Frontend/src/pages/music/ArtistList.tsx:31-35`

**Issue:** The `.catch()` handlers in both `CharacterList` and `ArtistList` silently ignore errors:

```tsx
.catch(() => {
    if (id !== fetchId.current) return
    setCharacters([])
    setIsLoading(false)
})
```

When a fetch fails (network error, server error), the user sees an empty list with no error message, loading state, or retry option. This is a poor UX — the user cannot distinguish between "no data exists" and "failed to load data."

**Fix:** Add error state and display:

```tsx
// Add state:
const [error, setError] = useState<string | null>(null)

// In catch handler:
.catch((err) => {
    if (id !== fetchId.current) return
    setError(err instanceof Error ? err.message : 'Failed to load characters')
    setCharacters([])
    setIsLoading(false)
})

// In render, before the empty check:
if (error) return <ErrorDisplay message={error} onRetry={() => { setError(null); /* retrigger fetch */ }} />
```

---

### WR-04: Silent Failure on OIDC Token Renewal

**File:** `src/OpenCode.Frontend/src/auth/AuthProvider.tsx:33-34`

**Issue:** The `onAccessTokenExpired` handler silently swallows silent sign-in failures:

```tsx
const onAccessTokenExpired = () => {
    userManager.signinSilent().catch(() => {})
}
```

If the user's Keycloak session has expired and silent renewal fails, there's no fallback. The user remains in an "authenticated" state in the UI but all write API calls will fail with 401. The app should either redirect to login or show a notification.

**Fix:** Add error handling to the silent renew failure:

```tsx
const onAccessTokenExpired = () => {
    userManager.signinSilent().catch(() => {
      // Silent renewal failed — user session likely expired
      // Option 1: Redirect to login
      userManager.signinRedirect()
      // Option 2: Log the user out gracefully
      // userManager.signoutRedirect()
    })
}
```

---

## Info Items

### IN-01: `ProtectedRoute` Component is Dead Code (Never Used)

**File:** `src/OpenCode.Frontend/src/auth/ProtectedRoute.tsx`

**Issue:** `ProtectedRoute` is defined but never imported or used anywhere in the codebase. Route-level authentication is handled via `WriteGuard` at the component level instead. This is unused code that should either be removed or actually wired into the route definitions in `App.tsx`.

**Fix:** Either remove the file or use it to guard protected routes in `App.tsx`:

```tsx
// App.tsx — example usage:
<Route path="/dragonball/*" element={<ProtectedRoute><DragonBallPage /></ProtectedRoute>} />
```

---

### IN-02: Duplicate Hook Implementations (`useApiError` vs `useToast`)

**File:** `src/OpenCode.Frontend/src/hooks/useApiError.ts`

**Issue:** Two very similar hooks are exported from the same file:
- `useApiError()` — `showError(message, correlationId?)`, `handleError`, `dismissToast` (5s auto-dismiss, no success)
- `useToast()` — `showError(err)`, `handleError`, `dismissToast`, `success` (3s auto-dismiss success)

The duplicate logic adds maintenance burden (two places to fix bugs) and inconsistent behavior across pages (DragonBallPage uses different error handling than MusicPage). Their `showError` functions have different signatures, making them non-interchangeable.

**Fix:** Consolidate into a single hook:

```ts
export function useToast() {
  const [toasts, setToasts] = useState<Toast[]>([])

  const showError = useCallback((message: string, correlationId?: string) => {
    const id = nextId++
    setToasts(prev => [...prev, { id, message, correlationId, type: 'error' }])
    setTimeout(() => setToasts(prev => prev.filter(t => t.id !== id)), 5000)
  }, [])

  const handleError = useCallback((err: unknown) => {
    const { message, correlationId } = extractError(err)
    showError(message, correlationId)
  }, [showError])

  const success = useCallback((msg: string) => {
    const id = nextId++
    setToasts(prev => [...prev, { id, message: msg, type: 'success' }])
    setTimeout(() => setToasts(prev => prev.filter(t => t.id !== id)), 3000)
  }, [])

  const dismissToast = useCallback((id: number) => {
    setToasts(prev => prev.filter(t => t.id !== id))
  }, [])

  return { toasts, showError, handleError, success, dismissToast }
}
```

Then replace `useApiError` with `useToast` in all consumers and remove the duplicate export.

---

### IN-03: Missing Zod Validation in Forms

**Files:**
- `src/OpenCode.Frontend/src/pages/dragonball/CharacterForm.tsx`
- `src/OpenCode.Frontend/src/pages/music/MusicForm.tsx`

**Issue:** The planned architecture described "admin CRUD modals with Zod validation" and the review instructions mention "Form validation (Zod schemas)." However, no Zod dependency is declared in `package.json`, no Zod schemas exist, and both forms rely only on basic HTML5 `required` attributes and a manual trim check (`if (!name.trim() || !race.trim() || !ki.trim()) return`). This provides no structured validation, no type-safe error messages, and no client-side schema enforcement matching the backend FluentValidation rules.

**Fix:** Add Zod dependency and schemas:

```json
// package.json
"zod": "^3.24.0"
```

```tsx
// CharacterForm.tsx
import { z } from 'zod'

const characterSchema = z.object({
  name: z.string().min(1, 'Name is required').max(100),
  race: z.string().min(1, 'Race is required'),
  ki: z.string().min(1, 'Ki is required'),
  maxKi: z.string().optional(),
  description: z.string().optional(),
  planetId: z.number().int().positive().optional(),
})

// In handleSubmit, replace manual checks:
const result = characterSchema.safeParse({ name, race, ki, maxKi, description, planetId })
if (!result.success) {
  setErrors(result.error.flatten().fieldErrors)
  return
}
await onSubmit(result.data)
```

---

### IN-04: Missing React Query (TanStack Query)

**File:** `src/OpenCode.Frontend/package.json`

**Issue:** The review instructions mention "React Query usage patterns" but the dependency is not declared. All data fetching is done manually with `useState` + `useEffect` + stale-avoidance patterns (`useRef` counter). This is the older approach that lacks automatic caching, deduplication, background refetching, and loading/error state management that React Query provides. The manual `fetchId` pattern (used in CharacterList and ArtistList) is fragile and adds boilerplate.

**Fix (optional enhancement):** Add `@tanstack/react-query` and refactor data fetching:

```json
"@tanstack/react-query": "^5.60.0"
```

```tsx
// Example in DragonBallPage — replace manual fetch with useQuery
import { useQuery } from '@tanstack/react-query'

// The query key structure could be: ['characters', { page, search, race }]
```

---

### IN-05: MusicForm Initial Value Pattern is Fragile

**File:** `src/OpenCode.Frontend/src/pages/music/MusicForm.tsx:24-33`

**Issue:** Initial values are set on every render inside the component body (not in `useEffect` or `useState` initializer):

```tsx
if (initial) {
    if ('name' in initial && name === '') setName((initial as Artist).name || '')
    if ('title' in initial && title === '') setTitle((initial as Album | Track).title || '')
    // ...
}
```

This runs on every render and depends on `name === ''` to prevent re-setting. If the user clears a field and the `initial` prop changes, the field won't update because the condition checks `name === ''`. A `useEffect` with `initial` as dependency or `useState` initializer functions would be more robust.

**Fix:** Use `useEffect` for initial value population:

```tsx
useEffect(() => {
  if (!initial) return
  if ('name' in initial) setName((initial as Artist).name || '')
  if ('title' in initial) setTitle((initial as Album | Track).title || '')
  if ('genre' in initial) setGenre((initial as Artist | Album).genre || '')
  if ('biography' in initial) setBiography((initial as Artist).biography || '')
  if ('releaseYear' in initial) setReleaseYear(String((initial as Album).releaseYear ?? ''))
  if ('duration' in initial) setDuration(String((initial as Track).duration ?? ''))
  if ('trackNumber' in initial) setTrackNumber(String((initial as Track).trackNumber ?? ''))
  if ('lyrics' in initial) setLyrics((initial as Track).lyrics || '')
}, [initial])
```

---

### IN-06: Toast Dismiss `×` Button Missing `aria-label`

**Files:**
- `src/OpenCode.Frontend/src/pages/dragonball/DragonBallPage.tsx:72`
- `src/OpenCode.Frontend/src/components/ErrorDisplay.tsx:30`

**Issue:** The toast dismiss buttons use `×` as visible content (`&times;`) without an `aria-label`. Screen readers will announce "times" or the raw entity character, which is not descriptive. This is an accessibility concern.

**Fix:** Add `aria-label` to dismiss buttons:

```tsx
// DragonBallPage.tsx line 72
<button onClick={() => dismissToast(t.id)} aria-label="Close notification" className="...">&times;</button>

// ErrorDisplay.tsx line 30
<button onClick={onDismiss} aria-label="Dismiss error" className="...">&times;</button>
```

---

### IN-07: `noUnusedLocals`/`noUnusedParameters` Without `strict: true`

**File:** `src/OpenCode.Frontend/tsconfig.app.json`

**Issue:** The tsconfig enables `noUnusedLocals` and `noUnusedParameters` but does not enable `"strict": true`. Without strict mode, several type-safety features are disabled: `strictNullChecks`, `noImplicitAny`, `strictFunctionTypes`, etc. This means potential null-reference errors and implicit `any` types won't be caught at compile time.

**Fix:** Enable strict mode:

```json
{
  "compilerOptions": {
    "strict": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "erasableSyntaxOnly": true,
    "noFallthroughCasesInSwitch": true
  }
}
```

---

_Reviewed: 2026-04-30T12:00:00Z_
_Reviewer: gsd-code-reviewer (deepseek-reasoner)_
_Depth: standard_
