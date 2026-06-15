# 0007 — Step Type is a closed two-value taxonomy (Action, Check)

## Context

Workshop hotspot **H3** (initiative 04). A Step gains a **Step Type** classifying the
kind of work it represents (PRD-04; spec FR-002/012). The contested part is the
taxonomy: which values exist, whether the set is open or closed, and whether a type
carries behavior. This was resolved by the human during PRD clarification and ratified
into the binding glossary on 2026-06-15 (Step Type added; 9 terms); this ADR records
*why*, so the choice is not silently re-litigated when branching arrives.

The neighbouring field tools model richer step kinds — incident.io Decision Flows,
AWS SSM `aws:branch` and `aws:approve` — but those imply **control flow** (a step that
routes or gates), which this initiative explicitly defers (branching is a separate
initiative; PRD non-goals).

## Options considered

1. **Closed enum {Action, Check}, descriptive only.** Two values: Action (do
   something), Check (verify/observe). The type changes labelling only — not which
   fields are required, not how a Step is recorded or executed.
2. **Include control-flow types now (Decision, Gate).** Rejected: a Decision step
   routes to another step and a Gate pauses for approval — both are branching/control
   flow, which breaks the linear-execution and "coverage = every step" invariants and
   belongs to the deferred branching initiative with its own ADRs.
3. **Open / user-extensible type set.** Rejected: unearned complexity (no requirement
   asks for custom types), and it would put the taxonomy outside the binding glossary,
   which is meant to fix one meaning per term.

## Decision

Adopt **option 1**. Step Type is a **closed enum with exactly two members, Action and
Check**, and is **descriptive only** — it does not alter a Step's required fields,
validation, or execution (FR-012). Decision and Gate are explicitly **not** members in
this slice; they are deferred with the branching initiative. The set is settled by the
ratified glossary (2026-06-15), not reopened here.

## Trade-offs accepted

- **Only two types.** Accepted: they cover the action/verification distinction that
  makes a runbook readable; more types without a behavior to back them would be
  decoration. The set can grow when branching gives Decision/Gate real meaning.
- **A descriptive-only type may feel underpowered** to someone expecting per-type
  behavior. Accepted: behavior is exactly what this slice fences out (no automation,
  no branching); the type earns its keep as a label and as the seam future control-flow
  types will extend.

## Consequences

- `StepType` is modelled as a closed enum, stored as text with default `Action` so
  existing rows backfill and stay valid (research R3/R8).
- No per-type required-field rules or execution branches exist; FR-012 is a property of
  the model, not a runtime check.
- When the branching initiative lands, adding Decision/Gate is a glossary amendment +
  a superseding/extending ADR, not an ad hoc code change.

## Flip condition

When the branching initiative is built, supersede or extend this ADR to add the
control-flow types (Decision, Gate) with their routing/gating semantics, amending the
glossary in the same ratified step. Until then the set stays closed at two.

## Status + date

Proposed — 2026-06-15
