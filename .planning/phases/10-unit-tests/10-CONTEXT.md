# Phase 10: Unit Tests - Context

**Gathered:** 2026-04-29
**Status:** Ready for planning (replan)

<domain>
## Phase Boundary

Comprehensive unit test coverage for validators, DTO mappings, services, middleware, and auth components across both APIs, plus edge-case tests for domain models. Covers the test pyramid's unit level — integration tests with real databases are Phase 11.

</domain>

<decisions>
## Implementation Decisions

### Test Framework & Tooling
- **D-01:** xUnit v3 as test runner (inherited from Phase 2 Domain.Tests project)
- **D-02:** Moq for mocking dependencies (replacing NSubstitute — migrate existing tests)
- **D-03:** Microsoft.AspNetCore.TestHost 10.0.7 for middleware integration testing (CorrelationIdMiddleware via WebApplication + TestServer)
- **D-04:** FluentValidation.TestHelper (built into FluentValidation) for validator unit tests with `TestValidate` pattern
- **D-05:** coverlet.collector for code coverage collection

### Coverage Scope
- **D-06:** 10 validators fully tested (Create + Update for Character, Genre, Artist, Album, Track) — 38 existing tests
- **D-07:** DTO mappings tested for all 5 main entities + Planet + Transformation — 18 existing tests
- **D-08:** Services tested: CorrelationIdMiddleware (4 tests via TestHost), KeycloakRolesClaimsTransformation (5 tests) — 9 existing tests
- **D-09:** Domain tests: PagedResult pagination math (11), Entity properties (8), Schema isolation (4), Inheritance (2) — 25 existing tests
- **D-10:** Add in-memory EF Core repository tests for all 5 repositories (Character, Genre, Artist, Album, Track) — basic CRUD operations
- **D-11:** Add full FluentValidation auto-validation pipeline integration tests (test the middleware that validates requests before they reach endpoints)
- **D-12:** Real database repository tests deferred to Phase 11 (TestContainers)

### Test Patterns & Organization
- **D-13:** Per-class test files (one test class per component under test)
- **D-14:** Inline test data — no shared data factories or fixtures
- **D-15:** Arrange-Act-Assert (AAA) pattern throughout
- **D-16:** No shared state between tests — each test creates its own data

### Quality Bar
- **D-17:** Every public validation rule gets at least one pass and one fail test
- **D-18:** Every DTO mapping property is individually asserted
- **D-19:** Every service/middleware method has 3+ tests covering: happy path, edge case, and error path

### Migration Note
- **D-20:** Replace NSubstitute with Moq across `tests/OpenCode.Api.Tests/` — requires updating existing service tests (KeycloakRolesClaimsTransformationTests uses NSubstitute)
- **D-21:** Moq package to be added to `Directory.Packages.props`

</decisions>

<canonical_refs>
## Canonical References

### Requirements
- `.planning/REQUIREMENTS.md` §Testing & Quality — TEST-01 through TEST-08

### Existing Test Infrastructure
- `tests/OpenCode.Domain.Tests/OpenCode.Domain.Tests.csproj` — Existing domain test project (xUnit v3)
- `tests/OpenCode.Api.Tests/OpenCode.Api.Tests.csproj` — New API test project (needs Moq migration)
- `Directory.Packages.props` — Centralized NuGet version management (needs Moq package version)

### Code Under Test
- `src/OpenCode.DragonBall.Api/Validators/` — Character and all Music validators
- `src/OpenCode.DragonBall.Api/Dtos/` — Character DTOs and mapping extensions
- `src/OpenCode.Music.Api/Dtos/` — Genre, Artist, Album, Track DTOs and mapping extensions
- `src/OpenCode.DragonBall.Api/Auth/KeycloakRolesClaimsTransformation.cs` — Auth claims transformation
- `src/OpenCode.ServiceDefaults/Extensions.cs` — CorrelationIdMiddleware implementation
- `src/OpenCode.Domain/Pagination/PagedResult.cs` — Pagination helper
- `src/OpenCode.Domain/Entities/` — All domain entities

### Prior Decisions
- `.planning/phases/02-database/02-CONTEXT.md` — Entity definitions, pagination contract
- `.planning/phases/04-keycloak-authentication-authorization/04-CONTEXT.md` — Auth decisions, claims transformation pattern

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `tests/OpenCode.Domain.Tests/` — Existing xUnit v3 test project with established patterns (Fact attributes, Assert patterns, xUnit v3 conventions)
- `tests/OpenCode.Api.Tests/` — New API test project structure (validators/, mappings/, services/ directories)

### Established Patterns
- Inline `[Fact]` attributes (no Theory/InlineData used yet)
- `FluentValidation.TestHelper.TestValidate()` with `ShouldHaveValidationErrorFor()` / `ShouldNotHaveAnyValidationErrors()`
- `NSubstitute` for mocking (needs migration to Moq)
- `WebApplication.CreateBuilder() + UseTestServer()` for integration-style middleware tests

### Integration Points
- `Directory.Packages.props` — Central version management needs Moq addition, NSubstitute removal
- `tests/OpenCode.Api.Tests/OpenCode.Api.Tests.csproj` — Package references to update
- Phase 11 (TestContainers) will use same Moq pattern for any mocked dependencies

</code_context>

<specifics>
## Specific Ideas

- "Use Moq instead of NSubstitute — industry standard for .NET"
- "Add in-memory EF Core tests for repositories — exercises repository pattern without needing a real PostgreSQL"
- "Validator pipeline tests should verify that invalid requests return ProblemDetails 400 with FluentValidation errors wired through the middleware"

</specifics>

<deferred>
## Deferred Ideas

- Real database integration tests — Phase 11 (TestContainers)
- Code coverage percentage targets — too early in project to set hard thresholds

</deferred>

---

*Phase: 10-unit-tests*
*Context gathered: 2026-04-29*
