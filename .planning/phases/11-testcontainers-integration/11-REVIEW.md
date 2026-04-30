---
phase: 11-testcontainers-integration
reviewed: 2026-04-30T12:00:00Z
depth: standard
files_reviewed: 12
files_reviewed_list:
  - tests/OpenCode.Integration.Tests/OpenCode.Integration.Tests.csproj
  - tests/OpenCode.Integration.Tests/Fixtures/PostgresFixture.cs
  - tests/OpenCode.Integration.Tests/Fixtures/IntegrationTestBase.cs
  - tests/OpenCode.Integration.Tests/Repositories/CharacterRepositoryTests.cs
  - tests/OpenCode.Integration.Tests/Repositories/GenreRepositoryTests.cs
  - tests/OpenCode.Integration.Tests/Repositories/ArtistRepositoryTests.cs
  - tests/OpenCode.Integration.Tests/Repositories/AlbumRepositoryTests.cs
  - tests/OpenCode.Integration.Tests/Repositories/TrackRepositoryTests.cs
  - tests/OpenCode.Integration.Tests/Endpoints/CharactersEndpointsTests.cs
  - tests/OpenCode.Integration.Tests/Endpoints/MusicEndpointsTests.cs
  - tests/OpenCode.Integration.Tests/Endpoints/CorrelationIdTests.cs
  - tests/OpenCode.Integration.Tests/Schema/SchemaIsolationTests.cs
findings:
  critical: 0
  warning: 4
  info: 4
  total: 8
status: issues_found
---

# Phase 11: Code Review Report — TestContainers Integration Tests

**Reviewed:** 2026-04-30T12:00:00Z
**Depth:** standard
**Files Reviewed:** 12
**Status:** issues_found

## Summary

Reviewed 12 files comprising the TestContainers-based integration test suite for the OpenCode project. The suite covers:

- **Infrastructure:** PostgreSQL TestContainer lifecycle (`PostgresFixture`), base class with DbContext factories (`IntegrationTestBase`)
- **Repository tests:** 25 tests across 5 repository test classes (Characters, Genres, Artists, Albums, Tracks) — CRUD, filtering, pagination against real PostgreSQL
- **E2E tests:** 13 API endpoint tests via `TestServer` + TestContainers (Characters, Music endpoints)
- **Middleware tests:** 3 correlation ID header propagation tests
- **Schema isolation tests:** 6 tests (EF Core positive/negative + raw SQL credential-based)

**High-level assessment:** The test infrastructure is well-structured and follows established patterns (xUnit `IClassFixture`, `IAsyncLifetime`, TestContainers). However, there are several significant concerns around container sprawl (each test class spawns its own PostgreSQL instance), test isolation via minimal `TestServer` that bypasses the full middleware pipeline, and fragile test assertions coupled to seed data counts. No security vulnerabilities or correctness bugs were found.

---

## Warnings

### WR-01: Multiple PostgreSQL containers per test suite run

**File:** `tests/OpenCode.Integration.Tests/Fixtures/PostgresFixture.cs:9`
**Issue:** Each test class declares `IClassFixture<PostgresFixture>`, which means xUnit creates a **separate `PostgresFixture` instance per test class**. With 9 test classes inheriting from `IntegrationTestBase`, a full run creates **9 concurrent PostgreSQL containers** (17-alpine images, each consuming ~200-400MB RAM + CPU). This is resource-intensive and may cause CI failures on runners with constrained Docker resources.

**Fix:**
Replace `IClassFixture` with `ICollectionFixture` to share a single container across all test classes:

1. Create a collection definition:
```csharp
[CollectionDefinition("PostgresIntegration")]
public class PostgresIntegrationCollection : ICollectionFixture<PostgresFixture> { }
```

2. Remove `IClassFixture<PostgresFixture>` from `IntegrationTestBase`:
```csharp
public abstract class IntegrationTestBase
{
    protected PostgresFixture Fixture { get; }
    protected IntegrationTestBase(PostgresFixture fixture) { Fixture = fixture; }
    // ...
}
```

3. Decorate each test class with `[Collection("PostgresIntegration")]`:
```csharp
[Collection("PostgresIntegration")]
public class CharacterRepositoryTests : IntegrationTestBase { ... }
```

4. Add a `CollectionDefinition` file (e.g., `IntegrationCollection.cs`):
```csharp
[CollectionDefinition("PostgresIntegration")]
public class PostgresIntegrationCollection : ICollectionFixture<PostgresFixture> { }
```

**Alternatively**, keep `IClassFixture` but add `[Collection]` attributes to limit parallelism if the container-per-class pattern is deliberate for isolation. Document the intent.

---

### WR-02: Minimal TestServer bypasses full middleware pipeline

**Files:**
- `tests/OpenCode.Integration.Tests/Endpoints/CharactersEndpointsTests.cs:22-33`
- `tests/OpenCode.Integration.Tests/Endpoints/MusicEndpointsTests.cs:22-39`

**Issue:** The `CreateTestHost()` methods construct a minimal `WebApplication` with `UseTestServer()` that registers only the DbContext and repository services. This **bypasses all production middleware**: Correlation ID, Keycloak auth, Kong gateway, OpenTelemetry, Serilog, CORS, and any other middleware from the actual API startup. These tests validate route-to-database integration but give false confidence about full-stack behavior.

**Fix:**
Use `WebApplicationFactory<TEntryPoint>` instead of manual `CreateTestHost` to spin up the full application pipeline, then override only what's needed:

```csharp
public class CharactersEndpointsTests : IntegrationTestBase, IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CharactersEndpointsTests(PostgresFixture fixture, WebApplicationFactory<Program> factory)
        : base(fixture)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:PostgreSQL", ConnectionString);
            builder.ConfigureTestServices(services =>
            {
                // Override auth for tests if needed
                // services.RemoveAll<IAuthorizationHandler>();
            });
        });
    }

    [Fact]
    public async Task GetCharacters_ReturnsPagedResults()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/characters?Page=1&PageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

**Note:** The API projects need a public `Program` class or `partial class Program` for `WebApplicationFactory` to work. If not yet present, add `public partial class Program { }` to each API project.

---

### WR-03: Seed data coupling in count assertions

**Files:**
- `tests/OpenCode.Integration.Tests/Repositories/CharacterRepositoryTests.cs:47`
- `tests/OpenCode.Integration.Tests/Repositories/GenreRepositoryTests.cs:39`

**Issue:** These tests assert exact total counts that include seed data from `PostgresFixture.SeedBaselineAsync()`:

- `CharacterRepositoryTests.GetAll_Pagination_CorrectCount` expects `TotalCount == 16` = 15 test characters + 1 seed "Goku"
- `GenreRepositoryTests.GetAll_Pagination_CorrectCount` expects `TotalCount == 4` = 3 test genres + 1 seed "Rock"

If seed data is modified (e.g., more characters added, baseline Rock genre renamed), these tests break silently. The seed data is an invisible dependency.

**Fix:**
Make the assertions relative to what the test itself inserts, or isolate seed data from test data:

**Option A — Track seed independently:**
```csharp
// CharacterRepositoryTests
var seedCount = 1; // from SeedBaselineAsync
// ... insert 15 test characters ...
var result = await repo.GetAllAsync(null, null, null, null, null, 1, 10);
Assert.Equal(10, result.Data.Count()); // verify page size boundary, not total count
Assert.Equal(seedCount + 15, result.TotalCount); // explicit seed dependency
```

**Option B — Filter to exclude seed data:**
```csharp
var result = await repo.GetAllAsync(name: "C", null, null, null, null, 1, 10);
Assert.Equal(10, result.Data.Count()); // only test data matches filter "C"
```

---

### WR-04: Repository-owned DbContexts not disposed in tests

**Files (all repository test classes):**
- `tests/OpenCode.Integration.Tests/Repositories/CharacterRepositoryTests.cs:13-16`
- `tests/OpenCode.Integration.Tests/Repositories/GenreRepositoryTests.cs:12-15`
- `tests/OpenCode.Integration.Tests/Repositories/ArtistRepositoryTests.cs:13-16`
- `tests/OpenCode.Integration.Tests/Repositories/AlbumRepositoryTests.cs:12-15`
- `tests/OpenCode.Integration.Tests/Repositories/TrackRepositoryTests.cs:13-16`

**Issue:** The `CreateRepo()` helper creates a new `DbContext` and passes it to the repository constructor. The `Repository<T>` base class (at `src/OpenCode.Domain/Implementations/Repository.cs`) does **not** implement `IDisposable` and never disposes the injected `DbContext`. Each test creates at least two contexts: one inside `CreateRepo()` and one for seed data via `CreateDragonBallContext()`/`CreateMusicContext()`. These contexts accumulate without explicit disposal until GC collection.

While this does not cause test failures (the containers handle many connections via pooling), it's a resource leak pattern that may cause connection pool exhaustion with a large test suite.

**Fix:**
Option A — Make `Repository<T>` implement `IDisposable`:
```csharp
public class Repository<T> : IRepository<T>, IDisposable where T : class
{
    // ...
    public void Dispose() => Context.Dispose();
}
```

Option B — Use `using` in tests:
```csharp
private ICharacterRepository CreateRepo()
{
    // Keep reference for disposal, or wrap in using
    var ctx = CreateDragonBallContext();
    return new CharacterRepository(ctx);
}

// In each test, add disposal tag:
// But this is awkward since ICharacterRepository doesn't expose IDisposable
```

Option C — Use `DbContextFactory` instead (preferred):
```csharp
// IntegrationTestBase
private IDbContextFactory<DragonBallContext> _dbFactory;

protected DragonBallContext CreateDragonBallContext()
    => _dbFactory.CreateDbContext();
```

Then dispose context per test with `using`:
```csharp
using var ctx = CreateDragonBallContext();
var repo = new CharacterRepository(ctx);
```

---

## Info

### IN-01: CorrelationIdTests starts unnecessary PostgreSQL container

**File:** `tests/OpenCode.Integration.Tests/Endpoints/CorrelationIdTests.cs:10-12`

**Issue:** `CorrelationIdTests` inherits from `IntegrationTestBase` which requires `IClassFixture<PostgresFixture>`. However, the correlation ID tests do **not** use the database at all — they test middleware-only behavior with a static `CreateTestHost()`. A full PostgreSQL container starts up and runs migrations for every correlation ID test with zero benefit.

**Fix:**
Remove the `PostgresFixture` dependency and use a plain class fixture or no fixture:
```csharp
public class CorrelationIdTests
{
    private static async Task<IHost> CreateTestHost()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();
        app.UseCorrelationId();
        app.Run(async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });
        await app.StartAsync();
        return app;
    }

    // ... test methods remain unchanged
}
```

---

### IN-02: EF Core schema isolation test validates model scope, not database isolation

**File:** `tests/OpenCode.Integration.Tests/Schema/SchemaIsolationTests.cs:42-48`

**Issue:** The test `DragonBallContext_CannotAccessMusicTables` expects `InvalidOperationException` when calling `ctx.Set<Genre>()` on a `DragonBallContext`. This exception occurs because `Genre` is not registered in the `DragonBallContext` model — it throws before any SQL is generated. This test validates **EF Core model isolation** (each context only knows its own entities), not **database-level schema isolation** (PostgreSQL permissions on schemas).

The raw SQL tests (`DragonBallUser_CannotQueryMusicSchema` and `MusicUser_CannotQueryDragonballSchema`) properly validate database-level schema isolation by using separate database credentials and expecting `PostgresException`.

**Fix:**
Rename the test to clarify its scope, or change it to verify database-level isolation using the admin connection (testing that the `dragonball` schema table doesn't exist in `music` schema context):
```csharp
[Fact]
public async Task DragonBallContext_ModelDoesNotIncludeMusicEntities()
{
    using var ctx = CreateDragonBallContext();
    var ex = Assert.Throws<InvalidOperationException>(() => ctx.Set<Genre>());
    Assert.Contains("Genre", ex.Message); // Verify it's a model-not-found error
}
```

---

### IN-03: No xunit.runsettings for parallelism control

**File:** (project root)

**Issue:** No `.runsettings` file exists to control xUnit behavior. xUnit v3 runs test classes in parallel by default (sequential within a class). Combined with `IClassFixture<PostgresFixture>` (9 separate containers), this means up to 9 PostgreSQL containers start concurrently. Adding an explicit configuration would make parallelism behavior predictable and documented.

**Fix:**
Add a `xunit.runsettings` file:
```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <RunConfiguration>
    <MaxCpuCount>4</MaxCpuCount>
  </RunConfiguration>
  <xUnit>
    <ParallelizeTestCollections>true</ParallelizeTestCollections>
    <MaxParallelThreads>4</MaxParallelThreads>
  </xUnit>
</RunSettings>
```

Reference it in the csproj:
```xml
<PropertyGroup>
  <RunSettingsFilePath>$(MSBuildProjectDirectory)\xunit.runsettings</RunSettingsFilePath>
</PropertyGroup>
```

This caps concurrent containers to a manageable number and documents the parallelism strategy.

---

### IN-04: Seed data conditional guards are unnecessary with per-class containers

**File:** `tests/OpenCode.Integration.Tests/Fixtures/PostgresFixture.cs:60,85`

**Issue:** `SeedBaselineAsync()` uses `if (!await ctx.Planets.AnyAsync())` and `if (!await ctx.Genres.AnyAsync())` guards to prevent duplicate seeding. Since each `PostgresFixture` creates a fresh container (each test class gets its own instance), the database is always empty at initialization. The guard is never triggered.

**Fix:**
Remove the conditional guards for clarity (the seed always runs exactly once per container):
```csharp
private async Task SeedBaselineAsync()
{
    using (var ctx = new DragonBallContext(
        new DbContextOptionsBuilder<DragonBallContext>().UseNpgsql(ConnectionString).Options))
    {
        var earth = new Planet { Name = "Earth" };
        ctx.Planets.Add(earth);
        await ctx.SaveChangesAsync();

        ctx.Characters.Add(new Character { Name = "Goku", Race = "Saiyan", Ki = "10.000.000", MaxKi = "100.000.000.000.000", IntroductionPhase = "Dragon Ball", Description = "Main protagonist", PlanetId = earth.Id });
        await ctx.SaveChangesAsync();
    }

    using (var ctx = new MusicContext(
        new DbContextOptionsBuilder<MusicContext>().UseNpgsql(ConnectionString).Options))
    {
        ctx.Genres.Add(new Genre { Name = "Rock", Description = "Rock music genre" });
        await ctx.SaveChangesAsync();
    }
}
```

---

## Summary of Key Concerns

| Severity | Issue | File | Recommendation |
|----------|-------|------|----------------|
| Warning | 9 PostgreSQL containers per run | PostgresFixture.cs | Switch to `ICollectionFixture` |
| Warning | TestServer bypasses full pipeline | CharactersEndpointsTests.cs, MusicEndpointsTests.cs | Use `WebApplicationFactory<TEntryPoint>` |
| Warning | Count assertions depend on seed data | CharacterRepositoryTests.cs:47, GenreRepositoryTests.cs:39 | Assert on test-local data only |
| Warning | DbContexts created by repos never disposed | All repository test classes | Add `IDisposable` or use factory pattern |
| Info | CorrelationIdTests starts unused container | CorrelationIdTests.cs | Remove `IntegrationTestBase` dependency |
| Info | Schema isolation test checks model, not DB permissions | SchemaIsolationTests.cs:42-48 | Rename or restructure |
| Info | No parallelism configuration | (project root) | Add `xunit.runsettings` |
| Info | Unnecessary duplicate-seed guards | PostgresFixture.cs:60,85 | Remove conditional checks |

---

_Reviewed: 2026-04-30T12:00:00Z_
_Reviewer: gsd-code-reviewer agent_
_Depth: standard_
