---
phase: 02-database
fixed_at: 2026-04-30T10:00:00Z
review_path: .planning/phases/02-database/02-REVIEW.md
iteration: 1
findings_in_scope: 5
fixed: 5
skipped: 0
status: all_fixed
---

# Phase 02: Code Review Fix Report — Database / Domain Layer

**Fixed at:** 2026-04-30T10:00:00Z
**Source review:** `.planning/phases/02-database/02-REVIEW.md`
**Iteration:** 1

**Summary:**
- Findings in scope: 5
- Fixed: 5
- Skipped: 0

## Fixed Issues

### CR-01: Missing semicolon in PL/pgSQL anonymous block causes syntax error

**Files modified:** `src/OpenCode.Domain/Data/init.sql`
**Commit:** `b6f3ab1`
**Applied fix:** Added missing semicolon after `END` in the `DO $$ ... $$` anonymous block (line 40), changing `END` to `END;`. This resolves the PostgreSQL syntax error that would prevent the container from starting via `/docker-entrypoint-initdb.d/`.

### WR-01: Overly permissive schema grants for application users

**Files modified:** `src/OpenCode.Domain/Data/init.sql`
**Commit:** `9409db8`
**Applied fix:** Removed three overly permissive `GRANT ALL PRIVILEGES ON SCHEMA ... TO ..._user;` statements (one per schema) that granted `CREATE` privilege to application users. Each schema section now only grants `USAGE` on the schema, `ALL` on existing tables/sequences, and `ALTER DEFAULT PRIVILEGES` for future objects — which is sufficient for CRUD operations without violating least-privilege principles.

### WR-02: Division by zero in PagedResult.TotalPages when PageSize = 0

**Files modified:** `src/OpenCode.Domain/Pagination/PagedResult.cs`
**Commit:** `d4b195d`
**Applied fix:** Added a `PageSize > 0` guard to the `TotalPages` computed property. If `PageSize` is zero (or negative), returns `0` instead of computing `TotalCount / 0.0` which would produce `Infinity` → `int.MinValue`.

### WR-03: Inconsistent return types between AddAsync and UpdateAsync

**Files modified:** `src/OpenCode.Domain/Interfaces/IRepository.cs`, `src/OpenCode.Domain/Implementations/Repository.cs`
**Commit:** `da8e91a`
**Applied fix:** Changed `UpdateAsync` return type from `Task` (void) to `Task<T>` (the updated entity) in both the interface and implementation, matching `AddAsync`'s contract. Callers who need the updated entity state (e.g., to read computed properties or check concurrency) can now capture the return value instead of re-fetching. No overrides of `UpdateAsync` exist in custom repository classes, so the change is fully backward compatible.

### WR-04: UpdateAsync marks entire entity as Modified, causing full column updates

**Files modified:** `src/OpenCode.Domain/Implementations/Repository.cs`
**Commit:** `5dc43ac`
**Applied fix:** Added XML documentation to `UpdateAsync` explaining that it marks the entire entity as `EntityState.Modified`, generating an UPDATE statement for every column. The doc also provides guidance: for partial updates, fetch the entity first and apply changes to the tracked instance, or accept a dictionary of changed properties.

---

_Fixed: 2026-04-30T10:00:00Z_
_Fixer: gsd-code-fixer (auto mode)_
_Iteration: 1_
