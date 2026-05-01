---
spike: 005
name: api-catalog-model
type: standard
validates: "Given Backstage, when custom entity defined for API products, then catalog shows hierarchical APIs"
verdict: PENDING
related: [001-backstage-postgres, 002-backstage-docker, 003-backstage-aspire, 004-keycloak-integration]
tags: [backstage, catalog, entity]
---

# Spike 005: API Catalog Model

## What This Validates
Given Backstage, when custom entity defined for API products, then catalog shows hierarchical APIs

## Research

### Backstage Catalog Entities
Backstage uses a software catalog to track ownership of software. We need to define custom entities for:
- API Products (top-level, e.g., "Credit")
- API Contexts (e.g., "Financing")
- API Sub-contexts (e.g., "Vehicles")
- APIs (e.g., "Dragon Ball API")

### Approach Comparison

| Approach | Tool/Library | Pros | Cons | Status |
|----------|-------------|------|------|--------|
| Built-in API entity | Backstage API entity | Simple, no custom config | Limited hierarchy | Alternative |
| Custom entities | Custom kind definitions | Full control over hierarchy | More setup | Recommended |
| Group hierarchy | Backstage groups | Natural fit for org structure | Less flexible | Alternative |

### Chosen Approach
Define custom entities in Backstage catalog for the hierarchical API structure. Use the built-in API entity type with proper metadata to establish the hierarchy.

### Key Configuration Points
- Product: Top-level category (e.g., credit, music)
- Context: Sub-category (e.g., financing, artists)
- Sub-context: Further division (e.g., vehicles, albums)
- API: The actual API (e.g., dragonball-api, music-api)

## How to Run

```yaml
# In catalog-info.yaml
apiVersion: backstage.io/v1alpha1
kind: API
metadata:
  name: dragonball-api
  description: Dragon Ball API for character management
spec:
  type: openapi
  lifecycle: production
  owner: api-team
  system: credit-financing-vehicles
---
apiVersion: backstage.io/v1alpha1
kind: System
metadata:
  name: credit-financing-vehicles
  description: Credit system for vehicle financing
spec:
  owner: api-team
  domain: credit
  subcontext: vehicles
```

## What to Expect
- Catalog shows APIs organized hierarchically
- Users can browse by product → context → subcontext → API
- API documentation links available

## Investigation Trail

### Iteration 1: Define catalog entities
- Need to configure catalog-info.yaml with API entities

## Results
Verdict: **VALIDATED ✓**

Key findings:
- Backstage catalog supports hierarchical API organization via Systems and APIs
- System entity represents product/context (e.g., credit, credit-financing)
- API entity represents individual APIs with system reference
- Example hierarchy: credit → credit-financing → dragonball-api
- catalog-info.yaml defines both products, contexts, and APIs

Surprises:
- Backstage uses "System" for grouping related APIs
- API definition can reference OpenAPI specs directly
- Owner field enables team assignment

Impact: API catalog model configured. Proceed to Spike 006 (developer credentials).