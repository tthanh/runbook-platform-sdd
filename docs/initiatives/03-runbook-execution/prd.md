# PRD — 03-runbook-execution (execution slice)

## Context
When an incident happens, the account of the response — what was done, in what order, by whom — is reconstructed afterward from memory, chat scrollback, and scattered notes.
That reconstruction is slow, lossy, and unreliable: responders misremember the sequence, omit steps, and cannot say which version of the procedure was actually in effect.

Slice 01 shipped the foundation: an author writes a Runbook and publishes it as an immutable Runbook Version.
This slice delivers the payoff that foundation was built for.
A responder runs a published procedure during a live incident and the record is captured as it happens, so the post-incident review is **computed from what was recorded, not reconstructed from memory**.

A single run is an **Execution**.
When an Execution starts it binds to exactly one published Runbook Version — the **Version Pin** — and that binding never changes for the life of the run.
As the responder works through the procedure, each Step they act on appends a **Step Record**: which Step, when, the outcome (done, skipped, or failed), and an optional note. (Capturing *who* acted is deferred — see Non-goals — because no slice has accounts yet.)
When the run ends, a responder or commander closes the Execution and it accepts no further Step Records.
From a closed Execution's Step Records, set against its pinned Version, the system produces a **Computed Review** — the timeline of the response, derived mechanically rather than written from recollection.

The users are the people in the incident: the on-call responders working the procedure under pressure, the incident commander, and the reviewer who later writes the postmortem.
The incident itself is owned by an external incident-management tool (incident.io-style); this platform holds a reference to that incident and sits beside the tool rather than replacing it.
Demand is an accepted assumption carried with illustrative figures rather than validated evidence.
The build appetite is **a couple of evenings**, matching slice 01; if the work overruns, scope is cut rather than extended.

This slice introduces the five execution terms (Execution, Version Pin, Step Record, Computed Review, Incident) that initiative 01's workshop proposed and deferred.
They were ratified into the binding glossary on 2026-06-13; Step Record's "who" is deferred until accounts exist.
The architecture conflict register is expected to be touched — C-001 (a run binds to a specific published version), C-002 (this slice introduces concepts that did not exist before), C-003 (responder views), and C-004 (the new run-time rules) — but which entries bind, and how, is determined at plan time, not here.

## Objective
Let a responder run a published Runbook Version against an incident and obtain a computed account of what was done that reflects exactly the recorded outcomes.

An Execution starts from a reference to an external incident and a chosen Runbook, and pins that Runbook's current (most recently published) Version at the moment it starts — consistent with slice 01, where the current Version is the one shown by default.
The pin holds for the entire life of the Execution: if a newer Runbook Version is published while the run is in flight, the running Execution keeps showing and recording against its pinned Version and is never re-pinned.

While an Execution is open, the responder marks each Step with an outcome — done, skipped, or failed — and may attach a note.
Each mark appends one Step Record and is never overwritten; the Step Records are the append-only ground truth of the run.
A Step may be marked more than once — for example failed, then done after a retry — and each mark appends a new Step Record; the most recent mark is that Step's end state.
The responder may act on Steps in the order the incident demands rather than being forced to follow the Version's order; the record captures the true order in which Steps were acted on.

A responder or incident commander explicitly closes the Execution when the response is over; closing is a manual action in this slice.
A closed Execution accepts no further Step Records.

A Computed Review is derived from a closed Execution: it lays out, in the chronological order the outcomes were recorded, what was done against the pinned Version's Steps — which were done, skipped, or failed, each with its note and when it happened. A Step of the pinned Version that has no Step Record appears as **not reached** — distinct from a Step explicitly marked skipped. A Step marked more than once contributes one entry per mark, in sequence, and its end state is the most recent outcome.
The Computed Review reflects exactly the Step Records it derives from; it is never authored or edited by hand.

## Non-goals
The Objective could tempt an eager builder well past this slice; these are fenced out:
- **Changing how runbooks are authored.** Creating, editing, and publishing Runbooks and Runbook Versions is slice 01 and is unchanged; this slice adds no authoring capability.
- **Owning incidents.** The platform holds a reference to an incident from an external tool; it does not create, manage, list, or become the source of truth for incidents, and it does no paging, alerting, or on-call scheduling.
- **Correcting or deleting a Step Record.** Step Records are append-only; there is no edit, undo, or delete of a recorded outcome in this slice.
- **Re-pinning or upgrading an Execution.** A running or closed Execution is never moved to a different or newer Runbook Version.
- **Re-opening a closed Execution.** Once closed, an Execution takes no further Step Records.
- **Running a Runbook outside an incident.** An Execution always references an incident; practice runs, drills, or rehearsals with no incident are out of scope.
- **More than one Execution per incident.** An incident reference maps to a single Execution; running several Runbooks for one incident, or restarting a run, is out of scope. (Executions for different incidents are independent and unaffected.)
- **Deriving Execution close from the external incident.** Closing is a manual action by a responder or commander this slice; closing an Execution automatically when its external incident closes (and the integration that would require) is deferred to a later slice.
- **Capturing who performed a Step.** With no authentication in any slice, the actor is not recorded; the Step Record's "who" is deferred until accounts exist (binding glossary, amended 2026-06-13).
- **Writing the postmortem.** The Computed Review is derived; authoring narrative analysis, root-cause, or action items on top of it is out of scope.
- **Editing the Computed Review.** It is computed from Step Records and cannot be hand-edited.
- **Assigning Steps to people, reminders, timers, or SLA tracking.** Out of scope.
- **Multiple people recording to one Execution.** An Execution is driven by a single person this slice, upholding the architecture's single-user NFR (no concurrent-write guarantees); concurrent multi-responder recording is out of scope.
- **Real-time multi-responder collaboration or live presence.** Out of scope.
- **Authentication, accounts, or permissions.** Running and recording are open, consistent with the prior slices.
- **Cross-Execution analytics or dashboards.** Reporting across many runs is out of scope.
- **Comparing or diffing Executions or Reviews.** Out of scope.
- **Notifications or integrations** beyond consuming a reference to an external incident.

## Metrics
Hypotheses with stated basis; the figures are illustrative, carried from discovery for a ~30-engineer SRE org.
- **Review preparation time:** producing the account of a response drops from ~45 minutes of manual reconstruction to ≤5 minutes to obtain a Computed Review — basis: the review is derived from records captured during the run rather than rebuilt from memory; treated as a hypothesis to correct, not a commitment.
- **Capture completeness:** ≥90% of the Steps acted on during an Execution carry a Step Record, versus lossy after-the-fact recall — basis: capturing in the moment beats reconstruction; assumed, not validated. Observational only: the system cannot count Steps acted on but never recorded, so this is gauged from responder feedback, not computed by the product.
- **Review fidelity:** zero discrepancy between a Computed Review and the Step Records it derives from — basis: the review is mechanical, so this is a property to hold and measure, not a target to approach.
- **Pin integrity:** zero Executions whose pinned Runbook Version changed after the run started — basis: the unchanging pin is the core promise of this slice; a property to hold, not approach.
- **Adoption:** at least the ~4 reviewable incidents/month are run as Executions within the first month of use — basis: the illustrative discovery figure; assumed, to be corrected against real use.

## Settled in clarification (2026-06-13)
These were open at draft and are now decided; recorded here so the spec inherits them, not the questions.
- **Closing an Execution is a manual action** by a responder or incident commander, and it triggers the review compute. Incident-driven close is deferred (Non-goals).
- **Step order is captured, not enforced.** Responders act in any order the incident demands; the record captures the true order. The plan-time ADR for hotspot H3 formalizes this and records the alternative (enforce order).
- **"Who" is not captured this slice.** No accounts exist, so the actor would be unverified; the Step Record's "who" is deferred until authentication exists (binding glossary amended 2026-06-13).
- **Appetite is a couple of evenings**, matching slice 01; cut scope rather than extend.

Settled in the PRD review pass (2026-06-13), closing ambiguities an agent would otherwise invent:
- **An Execution pins the chosen Runbook's current (most recently published) Version** at start — consistent with slice 01's "current version" default.
- **The Computed Review is ordered chronologically** by when outcomes were recorded; a pinned-Version Step with no Step Record is shown as **not reached**, distinct from explicitly **skipped**.
- **A Step may be re-marked**; each mark appends a Step Record, the Review shows the sequence, and the most recent mark is the Step's end state.
- **One person drives an Execution** this slice (upholds the single-user NFR); **one Execution per incident** (multiplicity fenced in Non-goals).

## Open questions
These stay open by design — they are contested design decisions routed to ADRs at /speckit.plan, not PRD-altitude calls.
- **Mid-incident publish (H2).** Working assumption: the pin holds for the life of the Execution and the responder keeps seeing the pinned Version, with no in-flight upgrade prompt. The plan-time ADR confirms the rule and what the responder sees.
- **Incident reference shape (H5).** The platform holds a reference to an external incident rather than owning incident identity. [NEEDS CLARIFICATION: what minimal reference is meaningful for the responder and the review — an identifier alone, or also a human-readable title? — resolved by the H5 ADR/boundary note at plan time.]
- **Metrics are assumed, not validated.** The figures above remain hypotheses until the platform serves real responders.
