# 0010 — Extend the Step Type taxonomy to add Decision

## Context

Workshop hotspot carried from **H (Step Type)** and required by governance, not by a
contested choice. ADR-0007 fixed `StepType` as a **closed enum {Action, Check}** and
recorded (conflict register **C-006**) that *adding a step type requires an ADR amending
the taxonomy*. ADR-0007's own **flip condition** states: "When the branching initiative
is built, supersede or extend this ADR to add the control-flow types (Decision, Gate)
with their routing/gating semantics, amending the glossary in the same ratified step."

That step has happened: the branching language — the **Decision** Step Type plus the
routing terms — was **ratified into the binding glossary on 2026-06-28 (v1.1.0)**. So the
*meaning* of Decision is already settled by the constitution; this ADR is the taxonomy
amendment C-006 and ADR-0007 require, recording that the closed set grows by exactly one.

This is not a contested decision — the glossary settles it — so no options are
re-litigated; this ADR exists to keep C-006 honest and to extend ADR-0007 per its flip
condition.

## Options considered

1. **Extend the closed enum to {Action, Check, Decision}.** The ratified outcome: one new
   member. Action and Check stay descriptive-only; **Decision** is the one member that
   changes execution navigation (its completion is choosing an Option that routes the
   run). Gate remains deferred.
2. **Open / user-extensible type set.** Rejected again, for the same reason as ADR-0007:
   unearned complexity, and it would move the taxonomy outside the binding glossary.
3. **Fold Decision's behavior into the enum without an ADR.** Rejected: C-006 forbids
   changing the taxonomy without an ADR, and ADR-0007's flip condition mandates a
   superseding/extending record.

## Decision

Adopt **option 1**, extending **ADR-0007**. `StepType` becomes a **closed enum
{Action, Check, Decision}** (default remains `Action`). Action and Check are unchanged and
remain descriptive-only. **Decision** carries navigation semantics: its completion is the
choice of a named Option that routes the Execution (see ADR-0008 for routing, ADR-0009 for
how the choice is recorded). **Gate stays deferred.** The set stays **closed** — it grows
by exactly one member, as ratified in glossary v1.1.0.

## Trade-offs accepted

- **The enum is no longer purely descriptive.** Accepted: exactly one member (Decision)
  gains behavior, and only the behavior branching exists to add; Action/Check keep their
  descriptive-only contract, so ADR-0007's property holds for them.
- **Two ADRs now describe the taxonomy** (0007 for Action/Check, 0010 adding Decision).
  Accepted: ADRs are immutable once Accepted, so extension is a new record, not an edit —
  this is the mechanism ADR-0007's flip condition prescribed.

## Consequences

- C-006 is amended: the closed set is `{Action, Check, Decision}`, still stored as text
  with default `Action`, validated in the aggregate; the enum↔string mapping stays in
  sync (one additive concern, no migration beyond the existing text column accepting a new
  value).
- Existing rows (`Action`/`Check`) are unaffected; no backfill.
- ADR-0007 is **extended** by this ADR (its flip condition is now exercised); on
  acceptance, ADR-0007's status line gains an "Extended-by 0010" note by the human commit
  that accepts this ADR.
- Adding **Gate** in a later initiative remains a further ADR + glossary amendment.

## Flip condition

If a later initiative adds Gate (or any further control-flow type), extend or supersede
this ADR with that member's semantics, amending the glossary in the same ratified step.

## Status + date

Proposed — 2026-07-01
