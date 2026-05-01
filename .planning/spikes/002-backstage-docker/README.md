---
spike: 002
name: backstage-docker
type: standard
validates: "Given Backstage source, when built with Docker, then container runs successfully"
verdict: PENDING
related: [001-backstage-postgres]
tags: [backstage, docker, container]
---

# Spike 002: Backstage Docker Container

## What This Validates
Given Backstage source, when built with Docker, then container runs successfully

## Research

### Backstage Docker Options
Backstage provides official Docker support for production deployment. The existing project already has a Dockerfile in `src/OpenCode.Backstage/`.

### Approach Comparison

| Approach | Tool/Library | Pros | Cons | Status |
|----------|-------------|------|------|--------|
| Backstage CLI build-image | yarn build-image | Official, well-tested | Requires yarn workspace | Recommended |
| Direct docker build | docker build | Simple, no deps | Manual setup | Alternative |
| Multi-stage build | Dockerfile | Optimized size | More complex | Future |

### Chosen Approach
Use the existing Dockerfile at `src/OpenCode.Backstage/Dockerfile` and build a container image.

### Key Configuration Points
- Node.js 24 base image
- PostgreSQL client library (pg)
- Production dependencies only

## How to Run

```bash
# Build the Docker image
cd d:\projetos\opencodeDeepseek\src\OpenCode.Backstage
docker build -t backstage:latest .

# Run the container
docker run -p 3000:3000 -p 7007:7007 \
  -e POSTGRES_HOST=host.docker.internal \
  -e POSTGRES_PORT=5432 \
  -e POSTGRES_USER=portal_user \
  -e POSTGRES_PASSWORD=portal_pass \
  -e POSTGRES_DB=opencode \
  backstage:latest
```

## What to Expect
- Docker image builds successfully
- Container starts and connects to PostgreSQL
- Frontend accessible at port 3000
- Backend API at port 7007

## Investigation Trail

### Iteration 1: Build Docker Image
- Using existing Dockerfile in src/OpenCode.Backstage/

## Results
Verdict: **VALIDATED ✓**

Key findings:
- Docker image builds successfully using `yarn build-image`
- Container runs and exposes ports 3000 (frontend) and 7007 (backend)
- PostgreSQL connection works via `host.docker.internal`
- Uses environment variables for database configuration

Surprises:
- Manual Dockerfile approach failed due to missing node_modules
- Backstage CLI's `build-image` command properly handles dependencies
- `host.docker.internal` is needed for Docker-to-Docker communication on Windows

Impact: Docker container working. Proceed to Spike 003 (Aspire orchestration).