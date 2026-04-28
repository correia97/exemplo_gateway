#!/usr/bin/env bash
# =============================================================================
# Cadastro das APIs no Apache APISIX
# APIs: DragonBall v1 | Music v1
#
# Ajuste as variáveis abaixo antes de executar.
# =============================================================================

set -euo pipefail

# ---------------------------------------------------------------------------
# Configuração – edite conforme seu ambiente
# ---------------------------------------------------------------------------
APISIX_ADMIN_URL="${APISIX_ADMIN_URL:-http://localhost:9180}"
ADMIN_KEY="${APISIX_ADMIN_KEY:-edd1c9f034335f136f87ad84b625c8f1}"

DRAGONBALL_UPSTREAM="${DRAGONBALL_UPSTREAM:-http://192.168.3.4:5000}"
MUSIC_UPSTREAM="${MUSIC_UPSTREAM:-http://192.168.3.4:5002}"

# ---------------------------------------------------------------------------
# Helper
# ---------------------------------------------------------------------------
log()  { echo -e "\033[1;34m[INFO]\033[0m  $*"; }
ok()   { echo -e "\033[1;32m[ OK ]\033[0m  $*"; }
fail() { echo -e "\033[1;31m[FAIL]\033[0m  $*"; exit 1; }

apisix_put() {
  local path="$1"
  local body="$2"
  local url="${APISIX_ADMIN_URL}/apisix/admin${path}"
  local http_code

  http_code=$(curl -s -o /tmp/apisix_resp.json -w "%{http_code}" \
    -X PUT "$url" \
    -H "X-API-KEY: ${ADMIN_KEY}" \
    -H "Content-Type: application/json" \
    -d "$body")

  if [[ "$http_code" =~ ^2 ]]; then
    ok "PUT ${path}  →  HTTP ${http_code}"
  else
    echo "  Resposta: $(cat /tmp/apisix_resp.json)"
    fail "PUT ${path}  →  HTTP ${http_code}"
  fi
}

# ===========================================================================
# ██████╗ ██████╗  █████╗  ██████╗  ██████╗ ███╗   ██╗██████╗  █████╗ ██╗     ██╗
# ██╔══██╗██╔══██╗██╔══██╗██╔════╝ ██╔═══██╗████╗  ██║██╔══██╗██╔══██╗██║     ██║
# ██║  ██║██████╔╝███████║██║  ███╗██║   ██║██╔██╗ ██║██████╔╝███████║██║     ██║
# ██║  ██║██╔══██╗██╔══██║██║   ██║██║   ██║██║╚██╗██║██╔══██╗██╔══██║██║     ██║
# ██████╔╝██║  ██║██║  ██║╚██████╔╝╚██████╔╝██║ ╚████║██████╔╝██║  ██║███████╗███████╗
# ===========================================================================

log "=== Cadastrando Upstream: DragonBall API ==="
apisix_put "/upstreams/dragonball-upstream" '{
  "id": "dragonball-upstream",
  "name": "DragonBall API Upstream",
  "type": "roundrobin",
  "nodes": {
    "'"${DRAGONBALL_UPSTREAM#http://}"'": 1
  },
  "scheme": "http",
  "pass_host": "pass",
  "keepalive_pool": { "idle_timeout": 60, "requests": 1000, "size": 320 }
}'

log "=== Cadastrando Rotas: DragonBall – Characters ==="

# GET /api/characters  (listar com paginação e filtros)
apisix_put "/routes/dragonball-characters-list" '{
  "id": "dragonball-characters-list",
  "name": "DragonBall – GET /api/characters",
  "uri": "/dragonball/api/characters",
  "methods": ["GET"],
  "upstream_id": "dragonball-upstream",
  "plugins": {
    "proxy-rewrite": { "regex_uri": ["^/dragonball(.*)", "$1"] }
  }
}'

# POST /api/characters  (criar personagem)
apisix_put "/routes/dragonball-characters-create" '{
  "id": "dragonball-characters-create",
  "name": "DragonBall – POST /api/characters",
  "uri": "/dragonball/api/characters",
  "methods": ["POST"],
  "upstream_id": "dragonball-upstream",
  "plugins": {
    "proxy-rewrite": { "regex_uri": ["^/dragonball(.*)", "$1"] }
  }
}'

# GET /api/characters/{id}
apisix_put "/routes/dragonball-characters-get" '{
  "id": "dragonball-characters-get",
  "name": "DragonBall – GET /api/characters/{id}",
  "uri": "/dragonball/api/characters/*",
  "methods": ["GET"],
  "upstream_id": "dragonball-upstream",
  "plugins": {
    "proxy-rewrite": { "regex_uri": ["^/dragonball(.*)", "$1"] }
  }
}'

# PUT /api/characters/{id}
apisix_put "/routes/dragonball-characters-update" '{
  "id": "dragonball-characters-update",
  "name": "DragonBall – PUT /api/characters/{id}",
  "uri": "/dragonball/api/characters/*",
  "methods": ["PUT"],
  "upstream_id": "dragonball-upstream",
  "plugins": {
    "proxy-rewrite": { "regex_uri": ["^/dragonball(.*)", "$1"] }
  }
}'

# DELETE /api/characters/{id}
apisix_put "/routes/dragonball-characters-delete" '{
  "id": "dragonball-characters-delete",
  "name": "DragonBall – DELETE /api/characters/{id}",
  "uri": "/dragonball/api/characters/*",
  "methods": ["DELETE"],
  "upstream_id": "dragonball-upstream",
  "plugins": {
    "proxy-rewrite": { "regex_uri": ["^/dragonball(.*)", "$1"] }
  }
}'

log "=== Cadastrando Rotas: DragonBall – Seed ==="

# POST /api/seed
apisix_put "/routes/dragonball-seed" '{
  "id": "dragonball-seed",
  "name": "DragonBall – POST /api/seed",
  "uri": "/dragonball/api/seed",
  "methods": ["POST"],
  "upstream_id": "dragonball-upstream",
  "plugins": {
    "proxy-rewrite": { "regex_uri": ["^/dragonball(.*)", "$1"] }
  }
}'

ok "=== DragonBall API registrada com sucesso! ==="

# ===========================================================================
# ███╗   ███╗██╗   ██╗███████╗██╗ ██████╗      █████╗ ██████╗ ██╗
# ████╗ ████║██║   ██║██╔════╝██║██╔════╝     ██╔══██╗██╔══██╗██║
# ██╔████╔██║██║   ██║███████╗██║██║          ███████║██████╔╝██║
# ██║╚██╔╝██║██║   ██║╚════██║██║██║          ██╔══██║██╔═══╝ ██║
# ██║ ╚═╝ ██║╚██████╔╝███████║██║╚██████╗     ██║  ██║██║     ██║
# ===========================================================================

log "=== Cadastrando Upstream: Music API ==="
apisix_put "/upstreams/music-upstream" '{
  "id": "music-upstream",
  "name": "Music API Upstream",
  "type": "roundrobin",
  "nodes": {
    "'"${MUSIC_UPSTREAM#http://}"'": 1
  },
  "scheme": "http",
  "pass_host": "pass",
  "keepalive_pool": { "idle_timeout": 60, "requests": 1000, "size": 320 }
}'

log "=== Cadastrando Rotas: Music – Genres ==="

apisix_put "/routes/music-genres-list" '{
  "id": "music-genres-list",
  "name": "Music – GET /api/genres",
  "uri": "/music/api/genres",
  "methods": ["GET"],
  "upstream_id": "music-upstream",
  "plugins": { "proxy-rewrite": { "regex_uri": ["^/music(.*)", "$1"] } }
}'

apisix_put "/routes/music-genres-create" '{
  "id": "music-genres-create",
  "name": "Music – POST /api/genres",
  "uri": "/music/api/genres",
  "methods": ["POST"],
  "upstream_id": "music-upstream",
  "plugins": { "proxy-rewrite": { "regex_uri": ["^/music(.*)", "$1"] } }
}'

apisix_put "/routes/music-genres-get" '{
  "id": "music-genres-get",
  "name": "Music – GET /api/genres/{id}",
  "uri": "/music/api/genres/*",
  "methods": ["GET"],
  "upstream_id": "music-upstream",
  "plugins": { "proxy-rewrite": { "regex_uri": ["^/music(.*)", "$1"] } }
}'

apisix_put "/routes/music-genres-update" '{
  "id": "music-genres-update",
  "name": "Music – PUT /api/genres/{id}",
  "uri": "/music/api/genres/*",
  "methods": ["PUT"],
  "upstream_id": "music-upstream",
  "plugins": { "proxy-rewrite": { "regex_uri": ["^/music(.*)", "$1"] } }
}'

apisix_put "/routes/music-genres-delete" '{
  "id": "music-genres-delete",
  "name": "Music – DELETE /api/genres/{id}",
  "uri": "/music/api/genres/*",
  "methods": ["DELETE"],
  "upstream_id": "music-upstream",
  "plugins": { "proxy-rewrite": { "regex_uri": ["^/music(.*)", "$1"] } }
}'

log "=== Cadastrando Rotas: Music – Artists ==="

apisix_put "/routes/music-artists-list" '{
  "id": "music-artists-list",
  "name": "Music – GET /api/artists",
  "uri": "/music/api/artists",
  "methods": ["GET"],
  "upstream_id": "music-upstream",
  "plugins": { "proxy-rewrite": { "regex_uri": ["^/music(.*)", "$1"] } }
}'

apisix_put "/routes/music-artists-create" '{
  "id": "music-artists-create",
  "name": "Music – POST /api/artists",
  "uri": "/music/api/artists",
  "methods": ["POST"],
  "upstream_id": "music-upstream",
  "plugins": { "proxy-rewrite": { "regex_uri": ["^/music(.*)", "$1"] } }
}'

apisix_put "/routes/music-artists-get" '{
  "id": "music-artists-get",
  "name": "Music – GET /api/artists/{id}",
  "uri": "/music/api/artists/*",
  "methods": ["GET"],
  "upstream_id": "music-upstream",
  "plugins": { "proxy-rewrite": { "regex_uri": ["^/music(.*)", "$1"] } }
}'

apisix_put "/routes/music-artists-update" '{
  "id": "music-artists-update",
  "name": "Music – PUT /api/artists/{id}",
  "uri": "/music/api/artists/*",
  "methods": ["PUT"],
  "upstream_id": "music-upstream",
  "plugins": { "proxy-rewrite": { "regex_uri": ["^/music(.*)", "$1"] } }
}'

apisix_put "/routes/music-artists-delete" '{
  "id": "music-artists-delete",
  "name": "Music – DELETE /api/artists/{id}",
  "uri": "/music/api/artists/*",
  "methods": ["DELETE"],
  "upstream_id": "music-upstream",
  "plugins": { "proxy-rewrite": { "regex_uri": ["^/music(.*)", "$1"] } }
}'

# GET /api/artists/{artistId}/albums  (sub-rota específica – deve vir antes do wildcard simples)
apisix_put "/routes/music-artists-albums" '{
  "id": "music-artists-albums",
  "name": "Music – GET /api/artists/{artistId}/albums",
  "uri": "/music/api/artists/*/albums",
  "methods": ["GET"],
  "upstream_id": "music-upstream",
  "plugins": { "proxy-rewrite": { "regex_uri": ["^/music(.*)", "$1"] } }
}'

log "=== Cadastrando Rotas: Music – Albums ==="

apisix_put "/routes/music-albums-list" '{
  "id": "music-albums-list",
  "name": "Music – GET /api/albums",
  "uri": "/music/api/albums",
  "methods": ["GET"],
  "upstream_id": "music-upstream",
  "plugins": { "proxy-rewrite": { "regex_uri": ["^/music(.*)", "$1"] } }
}'

apisix_put "/routes/music-albums-create" '{
  "id": "music-albums-create",
  "name": "Music – POST /api/albums",
  "uri": "/music/api/albums",
  "methods": ["POST"],
  "upstream_id": "music-upstream",
  "plugins": { "proxy-rewrite": { "regex_uri": ["^/music(.*)", "$1"] } }
}'

apisix_put "/routes/music-albums-get" '{
  "id": "music-albums-get",
  "name": "Music – GET /api/albums/{id}",
  "uri": "/music/api/albums/*",
  "methods": ["GET"],
  "upstream_id": "music-upstream",
  "plugins": { "proxy-rewrite": { "regex_uri": ["^/music(.*)", "$1"] } }
}'

apisix_put "/routes/music-albums-update" '{
  "id": "music-albums-update",
  "name": "Music – PUT /api/albums/{id}",
  "uri": "/music/api/albums/*",
  "methods": ["PUT"],
  "upstream_id": "music-upstream",
  "plugins": { "proxy-rewrite": { "regex_uri": ["^/music(.*)", "$1"] } }
}'

apisix_put "/routes/music-albums-delete" '{
  "id": "music-albums-delete",
  "name": "Music – DELETE /api/albums/{id}",
  "uri": "/music/api/albums/*",
  "methods": ["DELETE"],
  "upstream_id": "music-upstream",
  "plugins": { "proxy-rewrite": { "regex_uri": ["^/music(.*)", "$1"] } }
}'

# GET /api/albums/{albumId}/tracks
apisix_put "/routes/music-albums-tracks" '{
  "id": "music-albums-tracks",
  "name": "Music – GET /api/albums/{albumId}/tracks",
  "uri": "/music/api/albums/*/tracks",
  "methods": ["GET"],
  "upstream_id": "music-upstream",
  "plugins": { "proxy-rewrite": { "regex_uri": ["^/music(.*)", "$1"] } }
}'

log "=== Cadastrando Rotas: Music – Tracks ==="

apisix_put "/routes/music-tracks-list" '{
  "id": "music-tracks-list",
  "name": "Music – GET /api/tracks",
  "uri": "/music/api/tracks",
  "methods": ["GET"],
  "upstream_id": "music-upstream",
  "plugins": { "proxy-rewrite": { "regex_uri": ["^/music(.*)", "$1"] } }
}'

apisix_put "/routes/music-tracks-create" '{
  "id": "music-tracks-create",
  "name": "Music – POST /api/tracks",
  "uri": "/music/api/tracks",
  "methods": ["POST"],
  "upstream_id": "music-upstream",
  "plugins": { "proxy-rewrite": { "regex_uri": ["^/music(.*)", "$1"] } }
}'

apisix_put "/routes/music-tracks-get" '{
  "id": "music-tracks-get",
  "name": "Music – GET /api/tracks/{id}",
  "uri": "/music/api/tracks/*",
  "methods": ["GET"],
  "upstream_id": "music-upstream",
  "plugins": { "proxy-rewrite": { "regex_uri": ["^/music(.*)", "$1"] } }
}'

apisix_put "/routes/music-tracks-update" '{
  "id": "music-tracks-update",
  "name": "Music – PUT /api/tracks/{id}",
  "uri": "/music/api/tracks/*",
  "methods": ["PUT"],
  "upstream_id": "music-upstream",
  "plugins": { "proxy-rewrite": { "regex_uri": ["^/music(.*)", "$1"] } }
}'

apisix_put "/routes/music-tracks-delete" '{
  "id": "music-tracks-delete",
  "name": "Music – DELETE /api/tracks/{id}",
  "uri": "/music/api/tracks/*",
  "methods": ["DELETE"],
  "upstream_id": "music-upstream",
  "plugins": { "proxy-rewrite": { "regex_uri": ["^/music(.*)", "$1"] } }
}'

log "=== Cadastrando Rotas: Music – Seed ==="

apisix_put "/routes/music-seed" '{
  "id": "music-seed",
  "name": "Music – POST /api/seed",
  "uri": "/music/api/seed",
  "methods": ["POST"],
  "upstream_id": "music-upstream",
  "plugins": { "proxy-rewrite": { "regex_uri": ["^/music(.*)", "$1"] } }
}'

ok "=== Music API registrada com sucesso! ==="
echo ""
echo "============================================================"
echo "  Todas as rotas foram cadastradas no Apache APISIX."
echo "  Gateway base : ${APISIX_ADMIN_URL/9180/9080}"
echo "  DragonBall   : <gateway>/dragonball/api/..."
echo "  Music        : <gateway>/music/api/..."
echo "============================================================"
