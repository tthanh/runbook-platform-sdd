# Workshop — 03-runbook-execution

Inherited workshop: the big-picture work for this slice was already done in
[01's workshop](../01-versioned-runbook-execution/workshop.md), which deliberately
scoped execution *out* of the first PRD and parked its language, events, and
hotspots as **proposed, awaiting a later ratified workshop — this one**.

No fresh Example Mapping is re-run; this file ratifies the deferred half of 01.

## Language to ratify (proposed in 01, enter the constitution glossary with this slice)

| Term | One meaning |
|------|-------------|
| **Execution** | A single run of a runbook against one incident. It pins exactly one runbook version when it starts and never re-pins. |
| **Version Pin** | The fixed binding of an execution to the one runbook version in effect when the execution started. |
| **Step Record** | The append-only captured outcome of acting on a step during an execution: which step, who, when, outcome (done/skipped/failed), optional note. The ground-truth fact of what was done. |
| **Computed Review** | The post-incident timeline derived mechanically from an execution's step records against its pinned version — computed, never authored from memory. |
| **Incident** | The real-world event an execution responds to. *Boundary term:* identity is owned by an external incident-management tool; we hold a reference, not the source of truth. |

> **Human gate (not done by the PRD author):** these five terms enter the binding
> glossary in `.specify/memory/constitution.md` only on a human-ratified amendment,
> per the constitution's workshop rule. Until then they are this slice's working
> vocabulary, used by the PRD and pending ratification.

## Events delivered by this slice (3–7 from 01's timeline)

3. **Incident declared** — *external* trigger; the platform consumes a reference.
4. **Execution started** — a run begins and pins exactly one runbook version.
5. **Step marked done / skipped / failed** — each appends one step record.
6. **Execution closed** — the run ends; no further step records.
7. **Review computed** — the timeline is derived from step records + pinned version.

## Hotspots carried into this slice (→ ADRs at /speckit.plan)

| # | Hotspot | Destination |
|---|---------|-------------|
| H2 | Mid-incident publish — a new version published while an execution is in flight; working assumption is the pin holds for the life of the execution | ADR |
| H3 | Step-ordering discipline — must an execution follow the version's order, or may steps be done/skipped out of order? | ADR |
| H4 | Step-record persistence vs the no-dual-writes rule (records + any review projection ride the outbox) | Spike |
| H5 | Incident identity ownership — own an Incident entity, or hold only a foreign reference to the external tool? | ADR + boundary note |

H1 (version identity scheme) was resolved with slice 01 — sequential version numbers (ADR-0001).

> **Human gate:** ADRs for H2, H3, H5 must be drafted Proposed during /speckit.plan
> and Accepted by human commit before tasks run; H4 is a plan-time spike.

## Boundary note

Bounded context: **Runbook Execution** — authoring and running live together for now.
Upstream context: **incident management** (incident.io-style), which owns *Incident*
identity; this platform consumes a reference and sits beside it (resolved by H5).
