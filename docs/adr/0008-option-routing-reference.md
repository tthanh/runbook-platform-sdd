# 0008 — An Option references its Target Step by position

## Context

Workshop hotspot **H1** (initiative 05, branching). A **Decision** Step carries named
**Options**, each routing to a **Target Step** (spec FR-002/FR-007). The contested
question is how an Option *names* its Target Step so the reference (a) survives Step
reorder/insert while the author edits the draft and (b) freezes immutably into the
Runbook Version at publish (FR-003/FR-004).

How the codebase identifies Steps today constrains the choice:
- `StepRecord.StepPosition` is an `int` — the execution slice already identifies a Step
  by its **position**, not by a durable id.
- `Runbook.ReplaceSteps` **regenerates every draft Step's `Guid` on each save**
  (`backend/.../Domain/Runbook.cs`), so the `Guid` on a working Step is ephemeral;
  there is no stable Step identity in use anywhere.
- A published `RunbookVersionStep` is frozen with an immutable `Position`; within a
  published Version nothing reorders, so a position reference is stable at run time.
- The authoring API is a **full ordered replacement** — the client sends the whole Step
  list on every save — so routing can travel *with* that ordered payload.

## Options considered

1. **Reference by position (ordinal within the Version).** An Option stores the target
   Step's position. Consistent with `StepRecord.StepPosition` and the frozen
   `RunbookVersionStep.Position`. Routing is authored as part of the full-replacement
   payload and frozen at publish; the editor remaps targets when the author
   reorders/inserts so intent is preserved. Acyclicity is structural: "forward-only"
   means an Option's target position is greater than its Decision Step's position, so
   loops (FR-016) are rejected by a simple ordinal comparison.
2. **Introduce a stable Step identity.** Give draft Steps a durable key that survives
   `ReplaceSteps` (stop regenerating `Guid`s / carry a client-supplied id); Options
   reference that key, frozen into the Version. References survive reorder without
   remapping — but it departs from the position-based model used everywhere else
   (notably `StepRecord`), adds an identity concept the schema lacks today, and makes
   the acyclic/forward-only check non-structural.
3. **Hybrid (position now, id later).** Ship position and migrate to ids if editing
   grows. Rejected as premature — it is option 1 plus speculative groundwork.

## Decision

Adopt **option 1**. An **Option references its Target Step by position** — the ordinal
of the Step within the Runbook Version. Routing is authored inside the existing full
ordered-replacement payload and **frozen at publish** alongside the Step, exactly as
Step content is (C-004). Because a published Version is immutable, position references
never shift at run time. **Forward-only routing** is enforced as *target position >
Decision Step position*, which makes loops (FR-016) structurally impossible. The
**editor is responsible for keeping an Option pointing at the Step the author meant**
when Steps are reordered or inserted in the draft (FR-003), because routing is carried
in the same ordered payload it remaps.

*(Chosen by the human during /speckit.plan, 2026-07-01.)*

## Trade-offs accepted

- **Position references are fragile under draft reorder.** Accepted: routing travels in
  the same full-replacement payload as the ordered Steps, so the editor remaps targets
  at edit time; there is no separately-stored position that can drift out of sync, and
  the frozen Version is immutable.
- **No durable cross-edit Step identity.** Accepted: nothing else in the system needs
  one (records are already position-keyed), and adding one would be unearned complexity
  for the minimal Decision-Flow appetite.

## Consequences

- An Option is modelled with a **target position (`int`)** plus its label and an
  ordinal; it is set only through aggregate behavior (`Runbook.ReplaceSteps`) and copied
  into the frozen Version by `RunbookVersion.Freeze` (C-004). See `data-model.md`.
- Publish validation (FR-017) checks, by position: every Option's target position exists
  in the Version, every Option's target position is greater than its Decision Step's
  position (forward-only, no loops), and each Decision Step has ≥2 Options.
- The Taken Path (FR-011) is computed at read time by walking positions and following
  each resolved Option's target position — no new persisted state (see ADR-0009,
  research R-Taken-Path).
- C-001 (`(RunbookId, Number)` Version numbering) is **unaffected** — this is Step
  routing within a Version, not Version identity.

## Flip condition

If a future initiative introduces partial/collaborative draft editing (not full ordered
replacement), position remapping becomes unreliable and this ADR is superseded by a
stable-Step-identity ADR (option 2), migrating frozen Options to id references.

## Status + date

Proposed — 2026-07-01
