#!/bin/bash
set -e

ADMIN_API="http://127.0.0.1:9180/apisix/admin"
ADMIN_KEY="${ADMIN_KEY:-edd1c9f034335f136f87ad84b625c8f1}"

UPSTREAM_DRAGONBALL="${UPSTREAM_DRAGONBALL_API:-http://dragonball-api:8080}"
UPSTREAM_MUSIC="${UPSTREAM_MUSIC_API:-http://music-api:8080}"

CORS_ORIGINS="${CORS_ORIGINS:-http://localhost:5173,http://localhost:3000,http://localhost:4200,http://localhost,http://localhost:5003}"

echo "Waiting for APISIX Admin API..."
until curl -s -o /dev/null -w "%{http_code}" "$ADMIN_API/routes" \
    -H "X-API-KEY: $ADMIN_KEY" | grep -q "200"; do
    sleep 2
done
echo "Admin API ready."

echo "Creating global rule: request-id (X-Correlation-Id)"
curl -s -X PUT "$ADMIN_API/global_rules/correlation-id" \
    -H "X-API-KEY: $ADMIN_KEY" \
    -H "Content-Type: application/json" \
    -d "$(cat <<EOF
{
    "plugins": {
        "request-id": {
            "header_name": "X-Correlation-Id",
            "include_in_response": true,
            "algorithm": "uuid"
        }
    }
}
EOF
)"

echo "Creating upstream: dragonball -> $UPSTREAM_DRAGONBALL"
curl -s -X PUT "$ADMIN_API/upstreams/dragonball" \
    -H "X-API-KEY: $ADMIN_KEY" \
    -H "Content-Type: application/json" \
    -d "$(cat <<EOF
{
    "type": "roundrobin",
    "nodes": {
        "$UPSTREAM_DRAGONBALL": 1
    }
}
EOF
)"

echo "Creating upstream: music -> $UPSTREAM_MUSIC"
curl -s -X PUT "$ADMIN_API/upstreams/music" \
    -H "X-API-KEY: $ADMIN_KEY" \
    -H "Content-Type: application/json" \
    -d "$(cat <<EOF
{
    "type": "roundrobin",
    "nodes": {
        "$UPSTREAM_MUSIC": 1
    }
}
EOF
)"

echo "Creating GET route: /api/dragonball/*"
curl -s -X PUT "$ADMIN_API/routes/dragonball-get" \
    -H "X-API-KEY: $ADMIN_KEY" \
    -H "Content-Type: application/json" \
    -d "$(cat <<EOF
{
    "uri": "/api/dragonball/*",
    "methods": ["GET"],
    "strip_path": true,
    "upstream_id": "dragonball",
    "plugins": {
        "cors": {
            "allow_origins": "$CORS_ORIGINS",
            "allow_methods": "GET,POST,PUT,DELETE,OPTIONS",
            "allow_headers": "*",
            "expose_headers": "X-Correlation-Id",
            "allow_credential": true,
            "max_age": 86400
        }
    }
}
EOF
)"

echo "Creating GET route: /api/music/*"
curl -s -X PUT "$ADMIN_API/routes/music-get" \
    -H "X-API-KEY: $ADMIN_KEY" \
    -H "Content-Type: application/json" \
    -d "$(cat <<EOF
{
    "uri": "/api/music/*",
    "methods": ["GET"],
    "strip_path": true,
    "upstream_id": "music",
    "plugins": {
        "cors": {
            "allow_origins": "$CORS_ORIGINS",
            "allow_methods": "GET,POST,PUT,DELETE,OPTIONS",
            "allow_headers": "*",
            "expose_headers": "X-Correlation-Id",
            "allow_credential": true,
            "max_age": 86400
        }
    }
}
EOF
)"

echo "Creating write route: /api/dragonball/* (OIDC)"
curl -s -X PUT "$ADMIN_API/routes/dragonball-write" \
    -H "X-API-KEY: $ADMIN_KEY" \
    -H "Content-Type: application/json" \
    -d "$(cat <<EOF
{
    "uri": "/api/dragonball/*",
    "methods": ["POST", "PUT", "DELETE"],
    "strip_path": true,
    "upstream_id": "dragonball",
    "plugins": {
        "cors": {
            "allow_origins": "$CORS_ORIGINS",
            "allow_methods": "GET,POST,PUT,DELETE,OPTIONS",
            "allow_headers": "*",
            "expose_headers": "X-Correlation-Id",
            "allow_credential": true,
            "max_age": 86400
        },
        "openid-connect": {
            "client_id": "dragonball-api",
            "client_secret": "${CLIENT_SECRET_DRAGONBALL:-dragonball-secret}",
            "discovery": "http://keycloak:8080/realms/opencode/.well-known/openid-configuration",
            "bearer_only": true,
            "ssl_verify": false,
            "realm": "opencode"
        }
    }
}
EOF
)"

echo "Creating write route: /api/music/* (OIDC)"
curl -s -X PUT "$ADMIN_API/routes/music-write" \
    -H "X-API-KEY: $ADMIN_KEY" \
    -H "Content-Type: application/json" \
    -d "$(cat <<EOF
{
    "uri": "/api/music/*",
    "methods": ["POST", "PUT", "DELETE"],
    "strip_path": true,
    "upstream_id": "music",
    "plugins": {
        "cors": {
            "allow_origins": "$CORS_ORIGINS",
            "allow_methods": "GET,POST,PUT,DELETE,OPTIONS",
            "allow_headers": "*",
            "expose_headers": "X-Correlation-Id",
            "allow_credential": true,
            "max_age": 86400
        },
        "openid-connect": {
            "client_id": "music-api",
            "client_secret": "${CLIENT_SECRET_MUSIC:-music-secret}",
            "discovery": "http://keycloak:8080/realms/opencode/.well-known/openid-configuration",
            "bearer_only": true,
            "ssl_verify": false,
            "realm": "opencode"
        }
    }
}
EOF
)"

echo "Route initialization complete."
