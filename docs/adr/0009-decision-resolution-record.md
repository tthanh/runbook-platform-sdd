# 0009 — A Decision resolution reuses the append-only Step Record

## Context

Workshop hotspot **H2** (initiative 05, branching). When a responder reaches a
**Decision** Step during an Execution, they choose a named **Option** and the run
continues from that Option's Target Step (spec FR-006/FR-007/FR-008). The contested
question is *what gets written* when the choice is made — and whether resolving a
Decision is a distinct new domain event or just another Step Record.

Existing execution model constrains the choice:
- `Execution.RecordStep` appends an immutable **Step Record** (`StepPosition`,
  `Outcome` ∈ {Done, Skipped, Failed}, optional note, `RecordedAt`, `Sequence`).
- Records are **append-only** with **no actor** captured; the *latest* record for a
  position is that Step's end state (architecture NFRs; C-009 ordering).
- The Computed Review is **derived on read**, never persisted (NFR "review computed on
  read").
- Constitution Principle: **no dual-writes**.

The workshop flagged event (6) "Decision resolved" as a *candidate* new event — "or is
the choice just a Step Record outcome?" — and routed it here.

## Options considered

1. **Reuse the Step Record.** Choosing an Option appends a Step Record for the Decision
   Step's position that captures which Option was chosen (a new nullable field on the
   record). "Latest record for a position wins" and append-only already hold, so no new
   write path. The "Decision resolved" event stays **derived**, not stored.
2. **Distinct Decision-resolution record/event.** Add a separate append-only table/type
   for resolutions, kept apart from Step Records. Makes the event first-class — but adds
   a second write path for what a Step Record can already carry, and the Computed Review
   must now merge two append-only streams on read.

## Decision

Adopt **option 1**. A **Decision resolution is recorded by appending a Step Record** for
the Decision Step's position, carrying the **chosen Option** (a new nullable reference on
`StepRecord`, by the Option's ordinal/target — see `data-model.md`). No separate table
and no second write path are introduced; the append-only, latest-wins, no-actor,
review-computed-on-read invariants are preserved unchanged. The workshop's "Decision
resolved" moment is **derived** from the record at read time, not persisted as its own
event.

*(Chosen by the human during /speckit.plan, 2026-07-01.)*

## Trade-offs accepted

- **Step Record now serves two shapes** — an Action/Check outcome, or a Decision's chosen
  Option. Accepted: one append-only stream keeps the model and the read-time Computed
  Review simple, and honors no-dual-write; the shape is disambiguated by the Step's Type.
- **The "Decision resolved" event is not first-class.** Accepted: nothing in the appetite
  needs to subscribe to it; deriving it from the record is sufficient for the Computed
  Review and the Taken Path.

## Consequences

- `StepRecord` gains a **nullable chosen-Option reference** (null for Action/Check
  records). Set only through `Execution.RecordStep` behavior (C-004); records stay
  append-only. See `data-model.md` for whether the outcome set gains a `Decided` marker
  or the chosen-Option field alone distinguishes it.
- The Computed Review reads the latest record per position to show which Option was chosen
  at each Decision (FR-014) and to compute the **Taken Path** (FR-011): walk from the
  first Step, and at each resolved Decision follow the chosen Option's target position.
- Steps on a branch not taken are **NotReached** — reusing the existing state (H3), not a
  new one (spec FR-013; glossary v1.1.0).
- One additive migration adds the nullable column (C-002); existing records backfill as
  null and behave exactly as before.

## Flip condition

If Decision resolutions later need attributes a Step Record cannot carry (multi-select,
a separate resolution timeline, per-resolution actor once auth exists), supersede this
ADR with a dedicated resolution record.

## Status + date

Proposed — 2026-07-01
