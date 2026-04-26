# Phase 3: API Endpoints — Dragon Ball & Music CRUD — Research

**Researched:** 2026-04-24
**Domain:** .NET 10 Minimal APIs / FluentValidation / ProblemDetails / Scalar / OpenAPI
**Confidence:** HIGH

## Summary

Phase 3 builds complete CRUD endpoints for both APIs on top of the Phase 2 domain layer. The core technical challenge is that Phase 2 created **only interfaces** (`ICharacterRepository`, `IGenreRepository`, etc.) but **no concrete `Repository<T>` implementation**. This phase must first implement the repository base class, then build endpoints against it.

Two API projects (`OpenCode.DragonBall.Api`, `OpenCode.Music.Api`) currently have placeholder weather forecast code and no Domain project reference. They need Domain references, EF Core DbContext registrations, repository DI wiring, and endpoint implementation.

The REPR (Request-Endpoint-Response) pattern is recommended for organizing Minimal API endpoints: each entity gets its own endpoint file with route group configuration. FluentValidation integrates via `AddFluentValidationAutoValidation()`. ProblemDetails is built-in via `AddProblemDetails()`. Scalar.UI provides the OpenAPI documentation UI.

<user_constraints>
## User Constraints (from ROADMAP.md)

### Phase 3 Requirements
- **Dragon Ball**: CRUD for characters with pagination (DBALL-05/06), filtering by name/phase (DBALL-07), Scalar UI (DBALL-08), OpenAPI server URL for APISIX (DBALL-09), FluentValidation (DBALL-10), ProblemDetails (DBALL-11)
- **Music**: CRUD for Genre/Artist/Album/Track (MUSIC-09), nested routes artists/{id}/albums and albums/{id}/tracks (MUSIC-10), pagination (MUSIC-11), filtering by name/genre/date (MUSIC-12), Scalar UI (MUSIC-13), OpenAPI server URL for APISIX (MUSIC-14), FluentValidation (MUSIC-15), ProblemDetails (MUSIC-16)

### Locked Decisions (from Phase 2 CONTEXT.md applicable to Phase 3)
1. Repository pattern with `IRepository<T>` base interface — must implement `Repository<T>` concrete class
2. `PagedResult<T>` envelope with `{ data, totalCount, page, pageSize, totalPages }`
3. Public reads (GET), protected writes (POST/PUT/DELETE) — auth comes in Phase 4, for now all endpoints are functional
4. Data flows through repository layer — endpoints call repositories, not DbContext directly
5. The OpenAPI server URL must be configurable for APISIX proxy — Phase 3 sets up the override, Phase 5 wires the actual URL
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| DBALL-05 | CRUD endpoints for characters | Minimal API route groups with MapPost, MapGet, MapPut, MapDelete |
| DBALL-06 | GET list with pagination | Query parameter binding + repository GetAllAsync(page, pageSize) |
| DBALL-07 | List filtering by name and introductionPhase | IQueryable Where() filter in repository implementation |
| DBALL-08 | Scalar UI | Scalar.AspNetCore package + MapScalarApiReference() |
| DBALL-09 | OpenAPI server URL for APISIX | OpenAPI document transformer to set Servers URL |
| DBALL-10 | FluentValidation on create/update | FluentValidation.AspNetCore + AbstractValidator<T> |
| DBALL-11 | ProblemDetails error responses | AddProblemDetails() + FluentValidation → ProblemDetails mapping |
| MUSIC-09 | CRUD for Genre, Artist, Album, Track | Route groups in separate endpoint files per entity |
| MUSIC-10 | Nested endpoints | Route group nesting: artists/{id}/albums, albums/{id}/tracks |
| MUSIC-11 | GET list pagination | Same pattern as Dragon Ball — shared PagedResult<T> |
| MUSIC-12 | Filtering by name, genre, release date | IQueryable filters on Name, GenreId, ReleaseDate |
| MUSIC-13 | Scalar UI | Same Scalar.AspNetCore integration |
| MUSIC-14 | OpenAPI server URL for APISIX | Shared document transformer |
| MUSIC-15 | FluentValidation | Per-entity validators |
| MUSIC-16 | ProblemDetails | Shared middleware |
</phase_requirements>

## Current Codebase State

```
Phase 2 Delivered:
├── OpenCode.Domain/
│   ├── Entities/          ✓ Entities with BaseEntity (Id, CreatedAt, UpdatedAt)
│   ├── Data/              ✓ DragonBallContext + MusicContext with HasDefaultSchema()
│   ├── Migrations/        ✓ InitialCreate migrations for both schemas
│   ├── Pagination/        ✓ PagedResult<T> envelope
│   └── Interfaces/        ✓ IRepository<T> + entity-specific interfaces (MISSING implementations)
│
├── OpenCode.DragonBall.Api/
│   ├── Program.cs         ✗ Placeholder weather forecast code (to replace)
│   └── .csproj            ✗ No Domain project reference
│
├── OpenCode.Music.Api/
│   ├── Program.cs         ✗ Placeholder weather forecast code (to replace)
│   └── .csproj            ✗ No Domain project reference
│
└── Directory.Packages.props  ✓ Pinned packages (NO FluentValidation yet)
```

## Standard Stack

### NuGet Packages to Add

| Package | Version | Purpose | Why |
|---------|---------|---------|-----|
| FluentValidation.AspNetCore | 11.3.0 | Auto-validation for Minimal APIs | FluentValidation.AspNetCore includes the auto-validation filter and DI registration |
| FluentValidation.DependencyInjectionExtensions | 11.11.0 | Assembly-scanning registration | Registers all validators in an assembly with one call |
| Scalar.AspNetCore | 2.x | OpenAPI documentation UI | Modern interactive API documentation replacing Swagger UI |

**NOTE:** `Scalar.AspNetCore` version depends on latest stable. Research at planning time confirmed that Scalar 2.x supports `MapScalarApiReference()` on `WebApplication`. The exact version should be the latest stable from NuGet.

### Architecture Patterns

#### Pattern 1: Repository<T> Base Implementation

```csharp
// src/OpenCode.Domain/Implementations/Repository.cs
namespace OpenCode.Domain.Implementations;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(DbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => await DbSet.FindAsync([id], ct);

    public virtual async Task<PagedResult<T>> GetAllAsync(
        int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking();
        var totalCount = await query.CountAsync(ct);
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<T>
        {
            Data = data,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ... AddAsync, UpdateAsync, DeleteAsync implementations
}
```

Key design:
- Uses `AsNoTracking()` for read queries (no change tracking overhead)
- `AddAsync` uses `DbSet.AddAsync()` and `Context.SaveChangesAsync()`
- `UpdateAsync` marks entity as Modified and saves
- `DeleteAsync` removes entity and saves
- `protected` members allow subclasses to extend with filtering

**How to get DbContext** — Since each API project has one DbContext, the specific repository implementations (e.g., `CharacterRepository`) can take `DragonBallContext` directly via constructor injection. The base `Repository<T>` stores it as `DbContext` for generic operations.

#### Pattern 2: Entity-Specific Repositories with Filtering

```csharp
// src/OpenCode.DragonBall.Api/Repositories/CharacterRepository.cs
public class CharacterRepository : Repository<Character>, ICharacterRepository
{
    private readonly DragonBallContext _context;

    public CharacterRepository(DragonBallContext context) : base(context)
    {
        _context = context;
    }

    public async Task<PagedResult<Character>> GetAllAsync(
        string? name, string? introductionPhase,
        int page, int pageSize, CancellationToken ct)
    {
        var query = _context.Characters.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(c => c.Name.Contains(name));

        if (!string.IsNullOrWhiteSpace(introductionPhase))
            query = query.Where(c => c.IntroductionPhase == introductionPhase);

        var totalCount = await query.CountAsync(ct);
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Character>
        {
            Data = data,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
```

For Music repositories:
- `GenreRepository`: filter by Name
- `ArtistRepository`: filter by Name and GenreId
- `AlbumRepository`: filter by Title, ArtistId (for nested), ReleaseDate
- `TrackRepository`: filter by Name, AlbumId (for nested)

Nested routes (e.g., `GET /artists/{id}/albums`) use additional methods like `GetAlbumsByArtistIdAsync(int artistId, ...)`.

#### Pattern 3: REPR Endpoint Organization

Each logical endpoint group gets its own file following the Request-Endpoint-Response pattern:

```
src/OpenCode.DragonBall.Api/
├── Endpoints/
│   └── Characters.cs           # All character endpoints in one file
├── Validators/
│   └── CreateCharacterValidator.cs
│   └── UpdateCharacterValidator.cs
├── Dtos/
│   └── CreateCharacterRequest.cs
│   └── UpdateCharacterRequest.cs
│   └── CharacterResponse.cs
├── Repositories/
│   └── CharacterRepository.cs
└── Program.cs
```

Endpoints file structure:
```csharp
public static class Characters
{
    public static RouteGroupBuilder MapCharacterEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id}", UpdateAsync);
        group.MapDelete("/{id}", DeleteAsync);
        return group;
    }

    // ... static handler methods
}
```

Registration in Program.cs:
```csharp
app.MapGroup("/api/characters")
   .MapCharacterEndpoints();
```

#### Pattern 4: FluentValidation Integration

```csharp
// Validator
public class CreateCharacterRequest
{
    public string Name { get; set; } = string.Empty;
    public bool IsEarthling { get; set; }
    public string? IntroductionPhase { get; set; }
    public string? PictureUrl { get; set; }
}

public class CreateCharacterValidator : AbstractValidator<CreateCharacterRequest>
{
    public CreateCharacterValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.IntroductionPhase).MaximumLength(100);
        RuleFor(x => x.PictureUrl).MaximumLength(500);
    }
}
```

Program.cs registration:
```csharp
builder.Services.AddValidatorsFromAssemblyContaining<CreateCharacterValidator>();
builder.Services.AddFluentValidationAutoValidation();
```

`AddFluentValidationAutoValidation()` automatically validates request bodies when they're bound with `[FromBody]` in Minimal APIs, returning 400 with validation errors.

#### Pattern 5: ProblemDetails Configuration

```csharp
// Program.cs
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
        ctx.ProblemDetails.Extensions["correlationId"] = 
            ctx.HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? "";
    };
});
```

For FluentValidation errors → ProblemDetails mapping, when `AddFluentValidationAutoValidation()` detects validation errors, it throws a `ValidationException`. We map this to a structured ProblemDetails response. Note: the FluentValidation.AspNetCore package handles this automatically for Minimal APIs by default — it produces a `ValidationProblemDetails` response.

#### Pattern 6: Scalar UI Configuration

```csharp
// Program.cs
builder.Services.AddOpenApi();  // Already present

// After app.Build():
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();  // Scalar.AspNetCore
}
```

Scalar configuration with custom theme and server URL:
```csharp
app.MapScalarApiReference(options =>
{
    options
        .WithTitle("OpenCode API")
        .WithTheme(ScalarTheme.Purple)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});
```

#### Pattern 7: OpenAPI Server URL Override

```csharp
// Program.cs
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        var serverUrl = builder.Configuration["OpenApi:ServerUrl"] ?? "http://localhost9080";
        document.Servers = [new OpenApiServer { Url = serverUrl }];
        return Task.CompletedTask;
    });
});
```

This sets the OpenAPI server URL which Scalar will use for "Try it" requests. Phase 5 configures the actual APISIX URL. For Phase 3, defaults to `http://localhost9080` (APISIX default port) or the API's direct URL.

#### Pattern 8: DTO Pattern for Create/Update

Separate request DTOs from entity models to avoid over-posting and to decouple API contracts from domain models:

```csharp
public record CreateCharacterRequest(
    string Name,
    bool IsEarthling,
    string? IntroductionPhase,
    string? PictureUrl
);

public record UpdateCharacterRequest(
    string Name,
    bool IsEarthling,
    string? IntroductionPhase,
    string? PictureUrl
);

public record CharacterResponse(
    int Id,
    string Name,
    bool IsEarthling,
    string? IntroductionPhase,
    string? PictureUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
```

Use mapping helpers or manual mapping between DTOs and entities.

## Anti-Patterns to Avoid

1. **Exposing entities directly in API responses** — Use DTOs/records instead to avoid over-posting and leaking internal state
2. **Multiple DbContext instances per request** — Register DbContexts as scoped, one per API project, never shared
3. **Inline validation in endpoints** — Use FluentValidation validators, not inline `if` checks
4. **Raw SQL for filtering** — Use IQueryable LINQ expressions which EF Core translates to parameterized SQL
5. **Putting all endpoints in Program.cs** — Use separate endpoint files (REPR pattern) for maintainability
6. **Hardcoded connection strings** — Use configuration/DI for connection strings (Aspire manages this)

## Dependencies Still Missing

| Dependency | Required By | Status | Action |
|------------|-------------|--------|--------|
| FluentValidation.AspNetCore | Phase 3 | Not in solution | Add NuGet |
| Scalar.AspNetCore | Phase 3 | Not in solution | Add NuGet |
| Repository<T> base impl | Phase 3 | Does not exist | Create in Domain |
| Domain reference in APIs | Phase 3 | Not referenced | Add to both csproj files |
| Real DI registration | Phase 3 | Not wired | Add in Program.cs |

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit (existing in solution) |
| Test project | `tests/OpenCode.DragonBall.Api.Tests/` (NEW) + existing `tests/OpenCode.Domain.Tests/` |
| Quick command | `dotnet test` |

### Phase Requirements → Test Map

| Req ID | Behavior | Test Type | Automated Command |
|--------|----------|-----------|-------------------|
| DBALL-05 | Character CRUD endpoints | integration | `dotnet test --filter "CharacterEndpointTests"` |
| DBALL-06 | Paginated character list | unit | `dotnet test --filter "CharacterEndpointTests"` |
| DBALL-07 | Character filtering | unit | `dotnet test --filter "CharacterFilteringTests"` |
| DBALL-10 | Character validation | integration | `dotnet test --filter "CharacterValidationTests"` |
| MUSIC-09 | Genre/Artist/Album/Track CRUD | integration | `dotnet test --filter "MusicEndpointTests"` |
| MUSIC-10 | Nested routes | integration | `dotnet test --filter "NestedRouteTests"` |
| MUSIC-11 | Paginated music lists | unit | `dotnet test --filter "MusicEndpointTests"` |
| MUSIC-12 | Music filtering | unit | `dotnet test --filter "MusicFilteringTests"` |
| MUSIC-15 | Music entity validation | integration | `dotnet test --filter "MusicValidationTests"` |

### Wave 0 Gaps
- No test project exists for API endpoint tests — integration tests are deferred (Phase 3 focuses on implementation). Basic validation tests in existing test project can be extended.
- Integration tests requiring a running database are out of scope for Phase 3 — endpoint testing is done via unit tests on repository logic and validation tests on validators.

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|-------------|----------------|-----------|
| Repository implementation | API / Backend | — | `Repository<T>` in Domain, consumed by API endpoints |
| CRUD endpoint logic | API / Backend | — | Minimal API handlers in each API project |
| Request validation | API / Backend | — | FluentValidation validators per request type |
| Response formatting | API / Backend | — | ProblemDetails middleware maps errors to RFC 7807 |
| OpenAPI documentation | API / Backend | — | Built-in OpenAPI + Scalar UI rendering |
| Pagination | API / Backend | — | `PagedResult<T>` from repository consumed by endpoints |
| Filtering | API / Backend | — | IQueryable extensions in repository implementations |
| API contract (DTOs) | API / Backend | — | Request/response records in each API project |

## Known Threat Patterns

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| Over-posting via direct entity binding | Tampering | Use DTOs/records for create/update, not entities |
| Missing input validation | Tampering | FluentValidation on all create/update requests |
| Enumeration via pagination | Information Disclosure | Cap page size at 100 (enforced in repository) |
| Stack trace exposure | Information Disclosure | ProblemDetails hides stack traces in production |

## Assumptions Log

| # | Claim | Risk if Wrong |
|---|-------|---------------|
| A1 | `AddFluentValidationAutoValidation()` works correctly with Minimal API endpoint filters in .NET 10 | Medium — FluentValidation.AspNetCore may need specific configuration for .NET 10 |
| A2 | `Scalar.AspNetCore` NuGet package is available and API-compatible with .NET 10 | Low — Scalar is actively maintained for .NET |
| A3 | `AddProblemDetails()` handles all unhandled exceptions automatically | Low — built-in ASP.NET Core behavior |
| A4 | FluentValidation errors are automatically mapped to ProblemDetails by the middleware | Medium — may need custom middleware to map FluentValidation ValidationException to ProblemDetails |
| A5 | The REPR pattern with static endpoint classes works with Minimal API dependency injection | Low — static methods use `HttpContext.RequestServices` or parameter injection |

## Open Questions

1. **Integration test strategy**: Should Phase 3 create a test project with a test server (WebApplicationFactory) for endpoint testing? Or defer integration tests to a later phase?
   - **Decision**: Defer full integration tests. Phase 3 creates the endpoints and validators. A future phase or Phase 3 extension can add integration tests with WebApplicationFactory.

2. **Scalar version**: Latest stable Scalar.AspNetCore version?
   - Check NuGet at implementation time. Target latest stable 2.x.

## Sources

### Primary (HIGH confidence)
- [VERIFIED: learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis] — Minimal API route groups, parameter binding, typed results
- [VERIFIED: docs.fluentvalidation.net] — FluentValidation ASP.NET Core integration, auto-validation
- [VERIFIED: learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors] — ProblemDetails built-in support
- [VERIFIED: github.com/scalar/scalar] — Scalar.AspNetCore NuGet package

### Secondary (MEDIUM confidence)
- [CITED: learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi] — OpenAPI document transformers for server URL override
- [CITED: learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/repository-implementation] — Generic repository pattern with EF Core

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — all packages documented and maintained
- Architecture: HIGH — Minimal API patterns are well-established
- REPR pattern: MEDIUM — pattern is emerging but well-documented for Minimal APIs
- Security: MEDIUM — DTO-based over-post prevention is standard but not verified against ASVS

**Research date:** 2026-04-24
**Valid until:** 2026-05-24
