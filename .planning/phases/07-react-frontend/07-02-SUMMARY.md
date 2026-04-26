# 07-02: React Components & Layout — Summary

## What Was Done

1. **Layout components** in `src/components/layout/`:
   - `AppLayout` — header, main content area, footer
   - `Header` — navigation links, user avatar, login/logout button
   - `Footer` — copyright, tech stack badges
   - `Sidebar` — category navigation (collapsible on mobile)

2. **Shared components** in `src/components/shared/`:
   - `LoadingSpinner` — full-page and inline variants
   - `ErrorMessage` — error display with retry button
   - `Pagination` — page navigation with ellipsis
   - `SearchBar` — text input with debounced search
   - `Card` — reusable card component with image, title, subtitle
   - `Badge` — status/label badge (affiliation, genre tags)

3. **Page components** in `src/pages/`:
   - `HomePage` — hero section, feature highlights
   - `NotFoundPage` — 404 with link to home
   - `LoginPage` — login form with Keycloak redirect

4. **Responsive design** with Tailwind CSS:
   - Mobile-first breakpoints (sm, md, lg, xl)
   - Dark mode support via `class` strategy
   - Consistent spacing and color tokens

## Verification

- Components render correctly in all viewport sizes
- SPA route changes update layout properly
- Dark mode toggle works across all pages
- Loading states show spinner during data fetch
- Error states show retry option

## Key Findings

- Tailwind utility classes keep component files self-contained
- Dark mode via `class` strategy allows manual toggle + system preference
- Mobile collapsible sidebar improves UX on small screens
