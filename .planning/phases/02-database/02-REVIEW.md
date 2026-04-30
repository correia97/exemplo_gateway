---
phase: 02-database
reviewed: 2026-04-30T10:00:00Z
depth: standard
files_reviewed: 37
files_reviewed_list:
  - src/OpenCode.Domain/OpenCode.Domain.csproj
  - src/OpenCode.Domain/Data/init.sql
  - src/OpenCode.Domain/Data/DragonBallContext.cs
  - src/OpenCode.Domain/Data/DragonBallContextFactory.cs
  - src/OpenCode.Domain/Data/MusicContext.cs
  - src/OpenCode.Domain/Data/MusicContextFactory.cs
  - src/OpenCode.Domain/Entities/BaseEntity.cs
  - src/OpenCode.Domain/Entities/Character.cs
  - src/OpenCode.Domain/Entities/Planet.cs
  - src/OpenCode.Domain/Entities/Transformation.cs
  - src/OpenCode.Domain/Entities/Artist.cs
  - src/OpenCode.Domain/Entities/Album.cs
  - src/OpenCode.Domain/Entities/Genre.cs
  - src/OpenCode.Domain/Entities/ArtistGenre.cs
  - src/OpenCode.Domain/Entities/Track.cs
  - src/OpenCode.Domain/Interfaces/ICharacterRepository.cs
  - src/OpenCode.Domain/Interfaces/IAlbumRepository.cs
  - src/OpenCode.Domain/Interfaces/IArtistRepository.cs
  - src/OpenCode.Domain/Interfaces/IGenreRepository.cs
  - src/OpenCode.Domain/Interfaces/IRepository.cs
  - src/OpenCode.Domain/Interfaces/ITrackRepository.cs
  - src/OpenCode.Domain/Implementations/Repository.cs
  - src/OpenCode.Domain/Pagination/PagedResult.cs
  - src/OpenCode.Domain/Migrations/DragonBall/20260424222104_InitialCreate.cs
  - src/OpenCode.Domain/Migrations/DragonBall/20260424222104_InitialCreate.Designer.cs
  - src/OpenCode.Domain/Migrations/DragonBall/20260425172722_AddPlanetTransformationAndCharacterFields.cs
  - src/OpenCode.Domain/Migrations/DragonBall/20260425172722_AddPlanetTransformationAndCharacterFields.Designer.cs
  - src/OpenCode.Domain/Migrations/DragonBall/DragonBallContextModelSnapshot.cs
  - src/OpenCode.Domain/Migrations/Music/20260424222105_InitialCreate.cs
  - src/OpenCode.Domain/Migrations/Music/20260424222105_InitialCreate.Designer.cs
  - src/OpenCode.Domain/Migrations/Music/MusicContextModelSnapshot.cs
  - tests/OpenCode.Domain.Tests/OpenCode.Domain.Tests.csproj
  - tests/OpenCode.Domain.Tests/Entities/EntityPropertyTests.cs
  - tests/OpenCode.Domain.Tests/Migrations/SchemaIsolationTests.cs
  - tests/OpenCode.Domain.Tests/Pagination/PagedResultTests.cs
findings:
  critical: 1
  warning: 4
  info: 7
  total: 12
status: issues_found
---

# Phase 02: Code Review Report — Database / Domain Layer

**Reviewed:** 2026-04-30T10:00:00Z
**Depth:** standard
**Files Reviewed:** 37
**Status:** issues_found

## Summary

This review covers the full domain layer for Phase 02: EF Core entities, DbContexts, design-time factories, PG schema init script, migrations (DragonBall + Music), repository interfaces + implementation, PagedResult, and unit tests.

**Overall assessment:** The domain layer is well-structured with clean schema isolation, proper EF Core Fluent API configuration, and sensible entity design. The repository pattern is consistent, and the test suite provides solid coverage for entity structure and schema isolation.

**Key concerns:**
1. **Critical:** A missing semicolon in `init.sql` will cause a PostgreSQL syntax error, blocking database initialization on container first-start.
2. Schema grants are overly permissive (grants `CREATE` on schemas to app users).
3. `PagedResult.TotalPages` crashes on `PageSize = 0` due to division by zero.
4. `UpdateAsync` returns `void` while `AddAsync` returns the entity — inconsistent contract.
5. `UpdatedAt` has a default value but no auto-update mechanism on entity modification.

---

## Critical Issues

### CR-01: Missing semicolon in PL/pgSQL anonymous block causes syntax error

**File:** `src/OpenCode.Domain/Data/init.sql:40`
**Issue:** The `DO $$ ... $$` anonymous block closes with `END` (no semicolon) followed by `$$;`. PostgreSQL PL/pgSQL requires `END;` (with semicolon). Without it, the script will throw a syntax error when the container first starts via `/docker-entrypoint-initdb.d/`, preventing users and schema permissions from being created.

```sql
-- Current (broken):
    END IF;
    END        -- <-- missing semicolon
$$;

-- Correct:
    END IF;
END;
$$;
```

**Fix:**
```sql
    END IF;
END;
$$;
```

---

## Warnings

### WR-01: Overly permissive schema grants for application users

**File:** `src/OpenCode.Domain/Data/init.sql:47-74`
**Issue:** Each application user (`dragonball_user`, `music_user`, `keycloak_user`) receives `GRANT ALL PRIVILEGES ON SCHEMA ... TO ..._user` which includes the `CREATE` privilege. This allows app users to create new tables and objects in their schema, which is unnecessary for CRUD operations and violates least-privilege principles. EF Core migrations should be run by a dedicated migration user, not the app user.

The following lines each contain an overly permissive grant that should be removed:
- Line 52: `GRANT ALL PRIVILEGES ON SCHEMA dragonball TO dragonball_user;`
- Line 63: `GRANT ALL PRIVILEGES ON SCHEMA music TO music_user;`
- Line 74: `GRANT ALL PRIVILEGES ON SCHEMA keycloak TO keycloak_user;`

The `GRANT USAGE ON SCHEMA` (lines 47, 58, 69) is sufficient and correct.

**Fix:** Remove the three `GRANT ALL PRIVILEGES ON SCHEMA` lines (52, 63, 74). The per-table and default privileges already cover everything the app user needs.

---

### WR-02: Division by zero in PagedResult.TotalPages when PageSize = 0

**File:** `src/OpenCode.Domain/Pagination/PagedResult.cs:9`
**Issue:** `TotalPages` computes `(int)Math.Ceiling(TotalCount / (double)PageSize)`. If `PageSize` is 0, this becomes `TotalCount / 0.0 = Infinity`, `Math.Ceiling(Infinity) = Infinity`, and `(int)Infinity = int.MinValue` (in default unchecked context). While `PageSize = 0` shouldn't happen in normal usage, nothing prevents it.

**Fix:**
```csharp
public int TotalPages => PageSize > 0
    ? (int)Math.Ceiling(TotalCount / (double)PageSize)
    : 0;
```

---

### WR-03: Inconsistent return types between AddAsync and UpdateAsync

**File:** `src/OpenCode.Domain/Interfaces/IRepository.cs:9-10`
**File:** `src/OpenCode.Domain/Implementations/Repository.cs:40-51`
**Issue:** `AddAsync` returns `Task<T>` (the created entity with generated Id), but `UpdateAsync` returns `Task` (void). After an update, callers who need the updated entity state (e.g., to read computed properties or check concurrency) must re-fetch it. This is inconsistent and forces extra round-trips.

**Fix (option A — return entity):**
```csharp
// Interface
Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

// Implementation
public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
{
    Context.Entry(entity).State = EntityState.Modified;
    await Context.SaveChangesAsync(cancellationToken);
    return entity;
}
```

**Fix (option B — stay void but document):** Add XML doc explaining that callers should re-fetch if they need updated state. Not preferred since it pushes work to callers.

---

### WR-04: UpdateAsync marks entire entity as Modified, causing full column updates

**File:** `src/OpenCode.Domain/Implementations/Repository.cs:49-50`
**Issue:** `UpdateAsync` sets `Context.Entry(entity).State = EntityState.Modified` on a (presumably) detached entity. This marks ALL properties as modified, generating an UPDATE SQL statement that sets every column — even those that haven't changed. This also bypasses any database-generated values or concurrency tokens.

**Fix:** For partial updates, consider accepting a dictionary of changed properties, or use AutoMapper to compare with a tracked snapshot. For the current pattern, at minimum document the limitation:
```csharp
/// <summary>
/// Updates all columns of the entity. For partial updates, fetch the entity first
/// and apply changes to the tracked instance.
/// </summary>
```

---

## Info

### IN-01: Hardcoded passwords in init.sql

**File:** `src/OpenCode.Domain/Data/init.sql:28-38`
**Issue:** Database user passwords are hardcoded (`dragonball_pass`, `music_pass`, `keycloak_pass`, `kong_pass`). For a development PoC this is acceptable, but for any shared or CI environment these should come from environment variables or Docker secrets.

**Fix (optional, for production readiness):** Use environment variables:
```sql
CREATE USER dragonball_user WITH PASSWORD :'DRAGONBALL_DB_PASSWORD';
```

---

### IN-02: UpdatedAt has default value but no auto-update mechanism

**File:** `src/OpenCode.Domain/Data/DragonBallContext.cs:51`
**File:** `src/OpenCode.Domain/Data/MusicContext.cs:47,80,102`
**Issue:** Both DbContexts configure `UpdatedAt` with `HasDefaultValueSql("NOW()")`, which sets the value only on INSERT. There is no mechanism to automatically update `UpdatedAt` when an entity is modified. All entities will have `CreatedAt == UpdatedAt` forever.

**Fix:** Override `SaveChangesAsync` to set `UpdatedAt = DateTime.UtcNow` for all `EntityState.Modified` entries that implement `BaseEntity`:
```csharp
public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    foreach (var entry in ChangeTracker.Entries<BaseEntity>()
        .Where(e => e.State == EntityState.Modified))
    {
        entry.Entity.UpdatedAt = DateTime.UtcNow;
    }
    return base.SaveChangesAsync(cancellationToken);
}
```

---

### IN-03: Planet and Transformation entities lack audit fields (no BaseEntity inheritance)

**File:** `src/OpenCode.Domain/Entities/Planet.cs`
**File:** `src/OpenCode.Domain/Entities/Transformation.cs`
**Issue:** `Planet` and `Transformation` do not inherit from `BaseEntity`, so they lack `CreatedAt` and `UpdatedAt` audit fields. This is a deliberate design choice (lookup/reference entity vs. aggregate root), but it means these entities have no change tracking. If the requirements ever call for tracking when a Transformation was created, this will need refactoring.

**Suggestion:** Either document this design decision explicitly, or add a `ICreationTracked` / `IAuditable` interface hierarchy for more granular opt-in.

---

### IN-04: AddAsync calls SaveChangesAsync immediately per entity

**File:** `src/OpenCode.Domain/Implementations/Repository.cs:42-43`
**Issue:** Each `AddAsync` call is its own unit of work that immediately commits to the database. There is no mechanism for batching operations (e.g., bulk-importing characters). For the CRUD API use case this is acceptable, but if batch endpoints are added later, the repository will need a `SaveChanges` or `UnitOfWork` pattern.

**Suggestion:** Consider adding a `SaveChangesAsync` method to `IRepository<T>` or introducing a `IUnitOfWork` abstraction.

---

### IN-05: PagedResult.Data is IEnumerable<T> allowing multiple enumeration

**File:** `src/OpenCode.Domain/Pagination/PagedResult.cs:5`
**Issue:** `Data` is typed as `IEnumerable<T>` with a default of `Enumerable.Empty<T>()`. Consumers that enumerate multiple times (e.g., counting then iterating) will cause re-execution of the underlying query or re-materialization. The data is already materialized by `ToListAsync()` in the repository, but the interface type doesn't enforce this.

**Fix:** Change to `IReadOnlyList<T>` or `IReadOnlyCollection<T>`:
```csharp
public IReadOnlyList<T> Data { get; set; } = Array.Empty<T>();
```

---

### IN-06: Fragile relative path traversal in schema isolation tests

**File:** `tests/OpenCode.Domain.Tests/Migrations/SchemaIsolationTests.cs:5-6`
**Issue:** The migration test discovers files using `Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "OpenCode.Domain")`. This makes 5 upward-directory hops, which is fragile against build output restructuring, test runner changes, or different `dotnet test` configurations (e.g., `-o` flag).

**Fix:** Use a `testsettings.json` or environment variable to configure the path, or discover the solution root via a marker file:
```csharp
private static string FindSolutionRoot()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir != null && !dir.GetFiles("*.sln").Any())
        dir = dir.Parent;
    return Path.Combine(dir!.FullName, "src", "OpenCode.Domain");
}
```

---

### IN-07: ArtistGenre_DoesNotInheritBaseEntity test encodes a design constraint

**File:** `tests/OpenCode.Domain.Tests/Entities/EntityPropertyTests.cs:132-136`
**Issue:** The test explicitly asserts that `ArtistGenre` does NOT inherit from `BaseEntity`. This encodes a specific design assumption about the join entity. While this is correct today, if the requirements later change and `ArtistGenre` needs audit fields, this test will break with no explanatory comment about why the constraint exists.

**Suggestion:** Add a brief comment explaining why `ArtistGenre` intentionally omits BaseEntity (it's a pure join table with no business-meaningful timestamps).

---

_Reviewed: 2026-04-30T10:00:00Z_
_Reviewer: gsd-code-reviewer (standard depth)_
_Depth: standard_
