# Data Model — 003 Runbook Execution (Phase 1)

Builds on the slice-01 model (Runbook, RunbookVersion, RunbookVersionStep, Step),
which is consumed unchanged. Two new persisted entities (Execution, StepRecord)
and one derived read model (Computed Review). No Incident table (ADR-0005).

## Execution *(aggregate root)*

A single run of a Runbook against one Incident.

| Field | Type | Notes |
|-------|------|-------|
| `Id` | Guid | identity |
| `IncidentId` | string, required | foreign reference to the external incident (ADR-0005); **unique** — enforces one Execution per incident (FR-015) |
| `IncidentTitle` | string, optional | human-readable label for the review (ADR-0005) |
| `PinnedRunbookVersionId` | Guid, FK → RunbookVersion | the Version Pin; set once at start, never reassigned (FR-003, ADR-0004) |
| `Status` | enum `Open` / `Closed` | one-way Open → Closed (FR-009/FR-010) |
| `StartedAt` | DateTimeOffset | set at start |
| `ClosedAt` | DateTimeOffset? | set at close |
| `StepRecords` | `IReadOnlyList<StepRecord>` | append-only; backing list private |

**Behavior (invariants inside the aggregate — ADR-0003 / C-004):**

- `static Execution Start(Runbook runbook, string incidentId, string? incidentTitle)` — requires `runbook.CurrentVersionNumber is not null` (FR-002, else `DomainException`); pins the current published Version; `Status = Open`.
- `StepRecord RecordStep(int stepPosition, StepOutcome outcome, string? note)` — allowed only while `Open` (else `DomainException`, FR-010); `stepPosition` must exist in the pinned Version; appends one Step Record (FR-004); re-marking allowed (FR-006).
- `void Close()` — `Open → Closed`; idempotency/refusal on already-closed is a `DomainException`.

**Validation / rules:**
- `IncidentId` non-empty (trimmed).
- Pin is immutable after `Start` — no method reassigns `PinnedRunbookVersionId` (ADR-0004).
- Resume-vs-refuse on duplicate start (clarification 2026-06-13) is enforced at the application seam using the unique `IncidentId`: an open Execution for that id is returned; a closed one causes refusal. The DB unique index guarantees at most one ever exists.

## StepRecord

The append-only captured outcome of acting on a Step during an Execution.

| Field | Type | Notes |
|-------|------|-------|
| `Id` | Guid | identity |
| `ExecutionId` | Guid, FK → Execution | owner |
| `StepPosition` | int | which Step of the pinned Version (R4); 1..n |
| `Outcome` | enum `Done` / `Skipped` / `Failed` | the recorded outcome (FR-004) |
| `Note` | string, optional | free-text note |
| `RecordedAt` | DateTimeOffset | when recorded; review orders by this (R6) |
| `Sequence` | int | per-Execution monotonic tiebreaker for ordering (R6) |

- Constructed only by `Execution.RecordStep`; never mutated or deleted (FR-005, FR-008).
- Does **not** capture an actor ("who") — deferred (FR-008, glossary 2026-06-13).

## Computed Review *(derived read model — not persisted, R1)*

Produced on read from a **closed** Execution. Not a table.

- **Header**: `IncidentTitle` if present, else `IncidentId`; the pinned Runbook name + Version number; started/closed times.
- **Timeline**: the Execution's Step Records in chronological order (`RecordedAt`, then `Sequence`), each showing its Step (position + the pinned Version's Step text), outcome, note, and time. A Step marked more than once appears once per mark, in sequence (FR-006, FR-011).
- **Coverage**: every Step of the pinned Version with **no** Step Record is listed as **not reached**, distinct from an explicit `Skipped` (FR-012).
- Never hand-authored or edited (FR-013).

## Relationships

```
Runbook (1) ──< RunbookVersion (1) ──< RunbookVersionStep        [slice 01, unchanged]
                      ▲
                      │ pinned by (FK, read-only)
Execution (1) ──< StepRecord                                     [new]
Execution.IncidentId  →  external incident (reference only, no table)  [ADR-0005]
```

## State transitions

```
(start) ──> Open ──(record steps, any order, re-markable)──> Open ──(close)──> Closed
                                                                         │
                                                                  Computed Review available
Closed: no further Step Records (FR-010); never re-opened (non-goal).
```

## Schema / persistence notes

- New tables `Executions`, `StepRecords` are introduced via **EF Core migrations**
  (R2 / C-002); the initial migration first reproduces the existing 001/002
  schema so authoring databases and the 18 tests are unaffected.
- Unique index on `Executions.IncidentId` (FR-015).
- The pinned Version is referenced by id; the immutable Version's Steps are read
  for both the run view and the review (C-001 read-only).
