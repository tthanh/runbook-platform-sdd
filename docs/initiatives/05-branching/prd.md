# PRD — 05-branching

> Inputs: [discovery.md](discovery.md), [workshop.md](workshop.md).
> Appetite: minimal Decision-Flow model (cut scope, don't extend).
> Language gate: **cleared** — the `Decision` Step Type and the routing terms
> (Option, Target Step, Taken Path) plus the Computed Review coverage redefinition
> were ratified into the constitution glossary on 2026-06-28 (v1.1.0). This PRD is
> expressible entirely in ratified glossary words and is ready for /speckit.specify
> once approved.

## Context

A Runbook Version is a single straight line of Steps.
An Execution walks every Step once, in order, and the Computed Review reports
coverage as every Step.
But incident response is not a straight line: what a responder does next depends
on what they just found.
A Check like "is the database reachable?" should send the responder one way if it
is and another way if it is not.

Today a runbook cannot say that.
An author has two bad choices: flatten every contingency into one long list and
trust the responder to mentally skip the Steps that do not apply, or maintain
several near-duplicate runbooks for the variations.
The first reintroduces exactly the under-pressure improvising that richer Steps
set out to remove; the second splits one procedure across drifting copies with no
single source of truth.
Either way the decision logic stays in the responder's head, and the published
version under-delivers for any incident that is not a straight line.

This initiative lets a Step offer a choice.
An author can mark a Step as a **Decision** and give it named **Options**, each
routing to a **Target Step**.
During an Execution the responder picks the Option that matches what they found,
and the run continues down that path.
The Computed Review then reports against the **Taken Path** — the Steps the run
actually reached — instead of against every Step in the version.

It enriches the *shape* of a procedure only.
It adds no new way to author Step content (that shipped in rich-steps), no
automation, and no condition logic the platform evaluates on the responder's
behalf — the human still chooses.

Constraints touched (pointers for the plan, stated at product level):
the Options and their routing must freeze into a published Runbook Version exactly
as Step content does today, so version immutability holds unchanged (C-001, C-004,
C-005 markdown rendering unaffected).
Routing must always resolve through the version an Execution pinned at start, never
the latest version (C-007).
The closed Step Type set grows by one ratified member, Decision (C-006).

## Objective

Let an author express a branching procedure, and let a responder follow the one
path their situation calls for, so the published runbook matches how an incident
actually unfolds.

In scope:

- A Step can be typed **Decision**: instead of being marked done/skipped/failed
  like an Action or Check, completing it means **choosing among named Options**.
- Each **Option** carries a label the responder reads (e.g. "Database reachable")
  and routes to a **Target Step** — the Step the run continues from when that
  Option is chosen.
- When a published version is run, reaching a Decision Step presents its Options;
  the responder selects one, and the Execution continues from that Option's Target
  Step.
- The Steps a run reaches form its **Taken Path**; a runbook with no Decision Steps
  has a Taken Path of every Step in order, exactly as today.
- The **Computed Review** reports over the Taken Path: Steps that were reached are
  covered as today (done/skipped/failed), and Steps on a branch the run did not
  take are shown as not reached — distinguished from a Step the responder
  deliberately skipped.
- The Options and their routing freeze into the Runbook Version at publish,
  alongside Step content, and are immutable thereafter — the same guarantee Step
  content already has.
- A runbook that already exists, with no Decision Steps, remains valid and behaves
  exactly as before.

## Non-goals

Fencing for an eager reader — each is plausibly buildable from the Objective and
is explicitly out, holding the minimal Decision-Flow appetite:

- **No condition-expression branching.**
  An Option is a human-read label the responder chooses; the platform never
  evaluates a rule like "if field = value" to pick a branch automatically.
- **No automated branch selection.**
  The platform does not run a command, read a metric, or decide which Option is
  correct; a human always chooses.
- **No loops or back-edges.**
  An Option routes forward only; a procedure cannot send the run back to an earlier
  Step to repeat it.
- **No Gate or approval Step.**
  Pausing a run for a sign-off remains deferred (the `Gate` type is not coined in
  this slice).
- **No parallel paths.**
  A run follows exactly one path at a time; Options do not fan out into branches
  that run at once or later rejoin and merge.
- **No automatic merge / rejoin semantics.**
  Two Options may happen to route to the same Target Step, but the platform defines
  no special "the branches converge here" behavior beyond the run simply continuing
  from that Step.
- **No visual flow-builder.**
  Authors express Options and their Target Steps without a drag-and-drop diagram
  canvas; how the routing is presented for editing is deliberately modest.
- **No new Step content fields.**
  This slice adds the Decision shape, not new per-Step detail beyond what rich-steps
  already gave a Step.
- **No change to version immutability, pinning, or deletion.**
  Published versions and their routing are never edited or deleted; an Execution
  still pins one version for its lifetime.
- **No more than one open Execution per Incident, no restart.**
  Unchanged from the execution slice; branching does not add re-runs or alternate
  attempts.

## Metrics

Hypotheses with a stated basis, to correct rather than commit to.
This is a demonstration platform, so targets are illustrative, carried from the
discovery assumptions (~30-engineer SRE org, ~4 reviewable incidents/month).

- **Branching adoption: at least some runbooks published after this slice use a
  Decision Step where the procedure genuinely forks.**
  Basis: if authors who have branching procedures still flatten them into one list
  or fork whole runbooks, the feature has not changed behavior and has failed its
  purpose.
  Treated as a hypothesis to correct, not a commitment.

- **Duplicate-runbook reduction: a procedure that previously needed near-duplicate
  runbooks for its variations can be expressed as one branched runbook.**
  Basis: collapsing drifting copies into one source of truth is the authoring pain
  branching exists to remove; demonstrated by rebuilding one such pair as a single
  runbook in the demo.
  Treated as a hypothesis to correct, not a commitment.

- **Path fidelity: in a walkthrough, a responder running a branched runbook reaches
  only the Steps their situation calls for, without being shown irrelevant Steps.**
  Basis: the discovery pain is responders mentally branching and risking wrong or
  irrelevant Steps; a run that presents only the Taken Path is the outcome that
  lowers that risk.
  Measured by observation in the demo, not instrumented; treated as a hypothesis to
  correct, not a commitment.

- **Review legibility: for a branched run, the Computed Review makes clear which
  path was taken and does not misreport unreached branches as skipped.**
  Basis: "coverage = every step" misreads a branched run; a reviewer must be able to
  tell a not-taken branch from a deliberate skip for the review to be trustworthy.
  Treated as a hypothesis to correct, not a commitment.

## Open questions

Carried from the workshop; each must be resolved before or during the relevant
downstream phase.
The hotspot ids (H1–H6) are the workshop's; their technical resolution belongs to
/speckit.plan as ADRs or spikes.

- **Routing stability across edits (H1).**
  When an author reorders or inserts Steps while drafting, an Option must keep
  pointing at the Step the author meant.
  How the routing reference survives editing and the version freeze is a plan-time
  decision (ADR); at product level the requirement is simply that routing never
  silently re-points to the wrong Step.

- **How a chosen Option appears in the run history (H2).**
  Choosing an Option is the responder's action at a Decision Step.
  Whether that choice is recorded as the Step's outcome or as a distinct moment in
  the timeline is a plan-time decision (ADR); the product requirement is that the
  Computed Review can show which Option was taken at each Decision Step.

- **How unreached Steps appear in the review (H3).**
  Steps on a branch the run did not take must be visibly distinct from Steps the
  responder deliberately skipped.
  Whether this reuses the existing "not reached" state or names a new one is a
  plan-time decision (ADR); the product requirement is that the two never look the
  same.

- **What a Decision Step itself contains (H4).**
  Beyond its Options, does a Decision Step also carry the instruction / expected-
  result content a Check Step can, to explain *what to evaluate* before choosing?
  [NEEDS CLARIFICATION: does a Decision Step reuse the rich-step detail fields for
  its prompt, or carry only a label plus its Options?]

- **What makes a branched runbook valid to publish (H5).**
  Publishing freezes the routing forever, so the platform should refuse to publish a
  broken flow.
  The product-level questions: must every Option point to a real Step; may a path
  end (a Step with no continuation) and is that a valid ending or an error; must
  every Step be reachable from the start, or are unreachable Steps allowed but
  flagged?
  [NEEDS CLARIFICATION: which of these are publish-blocking errors versus author
  warnings?]
  Cycle prevention (no loops) is already fixed by the non-goals; its enforcement is
  a plan-time decision (ADR).

- **How far the authoring and run surfaces go (H6).**
  Authors need to set an Option's label and Target Step, and responders need to see
  and pick Options at a Decision Step, all without a visual flow-builder (a non-goal)
  and within the existing navigation approach (C-003).
  Where the read and edit surfaces stop is part of the appetite and is settled in the
  spec and plan.

- **Demonstration demand, not validated.**
  Demand for branching is an accepted assumption carried from discovery, with
  illustrative figures from slice 01; the metrics above are hypotheses to correct,
  not commitments.
