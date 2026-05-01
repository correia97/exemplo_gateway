---
status: partial
phase: 14-api-developer-portal-with-backstage-io
source: [14-VERIFICATION.md]
started: "2026-05-01"
updated: "2026-05-01"
---

## Current Test

[awaiting human testing]

## Tests

### 1. Keycloak OIDC login flow
expected: Navigate to http://localhost:7007, browser redirects to Keycloak login page, successful login returns to Backstage portal as authenticated user

result: [pending]

### 2. Catalog entity hierarchy in UI
expected: Domain `opencode-platform`, Systems `dragonball-system` and `music-system`, Components and APIs all appear in catalog. API entity pages render the OpenAPI spec loaded via `$text: http://gateway:8000/...` through Kong

result: [pending]

### 3. Credentials guide rendered
expected: The `opencode-platform` Domain entity About card renders the "How to Get API Credentials" markdown section with step-by-step Keycloak client creation and token request instructions

result: [pending]

## Summary

total: 3
passed: 0
issues: 0
pending: 3
skipped: 0
blocked: 0

## Gaps
