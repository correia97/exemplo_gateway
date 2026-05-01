# Phase 13: Version Endpoints — Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions captured in CONTEXT.md — this log preserves the question/answer history.

**Date:** 2026-04-30
**Phase:** 13-version-endpoints
**Mode:** discuss (standard)

## Discussion Summary

### Scope Change
- User referenced Microsoft article on .NET 10 API versioning (`Asp.Versioning` v10 with OpenAPI)
- Scope changed from simple `/api/version` metadata endpoint to full API route versioning (`/api/v1/*`)

### Versioning Strategy
- **User choice:** URL path versioning (`/api/v1/characters`) — recommended option

### Migration Scope
- **User choice:** Full migration — all endpoints move to `/api/v1/*`. Clean break.

### Frontend Updates
- **User choice:** Include frontend API client path updates in this phase (recommended)

### Scalar Versioning
- **User choice:** Include versioned Scalar docs via `WithDocumentPerVersion()` (recommended)

### Kong Implications
- **Determined:** No Kong config changes needed. Existing `strip_path: true` handles `/api/v1/*` paths naturally.

### Test Strategy
- **User choice:** Update existing test paths + add new versioning-specific tests (recommended)

---

*Discussion completed: 2026-04-30*
