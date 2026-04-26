# 04-04: Music API Authorization — Summary

## What Was Done

1. **Authorization annotations** added to `ArtistsController`:
   - `[Authorize(Policy = "music:read")]` on GET endpoints
   - `[Authorize(Policy = "music:write")]` on POST, PUT, DELETE

2. **Authorization annotations** added to `AlbumsController`:
   - Read endpoints → `music:read`
   - Write endpoints → `music:write`

3. **Authorization annotations** added to `SongsController`:
   - Read endpoints → `music:read`
   - Write endpoints → `music:write`

4. **Admin-only endpoints**:
   - `DELETE` operations require both `music:write` AND `admin` policy

5. **Consistent auth behavior** across both APIs:
   - Same 401/403 response format as Dragon Ball API
   - Common `ProblemDetails` format for auth errors

## Verification

- `user@opencode.local` can read music catalog but not modify
- `admin@opencode.local` has full CRUD access
- Cross-API test: Dragon Ball token cannot access Music API endpoints
- Token scoped to `music:*` roles works correctly

## Key Findings

- Separate client roles (`music:*` vs `dragonball:*`) enforce API-level isolation
- Token contains only the roles assigned to the client; cross-API access is naturally blocked
- Consistent 401/403 handling across both APIs via shared `ExceptionMiddleware`
