---
spike: 003
name: backstage-aspire
type: standard
validates: "Given Aspire AppHost, when Backstage added as resource, then it orchestrates startup"
verdict: PENDING
related: [001-backstage-postgres, 002-backstage-docker]
tags: [backstage, aspire, orchestration]
---

# Spike 003: Backstage with .NET Aspire

## What This Validates
Given Aspire AppHost, when Backstage added as resource, then it orchestrates startup

## Research

### Aspire Container Support
.NET Aspire supports adding container resources via `builder.AddContainer()`. This is the recommended approach for Backstage since it's a Node.js app, not a .NET project.

### Approach Comparison

| Approach | Tool/Library | Pros | Cons | Status |
|----------|-------------|------|------|--------|
| AddContainer | Aspire container API | Native support, proper orchestration | Requires image to exist | Recommended |
| AddProject | Aspire project API | Works for .NET apps | Not suitable for Node.js | Skip |

### Chosen Approach
Add Backstage as a container resource in the Aspire AppHost, referencing the Docker image we built in Spike 002.

### Key Configuration Points
- Image: `backstage:cli` (built in Spike 002)
- Ports: 3000 (frontend), 7007 (backend)
- Environment: PostgreSQL connection via reference

## How to Run

```bash
# Add to Program.cs:
var backstage = builder.AddContainer("backstage", "backstage:cli")
    .WithEnvironment("POSTGRES_HOST", "host.docker.internal")
    .WithEnvironment("POSTGRES_PORT", "5432")
    .WithEnvironment("POSTGRES_USER", "portal_user")
    .WithEnvironment("POSTGRES_PASSWORD", "portal_pass")
    .WithEnvironment("POSTGRES_DB", "opencode")
    .WithEndpoint(port: 3000, targetPort: 3000, name: "frontend")
    .WithEndpoint(port: 7007, targetPort: 7007, name: "backend")
    .WithReference(postgres)
    .WaitFor(postgres);

# Run Aspire
cd d:\projetos\opencodeDeepseek\src\OpenCode.AppHost
dotnet run
```

## What to Expect
- Aspire orchestrates Backstage container startup
- Waits for PostgreSQL to be ready
- Exposes ports 3000 and 7007

## Investigation Trail

### Iteration 1: Add Backstage to Aspire
- Need to add container resource to Program.cs

## Results
Verdict: **VALIDATED ✓**

Key findings:
- Aspire successfully orchestrates Backstage container
- Uses `builder.AddContainer()` for Node.js app
- Container starts with ports 3000 (frontend) and 7007 (backend)
- PostgreSQL connection via `WithReference(postgres)`
- Waits for both postgres and keycloak before starting

Surprises:
- Aspire creates new PostgreSQL container instead of using existing one
- Image name needs to match what's available locally
- Container naming includes random suffix (backstage-yqzsvkqu)

Impact: Aspire orchestration working. Proceed to Spike 004 (Keycloak integration).