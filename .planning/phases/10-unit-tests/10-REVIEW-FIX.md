---
phase: 10-unit-tests
fixed_at: 2026-04-30T08:00:00Z
review_path: .planning/phases/10-unit-tests/10-REVIEW.md
iteration: 1
findings_in_scope: 7
fixed: 7
skipped: 0
status: all_fixed
---

# Phase 10: Code Review Fix Report

**Fixed at:** 2026-04-30T08:00:00Z
**Source review:** `.planning/phases/10-unit-tests/10-REVIEW.md`
**Iteration:** 1

**Summary:**
- Findings in scope: 7
- Fixed: 7
- Skipped: 0

## Fixed Issues

### WR-01: Update Validators Systematically Under-Tested

**Files modified:**
- `tests/OpenCode.Api.Tests/Validators/UpdateCharacterValidatorTests.cs`
- `tests/OpenCode.Api.Tests/Validators/GenreValidatorTests.cs`
- `tests/OpenCode.Api.Tests/Validators/ArtistValidatorTests.cs`
- `tests/OpenCode.Api.Tests/Validators/AlbumValidatorTests.cs`
- `tests/OpenCode.Api.Tests/Validators/TrackValidatorTests.cs`

**Commit:** `9015541`

**Applied fix:** Added 7 tests to `UpdateCharacterValidatorTests` (Name max length, Race empty, Race max length, Ki empty, Ki max length, MaxKi max length, Description max length). Added 3 tests to `GenreValidatorTests` (Create Description max length, Update Name max length, Update Description max length). Added 2 tests to `ArtistValidatorTests` (Update Name max length, Update Biography max length). Added 4 tests to `AlbumValidatorTests` (Create/Update Title max length, Create/Update CoverUrl max length). Added 6 tests to `TrackValidatorTests` (Create Duration zero/exactly 2h boundary, Update Name max length, Update TrackNumber zero, Update Duration negative, Update Duration exceeds 2h).

---

### WR-02: "NullBody_ReturnsBadRequest" Test Name Is Misleading — Exposes Production Bug

**Files modified:**
- `tests/OpenCode.Api.Tests/Services/ValidationPipelineTests.cs`

**Commit:** `f31ce74`

**Applied fix:**
1. Renamed `NullBody_ReturnsBadRequest` to `EmptyBody_ReturnsBadRequest` (it sends `{}`, not `null`).
2. Added a true null-body test `NullBody_ReturnsBadRequest` that sends `"null"` as raw JSON body content.
3. Added null guard in `AutoValidationFilter`: when `arg is null`, returns `TypedResults.BadRequest(new { error = "Request body cannot be null" })` instead of passing null to the endpoint handler.
4. Added `using System.Text;` for `Encoding.UTF8`.

---

### WR-03: SchemaIsolationTests Path Traversal Is Fragile

**Files modified:**
- `tests/OpenCode.Domain.Tests/Migrations/SchemaIsolationTests.cs`

**Commit:** `9fefbb8`

**Applied fix:** Replaced the hardcoded `"..", "..", "..", "..", ".."` path traversal from `AppContext.BaseDirectory` with a `FindProjectRoot()` method that walks up the directory tree looking for `*.slnx` (the solution marker file). This is resilient to build configuration changes (Debug vs Release), .NET version changes, and different output directory structures.

---

### WR-04: Album Validator Title and CoverUrl MaxLength Untested

**Files modified:**
- `tests/OpenCode.Api.Tests/Validators/AlbumValidatorTests.cs`

**Commit:** `9015541` (covered by WR-01 batch)

**Applied fix:** Added `CreateAlbum_Should_Fail_When_Title_Exceeds_MaxLength` (201 chars, boundary at 200), `CreateAlbum_Should_Fail_When_CoverUrl_Exceeds_MaxLength` (URL exceeding 500 chars), `UpdateAlbum_Should_Fail_When_Title_Exceeds_MaxLength`, and `UpdateAlbum_Should_Fail_When_CoverUrl_Exceeds_MaxLength`.

---

### WR-05: Genre Validator Description MaxLength Untested

**Files modified:**
- `tests/OpenCode.Api.Tests/Validators/GenreValidatorTests.cs`

**Commit:** `9015541` (covered by WR-01 batch)

**Applied fix:** Added `CreateGenre_Should_Fail_When_Description_Exceeds_MaxLength` (1001 chars, boundary at 1000) and `UpdateGenre_Should_Fail_When_Description_Exceeds_MaxLength`.

---

### WR-06: Track Duration Boundary Values Not Tested

**Files modified:**
- `tests/OpenCode.Api.Tests/Validators/TrackValidatorTests.cs`

**Commit:** `9015541` (covered by WR-01 batch)

**Applied fix:** Added `CreateTrack_Should_Fail_When_Duration_Zero` (`TimeSpan.Zero` — boundary for `GreaterThan(TimeSpan.Zero)`) and `CreateTrack_Should_Fail_When_Duration_Exactly_TwoHours` (`TimeSpan.FromHours(2)` — boundary for `LessThan(TimeSpan.FromHours(2))`). Also added Update variants for Duration negative, Duration exceeds 2h, TrackNumber zero, and Name max length.

---

### WR-07: KeycloakClaimsTransformation — Not Tested with Empty Roles Array

**Files modified:**
- `tests/OpenCode.Api.Tests/Services/KeycloakRolesClaimsTransformationTests.cs`

**Commit:** `4f13a23`

**Applied fix:** Added `TransformAsync_When_Roles_Empty_Returns_Original` test that passes `{ roles: [] }` and verifies no role claims are added. Also fixed a pre-existing build error (constructor requires `ILogger<T>`) by using `LoggerFactory().CreateLogger<T>()` instead of `new()`.

---

_Fixed: 2026-04-30T08:00:00Z_
_Fixer: the agent (gsd-code-fixer)_
_Iteration: 1_
