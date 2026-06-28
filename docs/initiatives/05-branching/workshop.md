# Workshop — 05-branching

Level: big-picture (language, events, contexts, hotspots).
Design-level detail belongs in /speckit.plan, where contested decisions
are promoted to ADRs. Process: see docs/workshop-guide.md.

**Workshop test result: RUN (not skipped).** Three legs of the test are a "no":
(1) **new/changed language** — the deferred `Decision` Step Type arrives, plus
terms for the routing structure (option → target step, the taken path);
(2) **changed invariants / a candidate new event** — branching rewrites
"Execution is a linear walk" and Computed Review "coverage = every step";
(3) **hotspots that need ADRs** — the routing model, the navigation/event
question, and the coverage redefinition. Sized narrowly to the **minimal
Decision-Flow model** the reviewer set as the appetite ([discovery.md](discovery.md)).

> **Compile note.** Demonstration build, no live human board. Acting as scribe,
> this file was compiled from the finalized [discovery one-pager](discovery.md)
> and the branching field research already captured **for this initiative** in
> [04 workshop.md](../04-rich-steps/workshop.md) ("Branching — deferred, recorded
> for the next initiative"). The proposed language is **NOT yet ratified** and is
> **NOT in the constitution** — it awaits the human reviewer's verdict before the
> PRD is written (the human gate that slices 01/03/04 passed). Compiled, not
> invented: every term/event/hotspot below traces to the discovery page, the 04
> field research, or the existing glossary/conflict register.

## Attendees & date
- Date: 2026-06-28
- Facilitator: (demo) — compiled by AI scribe
- Participants: none live (demonstration); framing from [discovery.md](discovery.md)
- Reviewer: human — **ratified 2026-06-28**. Accepted the `Decision` Step Type
  (enum grows by one; Gate stays deferred), the routing terms (Option, Target Step,
  Taken Path), and the Computed Review coverage redefinition exactly as proposed.
  Amended into the constitution in a separate ratification commit (v1.1.0), the way
  slices 01/03/04 split proposal from ratification.

## Event timeline
Past-tense events across authoring and a single branched run. Authoring events are
unchanged in *kind*; the execution path is where branching bites.

1. **Runbook drafted** — an author writes steps; a step may now carry named
   **routing options**, each pointing to a target step (a Decision step).
2. **Runbook version published** — the draft is frozen, **including the routing
   options and their targets**.
3. **Incident declared** — *external* (incident.io-style tool).
4. **Execution started** — pins exactly one runbook version (unchanged, C-007).
5. **Step marked done / skipped / failed** — appends one Step Record (unchanged).
6. **Decision resolved** — at a Decision step the responder selects a named
   option; the run routes to that option's target step. **Candidate NEW event** —
   or is the choice just a Step Record outcome? Contested → **H2**.
7. **Execution closed** — unchanged.
8. **Review computed** — now over the **taken path**: coverage is "every step on
   the taken path," and steps on untaken paths are not counted as Skipped.
   Changes the coverage invariant → **H3**.

> Scope guard inverted from slice 04: that slice held "no new events" as proof it
> hadn't drifted into branching. This slice *is* branching, so a new moment (6) is
> expected — but it is the **only** new event the minimal model may add. A second
> new event (a condition evaluated, a gate awaited, a loop re-entered) means the
> slice has drifted past its appetite and must stop.

## Language decisions → proposed glossary amendments (await ratification)
New or changed terms land in the constitution glossary, never ad hoc. The
following are **proposed here only** — they enter the binding glossary on a
ratified workshop, exactly as slices 01/03/04 did.

### Proposed new Step Type — **Decision** (amends the closed enum, C-006 / ADR-0007)
| Term | Proposed meaning |
|------|------------------|
| **Decision** (Step Type) | a step whose completion is **choosing among named options**, each routing the Execution to a target step. The third member of the Step Type set `{Action, Check, Decision}`. Unlike Action/Check, it is **not** descriptive-only — it changes execution navigation, which is the whole point of the slice. |

> **Gate stays deferred.** The 04 glossary deferred both `Decision` *and* `Gate`
> (manual approval) "with branching." This slice takes only `Decision`; `Gate`,
> condition-expressions, loops, and parallel paths remain deferred (discovery
> cut-line). The enum stays closed — it grows by exactly one member.

### Proposed new terms — the routing structure
| Term | Proposed meaning |
|------|------------------|
| **Option** | a named choice on a Decision step (e.g. "Database reachable" / "Database down") that, when selected during an Execution, routes to its **target step**. Frozen into the Version at publish alongside the step. |
| **Target step** | the step an Option routes to. Referenced so the reference survives the freeze (by stable step identity vs by position is **H1**). |
| **Taken path** | the ordered sequence of steps actually reached in an Execution, determined by the Options chosen at Decision steps. Replaces "the whole version" as the denominator for Computed Review coverage. |

### Proposed redefinitions of existing binding terms
| Term | Current (binding) | Proposed change |
|------|-------------------|-----------------|
| **Step Type** | one of Action or Check; descriptive only; Decision/Gate deferred | add **Decision**; note that Decision affects navigation (no longer "descriptive only" for that one member). Gate still deferred. |
| **Computed Review** | post-incident timeline + per-Step coverage over the pinned version ("every step") | coverage is computed over the **taken path**; untaken-path steps are reported as NotReached (reusing the existing state — see boundary notes), not Skipped. |

## Hotspots
Each becomes a spike or an ADR at /speckit.plan — none die in notes.

| # | Hotspot | Why it's contested | Destination | Owner |
|---|---------|--------------------|-------------|-------|
| H1 | **Routing model & version freeze** — how Options/targets are represented and frozen immutably into the Version | A target referenced by **position** collides with sequential numbering (C-001) on reorder/insert; a target referenced by **stable step id** needs an id Steps don't have today. Must freeze exactly like step detail (C-004) | ADR | planner |
| H2 | **Execution navigation & the new event** — does resolving a Decision append a Step Record (outcome = chosen Option) or a distinct "branch taken" event? How does the run compute the next step? | Touches append-only records and the linear-walk invariant; decides whether event (6) is real or derived | ADR | planner |
| H3 | **Coverage redefinition** — Computed Review "coverage = every step" → "every step on the taken path" | The architecture already distinguishes **NotReached** from **Skipped**; reusing NotReached for untaken paths may make this additive rather than a rewrite — confirm | ADR | planner |
| H4 | **Decision step's fields** — does a Decision step still carry instructions/command/expected-result, or only a prompt + Options? | Bounds the schema and the authoring form; interacts with the Action/Check field model (ADR-0007) | ADR or note | planner |
| H5 | **Publish-time graph validation** — targets must exist; no cycles/back-edges (loops are OUT); dead ends allowed only as explicit ends; unreachable steps flagged? | Without it an author can publish an immutable, broken/looping flow; defines what "a valid branched Version" means | ADR | planner |
| H6 | **Authoring UX for routes** — expressing "Option → target step" without a graph editor, within hash routing (C-003) and no new router/graph dependency | Where the read/edit surface stops is part of the appetite; risk of scope creep into a visual flow builder | Spike / note | planner |

## Boundary notes
- **Bounded context unchanged: `Runbook Authoring & Execution`.** No new external
  context; branching adds routing *within* a Version, no upstream/downstream
  dependency.
- **Forward-edge DAG only.** Options route forward; **no cycles/back-edges**
  (loops are OUT this slice, discovery cut-line). H5 enforces this at publish.
- **Reuse, don't invent, the coverage state.** The execution slice already has
  `NotReached` vs `Skipped` (architecture, slice 003). Untaken-path steps are the
  natural `NotReached` case — H3 confirms whether this makes coverage additive.
- **Still OUT (deferred again, per discovery):** condition-expression branching
  (`field op value`), `Gate`/approval steps, parallel paths, and any automated
  evaluation of which branch to take. The field's two paradigms were mapped in
  [04 workshop.md](../04-rich-steps/workshop.md); this slice builds **only** the
  explicit decision-routing one (AWS SSM `aws:branch` / incident.io Decision
  Flows), not the condition-gated one (PagerDuty/FireHydrant `when`).

## Open questions carried into the PRD
- **Target reference: stable step id or position?** (H1 — drives schema + freeze.)
- **Is "Decision resolved" a Step Record outcome or a new event?** (H2.)
- **Does coverage reuse `NotReached`, or add a `NotApplicable` state?** (H3.)
- **What fields does a Decision step require** — prompt + Options only, or also the
  Action/Check detail fields? (H4.)
- **Publish validation rules** — targets exist, acyclic, dead-ends-as-explicit-end,
  reachability of every step? (H5.)
- **How far does the authoring/run UI go** without a visual flow builder, inside
  the appetite and C-003 hash routing? (H6.)
