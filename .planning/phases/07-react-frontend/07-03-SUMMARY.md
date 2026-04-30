# 07-03: React API Integration & Auth — Summary

## What Was Done

1. **API client** in `src/services/`:
   - `apiClient.ts` — Axios instance with base URL, interceptors
   - Request interceptor: adds `Authorization: Bearer <token>` header
   - Response interceptor: handles 401 → redirect to login, 403 → show error

2. **React Query** integration:
   - QueryKey factory: `['characters']`, `['character', id]`, etc.
   - Hooks: `useCharacters`, `useCharacter`, `usePlanets`, `useArtists`, `useAlbums`, `useSongs`
   - Mutations: `useCreateCharacter`, `useUpdateCharacter`, `useDeleteCharacter`
   - Stale time: 30 seconds, cache time: 5 minutes

3. **Auth context** via React Context API:
   - `AuthContext` with `user`, `isAuthenticated`, `login()`, `logout()` methods
   - Keycloak OIDC client library (oidc-client-ts)
   - Token refresh on expiry
   - Role checking: `hasRole('dragonball:read')` helper

4. **Protected routes**:
   - `<ProtectedRoute>` component wrapping routes requiring auth
   - Redirects to `/login` if not authenticated
   - Shows permission error if missing required role

5. **Environment configuration**:
   - `VITE_KEYCLOAK_URL`, `VITE_KEYCLOAK_REALM`, `VITE_KEYCLOAK_CLIENT_ID`
   - `VITE_API_BASE_URL` — Kong gateway URL

## Verification

- Axios interceptor attaches valid Bearer token to all requests
- Token refresh happens transparently before expiry
- React Query caches responses and reduces network calls
- Protected routes block unauthenticated access
- Role-based UI elements shown/hidden correctly

## Key Findings

- oidc-client-ts handles the full OIDC flow (auth code + PKCE)
- Axios interceptors centralize auth logic in one place
- React Query stale/cache times balance freshness vs performance
- Environment variables via Vite `import.meta.env` keep config flexible
