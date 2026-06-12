# Discovery — 01-versioned-runbook-execution

> **DRAFT — strawman for facilitation, NOT finalized.** Status remains
> `Registered`. Framing below is a starting point to react to; the items marked
> **[PENDING author input]** are decisions only the author can make and must be
> answered before this clears to `Discovery`:
> 1. Demand — is the reconstruction pain actually felt, or assumed?
> 2. Appetite — how much for the first slice?
> 3. Constraints — team capacity, compliance/retention, tools it must sit beside.
> 4. Go / No-go verdict + date.

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
  obligations. [PENDING author input — who actually funds this]
- **Operators / constrainers:** [PENDING author input — team capacity; any
  compliance/retention requirements; existing tools (e.g. PagerDuty, incident.io,
  Git) this must coexist with]

## Evidence
Postmortem timeline reconstruction is a widely-felt pain in SRE practice;
"what did we actually do, in what order" is routinely rebuilt from chat logs.
General knowledge, **not validated demand**. [PENDING author input — direct
evidence or known users feeling this, vs. assumed]

## Cost of doing nothing
Reviews stay unreliable → the same incidents recur, blameless postmortems lack
ground truth, and audits rest on memory.

## Appetite
[PENDING author input — how much for the first slice, e.g. a couple of evenings]

## Go–No-go + date
[PENDING author verdict + date] — a no-go would look like: existing incident
tools already capture this well enough, or the reconstruction pain isn't
actually felt.
