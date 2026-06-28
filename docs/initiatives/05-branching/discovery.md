# Discovery — 05-branching

> **FINALIZED — 2026-06-28.** Verdict **Go** (human reviewer). Register moves
> `Registered → Discovery`; the workshop is next (branching fails the skip test).
> Appetite and the Go/No-go decision cannot be inherited (discovery template) —
> both are recorded by the reviewer below. Demonstration platform: demand is a
> stated assumption with illustrative numbers, not validated field evidence
> (carried from [01 discovery](../01-versioned-runbook-execution/discovery.md)).

Discovery test: this is a **new problem**, not an inherited consequence. The prior
slices deliberately kept execution linear; branching was named OUT of 03 and 04 on
purpose because it breaks shipped invariants and needs its own ADRs. It is the #1
item in the [deferred ledger](../../deferred.md). New problem → full one-pager.

## Problem
A Runbook Version is a **single linear sequence**: an Execution walks every
position once, and the Computed Review defines coverage as "every step." But real
incident response is **not** linear — what you do next depends on what you just
found. A Check like "is the database reachable?" should send the responder down
different paths. Today the procedure cannot encode "if X do A, else do B," so an
author must either flatten every contingency into one list (the responder mentally
skips the irrelevant steps — exactly the under-pressure improvisation rich-steps
set out to remove) or maintain several near-duplicate runbooks. The decision logic
stays in the responder's head, and the published version under-delivers for any
incident that isn't a straight line. (Solution unwelded: this states the pain. It
does **not** pick between the two field paradigms — explicit decision-routing vs
condition-gated steps — that choice is the workshop's job.)

## Whose problem
- **Users:** on-call / incident responders (primary) — today they branch mentally
  and risk running steps that don't apply or missing ones that do; authors who
  cannot express a contingent procedure without forking a runbook; the postmortem
  reviewer, for whom "coverage = every step" **misreports** a branched run — steps
  on the untaken path look Skipped/NotReached when they were never applicable.
- **Payer:** the engineering / reliability org carrying downtime cost
  (demo assumption: ~30-engineer SRE org, ~4 reviewable incidents/month,
  carried from 01).
- **Operators / constrainers:** still modeled beside an incident.io-style tool; no
  compliance/retention constraints bind this slice (demonstration build).

## Evidence
Field research for this initiative was already captured during slice 04 and
recorded **for this next initiative** — see "Branching (deferred)" in
[04 workshop.md](../04-rich-steps/workshop.md). Two paradigms exist in the field:
(1) **explicit goto/choice** — AWS SSM `aws:branch` (choice → `NextStep` +
`Default`), incident.io **Decision Flows** (node = prompt + options, each routing
to a next node; a human-navigated tree); (2) **condition-gated** — per-step
run/skip conditions (`field op value`, AND/OR/NOT), e.g. PagerDuty `Condition`
(AND-only, **no else, no jump**), FireHydrant/Rootly `when`, GitHub `if`. Even the
large players keep in-procedure branching deliberately narrow. The
human-runbook-appropriate analogue is the **Decision-Flow tree**. **Assumed demand,
not validated** — demonstration platform; illustrative figures carried from 01.
Sources listed in [04 workshop.md](../04-rich-steps/workshop.md).

## Cost of doing nothing
Rich step *content* shipped (04), but procedures still cannot encode contingency.
Responders keep improvising *which path* to take, authors fork near-duplicate
runbooks (drifting versions, no single source of truth), and the Computed Review
mis-scores every branched response. The decision variability a runbook exists to
reduce stays unaddressed for any incident that isn't a straight line — the
authoring + execution + rich-content foundation (01/03/04) under-delivers exactly
where pressure is highest.

## Appetite
**Set by the reviewer 2026-06-28: minimal Decision-Flow model, cut hard if it
overruns.** Branching is materially larger than the prior "couple of evenings"
slices: it breaks **two shipped invariants** — linear execution (C-007 reads pin
through positions; Execution is a linear walk) and Computed Review "coverage =
every step" — and needs its own ADRs (execution/navigation model, coverage
redefinition, the deferred `Decision`/`Gate` step types). The **first** branching
slice is boxed to:
- **IN:** a step can offer **named options**, each routing to a target step; an
  Execution follows the **taken path**; coverage is redefined as "every step on
  the taken path" (untaken-path steps are *not* counted as Skipped).
- **OUT (defer again):** condition-expression branching (`field op value`),
  loops/back-edges, gates/approvals, parallel paths, and automated evaluation.

This keeps the slice to one paradigm and one invariant rewrite. If it overruns,
**cut scope, don't extend** — the cut line is the OUT list above.

## Go–No-go + date
**Go — 2026-06-28** (human reviewer), with the narrow Decision-Flow scope above.
A No-go would have looked like: the linear model is good enough for the
demonstration's purposes, or branching cannot be scoped under the appetite without
dragging in a full graph/condition engine. Neither held for the minimal scope.
