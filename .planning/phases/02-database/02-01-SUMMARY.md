# 02-01 Summary — Domain Project + Init SQL

**Status**: ✅ Complete  
**Date**: 2026-04-24  

## Deliverables
- `src/OpenCode.Domain/OpenCode.Domain.csproj` — class library with EF Core + Npgsql references
- `src/OpenCode.Domain/Data/init.sql` — PostgreSQL init script with 3 schemas, 3 users, schema-scoped grants
- Updated `OpenCode.slnx` — 5 projects (AppHost, ServiceDefaults, DragonBall.Api, Music.Api, Domain)
- Updated `Directory.Packages.props` — EF Core 10.0.7, EFCore.Design 10.0.7, Npgsql.EFCore 10.0.1

## Verification
- `dotnet build OpenCode.slnx` — 0 errors, 0 warnings
- init.sql contains: 3x CREATE SCHEMA, 3x CREATE USER (idempotent), GRANT USAGE + ALL PRIVILEGES + ALTER DEFAULT PRIVILEGES per schema, GRANT CONNECT per user
