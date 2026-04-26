# Phase 2: Database & Models — Research

**Researched:** 2026-04-24
**Domain:** EF Core 10 / Npgsql / PostgreSQL 17 / Repository Pattern
**Confidence:** HIGH

## Summary

Phase 2 builds the database foundation: PostgreSQL schemas, domain entities, EF Core DbContexts with schema isolation, generic repository abstractions, and pagination helpers. The core technical decision is using `modelBuilder.HasDefaultSchema("schemaName")` per DbContext to enforce schema-level isolation — each API's DbContext targets only its own schema. Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1 is the latest stable version for .NET 10 and is confirmed compatible.

The Domain project (`src/OpenCode.Domain/`) is a new class library that will hold all entities, DbContexts, the pagination envelope, and repository interfaces. This creates a clean dependency: API projects reference Domain, Domain references EF Core packages (no direct database dependency from API projects).

The PostgreSQL init script must run before any service connects. It creates three schemas (`dragonball`, `music`, `keycloak`), three user accounts with schema-scoped permissions, and grants the minimum privileges needed for each user to operate within their schema.

**Primary recommendation:** Use `HasDefaultSchema()` on each DbContext's `OnModelCreating`, create a single `init.sql` for schema/user creation, implement `PagedResult<T>` as a generic envelope, and define `IRepository<T>` with async CRUD + pagination methods.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
1. **Domain Models**: Character (name, isEarthling, introductionPhase, pictureUrl), Genre (name, description), Artist (name, biography), Album (title, releaseDate, coverUrl, artistId), Track (name, trackNumber, duration, lyrics, albumId nullable, isStandalone)
2. **Genre-Artist**: Many-to-Many via `ArtistGenres` join table
3. **Audit fields**: CreatedAt, UpdatedAt on all entities
4. **Repository Pattern**: Generic `IRepository<T>` base with CRUD + pagination; specific interfaces inherit
5. **Pagination**: Envelope `{ data, totalCount, page, pageSize, totalPages }`
6. **Database users**: Per-schema users (dragonball_user, music_user, keycloak_user)
7. **EF Core**: HasDefaultSchema() per DbContext, Fluent API for mapping
8. **New project**: `src/OpenCode.Domain/` class library for entities + DbContexts + pagination helper

### the agent's Discretion
(None specified in CONTEXT.md for Phase 2)

### Deferred Ideas (OUT OF SCOPE)
(None specified in CONTEXT.md for Phase 2)
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| INFRA-01 | Single PostgreSQL 17 database with three schemas | `init.sql` creates dragonball, music, keycloak schemas via `CREATE SCHEMA IF NOT EXISTS` |
| INFRA-02 | PostgreSQL init script creates all schemas before any service starts | `init.sql` runs on container first-start via Docker volume mount to `/docker-entrypoint-initdb.d/` |
| INFRA-06 | Separate database users per schema with schema-level permissions | `CREATE USER` + `GRANT USAGE ON SCHEMA` + `GRANT ALL ON ALL TABLES IN SCHEMA` per user |
| DBALL-02 | EF Core DbContext targets `dragonball` schema with `HasDefaultSchema()` | `modelBuilder.HasDefaultSchema("dragonball")` in `DragonBallContext` |
| DBALL-03 | Repository pattern with EF Core for data access | `IRepository<T>` interface with CRUD + pagination, `Repository<T>` base implementation |
| DBALL-04 | Character entity fields | `Character` entity with Name, IsEarthling, IntroductionPhase, PictureUrl |
| MUSIC-02 | EF Core DbContext targets `music` schema with `HasDefaultSchema()` | `modelBuilder.HasDefaultSchema("music")` in `MusicContext` |
| MUSIC-03 | Repository pattern with EF Core for data access | Shared `IRepository<T>` and specific interfaces (IGenreRepository, etc.) |
| MUSIC-04 | Genre entity with name and description | `Genre` entity with Name, Description; M2M with Artist |
| MUSIC-05 | Artist entity with name, biography, genre association | `Artist` entity with Name, Biography; M2M Genre, 1:M Album |
| MUSIC-06 | Album entity with title, releaseDate, coverUrl, artist association | `Album` entity with Title, ReleaseDate, CoverUrl, ArtistId FK |
| MUSIC-07 | Track entity with name, trackNumber, duration, lyrics, album association | `Track` entity with Name, TrackNumber, Duration, Lyrics, AlbumId FK nullable |
| MUSIC-08 | Standalone singles modeled as tracks with nullable albumId + isStandalone flag | `IsStandalone` bool (default false) + nullable `AlbumId` FK |
</phase_requirements>

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|-------------|----------------|-----------|
| Schema creation (DDL) | Database / Storage | — | `init.sql` runs inside PostgreSQL container at startup |
| Domain entity definitions | API / Backend | — | Entities are C# classes in the Domain project, consumed by API projects |
| Database model mapping | API / Backend | — | EF Core DbContexts in Domain project define table mappings |
| Schema isolation | API / Backend | — | `HasDefaultSchema()` per DbContext ensures each schema gets its own tables |
| Data access (CRUD) | API / Backend | — | Repository pattern in Domain project, consumed by API endpoints |
| Pagination | API / Backend | — | `PagedResult<T>` envelope computed in repository layer |
| Migration management | API / Backend | — | `dotnet ef migrations` commands run against Domain project |
| Database user management | Database / Storage | — | `init.sql` creates users and grants permissions at container start |

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Microsoft.EntityFrameworkCore | 10.0.7 | ORM framework | Latest .NET 10 EF Core [VERIFIED: dotnet package search] |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.1 | PostgreSQL provider | Latest Npgsql EF Core provider for .NET 10 [VERIFIED: dotnet package search] |
| Microsoft.EntityFrameworkCore.Design | 10.0.7 | EF Core CLI tools | Required for `dotnet ef migrations` tooling [VERIFIED: dotnet package search] |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Microsoft.EntityFrameworkCore.Tools | 10.0.7 | PMC tools | Only if using Visual Studio Package Manager Console |
| Npgsql | 10.0.1 | ADO.NET provider | Transitive dependency of EF Core provider |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| EF Core | Dapper | Richer LINQ querying, migrations, change tracking — Dapper is faster but requires manual SQL; PoC favors rapid development with EF Core |
| Npgsql EF Core | Pomelo EF Core (MySQL) | Project is PostgreSQL-specific, Npgsql is the official provider |

**Installation:**
```bash
dotnet add src/OpenCode.Domain/OpenCode.Domain.csproj package Microsoft.EntityFrameworkCore --version 10.0.7
dotnet add src/OpenCode.Domain/OpenCode.Domain.csproj package Npgsql.EntityFrameworkCore.PostgreSQL --version 10.0.1
dotnet add src/OpenCode.Domain/OpenCode.Domain.csproj package Microsoft.EntityFrameworkCore.Design --version 10.0.7
```

**EF Core CLI tool (required for migrations):**
```bash
dotnet tool install --global dotnet-ef
```

**Central package management (Directory.Packages.props):**
```xml
<PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.7" />
<PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.7" />
<PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.1" />
```

## Architecture Patterns

### System Architecture Diagram

```
                          ┌─────────────────────────────┐
                          │     PostgreSQL 17 (1 DB)    │
                          │  ┌───────────────────────┐  │
                          │  │ dragonball schema     │  │
                          │  │ ┌───────────────────┐ │  │
  ┌──────────────┐        │  │ │ Characters        │ │  │
  │ DragonBall   │───────►│  │ └───────────────────┘ │  │
  │ API          │        │  └───────────────────────┘  │
  │ (DbContext   │        │  ┌───────────────────────┐  │
  │  → dragonball│        │  │ music schema          │  │
  │  schema)     │        │  │ ┌───────────────────┐ │  │
  └──────────────┘        │  │ │ Genres ←→ Artists │ │  │
                          │  │ │   (ArtistGenres)  │ │  │
  ┌──────────────┐        │  │ ├───────────────────┤ │  │
  │ Music        │───────►│  │ │ Artists → Albums  │ │  │
  │ API          │        │  │ ├───────────────────┤ │  │
  │ (DbContext   │        │  │ │ Albums → Tracks   │ │  │
  │  → music     │        │  │ └───────────────────┘ │  │
  │  schema)     │        │  └───────────────────────┘  │
  └──────────────┘        │  ┌───────────────────────┐  │
                          │  │ keycloak schema       │  │
                          │  │ (managed by Keycloak) │  │
                          │  └───────────────────────┘  │
                          └─────────────────────────────┘

  ┌──────────────┐        ┌─────────────────────────────┐
  │ PostgreSQL   │        │  src/OpenCode.Domain/       │
  │ Container    │        │                             │
  │ init.sql     │───────►│  ├─ Entities/               │
  │ (schemas +   │        │  │   ├─ Character.cs        │
  │  users +     │        │  │   ├─ Genre.cs            │
  │  grants)     │        │  │   ├─ Artist.cs           │
  │              │        │  │   ├─ Album.cs            │
  │              │        │  │   ├─ Track.cs            │
  │              │        │  │   └─ ArtistGenre.cs      │
  │              │        │  ├─ Data/                   │
  │              │        │  │   ├─ DragonBallContext.cs│
  │              │        │  │   ├─ MusicContext.cs     │
  │              │        │  │   └─ init.sql            │
  │              │        │  ├─ Pagination/             │
  │              │        │  │   └─ PagedResult.cs      │
  │              │        │  └─ Interfaces/             │
  │              │        │      ├─ IRepository.cs      │
  │              │        │      ├─ ICharacterRepo.cs   │
  │              │        │      ├─ IGenreRepository.cs │
  │              │        │      ├─ IArtistRepository.cs│
  │              │        │      ├─ IAlbumRepository.cs │
  │              │        │      └─ ITrackRepository.cs │
  └──────────────┘        └─────────────────────────────┘
```

### Recommended Project Structure
```
src/OpenCode.Domain/
├── Entities/
│   ├── BaseEntity.cs          # Id, CreatedAt, UpdatedAt
│   ├── Character.cs           # Dragon Ball
│   ├── Genre.cs               # Music — genre tags
│   ├── Artist.cs              # Music — artist/band
│   ├── ArtistGenre.cs         # Join entity for M2M
│   ├── Album.cs               # Music — album
│   └── Track.cs               # Music — track/single
├── Data/
│   ├── DragonBallContext.cs    # HasDefaultSchema("dragonball")
│   ├── MusicContext.cs         # HasDefaultSchema("music")
│   └── init.sql                # PostgreSQL init script
├── Pagination/
│   └── PagedResult.cs          # { Data, TotalCount, Page, PageSize, TotalPages }
├── Interfaces/
│   ├── IRepository.cs          # Generic IRepository<T>
│   ├── ICharacterRepository.cs
│   ├── IGenreRepository.cs
│   ├── IArtistRepository.cs
│   ├── IAlbumRepository.cs
│   └── ITrackRepository.cs
└── OpenCode.Domain.csproj
```

### Pattern 1: Schema Isolation with HasDefaultSchema()
**What:** Each DbContext sets its own default schema in `OnModelCreating`, ensuring all tables for that context are created only in its schema.
**When to use:** Always — this is the core isolation mechanism. Each API's DbContext targets exactly one schema.
**Example:**
```csharp
public class DragonBallContext : DbContext
{
    public DbSet<Character> Characters => Set<Character>();

    public DragonBallContext(DbContextOptions<DragonBallContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dragonball");

        modelBuilder.Entity<Character>(entity =>
        {
            entity.ToTable("Characters");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.IntroductionPhase).HasMaxLength(100);
            entity.Property(c => c.PictureUrl).HasMaxLength(500);
        });
    }
}
```
[VERIFIED: learn.microsoft.com/en-us/ef/core/modeling/entity-types#table-schema]

### Pattern 2: Many-to-Many with Join Entity (Artist ↔ Genre)
**What:** Explicit join entity `ArtistGenre` for full control over the relationship.
**When to use:** When the join table has no extra payload, explicit `ArtistGenre` entity is clearer than automatic shadow properties.
**Example:**
```csharp
public class Artist
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Biography { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<ArtistGenre> ArtistGenres { get; set; } = new List<ArtistGenre>();
    public ICollection<Album> Albums { get; set; } = new List<Album>();
}

public class ArtistGenre
{
    public int ArtistId { get; set; }
    public Artist Artist { get; set; } = null!;
    public int GenreId { get; set; }
    public Genre Genre { get; set; } = null!;
}

// In MusicContext.OnModelCreating:
modelBuilder.Entity<Artist>()
    .HasMany(a => a.ArtistGenres)
    .WithOne(ag => ag.Artist)
    .HasForeignKey(ag => ag.ArtistId);

modelBuilder.Entity<Genre>()
    .HasMany(g => g.ArtistGenres)
    .WithOne(ag => ag.Genre)
    .HasForeignKey(ag => ag.GenreId);

modelBuilder.Entity<ArtistGenre>()
    .HasKey(ag => new { ag.ArtistId, ag.GenreId });
```
[CITED: learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many]

### Pattern 3: Generic Repository with Pagination
**What:** `IRepository<T>` defines async CRUD methods. `PagedResult<T>` wraps paginated query results.
**When to use:** Every entity gets a specific interface that inherits from `IRepository<T>`. The base `Repository<T>` implementation uses EF Core generics.
**Example:**
```csharp
public class PagedResult<T>
{
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedResult<T>> GetAllAsync(int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
}
```

### Anti-Patterns to Avoid
- **Putting all entities in one DbContext:** Each API's DbContext should only include entities for its schema. `DragonBallContext` should NOT have `Genre` or `Track` entities.
- **Mixing schemas via migrations:** Running `dotnet ef migrations add` on a context that has `HasDefaultSchema("dragonball")` must only create tables in that schema. Verify migration SQL output.
- **Sharing connection strings with superuser:** Each DbContext should be configured to use its schema-specific user (e.g., `dragonball_user`), not the `postgres` superuser.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Database schema management | Raw SQL table creation scripts | EF Core Migrations | Migrations track schema evolution, generate Up/Down scripts, support rollback |
| Database connection pooling | Custom connection pool | Npgsql built-in pooling | Npgsql provides efficient pooling with configurable min/max pool sizes |
| Pagination math | Offset/limit with manual count | `PagedResult<T>` pattern | Standardizes pagination across all list endpoints, envelope format is locked decision |
| Audit timestamps | Manual `DateTime.Now` in every handler | EF Core shadow properties or base entity `SaveChanges` override | Centralizes audit logic, prevents inconsistently-set timestamps |
| Schema isolation | Multiple databases or manual table prefixing | `HasDefaultSchema()` per DbContext | Built into EF Core, automatically prefixes all table names with schema name |

**Key insight:** EF Core migrations paired with `HasDefaultSchema()` handle schema isolation automatically. The combination of generic `IRepository<T>` with `PagedResult<T>` eliminates repetitive pagination code across all endpoints.

## Common Pitfalls

### Pitfall 1: HasDefaultSchema() Overwritten by Entity-Specific ToTable()
**What goes wrong:** `modelBuilder.HasDefaultSchema("dragonball")` sets default schema, but if an entity also has `.ToTable("Characters", schema: "music")`, it overrides the default.
**Why it happens:** The default schema applies to all tables that don't explicitly specify a schema. Entity-level `ToTable()` with a schema argument takes precedence.
**How to avoid:** Never specify schema in per-entity `ToTable()` calls when using `HasDefaultSchema()`. Let the default apply.
**Warning signs:** Migration SQL shows `CREATE TABLE "music"."Characters"` instead of `CREATE TABLE "dragonball"."Characters"`.

### Pitfall 2: Migrations Cross-Contaminate Between DbContexts
**What goes wrong:** Running `dotnet ef migrations add` for DragonBallContext accidentally picks up Music entities.
**Why it happens:** If both DbContexts are in the same project and you don't use `--context`, EF Core might pick the wrong one or both.
**How to avoid:** Always specify `--context` or use separate migration directories per context. Use `--output-dir Migrations/DragonBall` and `--output-dir Migrations/Music`.
**Warning signs:** Migration `.cs` file contains entity types from both schemas.

### Pitfall 3: `dotnet ef` CLI Tool Not Installed
**What goes wrong:** `dotnet ef migrations add` fails with "command not found".
**Why it happens:** `dotnet-ef` is a global/local tool that must be explicitly installed, unlike `dotnet build` which ships with the SDK.
**How to avoid:** Install with `dotnet tool install --global dotnet-ef`. Verify with `dotnet ef --version`.
**Warning signs:** The first `dotnet ef` command fails.

### Pitfall 4: `DesignTimeDbContextFactory` Missing for Migrations
**What goes wrong:** `dotnet ef migrations add` fails with "Unable to create an object of type 'DragonBallContext'".
**Why it happens:** The EF Core CLI needs to instantiate the DbContext at design time to discover the model. Without a parameterless constructor or `IDesignTimeDbContextFactory`, it doesn't know how to create it.
**How to avoid:** Create `DragonBallContextFactory` and `MusicContextFactory` implementing `IDesignTimeDbContextFactory<T>`, returning a context configured with a development connection string.
**Warning signs:** `dotnet ef migrations add` errors about missing parameterless constructor.

## Code Examples

### Base Entity with Audit Fields
```csharp
// src/OpenCode.Domain/Entities/BaseEntity.cs
namespace OpenCode.Domain.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```
[CITED: learn.microsoft.com/en-us/ef/core/modeling/entity-types]

### Character Entity (Dragon Ball)
```csharp
// src/OpenCode.Domain/Entities/Character.cs
namespace OpenCode.Domain.Entities;

public class Character : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsEarthling { get; set; }
    public string? IntroductionPhase { get; set; }
    public string? PictureUrl { get; set; }
}
```

### Genre-Artist Many-to-Many Fluent Configuration
```csharp
// In MusicContext.OnModelCreating:
modelBuilder.Entity<Artist>(entity =>
{
    entity.ToTable("Artists");
    entity.HasKey(a => a.Id);
    entity.Property(a => a.Name).IsRequired().HasMaxLength(200);
    
    entity.HasMany(a => a.ArtistGenres)
          .WithOne(ag => ag.Artist)
          .HasForeignKey(ag => ag.ArtistId);
});

modelBuilder.Entity<Genre>(entity =>
{
    entity.ToTable("Genres");
    entity.HasKey(g => g.Id);
    entity.Property(g => g.Name).IsRequired().HasMaxLength(100);
    
    entity.HasMany(g => g.ArtistGenres)
          .WithOne(ag => ag.Genre)
          .HasForeignKey(ag => ag.GenreId);
});

modelBuilder.Entity<ArtistGenre>(entity =>
{
    entity.ToTable("ArtistGenres");
    entity.HasKey(ag => new { ag.ArtistId, ag.GenreId });
});
```
[CITED: learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many]

### DesignTimeDbContextFactory
```csharp
// src/OpenCode.Domain/Data/DragonBallContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OpenCode.Domain.Data;

public class DragonBallContextFactory : IDesignTimeDbContextFactory<DragonBallContext>
{
    public DragonBallContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DragonBallContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=opencode;Username=postgres;Password=postgres");
        return new DragonBallContext(optionsBuilder.Options);
    }
}
```

### PostgreSQL Init Script Pattern
```sql
-- src/OpenCode.Domain/Data/init.sql
-- Run by PostgreSQL container on first start
-- Mounted to /docker-entrypoint-initdb.d/

CREATE SCHEMA IF NOT EXISTS dragonball;
CREATE SCHEMA IF NOT EXISTS music;
CREATE SCHEMA IF NOT EXISTS keycloak;

-- Dragon Ball schema user
CREATE USER dragonball_user WITH PASSWORD 'dragonball_pass';
GRANT USAGE ON SCHEMA dragonball TO dragonball_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA dragonball TO dragonball_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA dragonball GRANT ALL ON TABLES TO dragonball_user;

-- Music schema user
CREATE USER music_user WITH PASSWORD 'music_pass';
GRANT USAGE ON SCHEMA music TO music_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA music TO music_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA music GRANT ALL ON TABLES TO music_user;

-- Keycloak schema user
CREATE USER keycloak_user WITH PASSWORD 'keycloak_pass';
GRANT USAGE ON SCHEMA keycloak TO keycloak_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA keycloak TO keycloak_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA keycloak GRANT ALL ON TABLES TO keycloak_user;
```
[ASSUMED — based on PostgreSQL documentation for schema-user isolation pattern; best practice for PoC]

### PagedResult<T>
```csharp
// src/OpenCode.Domain/Pagination/PagedResult.cs
namespace OpenCode.Domain.Pagination;

public class PagedResult<T>
{
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```
[VERIFIED: locked decision from CONTEXT.md]

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `dotnet ef` global tool | `dotnet ef` local/manifest tool | .NET 6+ | Must install explicitly; can pin version per-project |
| `.sln` (binary/solution) | `.slnx` (XML solution format) | .NET 10 | New XML-based format, easier to merge/manually edit |
| Separate EF Core + Npgsql packages | Unified Npgsql.EntityFrameworkCore.PostgreSQL | Always | Single package for PostgreSQL support |
| `ToTable("table", "schema")` per entity | `HasDefaultSchema("schema")` on model | EF Core 1.0 | Set schema once for entire context instead of per table |

**Deprecated/outdated:**
- `dotnet ef --tool-manifest` file management is being replaced by `dotnet tool manifest` commands
- `HasOne().WithMany()` without explicit FK still works but implicit shadow FKs are harder to debug; prefer explicit foreign keys

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | `modelBuilder.HasDefaultSchema("dragonball")` in `OnModelCreating` correctly scopes all tables of the `DragonBallContext` to the `dragonball` schema | Architecture Patterns, Pattern 1 | Low — verified in Microsoft docs; if wrong, tables end up in `public` schema |
| A2 | `init.sql` mounted to `/docker-entrypoint-initdb.d/` runs before any PostgreSQL client connections can be made | Code Examples, PostgreSQL Init Script | Low — standard PostgreSQL container behavior documented by Docker |
| A3 | `GRANT ALL` + `ALTER DEFAULT PRIVILEGES` is sufficient for EF Core migrations + CRUD operations | Code Examples, PostgreSQL Init Script | Low — may need `GRANT USAGE ON SCHEMA` explicitly; script already includes it |
| A4 | `dotnet ef migrations` commands require `IDesignTimeDbContextFactory<T>` | Common Pitfalls, Pitfall 4 | Medium — could also add parameterless constructor with default connection string, but factory is the recommended pattern |
| A5 | `dotnet ef --version` will show the latest installed version | Common Pitfalls, Pitfall 3 | Low — confirmed by EF Core documentation |
| A6 | `Npgsql.EntityFrameworkCore.PostgreSQL` 10.x is fully compatible with `Microsoft.EntityFrameworkCore` 10.x | Standard Stack | Low — both are 10.x major version, aligned with .NET 10 release |

## Open Questions

1. **PgBouncer or direct connection?**
   - What we know: Npgsql has built-in pooling. PgBouncer would add another container.
   - What's unclear: Whether Phase 8 Docker Compose deployment should include PgBouncer for connection pooling.
   - Recommendation: Skip PgBouncer for PoC. Built-in Npgsql pooling is sufficient. Revisit if connection exhaustion becomes an issue.

2. **EF Core connection string per schema user?**
   - What we know: Each DbContext should use its schema user's credentials.
   - What's unclear: How to inject connection strings into API projects via Aspire.
   - Recommendation: Phase 2 creates the users. Phase 4 (Aspire + Keycloak) will wire connection strings via environment variables.

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| .NET SDK | dotnet build, ef migrations | ✓ | 10.0.201 | — |
| dotnet ef tool | EF Core migrations | ✗ | — | Install in Plan 02-02: `dotnet tool install --global dotnet-ef` |
| PostgreSQL | Database (container) | ✗ (local) | — | Aspire manages PostgreSQL container (Phase 4+) |
| Npgsql.EFCore NuGet | Domain project | n/a (package) | 10.0.1 | — |

**Missing dependencies with no fallback:**
- `dotnet-ef` tool — must be installed before any migration commands work

**Missing dependencies with fallback:**
- None — all dependencies can be installed/run within the current environment

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit 10.x (latest) |
| Config file | None — xUnit auto-discovers tests |
| Quick run command | `dotnet test` |
| Full suite command | `dotnet test` |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| INFRA-01 | Schemas exist in init.sql | unit | `dotnet test --filter "SchemaInitTests"` | ❌ Wave 0 |
| DBALL-04 | Character entity properties | unit | `dotnet test --filter "CharacterEntityTests"` | ❌ Wave 0 |
| MUSIC-04 | Genre entity properties | unit | `dotnet test --filter "GenreEntityTests"` | ❌ Wave 0 |
| MUSIC-05 | Artist entity + Genre M2M | unit | `dotnet test --filter "ArtistEntityTests"` | ❌ Wave 0 |
| MUSIC-06 | Album entity properties | unit | `dotnet test --filter "AlbumEntityTests"` | ❌ Wave 0 |
| MUSIC-07 | Track entity properties | unit | `dotnet test --filter "TrackEntityTests"` | ❌ Wave 0 |
| Pagination | PagedResult envelope math | unit | `dotnet test --filter "PaginationTests"` | ❌ Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet test` (quick, all tests < 10s)
- **Per wave merge:** `dotnet test --no-restore`
- **Phase gate:** Full suite green before `/gsd-verify-work`

### Wave 0 Gaps
- [ ] `tests/OpenCode.Domain.Tests/` — covers all entity property tests, pagination tests
- [ ] `tests/OpenCode.Domain.Tests/OpenCode.Domain.Tests.csproj` — xUnit project referencing Domain

## Security Domain

### Applicable ASVS Categories
| ASVS Category | Applies | Standard Control |
|---------------|---------|-----------------|
| V5 Input Validation | yes | EF Core parameterized queries by default, entity validation via data annotations |
| V8 Data Protection | yes | Connection strings with database user credentials, not superuser |

### Known Threat Patterns for EF Core + PostgreSQL
| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| SQL injection via raw SQL | Tampering | EF Core uses parameterized queries by default; avoid `FromSqlRaw()` |
| Exposure of database credentials | Information Disclosure | Schema users have scoped permissions; `GRANT USAGE` limits access per user |

## Sources

### Primary (HIGH confidence)
- [VERIFIED: dotnet package search] — `Microsoft.EntityFrameworkCore` 10.0.7, `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.1
- [VERIFIED: learn.microsoft.com/en-us/ef/core/modeling/entity-types] — `HasDefaultSchema()` API, table schema configuration
- [VERIFIED: learn.microsoft.com/en-us/ef/core/managing-schemas/migrations] — Migration commands and patterns
- [VERIFIED: github.com/npgsql/efcore.pg] — Npgsql EF Core provider documentation

### Secondary (MEDIUM confidence)
- [CITED: learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many] — Many-to-many relationship patterns
- [CITED: learn.microsoft.com/en-us/ef/core/modeling/entity-types#table-schema] — Default schema configuration

### Tertiary (LOW confidence)
- None — all critical claims verified against official sources

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — both packages verified via NuGet registry
- Architecture: HIGH — `HasDefaultSchema()` verified via Microsoft docs
- Pitfalls: HIGH — based on documented EF Core behaviors and community patterns
- Security: MEDIUM — schema isolation via users is assumed correct but not verified against a specific security standard

**Research date:** 2026-04-24
**Valid until:** 2026-06-24 (stable NuGet packages, documentation changes are slow)
