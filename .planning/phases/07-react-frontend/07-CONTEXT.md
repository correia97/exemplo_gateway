# Phase 7: React Frontend — Design Context

## Vision

A React 19 + Vite SPA that consumes both APIs exclusively through Kong, with OIDC login via Keycloak (Authorization Code + PKCE), role-aware CRUD UI, Tailwind CSS styling, and correlation ID display for debugging.

## Key Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| UI Framework | Tailwind CSS | Utility-first, fast dev, small bundle with Vite |
| Auth Architecture | SPA-level OIDC | oidc-client-ts in browser, PKCE directly to Keycloak realm `opencode` |
| Error UX | Toast notifications | Non-intrusive, auto-dismiss, shows correlation ID per error |
| Layout | Sidebar + TopBar + Content | Collapsible sidebar nav, top bar with user avatar/login state |
| Routing | React Router v7 | Standard choice, lazy loading, type-safe params |
| HTTP Client | Fetch API wrapper | Lightweight custom wrapper with auth header + correlation ID injection |
| Frontend Serving | Vite dev server directly (port 5173) | Simpler dev setup, Kong only proxies API routes (CORS already configured) |
| Styling Approach | Tailwind CSS + custom theme | Consistent color palette, responsive breakpoints |
| Component Library | Custom components | No heavy UI framework, Tailwind-based reusable primitives |

## Auth Flow

1. Unauthenticated user lands on page → can browse GET data
2. Click "Login" → redirect to Keycloak `/realms/OpenCode/protocol/openid-connect/auth` with PKCE
3. Keycloak redirects back to SPA with authorization code
4. oidc-client-ts exchanges code + PKCE for tokens (access + refresh + id)
5. Access token used in `Authorization: Bearer` header for writes
6. Token refresh handled automatically by oidc-client-ts (silent refresh / refresh token)
7. Logout → call Keycloak end_session_endpoint, clear local state

## API Integration

- Base URL: `http://localhost9080/api/` (Kong proxy)
- All GET requests: no auth header needed
- All POST/PUT/DELETE requests: inject `Bearer <access_token>` from oidc-client-ts
- Custom `X-Correlation-Id` header on every request (generated per-request UUID)
- Response correlation ID displayed in toast on error

## Layout Structure

```
+------------------------------------------+
| TopBar [Logo] [Nav: DB | Music] [Login]  |
+----------+-------------------------------+
| Sidebar  | Main Content Area             |
| - Dragon |  <Outlet />                   |
|   Ball   |                               |
| - Music  |                               |
|          |                               |
+----------+-------------------------------+
```

## Error Handling

- Toast notifications (bottom-right corner)
- Each toast shows: error message + `X-Correlation-Id` (copyable)
- Auto-dismiss after 8 seconds
- 401 → redirect to login
- 403 → show "insufficient permissions" toast
- Network errors → friendly "service unavailable" message

## Route Map

| Path | Component | Auth Required | Role |
|------|-----------|---------------|------|
| `/` | HomePage | No | — |
| `/dragonball` | CharacterList | No (browse) | viewer/editor (write actions) |
| `/dragonball/:id` | CharacterDetail | No (view) | viewer/editor (write actions) |
| `/dragonball/new` | CharacterForm | Yes | editor |
| `/dragonball/:id/edit` | CharacterForm | Yes | editor |
| `/music` | ArtistList | No (browse) | viewer/editor (write actions) |
| `/music/artists/:id` | ArtistDetail | No (view) | viewer/editor (write actions) |
| `/music/artists/:id/albums/:albumId` | AlbumDetail | No (view) | viewer/editor (write actions) |
| `/music/new` | MusicForm | Yes | editor |
| `/music/edit/:id` | MusicForm | Yes | editor |
| `/callback` | OidcCallback | No | — (OIDC redirect target) |
