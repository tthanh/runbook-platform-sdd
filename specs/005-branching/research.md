# Research — 005-branching (Phase 0)

Resolves the spec's open questions and the workshop hotspots. Format per item:
**Decision / Rationale / Alternatives**.

## R-H1 — How an Option references its Target Step
- **Decision**: by **position** (ordinal within the Version). See ADR-0008.
- **Rationale**: matches the existing position identity (`StepRecord.StepPosition`,
  frozen `RunbookVersionStep.Position`); routing travels in the full ordered-replacement
  payload so it stays consistent per save; published Versions are immutable so run-time
  references never shift; forward-only (target position > source) makes loops structural.
- **Alternatives**: stable Step identity (departs from the position model, adds an id
  concept nothing else needs); hybrid (premature). Both rejected — see ADR-0008.

## R-H2 — How a Decision resolution is recorded
- **Decision**: **reuse the append-only Step Record** with a nullable chosen-Option
  reference. See ADR-0009.
- **Rationale**: preserves append-only, latest-wins, no-actor, review-on-read, and
  no-dual-write; the "Decision resolved" event is derived, not stored.
- **Alternatives**: a distinct resolution record/event — adds a second write path and a
  two-stream read merge for what one record can carry. Rejected — see ADR-0009.

## R-H3 — Coverage over the Taken Path / not-reached state
- **Decision**: reuse the **existing `NotReached` state**; the change is *additive*, not a
  rewrite. No ADR needed (settled by glossary v1.1.0 + clarify).
- **Rationale**: the execution slice already distinguishes `NotReached` from `Skipped`
  (architecture, slice 003). Untaken-branch Steps are the natural `NotReached` case; the
  Computed Review's shape is unchanged, only its denominator (the Taken Path).
- **Alternatives**: a new `NotApplicable`/`NotOnPath` state — rejected as unearned; the
  ratified glossary says untaken Steps report as `NotReached`.

## R-Taken-Path — Computing the Taken Path
- **Decision**: computed **on read** by walking positions from the first Step; at each
  Decision Step, read the latest Step Record for that position to get the chosen Option,
  then jump to that Option's target position; Steps between reached positions on the
  walked route are "reached", all others are `NotReached`. Never persisted.
- **Rationale**: consistent with "review computed on read"; forward-only routing
  guarantees the walk terminates (no loops). For a Runbook with no Decision Steps the walk
  is 1..N — every Step, exactly as today (spec FR-011).
- **Edge**: a Decision reached but not resolved before close → the walk stops there;
  Steps beyond are `NotReached`, the Decision itself is reached-but-unresolved (spec edge
  case). Ordering uses C-009 (in-memory, `Sequence` tiebreaker).

## R-H4 — Decision Step content
- **Decision**: a Decision Step **reuses the existing optional Step detail fields**
  (`Instructions`/`Command`/`ExpectedResult`) plus its Options; none required, none
  forbidden. Settled in `/speckit.clarify` (2026-07-01).
- **Rationale**: least special-casing — the fields already exist on every Step and are
  optional; forbidding them for Decision would be extra conditional validation for no
  gain. Authors typically pose the choice via title + instructions.
- **Alternatives**: prompt+Options only; prompt+instructions only — both need new
  conditional field rules. Rejected in clarify.

## R-H5 — Publish-time validation
- **Decision**: at publish, **BLOCK** if any Option's target position does not exist, if
  any Option is not forward-only (target ≤ source position → loop/back-edge), or if any
  Decision Step has <2 Options; **WARN** (non-blocking) on a Step unreachable from the
  start; **ALLOW** a path that simply ends. Settled in clarify (2026-07-01); FR-017.
- **Rationale**: refuse to freeze a genuinely broken flow forever, without over-policing
  the minimal model. Forward-only positions make the acyclic check a comparison, not a
  graph traversal.
- **Alternatives**: integrity-only (too permissive — allows 1-Option "decisions"); strict
  (blocks dead-ends/unreachable — too much authoring friction). Rejected in clarify.

## R-H6 — Authoring / run UI extent
- **Decision**: express Options and their target Steps with plain form controls **within
  hash routing (C-003)** — no visual flow-builder, no graph/router dependency (spec
  non-goal). A target is chosen from the existing ordered Step list.
- **Rationale**: bounded by appetite; a builder is explicit scope creep. Position-based
  targets map naturally to "pick a later Step in the list".
- **Alternatives**: drag-and-drop flow canvas — out of scope.

## R-StepType — Adding Decision to the taxonomy
- **Decision**: extend the closed enum to {Action, Check, Decision} via ADR-0010
  (extends ADR-0007, amends C-006); stored as text, default `Action`, no migration beyond
  the column accepting a new value.
- **Rationale**: C-006 requires an ADR to amend the taxonomy; the meaning is already
  ratified (glossary v1.1.0). Existing rows unaffected.
- **Alternatives**: open type set — rejected (ADR-0007/0010).

## R-BackCompat — Runbooks with no Decision Steps
- **Decision**: identical behavior to before across author/publish/run/review; their
  Taken Path is every Step in order; prior slices' tests stay green and unmodified.
- **Rationale**: additive-only changes (new optional child tables, one nullable column,
  one new enum value defaulted away); nothing existing is renamed or removed.

## R-Schema — Migration shape (C-002)
- **Decision**: one additive migration adds `StepOptions` (FK → `Steps`) and
  `RunbookVersionStepOptions` (FK → `RunbookVersionSteps`), each with label, target
  position, ordinal; plus a nullable chosen-Option column on `StepRecords`. Reproduce the
  existing schema first, then add (C-002); no table dropped.
- **Rationale**: mirrors how slice 004 added detail columns; keeps existing stores/tests
  valid.
- **Alternatives**: serialize Options as JSON on the Step row — rejected (opaque to
  queries/validation, breaks the aggregate's explicit modelling per C-004).
