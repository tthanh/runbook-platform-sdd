# Workshop — 06-run-history-and-replay

Level: big-picture (language, events, contexts, hotspots).
Design-level detail belongs in /speckit.plan, where contested decisions
are promoted to ADRs. Process: see docs/workshop-guide.md.

**Workshop test result: RUN (narrow) — a hotspot-harvesting workshop, not a
language one.** The three legs of the test all come back **no**:
(1) **no new binding language** — browse/search history and single-run replay are
*read-side views* over facts already named (`Execution`, `Step Record`,
`Taken Path`, `Computed Review`, `Incident`); no new domain word carries meaning;
(2) **no new events** — the slice is read-only; nothing new *happens*, it only
surfaces the append-only records slice 003 already writes;
(3) **one context** — everything stays inside `Runbook Authoring & Execution`.

By the literal test this is a skip profile (the shape of [02](../02-rich-domain-model/workshop.md)).
It runs anyway — the reviewer's call — because *depth scales with engineering risk*
(workshop guide) and this slice carries a **paradigm-level, ADR-worthy hotspot**
(persistence mechanism) plus a **cross-slice replay boundary** (05's Decision events
and Taken Path) that are worth airing and *routing* before the PRD, so neither dies
in the plan. The output below is therefore weighted to **hotspots + boundary/scope
guards**, and proposes **no glossary amendments**.

> **Compile note.** Demonstration build, no live human board. Acting as scribe,
> this file was compiled from the finalized [discovery one-pager](discovery.md), the
> existing binding [glossary](../../../.specify/memory/constitution.md) and
> [conflict register](../../architecture.md) (C-001…C-009), and the provenance the
> discovery already cites (deferred ledger "Around execution" cluster; 03 PRD
> non-goals; incident.io / PagerDuty postmortem-timeline field analogue). **No new
> language is proposed, so there is nothing here awaiting ratification** — the file
> records reuse decisions and routed hotspots. Compiled, not invented: every event,
> reuse call, and hotspot below traces to the discovery, the glossary, or the
> conflict register.

## Attendees & date
- Date: 2026-07-01
- Facilitator: (demo) — compiled by AI scribe
- Participants: none live (demonstration); framing from [discovery.md](discovery.md)
- Reviewer: human — **ratified 2026-07-01**. Confirmed the RUN-narrow framing, the
  "no new language / no new events" scope guard, and that every hotspot has an owner
  and a destination before the PRD is written. Nothing entered the constitution (no
  proposed terms), so this ratification stands alone with no glossary commit.

## Event timeline
Past-tense events across authoring and a single (possibly branched) run. **Every
event below already exists** — none is introduced by this slice. History and replay
are *read projections* over them; the new "moments" this slice adds are **queries and
navigation**, not domain events.

1. **Runbook version published** — the authored procedure is frozen (unchanged).
2. **Execution started** — pins exactly one runbook version (unchanged, C-007).
3. **Step marked done / skipped / failed** — appends one Step Record (unchanged;
   ordered in memory by timestamp with the per-Execution `Sequence` tiebreaker, C-009).
4. **Decision resolved** — at a Decision step the responder selects an Option; the run
   routes to the target step (from slice 05; **this slice must replay it**, not create it).
5. **Execution closed** — unchanged.
6. **Review computed** — over the Taken Path (unchanged; replay presents this timeline).

New **read-side moments** (queries / navigation — *not* domain events, listed here
only to place the capability on the timeline):
- **History browsed / searched** — a reviewer lists past Executions across incidents
  and filters them. A read query; appends nothing.
- **Run replayed** — a reviewer opens one Execution and steps through its Step Records
  in Taken-Path, time order. A read/navigation projection; appends nothing.

> Scope guard (inverse of slice 04's "no new events" proof and 05's "exactly one new
> event" budget): this slice's budget is **zero new write-side events**. If replay or
> history is found to *require* writing a new record or event to work, the slice has
> drifted out of "read-only history" and must stop — that is a different initiative.

## Language decisions → constitution glossary amendments
**None proposed.** The capability is expressible entirely in existing binding terms:

| Capability word | Expressed with existing glossary terms | Glossary action |
|-----------------|----------------------------------------|-----------------|
| **Run history** | a browsable/searchable collection of **Execution**s (each an `Execution` + `Incident` reference + `Computed Review`) across incidents | **not a domain term** — a view/query surface; define it in the PRD/spec, keep it out of the binding glossary |
| **Replay** | a time-ordered presentation of an Execution's **Step Record**s along its **Taken Path**, including **Decision** resolutions | **not a domain term** — a read projection; define it in the PRD/spec, keep it out of the binding glossary |

Rationale: a term enters the binding glossary only when a workshop ratifies it **and a
slice needs the shared meaning** (Principle III). Both words name *how a reviewer looks
at existing facts*, not a new fact — so they stay capability words in the PRD, not
binding vocabulary. If the plan discovers a genuinely new concept (e.g. a distinct read
model that needs one meaning across code and specs), that is a **new hotspot for a later
amendment**, not a silent add here.

## Hotspots
Each becomes a spike or an ADR at /speckit.plan — none die in notes.

| # | Hotspot | Why it's contested | Destination | Owner |
|---|---------|--------------------|-------------|-------|
| H1 | **Persistence mechanism for history/replay** — re-found Execution persistence on an **event stream / event store**, or **read the existing append-only Step Record log** as the replay source? | The discovery's central open question and a Principle I (complexity earned) call. Appetite explicitly does **not** mandate an event-store rewrite; if replay is cheap on today's Step Records + `Computed Review`, a new persistence layer is unearned complexity. But if branched replay needs more, the trade-off must be recorded. | **ADR** | planner |
| H2 | **Replay over a branched run (05 dependency)** — how the playback represents the **Taken Path**, **Decision** resolutions, and untaken branches | Build is gated behind 05's release; replay must show *which Option was chosen at each Decision* and mark untaken-path steps as **NotReached** (reuse the existing state), not fabricate them. Wrong here and replay misrepresents what the team did. | **ADR or note** | planner |
| H3 | **Query / search surface** — which fields are searchable (runbook, `IncidentId` reference, date/closed range, outcome/coverage), and how they're indexed within C-002 | Bounds the list surface to the *medium* appetite; unbounded search is the scope-creep risk toward cross-run analytics (explicitly OUT). Must respect C-008 (Incident is a foreign reference, not a joinable local entity). | **ADR or spike** | planner |
| H4 | **Deterministic replay ordering** — the time-ordered playback must be reproducible given Step Records with a `DateTimeOffset` and no actor | Directly on C-009: ordering is in memory with the per-Execution `Sequence` tiebreaker; SQLite `ORDER BY DateTimeOffset` is unreliable. Replay ordering must reuse this rule, not re-derive it. Step Record has no "who" (deferred), so replay shows *what/when*, not *who*. | **Note / spike** | planner |
| H5 | **Where the read surface stops** — replay + history UI within C-003 hash routing, **no new router/graph/charting dependency**, and the discovery cut-line (drop search before replay; drop replay before shipping nothing) | The appetite is a capability, not a product; without a stated stop line this grows a timeline-visualization / dashboard surface that is OUT. | **Spike / note** | planner |
| H6 | **History across incidents when Incident is external** — grouping/labelling runs by incident while we hold only an `IncidentId` reference (C-008, ADR-0005) | History spans incidents, but Incident identity is owned upstream; the slice can display the reference and Execution metadata but must not model Incident locally or imply it owns incident data. | **Note** | planner |

## Boundary notes
- **Bounded context unchanged: `Runbook Authoring & Execution`.** No new external
  context; history/replay are read projections within the existing one.
- **Read-only slice.** Zero new write-side events, zero new mutations. The existing
  **append-only** and **no-deletion** NFRs are the *guarantee history relies on* — this
  slice consumes them, it does not relax or extend them.
- **Reuse, don't invent, execution state.** `NotReached` vs `Skipped` already exist
  (slice 003, redefined by 05 for untaken paths). Replay presents them; it adds none.
- **Reuse C-009 ordering.** Replay's time order is the in-memory `DateTimeOffset` +
  `Sequence` rule already in force — not a new sort path.
- **Incident stays a foreign reference** (C-008 / ADR-0005). History displays the
  reference; it does not model or own Incident data.
- **Build gated behind 05-branching's release** (discovery). Replay is designed
  against the branched run shape 05 introduces, so it is not built against a run
  shape that is about to change.
- **Still OUT (deferred again, per discovery):** compare/diff two runs; cross-run
  analytics / dashboards / reporting; postmortem narrative authoring; live
  multi-user replay; and — as a *scope mandate* — re-founding persistence on an event
  store (that is H1's trade-off to record, not a foregone conclusion).

## Open questions carried into the PRD
- **Persistence source of truth for replay** — event stream or the existing append-only
  Step Record log? (H1 — the paradigm decision; likely an ADR under Principle I.)
- **How does replay represent a branched run** — Decision resolutions shown, untaken
  steps as `NotReached`? (H2 — depends on 05 shipping.)
- **What is searchable in history, and how is it bounded** to stay short of analytics?
  (H3 — respects C-002 indexing and C-008 foreign reference.)
- **How is replay ordering made deterministic** on today's Step Records? (H4 — reuse
  C-009; note the missing "who".)
- **Where does the read UI stop** inside C-003 hash routing and no new deps, and what
  is dropped first if the medium appetite overruns? (H5 — the cut line.)
- **How is cross-incident history presented** while Incident is external? (H6.)
