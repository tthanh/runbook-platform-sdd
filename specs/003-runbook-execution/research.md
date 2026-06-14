# Research — 003 Runbook Execution (Phase 0)

Resolves the plan's unknowns and the workshop items routed to plan time. Format
per phase: Decision / Rationale / Alternatives considered.

## R1 — Computed Review derivation vs the no-dual-writes rule (workshop H4 spike)

**Decision**: The Computed Review is **derived on read** from a closed
Execution's Step Records set against its pinned Runbook Version's Steps. It is
**not** persisted as a separate projection or store.

**Rationale**: The spec requires the review to "reflect exactly the Step Records"
(FR-011, FR-013). Step Records are append-only and a closed Execution takes no
more of them (FR-010), so the inputs are frozen once the Execution closes —
computing on read yields a stable, identical result every time. With a single
store (SQLite) and no second copy, the constitution's no-dual-writes rule is
satisfied **trivially**: there is nothing to propagate, so no outbox is needed.

**Alternatives considered**: Persist a Computed Review projection at close
(materialised view / second table). Rejected: it introduces a second write of
the same truth — exactly the dual-write the constitution forbids without an
outbox — and no requirement earns the complexity (constitution: complexity is
earned). Deriving on read is O(steps + records) over a single run's data, which
is tiny at this scale.

## R2 — Schema migration tooling (conflict register C-002)

**Decision**: Adopt **EF Core migrations**, replacing `EnsureCreated()`. Generate
an initial migration that captures the *existing* 001/002 schema (Runbooks,
Steps, RunbookVersions, RunbookVersionSteps), then a second migration adding the
execution tables (Executions, StepRecords). `Program.cs` calls
`Database.Migrate()` at startup instead of `EnsureCreated()`.

**Rationale**: This slice is the **first schema change** since slice 01 shipped
with `EnsureCreated()`. C-002 records the rule explicitly: "the first schema
change in any slice requires migrating to EF Core migrations before applying the
change." `EnsureCreated` does not version the schema and cannot evolve an
existing database; mixing it with migrations is unsupported. The initial
migration must reproduce the current schema byte-for-byte so existing databases
(and the 18 authoring tests) are unaffected.

**Alternatives considered**: Keep `EnsureCreated` and drop/recreate the dev
database. Rejected: violates C-002, discards data on every schema change, and
does not scale to any future slice. Hand-written SQL DDL: rejected — EF Core
migrations are the idiomatic, reversible tool already implied by the stack
(ADR-0002).

## R3 — Execution as an aggregate root (ADR-0003 consistency, C-004)

**Decision**: `Execution` is an **aggregate root**. Its invariants live in
behavior methods: `Start(...)` (static factory; pins the current published
Version, requires the Runbook to have one), `RecordStep(stepPosition, outcome,
note)` (allowed only while open; appends a Step Record), and `Close()` (open →
closed, one-way). `StepRecord` is constructed only by `Execution.RecordStep` and
is never mutated. The open/closed gate, the append-only rule, and the
set-once pin all sit inside the aggregate; endpoints stay thin (load → method →
save → map).

**Rationale**: Conflict register **C-004** (ADR-0003) makes this binding: new
invariants belong in aggregate behavior, not endpoints. The execution invariants
(can't record on a closed run, can't re-pin, can't edit a record) are precisely
the kind ADR-0003 says must be structural.

**Alternatives considered**: Anemic Execution + logic in endpoints — rejected by
C-004/ADR-0003.

## R4 — How a Step Record references "which Step"

**Decision**: A Step Record references the Step **within the pinned Runbook
Version** by that Version's Step position (1..n). Because a Runbook Version is
immutable (FR-006, slice 01), positions are stable for the life of the run, so a
position uniquely and permanently identifies a Step in the pinned Version.

**Rationale**: The pinned Version cannot change (ADR-0004), so position is a safe,
human-legible key — it also reads naturally in the Computed Review and in the
HTTP contract. Storing the `RunbookVersionStep` id is equivalent; position is
chosen for legibility and because the contract already speaks in positions
(slice 01).

**Alternatives considered**: Reference the live Runbook's working Step — rejected:
the working Steps drift after publish; only the frozen Version is meaningful to a
run.

## R5 — Step outcome and "not reached"

**Decision**: Outcome is a closed set — **done / skipped / failed** — stored on
each Step Record. **"Not reached" is derived, never stored**: in the Computed
Review, a pinned-Version Step with zero Step Records is rendered "not reached,"
distinct from an explicit "skipped" (FR-012).

**Rationale**: Not-reached is the *absence* of a record, not an outcome a
responder selects; modelling it as derived keeps the Step Record honest (it only
ever holds things that actually happened) and makes the distinction fall out of
the data.

**Alternatives considered**: A fourth stored outcome "not reached" — rejected: it
would require writing records for inaction, contradicting the append-only
ground-truth intent.

## R6 — Chronological ordering of the Computed Review

**Decision**: Each Step Record carries a `RecordedAt` timestamp; the Computed
Review orders entries by it. To avoid the SQLite `DateTimeOffset` ordering
pitfall caught in slice 01 (the retro noted EF Core could not translate
`OrderBy(DateTimeOffset)` on this provider), **order the records in memory** after
loading a single Execution's records, or carry a monotonic per-Execution sequence
number assigned at record time as the tiebreaker.

**Rationale**: A single Execution's record count is small, so in-memory ordering
is correct and cheap. The slice-01 retro is direct evidence this provider
mishandles `DateTimeOffset` ordering pushed to SQL.

**Alternatives considered**: Push `ORDER BY RecordedAt` to SQLite — rejected per
the slice-01 evidence. A sequence number is the robust tiebreaker if two records
share a timestamp.

## R7 — Frontend (conflict register C-003)

**Decision**: The execution responder views (start/resume a run, mark Steps, view
the Computed Review) are added to the existing React SPA and **stay within hash
routing — no router package** (C-003). No new frontend dependency is introduced.

**Rationale**: C-003 binds the slice to hash routing unless a router is earned via
ADR; nothing here earns it. The execution views are few (a run view and a review
view) and fit the existing pattern.

**Alternatives considered**: Add a routing library — rejected: not earned (C-003).
