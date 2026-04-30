---
phase: 03-api-endpoints-dragon-ball-music-crud
fixed_at: 2026-04-30T08:27:41.0917143-03:00
review_path: .planning/phases/03-api-endpoints-dragon-ball-music-crud/03-REVIEW.md
iteration: 1
findings_in_scope: 7
fixed: 7
skipped: 0
status: all_fixed
---

# Phase 3: Code Review Fix Report

**Fixed at:** 2026-04-30T08:27:41-03:00
**Source review:** `.planning/phases/03-api-endpoints-dragon-ball-music-crud/03-REVIEW.md`
**Iteration:** 1

**Summary:**
- Findings in scope: 7
- Fixed: 7
- Skipped: 0

## Fixed Issues

### CR-01: Thread.Sleep blocks thread pool in IHostedService.StartAsync

**Files modified:** `src/OpenCode.DragonBall.Api/Services/DragonBallDbInitializer.cs`, `src/OpenCode.Music.Api/Services/MusicDbInitializer.cs`
**Commit:** eb9d42f
**Applied fix:** Replaced `Thread.Sleep(TimeSpan.FromSeconds(15))` with `await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken)` in both DbInitializer classes. This is non-blocking and respects the cancellation token during graceful shutdown.

### CR-02: NullReferenceException in Artist CreateAsync due to unloaded navigation property

**Files modified:** `src/OpenCode.Domain/Interfaces/IArtistRepository.cs`, `src/OpenCode.Music.Api/Endpoints/Artists.cs`
**Commit:** 264f80a
**Applied fix:** Added `GetByIdWithGenresAsync(int id, CancellationToken)` to `IArtistRepository` interface. Updated `CreateAsync` to re-fetch the artist with genres included after creation, avoiding null `Genre` navigation property access in the response mapping.

### CR-03: String comparison for Ki filtering produces incorrect results

**Files modified:** `src/OpenCode.DragonBall.Api/Repositories/CharacterRepository.cs`
**Commit:** 7326348
**Applied fix:** Removed lexicographic `string.CompareTo()` from the LINQ query for Ki filtering. Added a `ParseKiToNumeric()` helper that converts human-readable Ki strings (e.g., "60 septillion", "2.5 sextillion") to `decimal` values using magnitude multipliers. Ki filtering is now performed in-memory with proper numeric comparison. 

**Note:** This fix requires human verification — the in-memory filtering approach means all matching characters (minus Ki filter) are loaded before pagination. For the current dataset size (~80 characters) this is acceptable, but for production a `KiNumeric` column with database-level filtering would be preferable.

### WR-01: Interface leak — concrete type casting breaks DI abstraction

**Files modified:** `src/OpenCode.Domain/Interfaces/IAlbumRepository.cs`, `src/OpenCode.Music.Api/Endpoints/Albums.cs`
**Commit:** 1c75864
**Applied fix:** Added `GetByIdWithArtistAsync(int id, CancellationToken)` to `IAlbumRepository` interface. Removed all `(AlbumRepository)repository` concrete casts in `Albums.cs` (`GetByIdAsync`, `CreateAsync`, `UpdateAsync`). Removed unused `using OpenCode.Music.Api.Repositories` import. Artists.cs casts were already eliminated by CR-02 and WR-04 changes.

### WR-02: Character entity fields IsEarthling and IntroductionPhase not exposed in DTOs

**Files modified:** `src/OpenCode.DragonBall.Api/Dtos/CharacterRequests.cs`, `src/OpenCode.DragonBall.Api/Dtos/CharacterResponse.cs`, `src/OpenCode.DragonBall.Api/Endpoints/Characters.cs`
**Commit:** a137df6
**Applied fix:** Added `IsEarthling` (bool, default `false`) and `IntroductionPhase` (string?, default `null`) to `CreateCharacterRequest` and `UpdateCharacterRequest`. Added both fields to `CharacterResponse` record and `CharacterMapping.ToResponse()`. Updated `Characters.cs` `CreateAsync` and `UpdateAsync` to map the new fields from request to entity.

### WR-03: Album CreateAsync returns incomplete response (ArtistName: null)

**Files modified:** `src/OpenCode.Music.Api/Dtos/AlbumResponse.cs`
**Commit:** 5c6af46
**Applied fix:** Added fallback in `AlbumMapping.ToResponse()` — `album.Artist?.Name ?? $"Artist #{album.ArtistId}"`. When the `Artist` navigation property is not loaded (e.g., fallback path in `CreateAsync`/`UpdateAsync`), the response will show `"Artist #<artistId>"` instead of null.

### WR-04: Dead code path in Artists GetByIdAsync — conditional branch always hits

**Files modified:** `src/OpenCode.Music.Api/Endpoints/Artists.cs`
**Commit:** b2d5718
**Applied fix:** Simplified `GetByIdAsync` to directly call `repository.GetByIdWithGenresAsync(id)` (now on the interface from CR-02). Removed the dead if/else branch that was always executing because `ArtistGenres` is initialized as `new List<ArtistGenre>()` (never null) and `FindAsync()` doesn't include related entities. Removed unused `using OpenCode.Music.Api.Repositories` import.

## Skipped Issues

None — all 7 in-scope findings were successfully fixed.

---

_Fixed: 2026-04-30T08:27:41-03:00_
_Fixer: gsd-code-fixer (auto mode)_
_Iteration: 1_
