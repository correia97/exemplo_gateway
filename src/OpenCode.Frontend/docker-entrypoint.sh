#!/bin/sh
set -e

cat > /usr/share/nginx/html/env-config.js << EOF
window.__ENV__ = {
  DRAGONBALL_API_URL: "${DRAGONBALL_API_URL:-http://localhost:5000}",
  MUSIC_API_URL: "${MUSIC_API_URL:-http://localhost:5002}",
  KEYCLOAK_URL: "${KEYCLOAK_URL:-http://localhost:8080}"
};
EOF

exec nginx -g "daemon off;"
