# Pointer-Spec: Rich Domain Model (structural)

**Created**: 2026-06-12

**Status**: Pointer-spec — this initiative enters through the ADR door, not the PRD door.

**Canonical record**: [ADR-0003 — Rich domain model](../../docs/adr/0003-rich-domain-model.md) (Accepted 2026-06-12)

**Discovery**: [docs/initiatives/02-rich-domain-model/discovery.md](../../docs/initiatives/02-rich-domain-model/discovery.md) (inherited form; Go 2026-06-12)

**Workshop**: skipped with receipt — no new language, no new events, single context.

## What this is

A structural refactor of the shipped 001 slice (`specs/001-runbook-authoring`).
No behavior changes, no new features, no schema changes.
The decision, options, and trade-offs live in ADR-0003; this file only fixes scope and the acceptance contract.

## Scope

Move every domain invariant from the HTTP endpoints into the aggregates, per ADR-0003's Decision:

- `Runbook` becomes the aggregate root; state changes go through behavior methods (`Create`, `ReplaceSteps`, `Publish`) that enforce their own invariants.
- `Runbook.Publish()` owns the publish gate (FR-003), the freeze (FR-004/FR-006), and sequential numbering (FR-005, ADR-0001).
- `RunbookVersion` and its frozen Steps are constructed only by `Runbook.Publish()` and expose no mutation after construction (FR-006 at the type level).
- One `DomainException` mapped once at the endpoint seam to the existing `{ "error": "…" }` 400 shape.

## Acceptance contract

- The 18 integration tests in `backend/tests/RunbookPlatform.Api.Tests/` pass **unmodified**.
  Any test edit means behavior changed and the refactor has gone wrong (ADR-0003 Consequences).
- Functional requirement coverage is unchanged: FR-001 through FR-011 from `specs/001-runbook-authoring/spec.md` remain covered by the same tests; this initiative introduces no new FRs.
- The database schema is unchanged (conflict register C-002: first schema change requires EF Core migrations first).
- The HTTP contract (`specs/001-runbook-authoring/contracts/http-api.md`) is unchanged.

## Non-goals

- No new endpoints, fields, or behavior.
- No `Rename` method: no rename code path exists in the shipped slice, so none is added (complexity is earned).
- No full encapsulation (factories, backing-field metadata gymnastics) — ADR-0003 chose the pragmatic variant.
- No frontend changes.

## Appetite

Two evenings (discovery) — cut scope before extending.
