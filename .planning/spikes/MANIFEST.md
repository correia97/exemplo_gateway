# Spike Manifest

## Idea
Build a Developer API Portal using Backstage.io that allows developers to register, browse APIs organized hierarchically (product→context→subcontext→api), and generate credentials for API consumption. Integrates with existing Keycloak and PostgreSQL infrastructure.

## Requirements
- [Must use existing PostgreSQL database with new schema]
- [Must integrate with existing Keycloak for auth]
- [Must run via Docker Compose]
- [Must run via .NET Aspire]
- [APIs organized as: product -> context -> subcontext -> api (e.g., credit -> financing -> vehicles -> dragonball-api)]

## Spikes

| # | Name | Type | Validates | Verdict | Tags |
|---|------|------|-----------|---------|------|
| 001 | backstage-postgres | standard | Given Backstage.io, when configured with PostgreSQL, then it starts and connects to DB | **VALIDATED ✓** | backstage, postgres, infrastructure |
| 002 | backstage-docker | standard | Given Backstage source, when built with Docker, then container runs successfully | **VALIDATED ✓** | backstage, docker, container |
| 003 | backstage-aspire | standard | Given Aspire AppHost, when Backstage added as resource, then it orchestrates startup | **VALIDATED ✓** | backstage, aspire, orchestration |
| 004 | keycloak-integration | standard | Given Backstage, when configured with Keycloak OIDC, then users can login via Keycloak | **VALIDATED ✓** | backstage, keycloak, auth |
| 005 | api-catalog-model | standard | Given Backstage, when custom entity defined for API products, then catalog shows hierarchical APIs | **VALIDATED ✓** | backstage, catalog, entity |
| 006 | developer-credentials | standard | Given registered developer, when they select APIs to consume, then credentials are generated | **VALIDATED ✓** | backstage, credentials, developer-portal |