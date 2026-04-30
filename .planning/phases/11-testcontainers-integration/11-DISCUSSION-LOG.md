# Phase 11: Integration Tests with TestContainers - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions captured in 11-CONTEXT.md — this log preserves the discussion.

**Date:** 2026-04-29
**Phase:** 11-testcontainers-integration
**Mode:** discuss
**Areas discussed:** Fixture & container lifecycle, Integration test scope, Schema isolation testing approach

## Discussion Summary

### Fixture & container lifecycle
- **Question:** Container lifecycle approach?
- **Answer:** Per-class container (one container per test class, starts once, stops after)

### Database setup
- **Question:** How to prepare database?
- **Answer:** Migrations + shared seed dataset + per-test additions

### Integration test scope
- **Question:** What scope for Phase 11?
- **Answer:** Full scope — repositories + E2E + schema isolation + correlation ID (~57 tests)

### Schema isolation testing
- **Question:** How to verify?
- **Answer:** Combined approach — raw SQL negative tests + EF Core positive tests

## Key Decisions

| # | Decision |
|---|----------|
| D-01 | Per-class TestContainers PostgreSQL container |
| D-02 | Migrations + shared seed + per-test data |
| D-03 | Full scope: repos + E2E + schema + correlation ID |
| D-04 | Real DB tests complement (not replace) Phase 10 in-memory tests |
| D-05 | Phase 10 test patterns carry forward (per-class files, AAA, no shared state) |
| D-06 | Combined schema isolation: raw SQL negative + EF Core positive |
| D-07 | Raw SQL with separate schema user credentials for negative tests |
| D-08 | EF Core HasDefaultSchema() verified in positive tests |
