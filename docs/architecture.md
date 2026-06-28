# Architecture

Cross-feature truth only; amended at release time per the constitution.

## Context map

**System boundary**: local developer machine (cross-platform .NET + browser).
No cloud hosting, no external integrations, no auth provider in scope.

### Components

| Component | Path | Role |
|-----------|------|------|
| Runbook Authoring API | `backend/` | ASP.NET Core minimal API; owns all domain logic and persistence; exposes HTTP endpoints at `/api/*` |
| Web UI | `frontend/` | React SPA (Vite / TypeScript); consumes the API via `/api` proxy; hash-routed; renders all three authoring user stories |
| SQLite database | `runbook-platform.db` (runtime, gitignored) | Single persistent store; owned by the API process; one file, one schema |

### Bounded context

One bounded context: **Runbook Authoring & Execution**.
The Runbook, Runbook Version, and Step entities (ratified glossary, constitution) live here, and
the execution slice (003) added Execution and Step Record alongside them. It stayed **one** context:
Incident is a foreign reference, not a local entity (ADR-0005), so no second bounded context was
introduced — correcting the slice-001 expectation that the execution slice would create one.

**Runbook is the aggregate root** (slice 002, ADR-0003). Every domain invariant — the publish
gate, the version freeze, and sequential numbering — is enforced inside aggregate behavior
(`Runbook.Create` / `ReplaceSteps` / `Publish`), not in HTTP handlers. Runbook Version and its
frozen Steps are constructed only by `Runbook.Publish()` and expose no mutation afterward.
Endpoints are load → call method → save → map; a single `DomainException` is mapped once at the
endpoint seam to the `{ "error": "…" }` 400 shape. Behavior, HTTP contract, and schema are
identical to slice 001 — this was a structural refactor, no functional change.

**Execution is a second aggregate root** (slice 003, ADR-0004/0005). `Execution.Start(runbook,
incidentId, incidentTitle?)` requires a published Version and **pins** it once
(`PinnedRunbookVersionId`, ADR-0004) — the pin holds even if a newer Version is published mid-incident.
`RecordStep` appends an immutable **Step Record** (Done/Skipped/Failed + optional note); records are
append-only with no actor captured, and the latest record for a position is its end state. `Close()`
moves Open → Closed (manual close only; no incident-driven path). The **Computed Review** is derived
on read — chronological timeline plus per-Step coverage, with `NotReached` distinguished from
`Skipped` — never a persisted projection. Incident is a foreign reference: `Execution.IncidentId` is a
string with a unique index, not a local entity (ADR-0005). All invariants live in the aggregate
(C-004); endpoints stay thin and add a `409` mapping for conflict cases (e.g. recording after close).

**Step carries structured detail** (slice 004, ADR-0006/0007). A Step is no longer a bare title:
it adds optional `Instructions`, `Command`, and `ExpectedResult` (markdown text) plus a required
`StepType` (`Action` | `Check`, default `Action`). All four fields are validated and frozen inside
the aggregate (`Runbook.ReplaceSteps` → `RunbookVersion.Freeze`) exactly as the title is, so C-004
holds; publish copies all four into each frozen `RunbookVersionStep`, leaving earlier Versions'
detail unchanged. Detail is rendered in the frontend through a hand-rolled escape-first markdown
renderer (`frontend/src/lib/markdown.ts`, ADR-0006) — HTML is escaped before a fixed whitelist is
applied — so no markdown or sanitizer package is added. Branching between steps is explicitly out
of scope.

## NFRs

Established by slice 001 (`specs/001-runbook-authoring`); binding on future slices unless explicitly superseded by an ADR.

| NFR | Statement | Source |
|-----|-----------|--------|
| No authentication | No sign-in, accounts, or permissions are required for any authoring action | FR-010 |
| Immutability | Published Runbook Versions cannot be modified or deleted; no code path exists to do so | FR-006, FR-011 |
| No deletion | Runbooks and Runbook Versions are never deleted; no DELETE handlers exist in the API | FR-011 |
| Single-user interactive | No concurrent-write guarantees beyond SQLite serialized writes; no multi-user session support | Slice 001 scope |
| Local only | No hosting, deployment, or cloud targets in this slice | Appetite |
| Append-only records | Step Records are never updated or deleted; re-marking a Step appends a new record and the latest wins | Slice 003 (FR-005/006) |
| Manual close only | An Execution is closed only by an explicit action; there is no incident-driven or automatic close path | Slice 003 (FR-009) |
| Review computed on read | The Computed Review is derived from records at read time, never persisted as a projection | Slice 003 (ADR-0005, R1) |

## Conflict register

Entries here record constraints that future initiatives must not violate without an ADR.

| ID | Constraint | Established by | Impact on future slices |
|----|------------|----------------|------------------------|
| C-001 | `(RunbookId, Number)` unique index on `RunbookVersion` — version numbers are immutable sequential integers per Runbook | ADR-0001 | Execution slice: version-pinning reads this index; must not alter the uniqueness rule |
| C-002 | Schema managed by EF Core migrations in `backend/src/RunbookPlatform.Api/Data/Migrations/` (the `EnsureCreated` mechanism of slice 001 has been retired) | adopted specs/003-runbook-execution; specs/004-rich-steps adds the additive `AddStepDetail` migration | Every schema change ships as an additive EF Core migration; no table is dropped, and migrations must apply cleanly over the existing store |
| C-003 | Frontend uses hash routing — no router package | specs/001-runbook-authoring (appetite) | Execution slice frontend must stay within hash routing or earn a router via ADR |
| C-004 | Domain invariants live inside aggregates, enforced through behavior methods; HTTP handlers stay thin (load → method → save → map) | ADR-0003 (specs/002-rich-domain-model) | New invariants in any slice belong in aggregate behavior, not in endpoints; bypassing the aggregate to mutate state requires an ADR |
| C-005 | Step detail is rendered through the escape-first whitelist renderer (`frontend/src/lib/markdown.ts`): raw HTML, images, tables, and non-`http(s)`/`mailto` links are neutralised | ADR-0006 (specs/004-rich-steps) | Any change to how step detail is displayed must preserve the escape-first sanitization invariants (covered by `markdown.test.ts`); adding a markdown or sanitizer dependency requires an ADR |
| C-006 | `StepType` is a closed enum `{ Action, Check }` (default `Action`), validated in the aggregate and persisted as a string | ADR-0007 (specs/004-rich-steps) | Adding a step type requires an ADR amending the taxonomy; the enum↔string mapping and aggregate validation must stay in sync |
| C-007 | An Execution pins its Runbook Version once at start; the pin is immutable for the run's lifetime and holds even if a newer Version is published mid-incident | ADR-0004 (specs/003-runbook-execution) | Any execution-time read of step content must resolve through the pinned Version, never "current"; changing the pin requires an ADR |
| C-008 | Incident is a foreign reference — `Execution.IncidentId` is a string with a unique index, not a local entity | ADR-0005 (specs/003-runbook-execution) | Incident data must not be modeled as a local aggregate; the `IncidentId` uniqueness rule (one open Execution per incident) must not be weakened without an ADR |
