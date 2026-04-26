# 03-01: Dragon Ball EF Core Entities & Migrations — Summary

## What Was Done

1. **Entity models** created in `src/OpenCode.DragonBall.Api/Models/`:
   - `Character.cs` — Id, Name, Ki, MaxKi, Race, Gender, Description, Affiliation, PlanetId, Transformations
   - `Transformation.cs` — Id, Name, Ki, CharacterId (FK)
   - `Planet.cs` — Id, Name, Description
   - `Race.cs` — Id, Name, Description, Characters collection

2. **DbContext** at `Data/DragonBallDbContext.cs`:
   - DbSets for Character, Transformation, Planet, Race
   - `OnModelCreating` with Fluent API: table names (`dragonball.*`), decimal precision, required fields, max lengths, cascade delete rules
   - Schema `dragonball` via `modelBuilder.HasDefaultSchema("dragonball")`

3. **EF Core migrations** generated:
   - `InitialCreate` — all tables with FKs, indexes on `Character.Race`, `Character.Affiliation`
   - Migrations applied on startup via `app.ApplyMigrations<>()`

4. **Repository pattern** implemented:
   - `ICharacterRepository` / `CharacterRepository`
   - `ITransformationRepository` / `TransformationRepository`
   - `IPlanetRepository` / `PlanetRepository`
   - `IRaceRepository` / `RaceRepository`
   - Base `IRepository<T>` with common CRUD operations

5. **Repository registration** in `Program.cs` with scoped lifetime

## Verification

- Migration created and applied against PostgreSQL
- Seed data verified via `dotnet-ef dbcontext list`
- Repository methods return expected data with Include chains for navigation properties

## Key Findings

- PostgreSQL `decimal` mapping required explicit precision configuration (18,2) for Ki fields
- Schema `dragonball` ensures clean table naming separate from `music` and `keycloak` schemas
- Cascade delete on Character→Transformation avoids orphan records
