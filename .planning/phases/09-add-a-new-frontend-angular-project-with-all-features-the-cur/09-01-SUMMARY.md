# 09-01-SUMMARY: Scaffold Angular 21 standalone SPA with OIDC auth

**Phase:** 09-angular-frontend
**Wave:** 1
**Status:** ✅ Complete

## Accomplished

1. **Scaffolded Angular 21 project** at `src/OpenCode.AngularFrontend/` using Angular CLI, standalone architecture, routing, Tailwind CSS v4 via `@tailwindcss/postcss`
2. **Installed dependencies:** `angular-auth-oidc-client@21`, `tailwindcss`, `@tailwindcss/postcss`, `postcss`
3. **Created OIDC auth infrastructure:** `auth.config.ts` (Keycloak realm `opencode`, client `frontend`, PKCE), `auth.service.ts` (login/logout/roles/tokens), `auth.guard.ts` (CanActivate redirect)
4. **Created API client interceptor:** `client.service.ts` with `API_BASE_URL`, `X-Correlation-Id` on every request, `Authorization: Bearer` on non-GET, structured error objects
5. **Created app shell:** `LayoutComponent` with collapsible sidebar (Dashboard, Dragon Ball, Music nav links) + top bar (hamburger toggle, logo, login/logout) + `router-outlet` content area
6. **Created pages:** Login (redirect to Keycloak), Callback (OIDC handler), Dashboard (authenticated/anonymous variants)
7. **Wired into AppHost:** `AddExecutable("angular-frontend", ...)` port 4200, waits for APISIX
8. **Build verification:** `ng build --configuration development` passes, `dotnet build OpenCode.slnx` passes
9. **Fixed Angular 21 API incompatibilities:** `withInterceptorsFromDi()` takes no args, `loadChildren` → `.then(m => m.routes)`, plain `aria-label` for `attr.aria-label`, arrow function template expressions moved to component methods

## Files Created/Modified

- `src/OpenCode.AngularFrontend/` — Full Angular 21 standalone project
- `src/OpenCode.AngularFrontend/src/auth/auth.config.ts` — OIDC config
- `src/OpenCode.AngularFrontend/src/auth/auth.service.ts` — AuthService
- `src/OpenCode.AngularFrontend/src/auth/auth.guard.ts` — AuthGuard
- `src/OpenCode.AngularFrontend/src/api/client.service.ts` — HTTP interceptor
- `src/OpenCode.AngularFrontend/src/api/types.ts` — Shared types
- `src/OpenCode.AngularFrontend/src/pages/login/` — Login page
- `src/OpenCode.AngularFrontend/src/pages/callback/` — OIDC callback
- `src/OpenCode.AngularFrontend/src/pages/dashboard/` — Dashboard
- `src/OpenCode.AngularFrontend/src/shared/components/layout/` — App shell layout
- `src/OpenCode.AppHost/Program.cs` — Angular frontend service definition

## Decisions

- `withInterceptorsFromDi()` takes no arguments in Angular 21; class-based interceptors via `{ provide: HTTP_INTERCEPTORS, useClass: ..., multi: true }`
- Angular 21 templates reject arrow function expressions — filtering/transformation moved to component methods or computed observables
- `$any()` cast required in templates when async pipe return types mismatch

## Verification

- `ng build --configuration development` — zero errors, zero warnings
- `dotnet build src/OpenCode.AppHost` — passes
- OIDC config points to `http://localhost:8080/realms/opencode`
- API base URL points to `http://localhost9080/api/`
