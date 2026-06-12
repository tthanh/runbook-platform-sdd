# Discovery — 01-versioned-runbook-execution

> **FINALIZED — 2026-06-12.** Author inputs resolved; verdict **Go**. Register
> moves `Registered → Discovery`. This is a **demonstration platform**, so demand
> is treated as a stated assumption with illustrative numbers rather than
> validated field evidence — see Evidence below.

Discovery test: this is a **new problem** → full one-pager (not an inherited
consequence).

## Problem
When an incident happens, the post-incident review — what happened, in what
order, and what was done — is **reconstructed afterward** from memory, chat
scrollback, and scattered notes. It is slow, lossy, and unreliable: responders
misremember the sequence, omit steps, and cannot say which version of the
procedure was actually in effect. The record of the response is assembled after
the fact rather than captured as it happened. (Solution unwelded: this states
the pain, not the versioned-runbook answer.)

## Whose problem
- **Users:** on-call / incident responders running procedures under pressure;
  the incident commander; the reviewer who writes the postmortem.
- **Payer:** the engineering / reliability org carrying downtime cost and audit
  obligations. (Assumed for the demo: a mid-size SRE org of ~30 engineers.)
- **Operators / constrainers:** the first slice is modeled on the **core
  incident.io-style use case** — a "page" / runbook that responders work through
  during an incident. It should sit alongside such tools rather than replace
  them; no hard compliance/retention or team-capacity constraints bind the first
  slice (demonstration build).

## Evidence
Postmortem timeline reconstruction is a widely-felt pain in SRE practice;
"what did we actually do, in what order" is routinely rebuilt from chat logs.
**Assumed demand, not validated** — this is a demonstration platform, so we
proceed on the hypothesis with illustrative figures rather than field evidence.
Working numbers for the demo: ~30-engineer SRE org, ~4 reviewable incidents/month,
~45 min spent reconstructing each timeline. These are stated assumptions to be
revisited if the platform ever serves real users.

## Cost of doing nothing
Reviews stay unreliable → the same incidents recur, blameless postmortems lack
ground truth, and audits rest on memory.

## Appetite
**A couple of evenings.** Following the workshop, the first slice is narrowed to
**authoring only — create a runbook and publish a version.** Executing a runbook
(pinning a version, recording steps, computing the review) is deferred to a later
slice; it is the eventual payoff, but the first PRD only lays the authoring
foundation. If it overruns this, cut scope, don't extend.

## Go–No-go + date
**Go — 2026-06-12.** Built as a demonstration of the SDD method on a realistic
problem; demand is an accepted assumption. A no-go would have looked like:
existing incident tools already capture this well enough, or the pain wasn't
worth modeling — neither blocks a demo build.
