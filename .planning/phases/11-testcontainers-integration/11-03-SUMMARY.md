# Plan 11-03: API E2E + Schema Isolation + Correlation ID Tests

**Completed:** 2026-04-29
**Status:** ✅ Complete

## Objective

Implement full API E2E tests via TestServer + TestContainers, schema isolation verification (combined approach per D-06), and correlation ID middleware integration tests.

## Files Created

| File | Tests | What It Validates |
|------|-------|-------------------|
| `Endpoints/CharactersEndpointsTests.cs` | 4 | GET list, GET by ID, POST create (201), DELETE 404 |
| `Endpoints/MusicEndpointsTests.cs` | 6 | GET genres/artists, POST create (201), DELETE 404 for genres/artists |
| `Endpoints/CorrelationIdTests.cs` | 3 | Header presence, existing ID preservation, unique ID generation |
| `Schema/SchemaIsolationTests.cs` | 6 | EF Core positive (3), cross-DbContext negative (1), raw SQL user credential negative (2) |

**Total: 19 tests** (all integration, requiring Docker for TestContainers)

## Schema Isolation Coverage (D-06 combined approach)

| Test | Type | What It Verifies |
|------|------|------------------|
| `DragonBallContext_QueriesOnlyDragonballSchema` | EF Core positive | HasDefaultSchema("dragonball") works |
| `MusicContext_QueriesOnlyMusicSchema` | EF Core positive | HasDefaultSchema("music") works |
| `DragonBallContext_CharactersTableExistsInDragonballSchema` | EF Core positive | Table location verified |
| `DragonBallContext_CannotAccessMusicTables` | EF Core negative | Cross-schema query throws |
| `DragonBallUser_CannotQueryMusicSchema` | Raw SQL neg (D-07) | Schema-level permission enforced |
| `MusicUser_CannotQueryDragonballSchema` | Raw SQL neg (D-07) | Schema-level permission enforced |

## Verification

- `dotnet build` on full solution: **0 errors**
- Existing unit tests: **138/138 passing** (no regressions)
