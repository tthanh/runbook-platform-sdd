# Workshop — 01-versioned-runbook-execution

Level: big-picture (language, events, contexts, hotspots).
Design-level detail belongs in /speckit.plan, where contested decisions
are promoted to ADRs.
Process: see docs/workshop-guide.md (human-only operating guide).

**Workshop test result: RUN (not skipped).** The glossary is empty and this
initiative introduces new language (*runbook*, *version*, *execution*, *step
record*, *computed review*), new events, and touches a boundary with an external
incident-management tool. Every leg of the test is a "no" → a workshop is
required, sized to seeding the foundational language.

> **Compile note.** This is a demonstration build with no live human board.
> Acting as scribe, this file is compiled from the finalized discovery one-pager
> and the product definition in the register — not from a real Example Mapping
> session. Treat the language section as a *proposed* first glossary for human
> ratification. **The constitution glossary stays empty for now — no amendment
> in this commit.** Per the workshop guide, terms only land in the constitution
> after a real workshop is run and a reviewer checks fidelity (within 24h),
> committed alongside that ratified workshop. Any term can be reopened as a
> future hotspot.

## Attendees & date
- Date: 2026-06-12
- Facilitator: (demo) — compiled by AI scribe
- Participants: none live (demonstration); framing sourced from
  [discovery.md](discovery.md)
- Reviewer: **[PENDING human ratification]** — fresh eyes confirm the glossary
  before /speckit.specify

## Event timeline
Past-tense events, in the order they occur across authoring and a single run:

1. **Runbook drafted** — an author writes/edits an ordered procedure.
2. **Runbook version published** — the draft is frozen into an immutable version.
3. **Incident declared** — *external* (incident.io-style tool); the trigger to respond.
4. **Execution started** — a run begins and **pins** exactly one runbook version.
5. **Step marked done / skipped / failed** — each appends one step record.
6. **Execution closed** — the run ends; no further step records.
7. **Review computed** — the timeline is derived from step records + pinned version.

## Language decisions → constitution glossary amendments
New or changed terms land in the constitution glossary, never ad hoc. The
following are **proposed** for the (currently empty) binding glossary. They are
**not yet amended into the constitution** — the glossary stays empty until a real
workshop is run and the reviewer ratifies these terms; only then do they land in
`.specify/memory/constitution.md`, committed with that ratified workshop:

| Term | One meaning |
|------|-------------|
| **Runbook** | A named, ordered procedure for responding to a recurring class of incident. The authored, evolving thing — not a single run of it. |
| **Runbook Version** | An immutable snapshot of a runbook's steps, frozen and identified at publish. Editing a published runbook produces a *new* version; a published version's content never changes. |
| **Step** | One ordered instruction within a runbook version. |
| **Execution** | A single run of a runbook against one incident. It pins exactly one runbook version when it starts and never re-pins. |
| **Version Pin** | The fixed binding of an execution to the one runbook version in effect when the execution started. |
| **Step Record** | The append-only captured outcome of acting on a step during an execution: which step, who, when, outcome (done/skipped/failed), optional note. The ground-truth fact of what was done. |
| **Computed Review** | The post-incident timeline derived mechanically from an execution's step records against its pinned version — computed, never authored from memory. |
| **Incident** | The real-world event an execution responds to. *Boundary term:* identity is owned by an external incident-management tool; we hold a reference, not the source of truth. |

## Hotspots
Each becomes a spike or an ADR — none die in notes.

| # | Hotspot | Why it's contested | Destination | Owner |
|---|---------|--------------------|-------------|-------|
| H1 | **Version identity scheme** — monotonic integer vs content hash vs semver | Determines how a pin is stored and how "which version was in effect" is proven | ADR (in /speckit.plan) | planner |
| H2 | **Mid-incident publish** — a new version published while an execution is in flight | Working assumption: the pin holds for the life of the execution; confirm the rule and what the responder sees | ADR | planner |
| H3 | **Step-ordering discipline** — must an execution follow the version's order, or may steps be done/skipped out of order? | Bounds what a Computed Review can truthfully claim | ADR | planner |
| H4 | **Step-record persistence vs no-dual-writes** — records + any review projection | Constitution forbids dual-writes; cross-store propagation must ride the outbox | Spike (in /speckit.plan) | planner |
| H5 | **Incident identity ownership** — own an Incident entity, or only a foreign reference to the external tool? | Sets the context boundary; affects every event that names an incident | ADR + boundary note | planner |

## Boundary notes
- **One bounded context for the first slice: `Runbook Execution`.** Authoring
  (runbooks, versions, steps) and running (executions, step records, computed
  review) live together for now. Flag for a split if authoring grows its own
  lifecycle.
- **Upstream context: incident management** (incident.io-style). It owns
  *Incident* identity; this platform consumes a reference and sits beside it,
  per the discovery constraint. Resolved by H5.

## Open questions carried into the PRD
- Metrics basis: carry the discovery's illustrative figures (~4 reviewable
  incidents/month, ~45 min/timeline reconstructed) into PRD hypotheses with
  their "assumed, not validated" caveat.
- How thin can runbook authoring be and still publish a version, given the
  "couple of evenings" appetite?
- Does the first slice *enforce* step order, or only *capture* it? (Depends on H3.)
- Who closes an execution and triggers the review compute — manual action or
  derived from the external incident closing?
