# 04-03: Dragon Ball API Authorization — Summary

## What Was Done

1. **Authorization annotations** added to `CharactersController`:
   - `[Authorize(Policy = "dragonball:read")]` on GET endpoints
   - `[Authorize(Policy = "dragonball:write")]` on POST, PUT, DELETE

2. **Authorization annotations** added to `TransformationsController`:
   - Read endpoints → `dragonball:read`
   - Write endpoints → `dragonball:write`

3. **Authorization annotations** added to `PlanetsController`:
   - Read endpoints → `dragonball:read`
   - Write endpoints → `dragonball:write`

4. **Admin-only endpoints**:
   - `DELETE /api/dragonball/characters/{id}` requires both `dragonball:write` AND `admin` policy

5. **401/403 responses**:
   - Unauthorized (no token / invalid token) → 401
   - Forbidden (missing role) → 403

## Verification

- `user@opencode.local` (read-only) can GET but not POST/PUT/DELETE
- `admin@opencode.local` can perform all operations
- No token → 401
- Token without required claim → 403

## Key Findings

- Policy-based auth allows fine-grained access control at endpoint level
- Combining multiple policies with `[Authorize]` attributes stacks requirements (AND logic)
- Read/write separation enables safe delegate access (auditors, read-only users)
