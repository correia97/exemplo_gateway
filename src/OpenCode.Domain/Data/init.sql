-- PostgreSQL initialization script for OpenCode
-- Runs on container first-start via /docker-entrypoint-initdb.d/
--
-- Creates 3 schemas for schema-based isolation:
--   dragonball   - Dragon Ball character data
--   music        - Music catalog (genres, artists, albums, tracks)
--   keycloak     - Keycloak authentication data
--
-- Creates 3 database users with schema-scoped permissions:
--   dragonball_user - CRUD on dragonball schema
--   music_user      - CRUD on music schema
--   keycloak_user   - CRUD on keycloak schema

-- ============================================================
-- SCHEMAS
-- ============================================================

CREATE SCHEMA IF NOT EXISTS dragonball;
CREATE SCHEMA IF NOT EXISTS music;
CREATE SCHEMA IF NOT EXISTS keycloak;

-- ============================================================
-- USERS
-- ============================================================

DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'dragonball_user') THEN
        CREATE USER dragonball_user WITH PASSWORD 'dragonball_pass';
    END IF;
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'music_user') THEN
        CREATE USER music_user WITH PASSWORD 'music_pass';
    END IF;
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'keycloak_user') THEN
        CREATE USER keycloak_user WITH PASSWORD 'keycloak_pass';
    END IF;
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'kong_user') THEN
        CREATE USER kong_user WITH PASSWORD 'kong_pass';
    END IF;
END
$$;

-- ============================================================
-- GRANTS -- Dragon Ball schema
-- ============================================================

GRANT USAGE ON SCHEMA dragonball TO dragonball_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA dragonball TO dragonball_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA dragonball TO dragonball_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA dragonball GRANT ALL ON TABLES TO dragonball_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA dragonball GRANT ALL ON SEQUENCES TO dragonball_user;
GRANT ALL PRIVILEGES ON SCHEMA dragonball TO dragonball_user;

-- ============================================================
-- GRANTS -- Music schema
-- ============================================================

GRANT USAGE ON SCHEMA music TO music_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA music TO music_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA music TO music_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA music GRANT ALL ON TABLES TO music_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA music GRANT ALL ON SEQUENCES TO music_user;
GRANT ALL PRIVILEGES ON SCHEMA music TO music_user;

-- ============================================================
-- GRANTS -- Keycloak schema
-- ============================================================

GRANT USAGE ON SCHEMA keycloak TO keycloak_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA keycloak TO keycloak_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA keycloak TO keycloak_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA keycloak GRANT ALL ON TABLES TO keycloak_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA keycloak GRANT ALL ON SEQUENCES TO keycloak_user;
GRANT ALL PRIVILEGES ON SCHEMA keycloak TO keycloak_user;
-- Ensure database-level CONNECT privilege for each user
GRANT CONNECT ON DATABASE opencode TO dragonball_user;
GRANT CONNECT ON DATABASE opencode TO music_user;
GRANT CONNECT ON DATABASE opencode TO keycloak_user;
GRANT CONNECT ON DATABASE opencode TO kong_user;

-- ============================================================
-- GRANTS -- Kong (uses public schema)
-- ============================================================

GRANT USAGE ON SCHEMA public TO kong_user;
GRANT CREATE ON SCHEMA public TO kong_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO kong_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO kong_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO kong_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO kong_user;
