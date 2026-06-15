# PRD — 04-rich-steps

> Inputs: [discovery.md](discovery.md), [workshop.md](workshop.md).
> Appetite: a couple of evenings (cut fields, don't extend).
> **Depends on a human gate:** the **Step** redefinition and **Step Type** term
> are drafted into the constitution glossary (initiative-04 workshop language);
> this PRD is ready for /speckit.specify once the human commits that amendment.

## Context

A Step today is a single line of text — a title and nothing else.
During an incident a responder has to *act* on that step, not merely be reminded
it exists.
A title like "Restart the service" does not say how to do it, what to run, or how
to know it worked.
That knowledge stays in the responder's head or scattered outside the tool, so a
published Runbook Version reads as a list of vague titles.
A runbook exists to reduce time-to-act and response variability; a bare title
delivers neither.

Every comparable tool models a step as far more than a title — instructions, the
command to run, and the expected result are the floor (see the field research in
[workshop.md](workshop.md)).
This initiative raises a Step to that floor.
It enriches the *content* of a Step only; it adds no new moment to the lifecycle
(a Step is still authored, frozen at publish, and marked done/skipped/failed in
an Execution exactly as today).

Constraints touched (pointers for the plan, stated at product level):
the new detail must freeze into a published Runbook Version exactly as the title
does today, so version immutability holds unchanged (C-001, C-004).
This is the first slice to enlarge what a published version records since the
schema-change procedure was established (C-002, as updated by the 2026-06-15
constitution amendment); the plan applies that procedure rather than inventing one.
Navigation is unaffected (C-003).

## Objective

Let an author give each Step enough detail to be executed under pressure, and let
a responder read that detail at the moment they act.

In scope:

- A Step carries, in addition to its required title, optional structured detail:
  an **instructions** body (what to do, in the author's words, written with
  lightweight markdown markup that renders when the step is read), a **command**
  (the exact command to run), and an **expected result** (how the responder knows
  the step worked).
- Each Step has a **Step Type** that names the kind of work it represents, drawn
  from a minimal set: **Action** (do something) and **Check** (verify a condition
  or observe a result).
- All detail is optional and may be empty, so every Step authored before this
  slice remains valid and unchanged.
- The detail is frozen into the Runbook Version at publish, alongside the title,
  and is immutable thereafter — the same guarantee the title already has.
- A responder running an Execution sees a Step's detail while working it, so the
  command and expected result are in front of them as they mark it
  done / skipped / failed.
- The detail appears wherever a published version is read (when a version is
  viewed, and in the Computed Review), so the record shows not just *that* a step
  was marked but *what doing it meant*.

## Non-goals

Fencing for an eager reader — each is plausibly buildable from the Objective and
is explicitly out:

- **No branching or decision steps.**
  A Step never routes to another Step; Execution stays a single ordered pass.
  Decision flows are a separate, later initiative (see the boundary note in
  [workshop.md](workshop.md)).
- **No between-step control flow.**
  No per-step conditions, gates, approvals, waits/delays, or loops.
- **No automation or integration.**
  The platform never *runs* a command; the command is reference text a human
  copies and executes elsewhere.
  No connections to chat, paging, ticketing, or observability tools.
- **No Step Type beyond Action and Check.**
  Decision and Gate types are deferred with branching; the set is not open or
  user-extensible in this slice.
- **Step Type is descriptive only.**
  It changes how a Step is labeled, not its required fields, its validation, or
  how it is executed; no per-type behavior.
- **No automated verification.**
  The expected result is reference text for the human to judge; the platform does
  not compare, validate, or gate on actual outcomes.
- **No per-Step owner or assignee.**
  Who does a step is not captured (consistent with the actor being deferred until
  accounts exist).
- **No reference links, attachments, or a rollback / on-failure field.**
  Candidates for a later slice; not in this appetite.
- **No WYSIWYG / visual editor.**
  Formatting is authored as lightweight markdown markup, not through a visual
  toolbar or a formatted-document editor.
- **No change to version immutability or deletion.**
  Published versions and their detail are never edited or deleted; this slice
  only adds to what a version freezes.

## Metrics

Hypotheses with a stated basis, to correct rather than commit to.
This is a demonstration platform, so targets are illustrative, carried from the
discovery assumptions (~30-engineer SRE org, ~4 reviewable incidents/month).

- **Detail adoption: at least 60% of Steps in versions published after this slice
  carry detail beyond the title.**
  Basis: if authors keep writing bare titles, the feature has not changed
  behavior and has failed its purpose; a clear majority adopting detail is the
  signal it was worth shipping.
  Treated as a hypothesis to correct, not a commitment.
- **Action-step completeness: of Steps typed Action, a majority carry both a
  command and an expected result.**
  Basis: command + expected result is the "production-grade" floor every
  comparable tool sets; an Action step missing both is still a vague title under
  a new name.
  Treated as a hypothesis to correct, not a commitment.
- **Responder reliance: in a walkthrough, a responder can execute a step from the
  runbook alone, without seeking the how elsewhere.**
  Basis: the discovery pain is responders improvising the *how* under pressure;
  removing the off-tool lookup is the outcome that would lower time-to-act and
  variability.
  Measured by observation in the demo, not instrumented; treated as a hypothesis
  to correct, not a commitment.

## Open questions

Carried from the workshop; each must be resolved before or during the relevant
downstream phase.

- **Field scope for the appetite — RESOLVED 2026-06-15.**
  Detail is instructions + command + expected result; reference links, per-step
  owner, and a rollback field are deferred to a later slice.
- **Step Type in this slice — RESOLVED 2026-06-15.**
  The minimal Action / Check type ships in slice 04 (descriptive only); Decision
  and Gate types defer with branching.
- **Formatting of the instructions body — RESOLVED 2026-06-15.**
  The body supports lightweight markdown markup, rendered when the step is read
  (WYSIWYG editing remains out of scope).
  Safe rendering of author-supplied markup (escaping / sanitization) is a
  plan-time concern — carried as H2.
- **Read surfaces for the detail — RESOLVED 2026-06-15.**
  Detail shows both while running an Execution and in the Computed Review.
- **Language ratification — amendment drafted 2026-06-15.**
  The Step redefinition and Step Type term are drafted into the constitution
  glossary as a proposed edit; this PRD is ready for /speckit.specify once the
  human commits that amendment.
