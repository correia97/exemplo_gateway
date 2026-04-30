---
phase: 11-testcontainers-integration
fixed_at: 2026-04-30T12:00:00Z
review_path: .planning/phases/11-testcontainers-integration/11-REVIEW.md
iteration: 1
findings_in_scope: 4
fixed: 4
skipped: 0
status: all_fixed
---

# Phase 11: Code Review Fix Report — TestContainers Integration Tests

**Fixed at:** 2026-04-30T12:00:00Z
**Source review:** `.planning/phases/11-testcontainers-integration/11-REVIEW.md`
**Iteration:** 1

**Summary:**
- Findings in scope: 4 (all warnings)
- Fixed: 4
- Skipped: 0

## Fixed Issues

### WR-01: Multiple PostgreSQL containers per test suite run

**Files modified:**
- `tests/OpenCode.Integration.Tests/Fixtures/IntegrationCollection.cs` (NEW)
- `tests/OpenCode.Integration.Tests/Fixtures/IntegrationTestBase.cs`
- `tests/OpenCode.Integration.Tests/Repositories/CharacterRepositoryTests.cs`
- `tests/OpenCode.Integration.Tests/Repositories/GenreRepositoryTests.cs`
- `tests/OpenCode.Integration.Tests/Repositories/ArtistRepositoryTests.cs`
- `tests/OpenCode.Integration.Tests/Repositories/AlbumRepositoryTests.cs`
- `tests/OpenCode.Integration.Tests/Repositories/TrackRepositoryTests.cs`
- `tests/OpenCode.Integration.Tests/Endpoints/CharactersEndpointsTests.cs`
- `tests/OpenCode.Integration.Tests/Endpoints/MusicEndpointsTests.cs`
- `tests/OpenCode.Integration.Tests/Endpoints/CorrelationIdTests.cs`
- `tests/OpenCode.Integration.Tests/Schema/SchemaIsolationTests.cs`

**Commit:** `6465e3d`

**Applied fix:**
- Created `IntegrationCollection.cs` with `[CollectionDefinition("PostgresIntegration")]` and `ICollectionFixture<PostgresFixture>` to share a single PostgreSQL container across all test classes.
- Removed `IClassFixture<PostgresFixture>` from `IntegrationTestBase` — the abstract base class no longer declares a fixture, letting the collection definition control lifecycle.
- Added `[Collection("PostgresIntegration")]` attribute to all 9 test classes that inherit from `IntegrationTestBase`. This ensures xUnit creates one `PostgresFixture` instance shared across all collection members instead of one per class (previously 9 concurrent containers).

---

### WR-02: Minimal TestServer bypasses full middleware pipeline

**Files modified:**
- `tests/OpenCode.Integration.Tests/Endpoints/CharactersEndpointsTests.cs`
- `tests/OpenCode.Integration.Tests/Endpoints/MusicEndpointsTests.cs`

**Commit:** `aaee764`

**Applied fix:**
- Added `app.UseCorrelationId()` middleware registration to both `CreateTestHost()` methods. Previously the minimal `TestServer` configured only DbContext and repository services with no middleware, causing endpoint tests to bypass the Correlation ID middleware that is part of the production pipeline.
- This ensures endpoint tests exercise the Correlation ID middleware while still using the minimal test host pattern.

---

### WR-03: Seed data coupling in count assertions

**Files modified:**
- `tests/OpenCode.Integration.Tests/Repositories/CharacterRepositoryTests.cs`
- `tests/OpenCode.Integration.Tests/Repositories/GenreRepositoryTests.cs`

**Commit:** `2a0c885`

**Applied fix:**
- **CharacterRepositoryTests.GetAll_Pagination_CorrectCount:** Changed `Assert.Equal(16, page1.TotalCount)` (exact count including seed Goku) to `Assert.True(page1.TotalCount >= 15)` (at least the test's own data). Changed `Assert.Equal(6, page2.Data.Count())` to `Assert.True(page2.Data.Count() >= 5)`.
- **GenreRepositoryTests.GetAll_Pagination_CorrectCount:** Changed `Assert.Equal(4, result.TotalCount)` (exact count including seed Rock) to `Assert.True(result.TotalCount >= 3)`.
- Assertions are now relative to what each test inserts rather than exact totals that depend on seed data changes.

---

### WR-04: Repository-owned DbContexts not disposed in tests

**Files modified:**
- `tests/OpenCode.Integration.Tests/Repositories/CharacterRepositoryTests.cs`
- `tests/OpenCode.Integration.Tests/Repositories/GenreRepositoryTests.cs`
- `tests/OpenCode.Integration.Tests/Repositories/ArtistRepositoryTests.cs`
- `tests/OpenCode.Integration.Tests/Repositories/AlbumRepositoryTests.cs`
- `tests/OpenCode.Integration.Tests/Repositories/TrackRepositoryTests.cs`

**Commit:** `ff3dc2c`

**Applied fix:**
- Changed all 5 `CreateRepo()` methods from creating an internal `DbContext` (never disposed) to accepting a `DbContext` parameter. The method signature is now `CreateRepo(DragonBallContext ctx)` or `CreateRepo(MusicContext ctx)`.
- Updated every test method to use `using var ctx = CreateDragonBallContext()` (or `CreateMusicContext()`) and pass `ctx` to `CreateRepo(ctx)`. The `using` statement ensures the `DbContext` is properly disposed when each test method completes.
- Added `using OpenCode.Domain.Data` directive to all 5 files for the `DragonBallContext`/`MusicContext` type resolution.
- This eliminates the resource leak where each `CreateRepo()` call created an undisposed `DbContext` (at least 2 per test — one in `CreateRepo()` and one in the test's seed data context).

---

## Skipped Issues

None — all 4 warning findings were successfully fixed.

---

_Fixed: 2026-04-30T12:00:00Z_
_Fixer: gsd-code-fixer agent_
_Iteration: 1_
