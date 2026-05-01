# Backstage Production Docker Image
#
# Host-build pattern: the Backstage app must be pre-built on the host before
# running `docker build`. Build steps:
#   1. yarn install --immutable
#   2. yarn tsc
#   3. yarn build:backend
#   4. docker build -f backstage/Dockerfile -t backstage:latest backstage/
#
# This image is used by both Aspire AppHost and docker-compose.yml (per D-01).
# Build once and reference by tag in both orchestration configurations.
#
# See: https://backstage.io/docs/deployment/docker/

# ---- Base Stage ----
FROM node:22-bookworm-slim AS base

# Build-time and runtime environment variables
ENV PYTHON=/usr/bin/python3
ENV NODE_ENV=production
ENV NODE_OPTIONS="--no-node-snapshot"

# Install system dependencies required for native module compilation
# Build-essential, python3, and g++ are needed for node-gyp and other native addons
# libsqlite3-dev is required by better-sqlite3 (optional local dev dependency)
RUN --mount=type=cache,target=/var/cache/apt,sharing=locked \
    --mount=type=cache,target=/var/lib/apt,sharing=locked \
    apt-get update && \
    apt-get install -y --no-install-recommends \
        python3 \
        g++ \
        build-essential \
        libsqlite3-dev \
    && rm -rf /var/lib/apt/lists/*

# Switch to non-root user for security (per deployment hardening pattern)
USER node

# ---- Build Stage ----
FROM base AS build

# Use non-root user
USER node
WORKDIR /app

# Copy Yarn configuration for deterministic installs
COPY --chown=node:node .yarn .yarn
COPY --chown=node:node .yarnrc.yml ./
COPY --chown=node:node backstage.json ./

# Copy dependency manifests
COPY --chown=node:node yarn.lock ./
COPY --chown=node:node package.json ./

# Copy pre-built backend skeleton (package.json structures only)
COPY --chown=node:node packages/backend/dist/skeleton.tar.gz ./packages/backend/dist/skeleton.tar.gz
RUN tar xzf packages/backend/dist/skeleton.tar.gz && rm packages/backend/dist/skeleton.tar.gz

# Install production dependencies only (workspace-focused install)
RUN yarn workspaces focus --all --production && yarn cache clean

# ---- App Stage ----
FROM build AS app

WORKDIR /app

# Copy pre-built backend bundle (compiled JavaScript)
COPY --chown=node:node packages/backend/dist/bundle.tar.gz ./packages/backend/dist/bundle.tar.gz
RUN tar xzf packages/backend/dist/bundle.tar.gz && rm packages/backend/dist/bundle.tar.gz

# Copy app configuration
COPY --chown=node:node app-config.yaml ./
COPY --chown=node:node app-config.production.yaml ./ 2>/dev/null || true

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=15s --retries=3 \
    CMD node -e "require('http').get('http://localhost:7007/api/health', (r) => { process.exit(r.statusCode === 200 ? 0 : 1) }).on('error', () => process.exit(1))"

# Expose Backstage backend port
EXPOSE 7007

# Start Backstage backend with production config
CMD ["node", "packages/backend", "--config", "app-config.yaml"]
