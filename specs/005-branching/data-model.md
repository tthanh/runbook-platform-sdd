# Data Model — 005-branching (Phase 1)

Additive to the slice-01/03/04 model. New types in **bold**; changed fields marked
*(new)*. All state is constructed only through aggregate behavior (C-004).

## StepType (enum) — extended (ADR-0010)

`{ Action, Check, Decision }` — closed set grows by one; default `Action`; stored as
text. `Decision` is the only member that carries navigation semantics.

## Step (draft) — holds Options

Unchanged fields (Id, RunbookId, Position, Text, Instructions?, Command?,
ExpectedResult?, Type). *(new)* A Step of Type `Decision` owns an ordered collection of
**StepOption**. Non-Decision Steps own none.

## **StepOption** (draft Option) — NEW

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | draft-local; regenerated on `ReplaceSteps` like the Step (ADR-0008) |
| StepId | Guid | owning draft Step |
| Ordinal | int | order of the Option within the Decision Step (1..n) |
| Label | string | required; what the responder reads (e.g. "Database down") |
| TargetPosition | int | position of the Target Step within the Runbook (ADR-0008) |

Validation (in `Runbook.ReplaceSteps` / `Publish`): a Decision Step has **≥2** Options;
each Label non-empty; each `TargetPosition` refers to an existing Step; `TargetPosition >`
the Decision Step's own position (forward-only, no loops). See FR-017 / research R-H5.

## RunbookVersionStep (frozen) — holds frozen Options

Unchanged fields plus *(new)* an ordered collection of **RunbookVersionStepOption**,
copied at `RunbookVersion.Freeze` exactly as Step detail is (immutable thereafter).

## **RunbookVersionStepOption** (frozen Option) — NEW

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | frozen identity |
| RunbookVersionStepId | Guid | owning frozen Step |
| Ordinal | int | order within the Decision Step |
| Label | string | frozen |
| TargetPosition | int | frozen; resolves within the same Version (C-007 at run time) |

No mutation after freeze; constructed only by `RunbookVersion.Freeze`.

## StepRecord — chosen Option added (ADR-0009)

Unchanged fields (Id, ExecutionId, StepPosition, Outcome, Note?, RecordedAt, Sequence)
plus:

| Field | Type | Notes |
|-------|------|-------|
| ChosenOptionOrdinal *(new)* | int? | null for Action/Check records; set when the recorded Step is a Decision — the Ordinal of the chosen frozen Option |

Append-only; the latest record per `StepPosition` is that Step's end state (unchanged).
For a Decision, the latest record's `ChosenOptionOrdinal` gives the taken branch. The
`Outcome` for a Decision record is not a done/skipped/failed judgement — the chosen
Option is the outcome; representation detail (reuse `Done` as a neutral "resolved" marker
vs. add a `Decided` outcome value) is a tasks-level choice, kept minimal and consistent
with ADR-0009 (no separate write path either way).

## Taken Path (derived, not persisted)

Computed on read (research R-Taken-Path): walk positions from the first Step; at each
Decision Step, read the latest record's `ChosenOptionOrdinal`, resolve that frozen
Option's `TargetPosition`, and continue from there. Reached Steps carry their recorded
outcome; unreached Steps report `NotReached` (reused state; H3). For a Version with no
Decision Steps the Taken Path is every Step in order.

## Computed Review (redefined denominator)

Unchanged in shape; coverage is computed over the Taken Path. Each Decision Step shows
its chosen Option (FR-014); not-taken-branch Steps show `NotReached`, never `Skipped`
(FR-013). Derived on read (NFR), ordered in memory by `RecordedAt` + `Sequence` (C-009).

## Relationships

```
Runbook 1───* Step 1───* StepOption
Runbook 1───* RunbookVersion 1───* RunbookVersionStep 1───* RunbookVersionStepOption
Execution 1───* StepRecord   (StepRecord.ChosenOptionOrdinal → the chosen RunbookVersionStepOption.Ordinal)
Execution *───1 RunbookVersion (pinned; C-007)
```
