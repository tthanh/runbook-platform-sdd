# Implementation Plan: Rich Steps

**Branch**: `004-rich-steps` | **Date**: 2026-06-15 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/004-rich-steps/spec.md`; approved PRD [docs/initiatives/04-rich-steps/prd.md](../../docs/initiatives/04-rich-steps/prd.md)

## Summary

Enrich a **Step** so it carries, beyond its required title, optional **instructions**
(lightweight markdown), a **command**, an **expected result**, and a **Step Type**
(Action or Check). The detail is authored on the working Runbook, **frozen** into
the Runbook Version at publish exactly as the title is (ADR-0001/0003 immutability,
C-004), and **read** in three places: the authoring/version view, the Execution run
view, and the Computed Review. Markdown is rendered by a **hand-rolled, escape-first
minimal renderer** on the frontend — no new runtime dependency (ADR-0006, C-003).
Step Type is a **closed enum** {Action, Check}, descriptive only (ADR-0007). The
schema change is **one additive migration** (four columns on `Steps` and on
`RunbookVersionSteps`) on the EF Core migration procedure slice 03 already adopted
(C-002). No new domain events, no lifecycle change; existing authoring and execution
tests stay green, with new fields and surfaces added on top.

## Technical Context

**Language/Version**: C# / .NET 10; TypeScript / React 19 — unchanged (ADR-0002).

**Primary Dependencies**: ASP.NET Core minimal APIs, EF Core (SQLite); React/Vite
SPA. **No new dependency** — markdown is rendered by a small in-repo module
(ADR-0006); the frontend keeps its react/react-dom-only runtime footprint (C-003).

**Storage**: SQLite, single store. One additive migration adds `Instructions`,
`Command`, `ExpectedResult` (nullable TEXT) and `StepType` (TEXT, not null,
default `Action`) to both `Steps` and `RunbookVersionSteps`. No table added (R7).

**Testing**: xUnit + `WebApplicationFactory` integration tests. The slice-01/03
behavior tests stay green and unmodified — this slice only *adds* fields and tests;
it renames nothing (R1).

**Target Platform**: local developer machine (cross-platform .NET + browser).

**Project Type**: web application (existing `backend/` + `frontend/`).

**Performance/Scale**: single-user, local; a Runbook holds a handful of Steps. No
new performance targets. Markdown render is per-step on view — negligible.

**Constraints**: appetite **a couple of evenings** — cut fields/surfaces before
extending (see Scope note). Frontend stays on hash routing, no router, no markdown
package (C-003 / ADR-0006).

## Constitution Check

*GATE: must pass before Phase 0. Re-checked after Phase 1 design — still passing.*

| Gate | Status | Note |
|------|--------|------|
| Approved PRD as input | PASS | PRD finalized 2026-06-15; six-pass review clean |
| Binding glossary respected | PASS | Step redefined + Step Type added, ratified 2026-06-15 (9 terms); used consistently in spec/plan/contracts |
| Complexity earned, never anticipated | PASS | No rename (R1), no value-object indirection (R2), no markdown dependency (ADR-0006), closed two-value enum only (ADR-0007); branching/automation fenced as non-goals |
| Acceptance criteria use EARS | PASS | Spec user stories use EARS; FR-001…FR-015 testable |
| No dual-writes | PASS | Single store; detail is plain frozen columns, no projection, nothing to propagate |
| Tests map to requirement ids | PLAN | `/speckit.tasks` maps integration tests to FR ids |
| Conflict register checked | PASS | Touches C-002, C-003, C-004; C-001 unchanged — see below |
| Contested decisions promoted to ADRs | PASS | ADR-0006 (H2 markdown rendering/sanitization) and ADR-0007 (H3 Step Type taxonomy) drafted **Proposed**; H1 freeze, H4 migration, H5 branching-deferred resolved in research (settled by existing decisions, no new ADR) |
| Tasks blocked while ADRs Proposed | BLOCKED | ADR-0006 and ADR-0007 are **Proposed**; `/speckit.tasks` is blocked until each is Accepted by a human-authored commit |

### Conflict register check (docs/architecture.md)

| ID | Touched? | How |
|----|----------|-----|
| C-001 | No change | Runbook Version identity (ADR-0001) is unchanged; freezing more columns per Step does not alter the `(RunbookId, Number)` uniqueness rule. |
| C-002 | **Yes** | First schema change since slice 03's migrations. Add **one** additive migration: four columns on `Steps` and on `RunbookVersionSteps`. Mixing `EnsureCreated` is forbidden; the migration procedure is already in place. |
| C-003 | **Yes** | New authoring inputs and three read surfaces are added to the existing SPA, staying within hash routing; markdown is rendered by an in-repo module — **no router and no markdown package** added. |
| C-004 | **Yes** | The new Step fields are set only through aggregate behavior (`Runbook.ReplaceSteps`, `RunbookVersion.Freeze`); validation (title required, Step Type ∈ {Action, Check}) lives in the aggregate, not endpoints. |

### Bounded-context note

This slice adds no bounded context — authoring and execution remain the one
**Runbook Execution** context. `docs/architecture.md` still carries the stale line
that "the execution slice will introduce a second bounded context" (slice 03 did
not, and neither does this); it is corrected at the **post-implement amendment**
per the constitution's release rhythm, not here.

## Decisions referenced (ADRs)

- **ADR-0006** — Markdown for the instructions body is rendered by a hand-rolled,
  escape-first minimal renderer with a fixed whitelist and safe link schemes; no
  markdown/sanitizer dependency (**Proposed**). `docs/adr/0006-markdown-rendering-and-sanitization.md`
- **ADR-0007** — Step Type is a closed enum {Action, Check}, descriptive only;
  Decision and Gate deferred with branching (**Proposed**). `docs/adr/0007-step-type-taxonomy.md`
- **ADR-0001** — Runbook Version identity; the freeze copies into a version unchanged (Accepted).
- **ADR-0003** — Rich domain model; new fields are constructed only via aggregate behavior (Accepted).
- **ADR-0002** — Stack unchanged (Accepted).

## Project Structure

### Documentation (this feature)

```text
specs/004-rich-steps/
├── plan.md            # this file
├── research.md        # Phase 0 — R1..R10
├── data-model.md      # Phase 1 — enriched Step / RunbookVersionStep, StepType
├── contracts/
│   └── http-api.md    # Phase 1 — authoring + read contract deltas
├── quickstart.md      # Phase 1 — end-to-end validation walk
├── checklists/
│   └── requirements.md
└── tasks.md           # Phase 2 — /speckit.tasks (not created here)
```

### Source code (repository root)

```text
backend/src/RunbookPlatform.Api/
├── Domain/
│   ├── Step.cs               # + Instructions?, Command?, ExpectedResult?, StepType
│   ├── RunbookVersionStep.cs # + same frozen columns
│   ├── StepType.cs           # NEW — enum Action/Check
│   └── Runbook.cs            # ReplaceSteps takes structured step inputs; Freeze copies detail
├── Data/
│   ├── AppDbContext.cs       # map new columns + StepType conversion
│   └── Migrations/           # NEW — one additive migration (C-002)
├── Endpoints/
│   ├── RunbookEndpoints.cs   # SaveStepItem gains optional detail + type; detail in responses
│   └── VersionEndpoints.cs   # version view returns detail + type
└── (ExecutionEndpoints.cs)   # run view + Computed Review return Step detail

frontend/src/
├── lib/markdown.ts           # NEW — escape-first minimal markdown → safe HTML (ADR-0006)
├── pages/RunbookDetail.tsx   # authoring inputs for detail + type
├── pages/VersionView.tsx     # show frozen detail + type (markdown rendered)
├── pages/ExecutionRun.tsx    # show detail while running, before recording
└── api/client.ts             # types + payloads gain detail + type

backend/tests/RunbookPlatform.Api.Tests/
└── (new rich-step tests; existing authoring/execution tests stay green)
```

**Structure Decision**: Extend the existing web-app structure; no new projects,
no new runtime dependency. New fields ride the existing Step/RunbookVersionStep
entities and the existing endpoints; the only new files are `StepType.cs`, the
migration, and `frontend/src/lib/markdown.ts`.

## Scope note (appetite: a couple of evenings)

Priority order if the work overruns — cut from the bottom, do not extend:

1. **P1** — Author the detail + Step Type, persist on the working Runbook, and
   **freeze** it into the Runbook Version at publish (domain + migration + authoring
   UI + version view). The core: a published procedure that says how.
2. **P2** — Show the detail **while running an Execution**, before the responder
   records an outcome (run view + the markdown renderer). The execution payoff.
3. **P3** — Show the detail in the **Computed Review** alongside each outcome. The
   review payoff; smallest increment, reuses the renderer.

The additive migration (C-002) and the markdown renderer (ADR-0006) are groundwork
for P1/P2 respectively and are not optional scope.

## Complexity Tracking

> No constitution violations — table intentionally empty.

## Human gate before implementation

**BLOCKED.** ADR-0006 and ADR-0007 are **Proposed**. Per the constitution,
`/speckit.tasks` is blocked until each is **Accepted** by its own human-authored
commit on the branch. After acceptance, `/speckit.tasks` then `/speckit.implement`
may proceed.
