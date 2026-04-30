# Plan 10-07: FluentValidation Auto-Validation Pipeline Tests

**Completed:** 2026-04-29
**Status:** ✅ Complete

## Objective

Add FluentValidation auto-validation middleware wiring and integration tests that verify invalid requests are rejected before reaching endpoint handlers.

## Changes Made

### Package References
- Added `FluentValidation.AspNetCore` (11.3.1 via central management) to both `OpenCode.DragonBall.Api.csproj` and `OpenCode.Music.Api.csproj`

### Program.cs Changes
- Added `using FluentValidation.AspNetCore;` to both Program.cs files
- Added `builder.Services.AddFluentValidationAutoValidation();` to both Program.cs files

### Test File Created
- `tests/OpenCode.Api.Tests/Services/ValidationPipelineTests.cs` with 8 tests

### AutoValidationFilter
- Implemented `AutoValidationFilter<T>` — a generic `IEndpointFilter` that intercepts requests, validates the request DTO using the registered `IValidator<T>`, and returns `ValidationProblem` (400) when validation fails

## Tests (8)

| Test | What It Validates |
|------|-------------------|
| InvalidRequest_ReturnsBadRequest | Empty Name returns 400 |
| InvalidRequest_ReturnsValidationProblemDetails | Response contains RFC 7807 ProblemDetails |
| InvalidRequest_IncludesErrorDetails | Error includes "Name" key with message |
| ValidRequest_ReturnsOk | Valid data returns 200 |
| MultipleErrors_ReturnsAllErrors | Multiple invalid fields return all errors |
| NullBody_ReturnsBadRequest | Empty JSON body returns 400 |
| ValidRequest_PassesCorrectData | Valid data passes through to handler |
| InvalidPictureUrl_ReturnsValidationError | Invalid URL returns 400 with PictureUrl error |

## Verification

- `dotnet build` on full solution: 0 errors
- `dotnet test --filter "ValidationPipeline"`: **8/8 passing**
- Full suite: **138 tests, 0 failures** (25 Domain + 113 API)
