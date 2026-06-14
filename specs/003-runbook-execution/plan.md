# Implementation Plan: Runbook Execution

**Branch**: `003-runbook-execution` | **Date**: 2026-06-13 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/003-runbook-execution/spec.md`; approved PRD [docs/initiatives/03-runbook-execution/prd.md](../../docs/initiatives/03-runbook-execution/prd.md)

## Summary

Add the execution slice on top of the shipped authoring backend: a responder
starts an **Execution** against an external **Incident** and a chosen Runbook,
which **pins** that Runbook's current published Runbook Version; the responder
records each Step outcome as an append-only **Step Record**; closes the Execution
manually; and obtains a **Computed Review** derived on read from the Step Records
against the pinned Version. The pin holds for the life of the run (ADR-0004); the
Incident is a foreign reference, not an owned entity (ADR-0005); the review is
derived, not a second store, so the no-dual-writes rule holds trivially (R1).
This is the first schema change since slice 01, so EF Core migrations replace
`EnsureCreated` (R2 / C-002). Invariants live inside the `Execution` aggregate
(ADR-0003 / C-004).

## Technical Context

**Language/Version**: C# / .NET 10 — unchanged (ADR-0002)

**Primary Dependencies**: ASP.NET Core minimal APIs, EF Core (SQLite); React/Vite
SPA frontend — unchanged. **New**: EF Core migrations tooling (`Microsoft.EntityFrameworkCore.Design`), replacing `EnsureCreated`.

**Storage**: SQLite, single store. New tables `Executions`, `StepRecords`;
introduced via migrations (R2). No second store / projection (R1).

**Testing**: xUnit + `WebApplicationFactory` integration tests, as in slice 01.
The 18 authoring tests must remain green and unmodified (regression guard).

**Target Platform**: local developer machine (cross-platform .NET + browser) —
unchanged.

**Project Type**: web application (existing `backend/` + `frontend/`).

**Performance/Scale**: single-user, local, one incident at a time; a run holds a
handful of Steps and records. No performance targets beyond slice-01 norms.

**Constraints**: appetite **a couple of evenings** — cut scope before extending
(see Scope note). Frontend stays on hash routing, no router package (C-003 / R7).

## Constitution Check

*GATE: must pass before Phase 0. Re-checked after Phase 1 design — still passing.*

| Gate | Status | Note |
|------|--------|------|
| Approved PRD as input | PASS | PRD accepted 2026-06-13; clarified + lint-clean (two review passes) |
| Binding glossary respected | PASS | Execution, Version Pin, Step Record, Computed Review, Incident ratified 2026-06-13; used consistently in spec/plan/contracts |
| Complexity earned, never anticipated | PASS | No Incident table (ADR-0005), no persisted review projection (R1), no re-pin, no fourth "not reached" outcome (R5) |
| Acceptance criteria use EARS | PASS | Spec user stories use EARS; FRs testable |
| No dual-writes | PASS | Single store; Computed Review derived on read (R1) — nothing to propagate, no outbox needed |
| Tests map to requirement ids | PLAN | `/speckit.tasks` will map integration tests to FR ids (FR-001…FR-017) |
| Conflict register checked | PASS | Touches C-001, C-002, C-003, C-004 — see below |
| Contested decisions promoted to ADRs | PASS | ADR-0004 (H2) and ADR-0005 (H5) drafted **Proposed**; H4 resolved in research (no contested decision — no dual-write); C-002 decided by the existing register entry |
| Tasks blocked while ADRs Proposed | **BLOCKED** | ADR-0004 and ADR-0005 are Proposed. Per the constitution, implementation is blocked until each is **Accepted** by a separate human commit on main. `/speckit.tasks` may generate the list; `/speckit.implement` must wait. |

### Conflict register check (docs/architecture.md)

| ID | Touched? | How |
|----|----------|-----|
| C-001 | Yes (read-only) | The pin reads a Runbook Version by its ADR-0001 identity; the uniqueness rule is unchanged. |
| C-002 | **Yes** | First schema change since `EnsureCreated`. Adopt EF Core migrations (R2): an initial migration reproduces the existing 001/002 schema, then a migration adds the execution tables. `Program.cs` switches to `Database.Migrate()`. |
| C-003 | Yes | Execution responder views are added to the existing SPA and stay within hash routing — no router package (R7). |
| C-004 | Yes | Execution invariants (open/closed gate, append-only records, set-once pin) live inside the `Execution` aggregate (ADR-0003). |

### Bounded-context reconciliation

`docs/architecture.md`'s context map states the execution slice "will introduce a
second bounded context." Per ADR-0005 and the slice-01 workshop boundary note,
authoring and execution remain **one** bounded context (*Runbook Execution*); the
only boundary is the **external** incident-management tool, which the platform
merely references. The architecture.md line is corrected at the **post-implement
amendment** (constitution's release rhythm), not now.

## Decisions referenced (ADRs)

- **ADR-0004** — The Version Pin holds for the life of an Execution; responder
  sees only the pinned Version, silently (Proposed). `docs/adr/0004-version-pin-holds-mid-incident-publish.md`
- **ADR-0005** — Incident is a foreign reference (id + optional title), not an
  owned entity (Proposed). `docs/adr/0005-incident-as-foreign-reference.md`
- **ADR-0003** — Rich domain model; the `Execution` aggregate follows it (Accepted).
- **ADR-0001** — Runbook Version identity; the pin reads it (Accepted).
- **ADR-0002** — Stack unchanged (Accepted).

## Project Structure

### Documentation (this feature)

```text
specs/003-runbook-execution/
├── plan.md            # this file
├── research.md        # Phase 0 — R1..R7
├── data-model.md      # Phase 1 — Execution, StepRecord, Computed Review
├── contracts/
│   └── http-api.md    # Phase 1 — execution endpoints
├── quickstart.md      # Phase 1 — end-to-end validation walk
├── checklists/
│   └── requirements.md
└── tasks.md           # Phase 2 — /speckit.tasks (not created here)
```

### Source code (repository root)

```text
backend/src/RunbookPlatform.Api/
├── Domain/
│   ├── Execution.cs          # NEW — aggregate root: Start / RecordStep / Close
│   ├── StepRecord.cs         # NEW — append-only; built only by Execution.RecordStep
│   ├── StepOutcome.cs        # NEW — enum Done/Skipped/Failed
│   └── (Runbook.cs, RunbookVersion.cs, … unchanged)
├── Data/
│   ├── AppDbContext.cs       # add Executions, StepRecords; unique index on IncidentId
│   └── Migrations/           # NEW — initial (existing schema) + execution tables (C-002)
├── Endpoints/
│   └── ExecutionEndpoints.cs # NEW — start/resume, run view, record, close, review
└── Program.cs                # EnsureCreated → Database.Migrate()

frontend/                     # execution run view + review view, hash-routed (C-003)

backend/tests/RunbookPlatform.Api.Tests/
└── (new execution tests; 18 authoring tests stay green & unmodified)
```

**Structure Decision**: Extend the existing web-app structure; no new projects.
The `Execution` aggregate and its endpoints mirror the slice-01/02 shape (thin
endpoints over rich aggregates).

## Scope note (appetite: a couple of evenings)

Priority order if the work overruns — cut from the bottom, do not extend:

1. **P1** — Start/resume an Execution, pin the current Version, record Step
   outcomes (backend + minimal run UI). The capture foundation.
2. **P2** — Manual close + Computed Review (backend + review UI). The payoff.
3. **P3** — Pin-integrity across a mid-incident publish — largely *free* from the
   pin design (the Execution stores its pinned Version id); mostly a test.

EF Core migration adoption (C-002) is unavoidable groundwork for P1 and is not
optional scope.

## Complexity Tracking

> No constitution violations — table intentionally empty.

## Human gate before implementation

ADR-0004 and ADR-0005 are **Proposed**. They must each be **Accepted** by a
separate human-authored status commit on `main` before `/speckit.implement` runs.
`/speckit.tasks` may proceed to generate the task list in the meantime.
