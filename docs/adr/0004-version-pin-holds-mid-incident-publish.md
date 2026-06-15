# 0004 — The Version Pin holds for the life of an Execution

## Context

Workshop hotspot **H2** (initiative 01, carried into 03). An Execution pins the
chosen Runbook's current published Runbook Version at start (FR-001) and the
glossary says it "never re-pins." But a Runbook keeps evolving: an author may
publish a newer Runbook Version *while an Execution is in flight*. The binding
glossary already fixes the rule — an Execution "never re-pins" — so this ADR
records *why* that rule holds and decides the genuinely open part: what a
responder running the pinned Version sees when a newer Version exists.

This matters because the Computed Review's truth depends on it: a review can only
say "this is the procedure we followed" if the procedure could not shift under
the run. Pin integrity is a stated property to hold (spec SC-004).

Appetite: a couple of evenings; the responder is acting under incident pressure.

## Options considered

1. **Pin holds; the responder sees only the pinned Version (silent).** The
   running Execution shows and records against its pinned Runbook Version; the
   existence of a newer Version is not surfaced at all during the run.
2. **Pin holds; a passive indicator.** Same binding, but the run shows a
   read-only "a newer Version exists" note (no action — re-pin stays a non-goal).
   More informative, more UI, and a potential distraction mid-incident.
3. **Re-pin / offer an in-flight upgrade.** Let the responder move the running
   Execution onto the newer Version. Rejected: it destroys the core guarantee —
   the Step Records would reference steps that changed meaning, the Computed
   Review could no longer say which Version was in effect, and "never re-pins"
   is in the glossary definition of Execution.

## Decision

Adopt **option 1**. An Execution's Version Pin is set once at start and holds for
the entire life of the run; it is never re-pinned, even if a newer Runbook
Version of the same Runbook is published while the Execution is open. The
responder sees and records against only the pinned Version, with **no
notification** that a newer Version exists.

## Trade-offs accepted

- **A responder may unknowingly run a superseded procedure.** Accepted: the
  middle of an incident is the wrong moment to swap procedures, and a provable
  "what we actually followed" is worth more than currency. Picking up the newer
  Version is a *new* Execution's job — and since this slice allows one Execution
  per incident (FR-015), that means a future, separate incident, which is the
  correct boundary.
- **No signal at all** may occasionally surprise an author who expected their
  republish to reach an in-flight run. Accepted for this slice; the flip
  condition covers it.

## Consequences

- FR-003 is satisfied structurally: the Execution stores its pinned Runbook
  Version and never reassigns it. SC-004 (zero Executions whose pinned Version
  changed) becomes a property the type holds, not a target.
- The Computed Review (FR-011) is always set against the pinned Version.
- Touches conflict-register **C-001** read-only: the Execution reads a Runbook
  Version by its ADR-0001 identity; it does not alter the uniqueness rule.
- No UI is built for "newer Version exists"; the run view shows the pinned
  Version only.
- **For a given incident, the pinned Version is final.** Combined with one
  Execution per incident and refuse-if-closed (FR-015, clarification
  2026-06-13), a newer Version cannot be adopted for that incident even if a
  responder discovers mid-run that a fix has shipped: the pinned steps are the
  only mid-incident path, and the newer Version applies to future incidents.
  The flip condition watches for this becoming a real pain.

## Flip condition

If responders report being misled by silently running a stale procedure — or if
post-incident reviews repeatedly note "we didn't realise a fix had shipped" —
supersede this ADR to add the passive indicator (option 2). The pin-holds rule
itself does not flip; only the silence does.

## Status + date

Accepted — 2026-06-13 (accepted on explicit author instruction)
