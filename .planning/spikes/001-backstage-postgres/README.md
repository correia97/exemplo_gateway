---
spike: 001
name: backstage-postgres
type: standard
validates: "Given Backstage.io, when configured with PostgreSQL, then it starts and connects to DB"
verdict: PENDING
related: []
tags: [backstage, postgres, infrastructure]
---

# Spike 001: Backstage with PostgreSQL

## What This Validates
Given Backstage.io, when configured with PostgreSQL, then it starts and connects to DB

## Research

### Backstage Database Options
Backstage natively supports SQLite (dev) and PostgreSQL (production). For this project, we need PostgreSQL to integrate with existing infrastructure.

### Approach Comparison

| Approach | Tool/Library | Pros | Cons | Status |
|----------|-------------|------|------|--------|
| Default PostgreSQL | `app-config.yaml` | Native support, well-documented | Requires config | Recommended |
| SQLite for dev | Built-in | No setup needed | Not suitable for production | Skip |

### Chosen Approach
Use PostgreSQL with native Backstage configuration via `app-config.yaml`. The existing project already has PostgreSQL running, so we'll add a new schema for Backstage.

### Key Configuration Points
- Database: existing PostgreSQL at `10.60.100.10:5432`
- New schema: `backstage` (create via init.sql)
- Authentication: via Keycloak OIDC (spike 004)

## How to Run

```bash
# From src/OpenCode.Backstage directory
cd d:\projetos\opencodeDeepseek\src\OpenCode.Backstage

# Install dependencies
npm install

# Start in development mode
npm start
```

## What to Expect
- Backstage starts on port 3000
- Connects to PostgreSQL database
- Shows the Backstage welcome page

## Investigation Trail

### Iteration 1: Initial Setup
- Created Backstage app using `npx @backstage/create-app@latest`
- Configured PostgreSQL connection in `app-config.yaml`

## Results
Verdict: **VALIDATED ✓**

Key findings:
- Backstage connects to PostgreSQL at localhost:5432
- Uses portal_user with portal_pass credentials
- Database schema defaults to 'public' (Backstage creates tables per-plugin)
- No connection errors in logs
- Frontend accessible at http://localhost:3000
- Backend API at http://localhost:7007

Surprises:
- PostgreSQL port was commented out in docker-compose.yml - had to enable it
- portal_user and portal schema didn't exist - had to create them manually
- Backstage uses public schema by default, not a dedicated schema

Impact: Database connection working. Proceed to Spike 002 (Docker containerization).