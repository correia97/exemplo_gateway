#!/bin/bash
set -e

ADMIN_API="http://127.0.0.1:9180/apisix/admin"
ADMIN_KEY="${ADMIN_KEY:-edd1c9f034335f136f87ad84b625c8f1}"

JAEGER_ENDPOINT="${JAEGER_ENDPOINT:-http://jaeger:4318}"

echo "Waiting for APISIX Admin API..."
until curl -s -o /dev/null -w "%{http_code}" "$ADMIN_API/routes" \
    -H "X-API-KEY: $ADMIN_KEY" | grep -q "200"; do
    sleep 2
done
echo "Admin API ready."

echo "Adding OpenTelemetry plugin to dragonball-get route"
curl -s -X PATCH "$ADMIN_API/routes/dragonball-get" \
    -H "X-API-KEY: $ADMIN_KEY" \
    -H "Content-Type: application/json" \
    -d "$(cat <<EOF
{
    "plugins": {
        "opentelemetry": {
            "collector": {
                "endpoint": "$JAEGER_ENDPOINT/v1/traces",
                "headers": {
                    "content-type": "application/json"
                }
            },
            "sampler": {
                "name": "always_on"
            }
        }
    }
}
EOF
)"

echo "Adding OpenTelemetry plugin to dragonball-write route"
curl -s -X PATCH "$ADMIN_API/routes/dragonball-write" \
    -H "X-API-KEY: $ADMIN_KEY" \
    -H "Content-Type: application/json" \
    -d "$(cat <<EOF
{
    "plugins": {
        "opentelemetry": {
            "collector": {
                "endpoint": "$JAEGER_ENDPOINT/v1/traces",
                "headers": {
                    "content-type": "application/json"
                }
            },
            "sampler": {
                "name": "always_on"
            }
        }
    }
}
EOF
)"

echo "Adding OpenTelemetry plugin to music-get route"
curl -s -X PATCH "$ADMIN_API/routes/music-get" \
    -H "X-API-KEY: $ADMIN_KEY" \
    -H "Content-Type: application/json" \
    -d "$(cat <<EOF
{
    "plugins": {
        "opentelemetry": {
            "collector": {
                "endpoint": "$JAEGER_ENDPOINT/v1/traces",
                "headers": {
                    "content-type": "application/json"
                }
            },
            "sampler": {
                "name": "always_on"
            }
        }
    }
}
EOF
)"

echo "Adding OpenTelemetry plugin to music-write route"
curl -s -X PATCH "$ADMIN_API/routes/music-write" \
    -H "X-API-KEY: $ADMIN_KEY" \
    -H "Content-Type: application/json" \
    -d "$(cat <<EOF
{
    "plugins": {
        "opentelemetry": {
            "collector": {
                "endpoint": "$JAEGER_ENDPOINT/v1/traces",
                "headers": {
                    "content-type": "application/json"
                }
            },
            "sampler": {
                "name": "always_on"
            }
        }
    }
}
EOF
)"

echo "OpenTelemetry plugin initialization complete."
