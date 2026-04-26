# Phase 2 Discussion Context — Database & Models

## Decisions

### Domain Models

**Dragon Ball — Character entity (DBALL-04):**
- Fields: `Id` (int, PK), `Name` (string, required), `IsEarthling` (bool), `IntroductionPhase` (string), `PictureUrl` (string)
- Audit: `CreatedAt`, `UpdatedAt` (auto-set by EF Core)

**Music — Genre entity (MUSIC-04):**
- Fields: `Id` (int, PK), `Name` (string, required), `Description` (string)
- Audit: `CreatedAt`, `UpdatedAt`
- Many-to-Many with Artist (join table `ArtistGenres`)

**Music — Artist entity (MUSIC-05):**
- Fields: `Id` (int, PK), `Name` (string, required), `Biography` (string)
- Audit: `CreatedAt`, `UpdatedAt`
- Many-to-Many with Genre via join table
- One-to-Many with Album

**Music — Album entity (MUSIC-06):**
- Fields: `Id` (int, PK), `Title` (string, required), `ReleaseDate` (DateOnly), `CoverUrl` (string), `ArtistId` (int, FK)
- Audit: `CreatedAt`, `UpdatedAt`
- One-to-Many with Track

**Music — Track entity (MUSIC-07, MUSIC-08):**
- Fields: `Id` (int, PK), `Name` (string, required), `TrackNumber` (int), `Duration` (TimeSpan?), `Lyrics` (string), `AlbumId` (int?, nullable FK for singles), `IsStandalone` (bool, default false)
- Audit: `CreatedAt`, `UpdatedAt`
- Nullable AlbumId + IsStandalone flag handles singles

### Repository Pattern
- Generic `IRepository<T>` base with CRUD + pagination
- Specific interfaces (`ICharacterRepository`, `IGenreRepository`, etc.) inherit from `IRepository<T>` for entity-specific query methods
- Paginated queries return `PagedResult<T>` envelope

### Pagination Contract
Response envelope:
```json
{
  "data": [],
  "totalCount": 100,
  "page": 1,
  "pageSize": 10,
  "totalPages": 10
}
```

### Database Strategy
- Single PostgreSQL 17 database, 3 schemas: `dragonball`, `music`, `keycloak`
- Each DbContext uses `HasDefaultSchema()` for its schema
- Per-schema database users: `dragonball_user`, `music_user`, `keycloak_user`
- Init SQL script creates schemas + users + grants
- EF Core migrations isolated per schema

### EF Core Configuration
- Schema isolation via `HasDefaultSchema()` in `OnModelCreating`
- Genre-Artist Many-to-Many via `ArtistGenres` join table
- Fluent API for relationship mapping
- All entities in solution root `src/OpenCode.Domain/` project (new project)
