# Implementation Plan: Branching (Decision Steps)

**Branch**: `005-branching` | **Date**: 2026-07-01 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/005-branching/spec.md`; approved PRD [docs/initiatives/05-branching/prd.md](../../docs/initiatives/05-branching/prd.md)

## Summary

Add the **minimal Decision-Flow model** to the one existing bounded context. An author
can type a **Step** as **Decision** and give it named **Options**, each routing to a
**Target Step** *by position* (ADR-0008). The Options and their routing are set through
aggregate behavior and **frozen** into the Runbook Version at publish exactly as Step
content is (C-004; ADR-0001/0003 immutability). At **publish**, a validation gate
rejects a broken flow — every Option targets an existing later position (forward-only, so
no loops), and each Decision Step has ≥2 Options (spec FR-017). During an **Execution**,
reaching a Decision Step presents its Options; choosing one **appends a Step Record**
capturing the chosen Option (ADR-0009 — reuse, no new write path) and the run continues
from that Option's Target Step. The **Taken Path** and the **Computed Review** are
computed on read: walk positions, follow each resolved Decision's chosen Option, mark
Steps on branches not taken as **NotReached** (reusing the existing state; H3, no new
state). `StepType` grows by one closed member, **Decision** (ADR-0010, extending
ADR-0007 / amending C-006). One additive migration (C-002). Runbooks with no Decision
Steps behave exactly as before; prior slices' tests stay green and unmodified.

## Technical Context

**Language/Version**: C# / .NET 10; TypeScript / React 19 — unchanged (ADR-0002).

**Primary Dependencies**: ASP.NET Core minimal APIs, EF Core (SQLite); React/Vite SPA.
**No new dependency** — routing UI stays within hash routing, no graph/flow-builder
package (C-003; spec non-goals, H6).

**Storage**: SQLite, single store. One additive migration (C-002): two child tables for
Options — `StepOptions` (draft, on `Steps`) and `RunbookVersionStepOptions` (frozen, on
`RunbookVersionSteps`) — plus a nullable chosen-Option column on `StepRecords`
(ADR-0009). `StepType` text column accepts the new `Decision` value with no schema
change (ADR-0010). No table dropped; existing rows backfill valid.

**Testing**: xUnit + `WebApplicationFactory` integration tests. Slice-01/03/04 behavior
tests stay green and unmodified — this slice only *adds* (routing, one new type value,
one nullable record column). Frontend: vitest for any routing-UI logic; existing
markdown.test.ts unaffected (C-005 untouched).

**Target Platform**: local developer machine (cross-platform .NET + browser).

**Project Type**: web application (existing `backend/` + `frontend/`).

**Performance/Scale**: single-user, local; a Runbook holds a handful of Steps and a
Decision a handful of Options. Taken Path / Computed Review are read-time walks over a
small step list — negligible. No new performance targets.

**Constraints**: appetite **minimal Decision-Flow model** — cut scope, don't extend
(spec/PRD). Cut order if it overruns: authoring routing UI polish → run-time branch
presentation → review-over-taken-path is the floor. Forward-only routing only; no loops,
gates, parallel paths, condition logic, or visual builder (spec non-goals).

## Constitution Check

*GATE: must pass before Phase 0. Re-checked after Phase 1 design — still passing.*

| Gate | Status | Note |
|------|--------|------|
| Approved PRD as input | PASS | PRD approved 2026-07-01 (human instruction); spec + clarify complete |
| Binding glossary respected | PASS | Decision/Option/Target Step/Taken Path + Computed Review redefinition ratified 2026-06-28 (v1.1.0); used consistently across spec/plan/ADRs |
| Complexity earned, never anticipated | PASS | Position routing reuses existing position identity (ADR-0008); resolution reuses Step Record (ADR-0009); enum grows by one closed member (ADR-0010); no stable-id concept, no second write path, no new event, no graph dep |
| Acceptance criteria use EARS | PASS | Spec user stories use EARS; FR-001…FR-020 testable |
| No dual-writes | PASS | Single store; Decision resolution is one appended Step Record (ADR-0009); Taken Path/Review derived on read, never persisted |
| Tests map to requirement ids | PLAN | `/speckit.tasks` maps integration tests to FR ids |
| Conflict register checked | PASS | Touches C-001, C-002, C-003, C-004, C-006, C-007, C-009; C-005/C-008 unaffected — see below |
| Contested decisions promoted to ADRs | PASS | ADR-0008 (H1 routing ref) and ADR-0009 (H2 resolution record) drafted **Proposed** with the human's chosen options; ADR-0010 (Step Type extension) **Proposed** per C-006/ADR-0007 flip condition; H3/H4/H5/H6 settled in research/clarify, no further ADR |
| Tasks blocked while ADRs Proposed | BLOCKED | ADR-0008/0009/0010 are **Proposed**; `/speckit.tasks` is blocked until each is Accepted by a human-authored commit |

### Conflict register check (docs/architecture.md)

| ID | Touched? | How |
|----|----------|-----|
| C-001 | No change | Version identity `(RunbookId, Number)` unchanged; routing is *within* a Version, by Step position, not Version numbering. |
| C-002 | **Yes** | One additive migration: `StepOptions`, `RunbookVersionStepOptions`, and a nullable chosen-Option column on `StepRecords`. No table dropped; migration reproduces existing schema first, then adds (C-002 procedure). |
| C-003 | **Yes** | Authoring routing inputs and the run-time Option presentation are added to the existing SPA within hash routing — **no router and no graph/flow-builder package** (H6). |
| C-004 | **Yes** | Options, routing, freeze, publish validation, and Decision resolution live in aggregate behavior (`Runbook.ReplaceSteps` / `Publish` / `RunbookVersion.Freeze`; `Execution.RecordStep`); endpoints stay thin, mapping `DomainException` → 400 and conflict → 409. |
| C-006 | **Yes** | `StepType` closed set grows to {Action, Check, Decision} via ADR-0010 (extends ADR-0007); enum↔string mapping + aggregate validation stay in sync. |
| C-007 | **Yes** | Run-time routing resolves Options/targets through the **pinned** Version, never "current" (spec FR-010). |
| C-008 | No change | Incident stays a foreign reference; branching adds nothing here. |
| C-009 | **Yes (reuse)** | Taken Path / Computed Review order records in memory by `RecordedAt` with the per-Execution `Sequence` tiebreaker — the existing rule, not a new sort path. |

### Bounded-context note

No new bounded context — branching adds routing *within* a Runbook Version in the one
**Runbook Authoring & Execution** context. `docs/architecture.md` is amended at the
post-implement release step per the constitution, not here.

## Decisions referenced (ADRs)

- **ADR-0008** — An Option references its Target Step **by position**; forward-only
  routing makes loops structurally impossible; frozen at publish (**Proposed**). `docs/adr/0008-option-routing-reference.md`
- **ADR-0009** — A Decision resolution **reuses the append-only Step Record** (chosen
  Option on the record); "Decision resolved" stays derived (**Proposed**). `docs/adr/0009-decision-resolution-record.md`
- **ADR-0010** — Extend `StepType` to the closed set {Action, Check, Decision}, extending
  ADR-0007 / amending C-006 (**Proposed**). `docs/adr/0010-step-type-decision-extension.md`
- **ADR-0007** — Step Type taxonomy (Action/Check); extended by ADR-0010 (Accepted).
- **ADR-0004** — Version Pin; run-time routing resolves through the pin (Accepted).
- **ADR-0003** — Rich domain model; all new state via aggregate behavior (Accepted).
- **ADR-0001** — Version identity; freeze copies routing into the Version unchanged (Accepted).

## Project Structure

### Documentation (this feature)

```text
specs/005-branching/
├── plan.md            # this file
├── research.md        # Phase 0 — R1..Rn (H1..H6 resolutions, Taken Path)
├── data-model.md      # Phase 1 — Option, frozen Option, StepType+Decision, StepRecord+chosenOption
├── contracts/
│   └── http-api.md    # Phase 1 — authoring, run, review contract deltas
├── quickstart.md      # Phase 1 — end-to-end branched-run validation walk
├── checklists/
│   └── requirements.md
└── tasks.md           # Phase 2 — /speckit.tasks (not created here)
```

### Source code (repository root)

```text
backend/src/RunbookPlatform.Api/
├── Domain/
│   ├── StepType.cs             # + Decision (closed enum grows by one; ADR-0010)
│   ├── StepOption.cs           # NEW — draft Option (label, targetPosition, ordinal)
│   ├── RunbookVersionStepOption.cs # NEW — frozen Option
│   ├── Step.cs                 # holds its Options (draft)
│   ├── RunbookVersionStep.cs   # holds its frozen Options
│   ├── Runbook.cs              # ReplaceSteps takes Options; Publish validation gate (FR-017); Freeze copies Options
│   ├── StepRecord.cs           # + nullable chosen-Option (ADR-0009)
│   └── Execution.cs            # RecordStep accepts a Decision choice; Taken Path + Computed Review over taken path
├── Data/
│   ├── AppDbContext.cs         # map new tables + chosen-Option column + StepType value
│   └── Migrations/             # NEW — one additive migration (C-002)
├── Endpoints/
│   ├── RunbookEndpoints.cs     # save Steps with Options; validation errors → 400
│   ├── PublishEndpoints.cs     # publish gate surfaces validation (FR-017)
│   └── ExecutionEndpoints.cs   # present Options at a Decision; record a choice; review over Taken Path
frontend/src/
├── pages/RunbookDetail.tsx     # author a Decision Step + its Options/targets (hash routing, no builder)
├── pages/ExecutionRun.tsx      # present Options at a Decision; continue from chosen target
├── pages/ComputedReview.tsx    # show chosen Option per Decision; NotReached vs Skipped over Taken Path
└── api/client.ts               # types + payloads gain Options + chosen-Option
backend/tests/RunbookPlatform.Api.Tests/
└── (new branching tests; existing authoring/execution/rich-step tests stay green)
```

**Structure Decision**: Extend the existing web-app structure; no new projects, no new
runtime dependency. New domain types are `StepOption` / `RunbookVersionStepOption`; the
only other new files are the migration. Everything else rides existing entities and
endpoints.

## Scope note (appetite: minimal Decision-Flow model)

Priority order if the work overruns — cut from the bottom, do not extend:

1. **P1** — Author a Decision Step with Options routing to Target Steps, validated and
   **frozen** at publish (domain + migration + publish gate + authoring UI). The core.
2. **P2** — Follow one path at run time: present Options, record the choice, continue
   from the target (execution). The branching payoff.
3. **P3** — Computed Review over the Taken Path: chosen Option shown, NotReached vs
   Skipped (review). The review payoff; reuses existing coverage machinery.

The additive migration (C-002) and the publish validation gate (FR-017) are groundwork
for P1 and not optional scope.

## Complexity Tracking

> No constitution violations — table intentionally empty.

## Human gate before implementation

**BLOCKED.** ADR-0008, ADR-0009, and ADR-0010 are **Proposed**. Per the constitution,
`/speckit.tasks` is blocked until each is **Accepted** by its own human-authored commit
on the branch (and ADR-0007 gets its "Extended-by 0010" note in that same acceptance
commit). After acceptance, `/speckit.tasks` then `/speckit.implement` may proceed.
