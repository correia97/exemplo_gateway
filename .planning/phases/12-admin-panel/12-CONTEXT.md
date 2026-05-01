# Phase 12: Admin Panel — Full CRUD Management UI (Both Frontends)

**Gathered:** 2026-04-30
**Status:** Ready for planning

<domain>
## Phase Boundary

A comprehensive admin panel in both React and Angular frontends that provides full CRUD management for ALL entities across both APIs (Dragon Ball: Characters, Transformations, Planets; Music: Artists, Albums, Tracks, Genres), plus an admin dashboard with entity overview.

**Frontend-only.** All data operations use existing API CRUD endpoints and the existing `editor` Keycloak role. No new backend endpoints, no new API project, no new Keycloak role, no backend audit infrastructure.

This phase extends the existing frontend applications (Phase 7 — React, Phase 9 — Angular) with dedicated admin-only views, routes, and controls beyond the browsing/editing capabilities already built.

</domain>

<decisions>
## Implementation Decisions

### Admin Architecture
- **D-01: Frontend-only admin** — All admin functionality is UI work. No new backend endpoints, no new API project, no new Keycloak role. Admin panel calls existing CRUD endpoints (GET for reads, POST/PUT for writes, DELETE for deletions).
- **D-02: Existing `editor` role suffices** — No new `admin` role. The existing `editor` role already provides full CRUD access to all API endpoints. Admin panel is accessible to any authenticated user with the `editor` role.
- **D-03: Shared admin layout** — Both frontends get an `/admin/*` route namespace with a dedicated sidebar/nav layout component organized by domain (Dashboard, Dragon Ball entities, Music entities).

### Entity CRUD Tables
- **D-04: Enhanced reusable DataTable** — The existing `DataTable` component (React) and shared table components (Angular) get enhanced with inline search/filter, column sorting, multi-select, and action buttons (Edit/Delete). One table configuration per entity type.
- **D-05: Delete protection** — All delete operations require a two-step confirmation dialog showing the entity name and type. Dialog includes entity detail so users verify before confirming.

### Audit Trail
- **D-06: Dates-only audit** — Entity tables show `CreatedAt` and `UpdatedAt` timestamps from existing API response data (already on every entity via `BaseEntity`). No user identity tracking (no `CreatedBy`/`UpdatedBy` on entities). Acceptable for PoC scope.

### Admin Dashboard
- **D-07: Entity count overview** — Dashboard shows stat cards for each entity type. Counts obtained by calling existing list endpoints with `pageSize=1` and reading `totalCount` from the paginated response. No dedicated aggregation endpoint.

### Dropped Features
- **ADMIN-05 (Bulk import/export):** Skipped — needs backend bulk endpoints that don't exist.
- **ADMIN-06 (Role management UI):** Skipped — Keycloak's built-in admin console handles this.

### Scope Boundaries
- **In scope**: Enhanced admin CRUD tables, admin dashboard with entity counts, delete confirmation, admin layout in both frontends, admin route guards for `editor` role, dates-only audit display
- **NOT in scope**: Backend admin endpoints (using existing CRUD)
- **NOT in scope**: New Keycloak roles (using existing `editor`)
- **NOT in scope**: Bulk import/export (no backend support)
- **NOT in scope**: Role management UI (use Keycloak admin console)
- **NOT in scope**: Audit trail with user identity (dates only)
- **NOT in scope**: Real-time updates (polling or on-load refresh only)
- **NOT in scope**: Advanced data visualization (simple count cards suffice)
- **NOT in scope**: Mobile-responsive admin (desktop-first with basic responsiveness)

### the agent's Discretion
- Exact column layout per entity type in admin tables
- Dashboard count polling interval (on-load only vs periodic refresh)
- Toast/snackbar feedback after successful admin operations
- Empty state design for entity tables
- Color scheme for admin layout (keep existing app theme)

### Plan Impact
- Existing Plan 12-01 (backend admin infrastructure) and parts of Plan 12-04 (bulk ops, role management) need restructuring. The planner should revisit all 4 plans to reflect the frontend-only scope.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Requirements
- `.planning/REQUIREMENTS.md` — ADMIN-01 through ADMIN-08 (ADMIN-05, ADMIN-06 dropped; ADMIN-04 limited to dates)

### Prior Phase Context
- `.planning/phases/07-react-frontend/07-CONTEXT.md` — React patterns: Tailwind CSS, custom components, React Router v7, Fetch API wrapper
- `.planning/phases/09-add-a-new-frontend-angular-project-with-all-features-the-cur/09-CONTEXT.md` — Angular patterns: standalone components, lazy-loaded modules
- `.planning/phases/04-keycloak-authentication-authorization/04-CONTEXT.md` — Keycloak roles (`viewer`, `editor`), claims transformation
- `.planning/phases/03-api-endpoints-dragon-ball-music-crud/03-CONTEXT.md` — Existing endpoint structure (CRUD, pagination)

### Domain Entities and API Responses
- `src/OpenCode.Domain/Entities/BaseEntity.cs` — Base with `CreatedAt`, `UpdatedAt` timestamps
- `src/OpenCode.Domain/Entities/Character.cs` — Character entity fields
- `src/OpenCode.Domain/Entities/Planet.cs` — Planet entity fields
- `src/OpenCode.Domain/Entities/Transformation.cs` — Transformation entity fields
- `src/OpenCode.Domain/Entities/Genre.cs` — Genre entity fields
- `src/OpenCode.Domain/Entities/Artist.cs` — Artist entity fields
- `src/OpenCode.Domain/Entities/Album.cs` — Album entity fields
- `src/OpenCode.Domain/Entities/Track.cs` — Track entity fields

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets

**Frontend (React):**
- `src/OpenCode.Frontend/src/components/DataTable.tsx` — Generic DataTable with column definition props — enhances for admin CRUD
- `src/OpenCode.Frontend/src/components/Pagination.tsx` — Pagination component for list views
- `src/OpenCode.Frontend/src/components/Layout.tsx` — App shell with navigation, adaptable for admin layout
- `src/OpenCode.Frontend/src/api/` — API client functions (reused as-is, no new endpoints needed)
- `src/OpenCode.Frontend/src/auth/AuthProvider.tsx` — Auth context, role checks, JWT access (reuse for admin role guard)

**Frontend (Angular):**
- `src/OpenCode.AngularFrontend/src/shared/components/` — Reusable shared Angular components (enhance for admin)
- `src/OpenCode.AngularFrontend/src/shared/services/` — Shared Angular services
- `src/OpenCode.AngularFrontend/src/auth/auth.service.ts` — OIDC auth service (reuse for admin guard)
- `src/OpenCode.AngularFrontend/src/pages/` — Feature page modules (pattern to follow for admin pages)

### Established Patterns
- **Role-based UI guards**: React uses `AuthProvider` role checks; Angular uses `HasRoleDirective`. Admin panel checks for `editor` role.
- **API consumption**: Both frontends call Kong proxy (`http://localhost:8000/api/`). All existing endpoints work for admin CRUD without changes.
- **Layout structure**: Both frontends have a sidebar + content area pattern — admin layout extends this.

### Integration Points
- Admin routes under `/admin/*` in both React Router and Angular router
- Admin sidebar links organized by domain: Dashboard (root), Dragon Ball (Characters, Planets, Transformations), Music (Artists, Albums, Tracks, Genres)
- Role guard on all admin routes: requires `editor` role (same as existing CRUD form guards)
- Delete confirmation dialog before calling existing DELETE endpoints
- Dashboard count cards via calling existing GET list endpoints with `pageSize=1`

</code_context>

<specifics>
## Specific Ideas

- "Dashboard entity counts from GET list endpoints with totalCount — no separate stats endpoint needed"
- "Admin layout sidebar organized by domain (Dragon Ball / Music) with collapsible groups"
- "Delete confirm dialog shows entity name and type — no dependency warnings needed since backend handles cascading"
- "Admin tables should support multi-select for bulk delete (sequential DELETE calls)"
- "Audit dates from existing CreatedAt/UpdatedAt on every entity — already provided by API responses"
- "Keycloak's own admin console handles user role management — no custom UI needed"

</specifics>

<deferred>
## Deferred Ideas

- Bulk import/export (CSV/JSON) — needs backend bulk endpoints, deferred to future phase
- Role management UI — Keycloak admin console already provides this; custom UI deferred
- Audit trail with user identity (CreatedBy/UpdatedBy) — deferred; would require backend changes
- Real-time dashboard updates via WebSockets — polling/on-load is sufficient for PoC
- Advanced data visualization (charts, graphs) — simple count cards sufficient for v1

</deferred>

---

*Phase: 12-admin-panel*
*Context gathered: 2026-04-30*
