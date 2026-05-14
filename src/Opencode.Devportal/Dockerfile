FROM node:24-trixie-slim AS build

ENV PYTHON=/usr/bin/python3
ENV CI=true

RUN --mount=type=cache,target=/var/cache/apt,sharing=locked \
    --mount=type=cache,target=/var/lib/apt,sharing=locked \
    apt-get update && \
    apt-get install -y --no-install-recommends \
      python3 g++ build-essential \
      libsqlite3-dev \
      curl ca-certificates && \
    rm -rf /var/lib/apt/lists/*

USER node
WORKDIR /app

COPY --chown=node:node .yarn ./.yarn
COPY --chown=node:node .yarnrc.yml package.json yarn.lock backstage.json tsconfig.json ./
COPY --chown=node:node packages/app/package.json ./packages/app/package.json
COPY --chown=node:node packages/backend/package.json ./packages/backend/package.json
COPY --chown=node:node plugins/opencode-api-registration/package.json ./plugins/opencode-api-registration/package.json

RUN --mount=type=cache,target=/home/node/.cache/yarn,sharing=locked,uid=1000,gid=1000 \
    yarn install --immutable

COPY --chown=node:node . .

RUN yarn tsc && yarn build:backend


FROM node:24-trixie-slim AS runtime

ENV PYTHON=/usr/bin/python3
ENV NODE_ENV=production
ENV NODE_OPTIONS="--no-node-snapshot"

RUN --mount=type=cache,target=/var/cache/apt,sharing=locked \
    --mount=type=cache,target=/var/lib/apt,sharing=locked \
    apt-get update && \
    apt-get install -y --no-install-recommends \
      python3 g++ build-essential \
      libsqlite3-dev \
      curl ca-certificates && \
    rm -rf /var/lib/apt/lists/*

USER node
WORKDIR /app

COPY --from=build --chown=node:node /app/.yarn ./.yarn
COPY --from=build --chown=node:node /app/.yarnrc.yml /app/backstage.json /app/yarn.lock /app/package.json /app/packages/backend/dist/skeleton.tar.gz ./
RUN tar xzf skeleton.tar.gz && rm skeleton.tar.gz

RUN --mount=type=cache,target=/home/node/.cache/yarn,sharing=locked,uid=1000,gid=1000 \
    yarn workspaces focus --all --production && rm -rf "$(yarn cache clean)"

COPY --from=build --chown=node:node /app/examples ./examples
COPY --from=build --chown=node:node /app/packages/backend/dist/bundle.tar.gz /app/app-config.yaml /app/app-config.production.yaml ./
RUN tar xzf bundle.tar.gz && rm bundle.tar.gz

EXPOSE 7007

HEALTHCHECK --interval=15s --timeout=5s --start-period=30s --retries=3 \
  CMD curl -fsS http://localhost:7007/healthcheck || exit 1

CMD ["node", "packages/backend", "--config", "app-config.yaml", "--config", "app-config.production.yaml"]
