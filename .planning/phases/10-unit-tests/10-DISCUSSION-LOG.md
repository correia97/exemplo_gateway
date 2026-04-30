# Phase 10: Unit Tests - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions captured in 10-CONTEXT.md — this log preserves the discussion.

**Date:** 2026-04-29
**Phase:** 10-unit-tests
**Mode:** discuss (retroactive)
**Areas discussed:** Test framework & tooling, Coverage scope & gaps, Test patterns & organization

## Discussion Summary

### Test framework & tooling
- **Question:** Any tooling changes wanted?
- **Answer:** Change mocking library from NSubstitute to Moq

### Mocking library choice
- **Question:** Which mocking library?
- **Answer:** Moq (industry standard, strict mocking semantics)

### Coverage scope & gaps
- **Question:** Any gaps to discuss?
- **Answer:** Add in-memory repository tests (all 5 repos), Expand validator coverage

### In-memory repository tests
- **Question:** How many repos to backfill?
- **Answer:** All 5 repositories (Character, Genre, Artist, Album, Track)

### Validator coverage expansion
- **Question:** What validator expansion?
- **Answer:** Full FluentValidation auto-validation pipeline integration tests

### Test patterns & organization
- **Question:** Keep current pattern or change?
- **Answer:** Keep current pattern (per-class files, inline data, AAA pattern, no shared fixtures)

## Key Decisions

| # | Decision |
|---|----------|
| D-01 | xUnit v3 test runner |
| D-02 | Moq for mocking (migrate from NSubstitute) |
| D-03 | TestHost for middleware tests |
| D-04 | FluentValidation.TestHelper for validator tests |
| D-10 | Add in-memory repository tests for all 5 repos |
| D-11 | Add FluentValidation auto-validation pipeline tests |
| D-12 | Real DB repos deferred to Phase 11 |
| D-13 | Per-class test files, inline data, AAA pattern |

## Decisions Overridden

- NSubstitute **→** Moq (previously used NSubstitute, now migrating to Moq for industry standard alignment)
