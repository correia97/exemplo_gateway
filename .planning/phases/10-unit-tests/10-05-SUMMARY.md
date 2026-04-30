# Plan 10-05: Package Migration — NSubstitute → Moq

**Completed:** 2026-04-29
**Status:** ✅ Complete

## Objective

Replace NSubstitute with Moq for mocking, and add EF Core InMemory provider for repository tests.

## Changes Made

### Directory.Packages.props
- Removed `NSubstitute 5.3.0`
- Added `Moq 4.20.72` (pinned version)
- Added `Microsoft.EntityFrameworkCore.InMemory 10.0.7` (pinned version)

### tests/OpenCode.Api.Tests/OpenCode.Api.Tests.csproj
- Removed `<PackageReference Include="NSubstitute" />`
- Added `<PackageReference Include="Moq" />` (version via central management)
- Added `<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" />` (version via central management)

## Verification

- No test `.cs` files used NSubstitute — clean package swap
- `dotnet build` on full solution: 0 errors
- Existing tests: **90/90 passing** (65 API + 25 Domain)

## Success Criteria Met

- Moq 4.20.72 available in Directory.Packages.props
- EF Core InMemory 10.0.7 available in Directory.Packages.props
- NSubstitute removed from all project references
- Solution builds and all tests pass
