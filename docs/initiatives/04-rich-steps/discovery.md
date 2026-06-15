# Discovery — 04-rich-steps

> **FINALIZED — 2026-06-15.** Verdict **Go**. Register moves
> `Registered → Discovery`. Demonstration platform: demand is a stated
> assumption with illustrative numbers, not validated field evidence (carried
> from [01 discovery](../01-versioned-runbook-execution/discovery.md)).

Discovery test: this is a **new problem** surfaced by using the shipped product
→ full one-pager. It is not an inherited consequence of an earlier slice — the
prior slices deliberately kept a Step minimal ("how thin can authoring be and
still publish a version"); dogfooding the result exposed a distinct pain.

## Problem
A **Step is a single line of text** — one title, nothing more
(`Step = { position, text }`). Under incident pressure a responder needs to
*act*, not just be reminded a thing exists: what exactly to do, the command to
run, and how to know it worked. None of that fits in a Step today, so a
published runbook is a list of vague titles ("Restart the service") that does
not reduce time-to-act or response variability — the two things a runbook exists
to reduce. The detail lives in the responder's head or scattered elsewhere, so
the published version under-delivers on its own promise. (Solution unwelded:
this states the pain, not the rich-step answer, and explicitly not branching.)

## Whose problem
- **Users:** on-call / incident responders executing under pressure (primary) —
  a title alone forces them to improvise the *how*; authors who currently cannot
  express a usable procedure in the tool; the postmortem reviewer, for whom a
  richer Step makes the Computed Review legible (what was the responder actually
  meant to do at that step).
- **Payer:** the engineering / reliability org carrying downtime cost
  (demo assumption: ~30-engineer SRE org, carried from 01).
- **Operators / constrainers:** still modeled beside an incident.io-style tool;
  no compliance/retention constraints bind this slice (demonstration build).

## Evidence
Field research run this session (PagerDuty, incident.io, FireHydrant, Rootly,
AWS SSM, Ansible, Google SRE workbook) is unanimous that a step is far more than
a title: SRE playbooks carry action + command + expected result + verification;
FireHydrant has a "Freeform Text" step for instructions/links; incident.io
Decision-Flow nodes carry a markdown *Prompt*. A bare title is below the floor
every comparable tool sets. **Assumed demand, not validated** — demonstration
platform; illustrative figures carried from 01 (~4 reviewable incidents/month).
Research and sources captured in [workshop.md](workshop.md).

## Cost of doing nothing
Published runbooks stay vague → responders improvise under pressure, MTTR and
variability stay high, and the Computed Review records *that* a step was marked
done but not *what doing it meant*. The authoring + execution foundation already
shipped (slices 01/03) keeps under-delivering until a Step can hold real detail.

## Appetite
**A couple of evenings** — matching slices 01/03. If it overruns, **cut fields,
don't extend**. The appetite is protected by one hard line: **branching /
decision steps are OUT** — explicitly deferred to a later initiative because they
break the linear-execution and "coverage = every step" invariants and need their
own ADRs. This slice enriches the *content* of a Step only.

## Go–No-go + date
**Go — 2026-06-15.** Built as a demonstration of the SDD method; demand is an
accepted assumption. A no-go would have looked like: a title is genuinely enough
for the demo's purposes, or the enrichment can't fit two evenings without
dragging in branching — neither holds, since rich content is purely additive to
the existing immutable-version model.
