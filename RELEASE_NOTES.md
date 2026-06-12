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
