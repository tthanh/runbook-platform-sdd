# Implementation Plan: Rich Domain Model (structural)

**Branch**: `claude/distracted-brahmagupta-013312` | **Date**: 2026-06-12 | **Spec**: [spec.md](spec.md)

**Input**: Pointer-spec from `/specs/002-rich-domain-model/spec.md`; canonical decision in ADR-0003 (Accepted)

## Summary

Refactor the 001 backend so every domain invariant lives inside the aggregates instead of the HTTP handlers, per ADR-0003.
Behavior, HTTP contract, and database schema are unchanged; the 18 existing integration tests pass unmodified.
This plan references ADR-0003 and carries no decision rationale itself.

## Technical Context

**Language/Version**: C# / .NET 10 — unchanged (ADR-0002)

**Primary Dependencies**: ASP.NET Core minimal APIs, EF Core (SQLite) — unchanged; no new packages

**Storage**: SQLite, schema unchanged (conflict register C-002 untouched)

**Testing**: The existing 18 xUnit integration tests are the acceptance contract — they must pass with zero edits

**Constraints**: Appetite two evenings; pragmatic encapsulation only (ADR-0003 Decision); no frontend changes

## Constitution Check

| Gate | Status | Note |
|------|--------|------|
| Approved PRD as input | N/A (structural) | Enters through the ADR door: ADR-0003 Accepted by human commit; pointer-spec references it by path |
| Binding glossary respected | PASS | Runbook, Runbook Version, Step — names unchanged in code |
| Complexity earned, never anticipated | PASS | No factories, no backing-field metadata beyond what EF mapping requires, no Rename (no code path needs it) |
| Acceptance criteria use EARS | N/A | No new behavior; acceptance contract is the unmodified 001 test suite |
| No dual-writes | PASS (trivially) | Single store, unchanged |
| Tests map to requirement ids | PASS | Same tests, same FR ids (FR-001–FR-011); no new FRs |
| Conflict register checked | PASS | Touches C-001 (numbering rule moves into `Runbook.Publish()`, uniqueness index unchanged) and C-002 (schema must not change; if EF mapping forces a change, migrations come first). C-003 untouched (no frontend work) |
| Contested decisions promoted to ADRs | PASS | ADR-0003, Accepted 2026-06-12 before any code |
| Tasks blocked while ADRs Proposed | CLEAR | ADR-0003 is Accepted |

## Change map (no new structure)

```text
backend/src/RunbookPlatform.Api/
├── Domain/
│   ├── DomainException.cs       # NEW — invariant violations, mapped at the seam
│   ├── Runbook.cs               # aggregate root: Create / ReplaceSteps / Publish
│   ├── RunbookVersion.cs        # constructed only by Runbook.Publish(); read-only after
│   ├── Step.cs                  # internal construction, private setters
│   └── RunbookVersionStep.cs    # internal construction, private setters
├── Data/AppDbContext.cs         # backing-field mapping where needed; schema identical
└── Endpoints/
    ├── RunbookEndpoints.cs      # shrink to load → call method → save → map; DomainException filter on the group
    ├── PublishEndpoints.cs      # gate/number/freeze logic removed (moves to Runbook.Publish())
    └── VersionEndpoints.cs      # read-only; unchanged apart from incidental shape
```

## Decisions referenced (ADRs)

- **ADR-0003** — Rich domain model: invariants enforced inside the aggregates. `docs/adr/0003-rich-domain-model.md` (Accepted)
- **ADR-0001** — numbering rule unchanged, now enforced in one place (`Runbook.Publish()`)
- **ADR-0002** — stack unchanged

## Complexity Tracking

> No constitution violations — table intentionally empty.
