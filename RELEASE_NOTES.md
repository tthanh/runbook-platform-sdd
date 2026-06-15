# Release Notes — v0.1.2

**Released**: 2026-06-15
**Initiative**: none — visual refresh, not a product initiative (no PRD, no spec folder)
**ADRs**: none

## What ships

No functional change. A hand-written CSS refresh turns the bare-HTML frontend into a styled
application:

- A design-token system in `frontend/src/index.css` — neutral surface ramp, brand/semantic
  colors, spacing, radius, and elevation scales — with a matching dark-mode set under
  `prefers-color-scheme`.
- A shared sticky app header (brand + link home) added in `frontend/src/App.tsx`; routing logic
  unchanged (extracted verbatim into `routeToPage`).
- Styled buttons (primary/secondary/ghost/disabled with focus rings), inputs, cards, the
  numbered step editor, and the execution run/timeline/coverage lists.
- Semantic outcome colors wired to the existing `step-done` / `step-skipped` / `step-failed`
  classes (green / amber / red) — previously referenced in the markup but unstyled.

Markup changes are limited to the new header; no component behavior, API call, route, or DTO
changed. The dependency-free intent of the slice is preserved — no styling packages added.

## Incidental fix

`frontend/src/api/client.ts` — `ApiError` used a TypeScript constructor parameter-property
(`public readonly status`), which the project's `erasableSyntaxOnly` setting rejects, breaking
`npm run build` on `main`. Rewritten as an explicit field assignment. Build now green.

## Verification

`npm run build` (tsc -b + vite build) passes. No backend or test changes.

---

# Release Notes — v0.1.1

**Released**: 2026-06-13
**Initiative**: 02-rich-domain-model (structural refactor)
**Spec folder**: specs/002-rich-domain-model/
**ADRs**: ADR-0003 (Rich domain model — invariants enforced inside aggregates)

## What ships

No user-facing change. Every domain invariant moved out of the HTTP handlers and into the
aggregates: `Runbook` is now the aggregate root and owns the publish gate (FR-003), the version
freeze (FR-004/FR-006), and sequential numbering (FR-005, ADR-0001) through behavior methods
(`Create` / `ReplaceSteps` / `Publish`). Runbook Version and its frozen Steps are construct-only.
A single `DomainException` is mapped once at the endpoint seam to the existing `400 { error }` shape.

## Acceptance contract

- The 18 integration tests pass **unmodified** — verified, and the refactor commit touched zero
  files under `backend/tests/`. Any test edit would have meant behavior changed.
- HTTP contract (`specs/001-runbook-authoring/contracts/http-api.md`) unchanged.
- Database schema unchanged — conflict register **C-002** untouched, no migrations introduced.
- Functional coverage unchanged: FR-001–FR-011, same tests, same ids. No new FRs.

## Architectural outcome

Conflict register gains **C-004** (ADR-0003): domain invariants live inside aggregates; HTTP
handlers stay thin. Future slices add new invariants in aggregate behavior, not in endpoints.

---

# Release Notes — v0.1.0

**Released**: 2026-06-12
**Initiative**: 01-versioned-runbook-execution (first slice: authoring only)
**Spec folder**: specs/001-runbook-authoring/
**ADRs**: ADR-0001 (Runbook Version identity), ADR-0002 (technology stack)

## What ships

A working web application: an author can create a named Runbook, add and reorder Steps,
publish an immutable Runbook Version, republish after edits (each publish produces the next
sequential version number), and browse all Runbooks and view any published version by number.

## User stories delivered

| Story | Description | Status |
|-------|-------------|--------|
| US1 | Create a Runbook and publish its first Runbook Version | Done |
| US2 | Edit and republish; earlier Runbook Versions stay intact | Done |
| US3 | Browse Runbooks and view any published Runbook Version | Done |

## Functional requirements covered

FR-001 through FR-011 — all 11 requirements from `specs/001-runbook-authoring/spec.md`.

## Test coverage

18 integration tests (xUnit / WebApplicationFactory) — all green.
Tests name the FR id(s) they cover per the constitution.
See `backend/tests/RunbookPlatform.Api.Tests/`.

## Quickstart

`specs/001-runbook-authoring/quickstart.md` — 13-step validation walk, verified against the live API.

## Deferred to next slice

The execution-layer concepts (Execution, Version Pin, Step Record, Computed Review, Incident)
are explicitly out of scope here; five workshop terms remain deferred.
See `docs/initiatives/01-versioned-runbook-execution/workshop.md` and the constitution glossary.

## Deviations from tasks.md

| Deviation | Reason |
|-----------|--------|
| `EnsureCreated` instead of EF Core migrations (T005 said "migration") | Single schema version at this slice; migration tooling is unearned complexity until slice 2 changes the schema |
| `OrderBy` in-memory for the Runbook list | SQLite cannot translate `OrderBy(DateTimeOffset)` in EF Core at this provider version; in-memory ordering is correct and fast at this scale |
