---
phase: 09
reviewed: 2026-04-30T14:00:00Z
depth: standard
files_reviewed: 42
files_reviewed_list:
  - src/OpenCode.AngularFrontend/src/main.ts
  - src/OpenCode.AngularFrontend/src/index.html
  - src/OpenCode.AngularFrontend/src/styles.css
  - src/OpenCode.AngularFrontend/src/app/app.config.ts
  - src/OpenCode.AngularFrontend/src/app/app.routes.ts
  - src/OpenCode.AngularFrontend/src/app/app.component.ts
  - src/OpenCode.AngularFrontend/src/app/app.component.html
  - src/OpenCode.AngularFrontend/src/auth/auth.config.ts
  - src/OpenCode.AngularFrontend/src/auth/auth.service.ts
  - src/OpenCode.AngularFrontend/src/auth/auth.guard.ts
  - src/OpenCode.AngularFrontend/src/auth/role.guard.ts
  - src/OpenCode.AngularFrontend/src/auth/has-role.directive.ts
  - src/OpenCode.AngularFrontend/src/api/client.service.ts
  - src/OpenCode.AngularFrontend/src/api/types.ts
  - src/OpenCode.AngularFrontend/src/api/dragonball.service.ts
  - src/OpenCode.AngularFrontend/src/api/music.service.ts
  - src/OpenCode.AngularFrontend/src/api/env.ts
  - src/OpenCode.AngularFrontend/src/shared/components/layout/layout.ts
  - src/OpenCode.AngularFrontend/src/shared/components/layout/layout.html
  - src/OpenCode.AngularFrontend/src/shared/components/data-table/data-table.ts
  - src/OpenCode.AngularFrontend/src/shared/components/pagination/pagination.ts
  - src/OpenCode.AngularFrontend/src/shared/components/empty-state/empty-state.ts
  - src/OpenCode.AngularFrontend/src/shared/components/error-display/error-display.ts
  - src/OpenCode.AngularFrontend/src/shared/components/error-display/error-display.html
  - src/OpenCode.AngularFrontend/src/shared/services/toast.service.ts
  - src/OpenCode.AngularFrontend/src/shared/services/global-error-handler.service.ts
  - src/OpenCode.AngularFrontend/src/pages/login/login.ts
  - src/OpenCode.AngularFrontend/src/pages/callback/callback.ts
  - src/OpenCode.AngularFrontend/src/pages/dashboard/dashboard.ts
  - src/OpenCode.AngularFrontend/src/pages/dragonball/dragonball.routes.ts
  - src/OpenCode.AngularFrontend/src/pages/dragonball/dragonball-page/dragonball-page.ts
  - src/OpenCode.AngularFrontend/src/pages/dragonball/character-list/character-list.ts
  - src/OpenCode.AngularFrontend/src/pages/dragonball/character-detail/character-detail.ts
  - src/OpenCode.AngularFrontend/src/pages/dragonball/character-form/character-form.ts
  - src/OpenCode.AngularFrontend/src/pages/music/music.routes.ts
  - src/OpenCode.AngularFrontend/src/pages/music/music-page/music-page.ts
  - src/OpenCode.AngularFrontend/src/pages/music/artist-list/artist-list.ts
  - src/OpenCode.AngularFrontend/src/pages/music/artist-detail/artist-detail.ts
  - src/OpenCode.AngularFrontend/src/pages/music/album-detail/album-detail.ts
  - src/OpenCode.AngularFrontend/src/pages/music/music-form/music-form.ts
  - src/OpenCode.AngularFrontend/angular.json
  - src/OpenCode.AngularFrontend/tsconfig.app.json
  - src/OpenCode.AngularFrontend/package.json
  - src/OpenCode.AngularFrontend/Dockerfile
  - src/OpenCode.AngularFrontend/nginx.conf
findings:
  critical: 0
  warning: 6
  info: 10
  total: 16
status: issues_found
---

# Phase 09: Code Review Report — Angular Frontend

**Reviewed:** 2026-04-30T14:00:00Z
**Depth:** standard
**Files Reviewed:** 42 (denoted config scope) + supplementary HTML templates and config files for context
**Status:** issues_found

## Summary

This review covers an Angular 21 standalone SPA with OIDC auth via `angular-auth-oidc-client`, Dragon Ball CRUD UI, Music Catalog CRUD UI, role-based directives, toast/error handling, and a responsive Tailwind v4 layout. The codebase is well-structured with clear separation of concerns (auth, API, shared components, pages). However, several issues were found:

- **Track CRUD incomplete**: edit/delete track operations have no UI entry points, making them unreachable
- **Form submitting state mismanaged**: `MusicFormComponent` resets the flag synchronously before API calls complete; `CharacterFormComponent` never resets it on error
- **Redundant auth interceptor**: `secureRoutes` in OIDC config and manual token attachment in `ApiClientInterceptor` overlap, potentially causing duplicate headers
- **Template type safety bypassed**: pervasive `$any()` casts in `music-page.html` mask real type mismatches
- **Dead code**: `trackByKey` method in `DataTableComponent` is never invoked
- **Missing Image URL field**: `CharacterFormComponent` lacks an `imageUrl` input despite it being part of the API schema

## Warnings

### WR-01: Track edit/delete CRUD operations unreachable from UI

**File:** `src/OpenCode.AngularFrontend/src/pages/music/music-page.html:56-58`

**Issue:** The `edit-track` and `edit-track` form is initialized with a hardcoded reference to `selectedAlbum.tracks[0]`, and there is no UI mechanism to select which track to edit or delete. The `AlbumDetailComponent` renders tracks as a read-only table without per-track action buttons. The `edit-track` and any delete-track paths are unreachable through normal user interaction. Track creation (via the "+ Add Track" button) works, but users cannot edit or delete individual tracks.

**Lines affected:**
- `album-detail.html:22-39` — track table has no edit/delete buttons
- `album-detail.ts` — no `selectTrack` output event defined
- `music-page.html:56-58` — `edit-track` case hardcodes `tracks[0]`
- `music-page.ts:95-101` — `handleDeleteTrack` exists but is never bound to any UI event

**Fix:**
Add edit/delete buttons per track in `album-detail.html`:

```html
@for (track of album.tracks; track track.id) {
  <tr class="border-b border-gray-200">
    <td class="p-2 text-gray-400">{{ track.trackNumber ?? '—' }}</td>
    <td class="p-2">{{ track.title }}</td>
    <td class="p-2">{{ formatDuration(track.duration) }}</td>
    <td class="p-2" *appHasRole="'editor'">
      <button (click)="editTrack.emit(track.id)" class="text-indigo-600 mr-2 cursor-pointer">Edit</button>
      <button (click)="deleteTrack.emit(track.id)" class="text-red-600 cursor-pointer">Delete</button>
    </td>
  </tr>
}
```

Wire up the new events in `AlbumDetailComponent` and `MusicPageComponent`, and fix the `edit-track` view to accept a selected track ID:

```html
@case ('edit-track') {
  @if (selectedTrack && selectedAlbum) {
    <app-music-form [mode]="'edit-track'" [initial]="selectedTrack" 
      (save)="handleUpdateTrack(selectedTrack.id, $any($event))" (cancel)="goBack()" />
  }
}
```

---

### WR-02: MusicFormComponent submitting flag resets synchronously before API call completes

**File:** `src/OpenCode.AngularFrontend/src/pages/music/music-form.ts:53-77`

**Issue:** The `submitting` flag is set to `true`, then `save.emit()` is called (synchronous), and the `finally` block immediately resets `submitting` to `false`. The parent's handler (e.g., `handleCreateArtist`) starts an async `subscribe()`, but the `try/finally` runs to completion before the HTTP request even starts. This means the submit button never actually disables during the API call — it flashes disabled for less than a tick.

```typescript
onSubmit(): void {
  this.submitting = true;      // set
  try {
    this.save.emit({...});     // synchronous — parent starts async op
  } finally {
    this.submitting = false;   // runs immediately — too early!
  }
}
```

**Fix:** Remove `try/finally` and reset `submitting` only after the async operation completes. Since `EventEmitter` is synchronous, the form component cannot know when the parent's HTTP call finishes. Either:

1. **Move submit state to parent** — have the form emit without managing `submitting`, let the parent set loading state.
2. **Make the output return an Observable** — change `@Output() save = new EventEmitter<...>()` to a pattern where the parent provides the save handler as an `@Input()` that returns `Observable<void>`.
3. **Simple fix** — remove the `submitting` field entirely and use the `required` attribute + form validation instead.

---

### WR-03: CharacterFormComponent submitting flag never reset on API error

**File:** `src/OpenCode.AngularFrontend/src/pages/dragonball/character-form.ts:37-48`

**Issue:** `this.submitting = true` is set before `save.emit()`, but there is no mechanism to reset it to `false` after the parent's API call succeeds or fails. If the API call fails (e.g., network error), the parent shows a toast but the form's "Saving..." state persists and the submit button stays permanently disabled.

```typescript
onSubmit(): void {
  if (!this.name.trim() || !this.race.trim() || !this.ki.trim()) return;
  this.submitting = true;        // never reset on error
  this.save.emit({...});
}
```

**Fix:** Add a `submitting` input or callback to allow the parent to reset the form state after the API call completes:

```typescript
@Input() set resetSubmitting(value: boolean) {
  if (value === false) this.submitting = false;
}
```

Then in the parent (`dragonball-page.ts`), reset after error:

```typescript
handleCreate(data: CharacterCreatePayload): void {
  this.dbService.createCharacter(data).subscribe({
    next: () => this.showList(),
    error: (err) => {
      this.toast.showError(err.message || 'Failed to create character');
      // this.selectedForm.resetSubmitting = false; — requires template ref
    },
  });
}
```

Alternatively, avoid the `submitting` flag in the form entirely and use template-driven form state (`form.invalid`, `ngSubmit`).

---

### WR-04: Redundant auth token attachment — OIDC `secureRoutes` + manual interceptor overlap

**File:**
- `src/OpenCode.AngularFrontend/src/auth/auth.config.ts:15`
- `src/OpenCode.AngularFrontend/src/api/client.service.ts:33-38`

**Issue:** The `secureRoutes` configuration in `auth.config.ts` instructs the `angular-auth-oidc-client` library's built-in HTTP interceptor to automatically attach the Bearer token to all requests matching `DRAGONBALL_API_URL` and `MUSIC_API_URL`. Simultaneously, `ApiClientInterceptor` manually attaches the same Bearer token to non-GET requests to the same URLs. This creates two competing interceptors:

1. The OIDC interceptor runs and attaches a token.
2. `ApiClientInterceptor` runs afterwards and overwrites the `Authorization` header.

For GET requests, the OIDC interceptor adds a token anyway (contradicting the "public reads" design intent). For write requests, the token is set twice (redundant, but functionally the same).

Additionally, `ApiClientInterceptor` calls `this.auth.accessToken$` which can trigger a token refresh, potentially causing the OIDC library to attempt a silent renew — which the OIDC interceptor already manages.

**Fix:** Pick one approach:

**Option A** — Let the OIDC library handle all auth. Remove manual token attachment from `ApiClientInterceptor`:
```typescript
intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
  const correlationId = crypto.randomUUID();
  const headers = req.headers
    .set('X-Correlation-Id', correlationId)
    .set('Content-Type', 'application/json');
  const clonedReq = req.clone({ headers });
  return next.handle(clonedReq).pipe(
    catchError((error: HttpErrorResponse) => { ... })
  );
}
```

**Option B** — Remove `secureRoutes` and keep manual attachment (gives explicit control over which methods get tokens):
```typescript
// auth.config.ts
config: {
  // ... no secureRoutes
}
```

---

### WR-05: Pervasive `$any()` type casts in music-page template mask type mismatches

**File:** `src/OpenCode.AngularFrontend/src/pages/music/music-page.html:37,41,45,49,53,57`

**Issue:** Every `(save)` event binding in the `music-page.html` template uses `$any($event)` to bypass Angular's strict template type checking. The `MusicFormComponent`'s `save` output is typed as `EventEmitter<ArtistCreatePayload | AlbumCreatePayload | TrackCreatePayload>`, which is a union type. The `$any()` cast suppresses compiler warnings but provides no runtime safety.

```html
<app-music-form (save)="handleCreateArtist($any($event))" />
<app-music-form (save)="handleUpdateAlbum(selectedAlbum.id, $any($event))" />
```

**Fix:** Use distinct output events per form mode instead of a single union emitter:

```typescript
@Output() saveArtist = new EventEmitter<ArtistCreatePayload>();
@Output() saveAlbum = new EventEmitter<AlbumCreatePayload>();
@Output() saveTrack = new EventEmitter<TrackCreatePayload>();
```

In the template:
```html
@case ('create-artist') {
  <app-music-form [mode]="'create-artist'" (saveArtist)="handleCreateArtist($event)" ... />
}
```

If keeping a single emitter, use discriminated unions and template type guards.

---

### WR-06: DataTableComponent `trackByKey` method is dead code

**File:** `src/OpenCode.AngularFrontend/src/shared/components/data-table/data-table.ts:25-27`

**Issue:** The `trackByKey(index: number): number` method is defined in the component class but never referenced in the template. The Angular `@for` control flow block in `data-table.html` uses `track keyExtractor(item)` directly, which is the valid Angular 17+ syntax. The `trackByKey` method is a leftover from the old `*ngFor` pattern (`*ngFor="let item of data; trackBy: trackByKey"`) and serves no purpose.

**Fix:** Remove the unused `trackByKey` method:

```typescript
// Remove this entirely:
// trackByKey(index: number): number {
//   return index;
// }
```

---

## Info

### IN-01: Character form silently fails validation with no user feedback

**File:** `src/OpenCode.AngularFrontend/src/pages/dragonball/character-form.ts:38`

**Issue:** The `onSubmit()` method checks required fields (`name`, `race`, `ki`) but just `return`s without informing the user. The user clicks the submit button and nothing happens — no error message, no field highlighting.

```typescript
if (!this.name.trim() || !this.race.trim() || !this.ki.trim()) return;
```

**Fix:** Use Angular template-driven form validation with visual indicators:
```html
<input [(ngModel)]="name" name="name" required #nameField="ngModel" />
@if (nameField.invalid && nameField.touched) {
  <p class="text-red-500 text-xs">Name is required</p>
}
```

---

### IN-02: MusicFormComponent lacks required-field validation entirely

**File:** `src/OpenCode.AngularFrontend/src/pages/music/music-form.ts:53-77`

**Issue:** The `onSubmit()` method performs no validation of required fields. For artist forms, `name` is required but not validated. For album/track forms, `title` is required but not validated. The HTML templates use the `required` attribute on some inputs, but Angular template-driven forms don't enforce this without the `ngModel` validation.

---

### IN-03: `console.error` in production entry point and error handler

**Files:**
- `src/OpenCode.AngularFrontend/src/main.ts:6`
- `src/OpenCode.AngularFrontend/src/shared/services/global-error-handler.service.ts:9`

**Issue:** Both `main.ts` and `GlobalErrorHandler` use `console.error()` which exposes error details in the browser console. Acceptable for a PoC, but in a production app these should be suppressed or replaced with structured logging.

---

### IN-04: `AuthService.userData$` typed as `Observable<any>` loses type safety

**File:** `src/OpenCode.AngularFrontend/src/auth/auth.service.ts:15-17`

**Issue:** The `userData$` getter returns `Observable<any>` instead of a typed interface. Callers must guess at the shape of the user data (e.g., `($any(userData$ | async)?.profile)?.preferred_username` in `layout.html:15`).

**Fix:** Define a typed interface for the user data shape returned by the OIDC library:
```typescript
interface UserData {
  profile?: {
    preferred_username?: string;
    realm_roles?: string[];
    resource_access?: Record<string, { roles?: string[] }>;
    roles?: string[];
    [key: string]: unknown;
  };
}

get userData$(): Observable<UserData> {
  return this.oidcSecurityService.userData$ as Observable<UserData>;
}
```

---

### IN-05: `HasRoleDirective` can create duplicate embedded views on repeated emissions

**File:** `src/OpenCode.AngularFrontend/src/auth/has-role.directive.ts:38-43`

**Issue:** The subscription calls `createEmbeddedView(this.templateRef)` every time the `hasAccess` value is `true`. If the combined observable emits `true` twice in a row (e.g., roles refresh after token renewal), a duplicate view is created. The `viewContainer.clear()` only runs when `hasAccess` is `false`. In practice this rarely triggers, but it is a latent bug.

**Fix:** Track whether the view is already created:
```typescript
private hasView = false;

.subscribe(hasAccess => {
  if (hasAccess && !this.hasView) {
    this.viewContainer.createEmbeddedView(this.templateRef);
    this.hasView = true;
  } else if (!hasAccess) {
    this.viewContainer.clear();
    this.hasView = false;
  }
});
```

---

### IN-06: `navigator.clipboard` used without secure context check

**File:** `src/OpenCode.AngularFrontend/src/shared/components/error-display/error-display.ts:38`

**Issue:** `navigator.clipboard.writeText()` requires a secure context (HTTPS or localhost). In non-secure contexts, the Promise rejects silently (`.catch(() => {})`). The feature degrades without user feedback.

**Fix:** Check for clipboard API availability:
```typescript
copyCorrelationId(id: number, correlationId: string): void {
  if (!navigator.clipboard) return;
  navigator.clipboard.writeText(correlationId).then(...).catch(...);
}
```

---

### IN-07: Display roles type cast is unnecessary

**File:** `src/OpenCode.AngularFrontend/src/shared/components/layout/layout.ts:19`

**Issue:** `(roles as string[])` casts to `string[]` but `getRoles()` already returns `Observable<string[]>`:
```typescript
displayRoles$ = this.auth.getRoles().pipe(
  map(roles => (roles as string[]).filter(...))  // <-- unnecessary cast
);
```

**Fix:** Remove the cast:
```typescript
map(roles => roles.filter(r => !r.startsWith('default-')).join(', ') || 'viewer')
```

---

### IN-08: Missing `imageUrl` field in CharacterFormComponent

**File:** `src/OpenCode.AngularFrontend/src/pages/dragonball/character-form.ts`

**Issue:** The API schema (`CharacterCreatePayload`) includes `imageUrl?: string`, but the character form UI has no field for it. Users cannot set or edit the character picture URL. The project requirements specify "Dragon Ball character picture: URL string (no file upload)", so this field should be available in the form UI.

---

### IN-09: `CallbackComponent` error state never clears

**File:** `src/OpenCode.AngularFrontend/src/pages/callback/callback.ts:29-31`

**Issue:** On authentication failure, `this.error` is set, showing an error message in the template. However, there is no retry mechanism or way to navigate away from the error state other than manual URL change. The user is stuck on the callback page.

**Fix:** Provide a fallback action:
```typescript
error: (err) => {
  this.error = err?.message || 'Authentication failed';
  setTimeout(() => this.router.navigate(['/login']), 3000);
}
```

---

### IN-10: `ErrorDisplayComponent` uses manual subscription instead of async pipe

**File:** `src/OpenCode.AngularFrontend/src/shared/components/error-display/error-display.ts:18-27`

**Issue:** The `ErrorDisplayComponent` manually manages a `Subscription` lifecycle with `ngOnInit`/`ngOnDestroy`. This pattern works but is more error-prone than Angular's `async` pipe. Since the component receives stream events via a Subject that emits discrete items (not an array), the async pipe approach would require accumulating toasts differently (e.g., using a `scan` operator).

Consider refactoring `ToastService` to expose a `toasts` signal or an observable of the current array:

```typescript
// toast.service.ts
private toasts = signal<Toast[]>([]);
readonly toastsList = this.toasts.asReadonly();

addToast(...): void {
  this.toasts.update(list => [...list, toast]);
}

dismiss(id: number): void {
  this.toasts.update(list => list.filter(t => t.id !== id));
}
```

---

## Security Notes

- **Auth interceptor** only attaches Bearer token to non-GET requests, consistent with the "Public reads, protected writes" design.
- **OIDC configuration** uses Authorization Code flow with PKCE (`responseType: 'code'`), refresh token rotation, and silent renew — all best practices.
- **nginx.conf** includes security headers: `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `X-XSS-Protection: 1; mode=block`.
- **Environment config** is injected at runtime via `docker-entrypoint.sh` — secrets are handled via environment variables, not baked into the image.
- **No eval**, `innerHTML`, or `dangerouslySetInnerHTML` used anywhere.
- **`confirm()`** for delete operations is acceptable for a PoC but blocks the UI thread. Consider a modal dialog for production.

## Architecture Observations

### Good Patterns
- **Standalone components** throughout — no NgModules, aligned with Angular 17+ best practices.
- **Lazy-loaded routes** using `loadComponent` and `loadChildren`.
- **Clear separation** between auth layer, API layer, shared components, and page components.
- **Error interceptor** wraps all API errors with `ApiError` shape including `correlationId`.
- **Tailwind CSS v4** configured correctly via `postcss.config.json` with `@tailwindcss/postcss`.
- **Template-driven forms** with `[(ngModel)]` used consistently across all form components.

### Red Flags
- **Route guards not applied** — `AuthGuard` and `RoleGuard` are implemented but never wired to any routes. The design document says "public reads, protected writes" which justifies this, but it means unauthenticated users can browse all pages.
- **Single component per page** — Dragon Ball and Music pages use view-state switching (`CharacterView`, `MusicView`) instead of Angular child routes. This works but misses URL-based navigation, browser back/forward, and deep linking.
- **Implicit `any`** — `AuthService.userData$` returns `any`, `$any($event)` used in templates, `as any` casts in toast service.

---

_Reviewed: 2026-04-30T14:00:00Z_
_Reviewer: gsd-code-reviewer (standard depth)_
