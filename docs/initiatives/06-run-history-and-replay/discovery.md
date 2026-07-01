# Discovery — 06-run-history-and-replay

> **FINALIZED — 2026-07-01.** Verdict **Go** (human reviewer). Register moves
> `Registered → Discovery`. Appetite set below to the **medium** scope (browse/search
> past executions + single-run replay); the go decision and appetite cannot be
> inherited (discovery template) and are recorded by the reviewer here.
> **Build is sequenced *after* 05-branching releases** — replay must account for the
> Decision events and taken path 05 introduces, so designing it against a run shape
> that is about to change would be wasted. Demonstration platform: demand is a stated
> assumption with illustrative numbers, not validated field evidence (carried from
> [01 discovery](../01-versioned-runbook-execution/discovery.md)).

Discovery test: this is a **new problem**, not an inherited consequence. It is picked
from the backlog — the "Around execution" cluster in the [deferred ledger](../../deferred.md)
(browse/search past executions, compare runs, reporting) named OUT of the 03 execution
slice on purpose. Picking it is a fresh human call, so: full one-pager.

## Problem
Today a closed Execution is a **write-only island**. You can open *one* run and read
its Computed Review, but there is no way to see runs **across incidents** as a history,
and no way to **replay** a run — to watch, step by step in the order it happened, how
the team actually moved through the procedure. The append-only Step Records that would
make this possible already exist (slice 003), but nothing surfaces them as a browsable
list or a time-ordered playback. The learning the platform exists to produce — *how did
we respond, and how do we respond better next time* — stays locked in individual runs
no one can navigate to or step through.

The reviewer named history + replay an **essential** capability of the platform, not a
supplement: a runbook exists to reduce decision variability, and you cannot reduce what
you cannot review across runs. (Solution unwelded: this states the pain. It does **not**
decide *how* to deliver it — specifically it does **not** pre-commit to re-founding
Execution persistence on an event stream vs. extending the append-only Step Record log
the system already has. That is a contested, ADR-worthy decision for the workshop/plan,
exactly as 05 left its paradigm choice open.)

## Whose problem
- **Users:** the **post-incident reviewer / retro facilitator** (primary) — wants to
  reconstruct and step through what the team actually did, not read a static summary;
  **on-call responders and trainees**, who learn incident response by replaying real
  past runs; **team leads**, who want to find and revisit prior incidents rather than
  re-derive them from memory. Once 05 lands, replay also answers *which path did the
  team take at each Decision* — the highest-value moment to review.
- **Payer:** the engineering / reliability org carrying downtime cost (demo assumption:
  ~30-engineer SRE org, ~4 reviewable incidents/month, carried from 01).
- **Operators / constrainers:** still a local demonstration build. The existing
  append-only and no-deletion NFRs already guarantee the history is intact and truthful;
  no retention/compliance/PII constraints bind this slice.

## Evidence
Provenance is the [deferred ledger](../../deferred.md) "Around execution" cluster, sourced
from the [03 PRD](../03-runbook-execution/prd.md) non-goals: *browse/search past executions*,
*compare/diff two executions*, *reporting across runs*. Slice 03 deliberately shipped a
single-run view and pushed history out; dogfooding then confirmed the gap the reviewer now
calls essential. Field analogue: incident-retrospective tooling universally centres on a
**navigable incident timeline** you replay after the fact (incident.io / PagerDuty postmortem
timelines, dedicated retro tools) — the human-review analogue of what Step Records already
capture. **Assumed demand, not validated** — demonstration platform; illustrative figures
carried from 01.

## Cost of doing nothing
Slices 01/03/04 (and 05, incoming) build increasingly rich, executed, soon-to-be-branched
runs — and then strand every one of them. The Step Records pile up append-only but stay
unreadable as history; the "how did we do?" payoff that motivates capturing them in the
first place never arrives. The platform keeps *producing* review material it gives no way to
*review across runs* — under-delivering exactly the learning loop a runbook tool exists to
close, and most visibly once branching makes "which path did we take?" a question worth asking.

## Appetite
**Set by the reviewer 2026-07-01: medium — browse/search past executions + single-run
replay. Cut hard if it overruns.** Larger than the "couple of evenings" content slices
because it adds a query/list surface and a playback experience, but deliberately bounded to
the *capability*, not a persistence rewrite.
- **IN:** a **browsable, searchable history** of past executions (across incidents); a
  **single-run replay** — step through one execution's records as a time-ordered playback of
  how the team moved through the (possibly branched) run.
- **OUT (defer again):** **compare/diff two runs** side by side; **cross-run analytics /
  dashboards / reporting**; **postmortem narrative authoring** on top of the review; live
  multi-user replay. Also explicitly OUT as a *scope mandate*: **re-founding persistence on an
  event stream** — the mechanism (event sourcing vs. extend the existing append-only log) is
  the workshop/plan's decision (likely an ADR under Principle I), and the appetite is bounded
  by the capability, not by any one pattern. If replay is cheaply satisfiable on today's Step
  Records, the slice must not grow a new persistence layer to justify the word "event-sourced."

If it overruns, **cut scope, don't extend** — the cut line is the OUT list above (drop search
before replay; drop replay before shipping nothing).

## Go–No-go + date
**Go — 2026-07-01** (human reviewer), with the medium scope above and **build gated behind
05-branching's release**. A No-go would have looked like: the single-run view is enough for the
demonstration's purposes, or "replay" cannot be scoped without dragging in a full
persistence-rewrite / event-store engine. Neither held for the capability-bounded scope; the
persistence question is contained to an ADR rather than assumed into the appetite.
