# Research: Runbook Authoring

Phase 0 output. All Technical Context unknowns resolved; decisions with real
alternatives are promoted to ADRs (referenced by id), per the constitution.

## R1 — Technology stack

- **Decision**: .NET 10 (LTS) ASP.NET Core minimal API backend + React (Vite,
  TypeScript) frontend. → formalized in **ADR-0002**.
- **Rationale**: Direction chosen by the author when asked (constitution:
  human-context questions before the Decision field). One small API project and
  one SPA fit the couple-of-evenings appetite.
- **Alternatives considered**: TypeScript full-stack (one language end to end);
  Python + FastAPI; Go + server-rendered HTML. All viable; author preference
  settled it — recorded in ADR-0002.

## R2 — Storage

- **Decision**: SQLite via EF Core, one database file. → part of **ADR-0002**.
- **Rationale**: Single-user demo, no ops burden, transactional (needed for
  version-number assignment), trivially portable. A server database adds setup
  cost no requirement has earned.
- **Alternatives considered**: PostgreSQL (ops weight unearned); JSON files on
  disk (no transactions — version numbering and immutability would be hand-rolled).

## R3 — Runbook Version identity

- **Decision**: Per-Runbook sequential integers starting at 1, assigned at
  publish inside a transaction; uniqueness enforced by the database on
  (Runbook, version number). → formalized in **ADR-0001**.
- **Rationale**: The PRD already fixes the author-facing behavior ("the first
  published version is 1, the next 2"); workshop hotspot H1 direction recorded
  2026-06-12. The DB-enforced unique pair makes "which version was in effect"
  provable and concurrency-safe.
- **Alternatives considered**: Content hash (opaque to authors, no ordering);
  semver (implies semantic judgement no requirement asks of authors). Recorded
  in ADR-0001.

## R4 — Immutability enforcement (FR-006, US2-3)

- **Decision**: Published Runbook Versions live in their own tables (version +
  frozen Step copies) that the application never updates or deletes; the API
  exposes no mutation endpoint for them. Editing only touches the Runbook's
  working Steps.
- **Rationale**: Structural immutability (no code path exists) beats validated
  immutability (code path exists but is guarded). Simplest thing that makes
  scenario US2-3 unfalsifiable by construction.
- **Alternatives considered**: Soft "is_published" flag on one shared steps
  table (single table, but every edit must remember the guard — easy to get
  wrong); event sourcing (massively unearned for this slice).

## R5 — Editing contract for working Steps (FR-002)

- **Decision**: The frontend edits the working Step list locally and saves it
  as one full ordered replacement (single PUT of the Step list).
- **Rationale**: Add/edit/remove/reorder collapse into one operation; no
  per-Step endpoints, no ordering patch protocol. Cheapest contract that
  satisfies FR-002; fits single-author use (no concurrent editing, a PRD
  non-goal).
- **Alternatives considered**: Per-Step CRUD + reorder endpoint (4+ endpoints
  and ordering edge cases for no extra user value this slice).

## R6 — Testing approach (constitution: tests map to requirement ids)

- **Decision**: xUnit integration tests driving the real HTTP API via
  WebApplicationFactory with a per-test SQLite database; each test's name (or
  trait) carries the FR id(s) it verifies. Frontend stays thin; behavior is
  asserted at the API seam.
- **Rationale**: Every FR is observable at the API boundary, so one test seam
  covers the requirement ids without UI-test overhead the appetite can't pay.
- **Alternatives considered**: Unit tests on domain classes only (don't prove
  the EARS scenarios end-to-end); browser E2E (Playwright — heavier than the
  appetite allows; revisit if a later slice earns it).
