#!/bin/bash
set -e

sleep 90

ADMIN_API="http://gateway:8001"
UPSTREAM_DB="${UPSTREAM_DRAGONBALL_API:-http://dragonball-api:8080}"
UPSTREAM_MUSIC="${UPSTREAM_MUSIC_API:-http://music-api:8080}"
CORS_ORIGINS="${CORS_ORIGINS:-\"http://localhost:5173\",\"http://localhost:3000\",\"http://localhost:4200\"}"

echo "Waiting for Kong Admin API..."
until curl -s -o /dev/null -w "%{http_code}" "$ADMIN_API/status" | grep -q "200"; do
  sleep 2
  echo "Waiting for Kong Admin API..."
done
echo "Kong Admin API ready."

echo "Creating service: dragonball-api -> $UPSTREAM_DB"
curl -s -X PUT "$ADMIN_API/services/dragonball-api" \
  -H "Content-Type: application/json" \
  -d "$(cat <<EOF
{
  "name": "dragonball-api",
  "url": "${UPSTREAM_DB}",
  "path": "/api"
}
EOF
)"

echo "Creating route: /api/dragonball for dragonball-api"
curl -s -X POST "$ADMIN_API/services/dragonball-api/routes" \
  -H "Content-Type: application/json" \
  -d "$(cat <<EOF
{
  "name": "dragonball",
  "paths": ["/api/dragonball"],
  "strip_path": true,
  "methods": ["GET", "POST", "PUT", "DELETE", "OPTIONS"]
}
EOF
)"

echo "Creating service: music-api -> $UPSTREAM_MUSIC"
curl -s -X PUT "$ADMIN_API/services/music-api" \
  -H "Content-Type: application/json" \
  -d "$(cat <<EOF
{
  "name": "music-api",
  "url": "${UPSTREAM_MUSIC}",
  "path": "/api"
}
EOF
)"

echo "Creating route: /api/music for music-api"
curl -s -X POST "$ADMIN_API/services/music-api/routes" \
  -H "Content-Type: application/json" \
  -d "$(cat <<EOF
{
  "name": "music",
  "paths": ["/api/music"],
  "strip_path": true,
  "methods": ["GET", "POST", "PUT", "DELETE", "OPTIONS"]
}
EOF
)"

echo "Adding CORS plugin to dragonball-api"
curl -s -X POST "$ADMIN_API/services/dragonball-api/plugins" \
  -H "Content-Type: application/json" \
  -d "$(cat <<EOF
{
  "name": "cors",
  "config": {
    "origins": [${CORS_ORIGINS}],
    "methods": ["GET", "POST", "PUT", "DELETE", "OPTIONS"],
    "headers": ["*"],
    "exposed_headers": ["X-Correlation-Id"],
    "credentials": true,
    "max_age": 86400
  }
}
EOF
)"

echo "Adding OIDC auth plugin to dragonball-api"
curl -s -X POST "$ADMIN_API/services/dragonball-api/plugins" \
  -H "Content-Type: application/json" \
  -d "$(cat <<EOF
{
  "name": "openid-connect",
  "config": {
    "issuer": "http://keycloak:8080/realms/opencode",
    "client_id": "kong-gateway",
    "client_secret": "\${OIDC_CLIENT_SECRET:-CHANGE_ME}",
    "auth_methods": ["bearer"],
    "bearer_token_param_type": ["header"],
    "run_on_preflight": false
  },
  "protocols": ["http", "https"],
  "enabled": true
}
EOF
)"

echo "Adding CORS plugin to music-api"
curl -s -X POST "$ADMIN_API/services/music-api/plugins" \
  -H "Content-Type: application/json" \
  -d "$(cat <<EOF
{
  "name": "cors",
  "config": {
    "origins": [${CORS_ORIGINS}],
    "methods": ["GET", "POST", "PUT", "DELETE", "OPTIONS"],
    "headers": ["*"],
    "exposed_headers": ["X-Correlation-Id"],
    "credentials": true,
    "max_age": 86400
  }
}
EOF
)"

echo "Adding OIDC auth plugin to music-api"
curl -s -X POST "$ADMIN_API/services/music-api/plugins" \
  -H "Content-Type: application/json" \
  -d "$(cat <<EOF
{
  "name": "openid-connect",
  "config": {
    "issuer": "http://keycloak:8080/realms/opencode",
    "client_id": "kong-gateway",
    "client_secret": "\${OIDC_CLIENT_SECRET:-CHANGE_ME}",
    "auth_methods": ["bearer"],
    "bearer_token_param_type": ["header"],
    "run_on_preflight": false
  },
  "protocols": ["http", "https"],
  "enabled": true
}
EOF
)"

echo "Adding correlation-id plugin globally"
curl -s -X POST "$ADMIN_API/plugins" \
  -H "Content-Type: application/json" \
  -d "$(cat <<EOF
{
  "name": "correlation-id",
  "config": {
    "header_name": "X-Correlation-Id",
    "generator": "uuid",
    "echo_downstream": true
  }
}
EOF
)"

echo "Route initialization complete."
