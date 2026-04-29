# Phase 7: React Frontend — Research

## Objective

Determine the technical approach and patterns for building a React 19 + Vite SPA that:
1. Authenticates via Keycloak OIDC (Authorization Code + PKCE)
2. Communicates exclusively through APISIX gateway
3. Provides role-aware CRUD UIs for both Dragon Ball and Music APIs

## Stack Selection

### React 19 (stable, Dec 2024)
- **npm**: `react@19`, `react-dom@19`
- **Key features**: Actions for form submission, `use()` for promise consumption, native `<title>`/`<meta>` support, `ref` as prop (no `forwardRef` needed), `useActionState` for form state, `useOptimistic` for optimistic updates
- **Why not React 18**: React 19 is the current stable line; no reason to use older
- **No SSR**: Pure client-side SPA — no Next.js or Remix needed for PoC scope

### Vite 6 (latest)
- **npm**: `vite@6`, `@vitejs/plugin-react@4`
- **Why Vite**: Fastest dev server, native ESM, simple config, React SPA template available
- **Dev server**: Port 5173 (default), enables HMR during development
- **Proxy config**: Not needed — frontend calls APISIX directly on port 8000

### oidc-client-ts v3.x
- **npm**: `oidc-client-ts@3`
- **Purpose**: OIDC/OAuth2 protocol support for browser SPAs
- **Why this over Keycloak JS adapter**:
  - Community-standard, framework-agnostic OIDC library
  - Actively maintained (fork of IdentityModel/oidc-client-js)
  - Same API works with any OIDC provider (not Keycloak-specific)
  - Supports Authorization Code + PKCE (OAuth 2.1 compliant)
  - Supports refresh tokens, silent renew, and session management
- **Flow**: Authorization Code Grant with PKCE (S256) — redirect browser to Keycloak, receive auth code, exchange for tokens

### react-router-dom v7
- **npm**: `react-router-dom@7`
- **Purpose**: Client-side routing with protected/guest route components
- **Why v7**: Current version, works with React 19, has loaders/actions pattern
- **Alternative**: TanStack Router — more features but heavier for PoC

### Additional Dependencies
| Package | Purpose | Why |
|---------|---------|-----|
| `@types/react` | TypeScript types | Required for React+TS |
| `@types/react-dom` | TypeScript types | Required for React+TS |
| `typescript` | Type checking | Vite React-TS template includes |
| `react-router-dom` | Client-side routing | Navigation between DB/Music sections |
| `oidc-client-ts` | OIDC PKCE flow | Keycloak authentication |

## Architecture

### Project Structure
```
src/OpenCode.Frontend/
├── index.html
├── package.json
├── tsconfig.json
├── tsconfig.app.json
├── vite.config.ts
├── public/
│   └── favicon.svg
├── src/
│   ├── main.tsx                 # App entry point, AuthProvider wrapper
│   ├── App.tsx                  # Layout shell (nav, routes, auth check)
│   ├── index.css                # Global styles
│   ├── vite-env.d.ts
│   ├── auth/
│   │   ├── config.ts            # OIDC client configuration
│   │   ├── AuthProvider.tsx      # React Context for auth state
│   │   └── ProtectedRoute.tsx   # Route guard component
│   ├── api/
│   │   ├── client.ts            # Fetch wrapper (base URL, auth headers, error handling)
│   │   └── types.ts             # API response types (shared)
│   ├── pages/
│   │   ├── Login.tsx            # Login initiation page
│   │   ├── Callback.tsx         # OIDC redirect handler
│   │   ├── dashboard/
│   │   │   └── Dashboard.tsx    # Post-login landing
│   │   ├── dragonball/
│   │   │   ├── CharacterList.tsx
│   │   │   ├── CharacterDetail.tsx
│   │   │   ├── CharacterForm.tsx
│   │   │   └── index.ts
│   │   └── music/
│   │       ├── ArtistList.tsx
│   │       ├── ArtistDetail.tsx
│   │       ├── AlbumDetail.tsx
│   │       ├── ArtistForm.tsx
│   │       ├── AlbumForm.tsx
│   │       ├── TrackForm.tsx
│   │       └── index.ts
│   └── components/
│       ├── Layout.tsx            # App shell with nav
│       ├── Pagination.tsx        # Reusable paginator
│       ├── ErrorDisplay.tsx      # Error + correlation ID display
│       └── LoadingSpinner.tsx
```

### Data Flow
```
Browser ──GET/POST/PUT/DELETE──→ APISIX (9080) ──→ .NET API ──→ PostgreSQL
                │                       ↑
                │                  (strips prefix,
                │                   adds CORS,
                │                   validates JWT on writes)
                │
        oidc-client-ts
                │
                ↓
           Keycloak (:8080)
         (Authorization Code + PKCE)
```

### Authentication Flow
1. User visits app → check OIDC `UserManager` for existing session
2. If no session → show Login page with "Login" button
3. Click Login → `userManager.signinRedirect()` → browser redirects to Keycloak login
4. User authenticates in Keycloak → redirect back to `http://localhost:5173/callback`
5. Callback page → `userManager.signinRedirectCallback()` extracts tokens
6. App redirects to dashboard
7. All API calls include `Authorization: Bearer <access_token>` header when authenticated
8. Token refresh: `userManager.events.addAccessTokenExpiring()` triggers silent refresh

### OIDC Configuration
```typescript
const settings: OidcClientSettings = {
  authority: "http://localhost:8080/realms/OpenCode",
  client_id: "frontend",
  redirect_uri: "http://localhost:5173/callback",
  post_logout_redirect_uri: "http://localhost:5173",
  response_type: "code",        // Authorization Code
  scope: "openid profile email roles",
  loadUserInfo: true,
  automaticSilentRenew: true,
};
```
- Public client (no client_secret) — PKCE handles security
- Keycloak realm MUST have `frontend` client configured with:
  - `public` (confidential=false) access type
  - `Standard Flow` enabled (Authorization Code)
  - Valid redirect URIs: `http://localhost:5173/*`
  - Valid post logout redirect URIs: `http://localhost:5173`
  - Web origins: `http://localhost:5173` (for CORS at Keycloak level)

### API Client Pattern
```typescript
const API_BASE = "http://localhost9080";

async function fetchWithAuth<T>(path: string, options?: RequestInit): Promise<T> {
  const user = await getUser(); // from OIDC user manager
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(user?.access_token ? { Authorization: `Bearer ${user.access_token}` } : {}),
  };

  const response = await fetch(`${API_BASE}${path}`, { ...options, headers });
  const correlationId = response.headers.get("X-Correlation-Id");

  if (!response.ok) {
    const error = await response.json();
    throw { status: response.status, ...error, correlationId };
  }

  return response.json();
}
```

### Paginated Response Type
```typescript
interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
```

## Keycloak Integration Points

From Phase 4 analysis:
- Keycloak realm `opencode` exists with OIDC clients
- `frontend` client already configured as public PKCE client (per D-XX in 04-CONTEXT)
- Roles exist: `viewer` (read-only), `editor` (full CRUD)
- Test users: `viewer1`/`viewer1` (viewer), `editor1`/`editor1` (editor)
- Keycloak at `http://localhost:8080` (from AppHost)
- `RequireHttpsMetadata = false` (development)
- Access tokens: 5-minute expiry (need refresh token flow)

## APISIX Integration Points

From Phase 5 analysis:
- APISIX proxy at `http://localhost9080`
- Routes: `/api/dragonball/*` → DragonBall API, `/api/music/*` → Music API
- CORS already configured for `http://localhost:5173`
- `request-id` plugin adds `X-Correlation-Id` header to all responses
- GET routes are public (no auth required for reads)
- POST/PUT/DELETE require valid JWT in `Authorization: Bearer` header

## Common Pitfalls

1. **CORS preflight**: APISIX must handle OPTIONS requests before they reach the .NET APIs (already configured in Phase 5)
2. **Token expiry**: Access tokens are 5-minute; silent refresh must be configured to avoid 401 errors during long sessions
3. **Redirect URI mismatch**: Keycloak redirect URIs must exactly match the frontend callback URL (no trailing slash mismatch)
4. **Public client CORS**: Keycloak requires `Web Origins` configuration for `http://localhost:5173` — the `frontend` client must have this set
5. **APISIX is not Keycloak**: The frontend talks to APISIX for API calls but directly to Keycloak for authentication (login redirect / token exchange)
6. **Multiple auth popups**: `automaticSilentRenew` with iframe-based renew may trigger popup blockers — use `silent_redirect_uri` with a dedicated HTML page
7. **State/Nonce validation**: `oidc-client-ts` handles this automatically, but an improperly configured `redirect_uri` will fail silently
