# PRD — 03-runbook-execution (execution slice)

## Context
When an incident happens, the account of the response — what was done, in what order, by whom — is reconstructed afterward from memory, chat scrollback, and scattered notes.
That reconstruction is slow, lossy, and unreliable: responders misremember the sequence, omit steps, and cannot say which version of the procedure was actually in effect.

Slice 01 shipped the foundation: an author writes a Runbook and publishes it as an immutable Runbook Version.
This slice delivers the payoff that foundation was built for.
A responder runs a published procedure during a live incident and the record is captured as it happens, so the post-incident review is **computed from what was recorded, not reconstructed from memory**.

A single run is an **Execution**.
When an Execution starts it binds to exactly one published Runbook Version — the **Version Pin** — and that binding never changes for the life of the run.
As the responder works through the procedure, each Step they act on appends a **Step Record**: which Step, who, when, the outcome (done, skipped, or failed), and an optional note.
When the run ends the Execution is closed and accepts no further Step Records.
From a closed Execution's Step Records, set against its pinned Version, the system produces a **Computed Review** — the timeline of the response, derived mechanically rather than written from recollection.

The users are the people in the incident: the on-call responders working the procedure under pressure, the incident commander, and the reviewer who later writes the postmortem.
The incident itself is owned by an external incident-management tool (incident.io-style); this platform holds a reference to that incident and sits beside the tool rather than replacing it.
Demand is an accepted assumption carried with illustrative figures rather than validated evidence.

This slice introduces the five execution terms (Execution, Version Pin, Step Record, Computed Review, Incident) that initiative 01's workshop proposed and deferred.
They become this slice's binding vocabulary only on a human-ratified glossary amendment.
The architecture conflict register is expected to be touched — C-001 (a run binds to a specific published version), C-002 (this slice introduces concepts that did not exist before), C-003 (responder views), and C-004 (the new run-time rules) — but which entries bind, and how, is determined at plan time, not here.

## Objective
Let a responder run a published Runbook Version against an incident and obtain a faithful, computed account of what was done.

An Execution starts from a reference to an external incident and pins exactly one published Runbook Version at the moment it starts.
The pin holds for the entire life of the Execution: if a newer Runbook Version is published while the run is in flight, the running Execution keeps showing and recording against its pinned Version and is never re-pinned.

While an Execution is open, the responder marks each Step with an outcome — done, skipped, or failed — and may attach a note.
Each mark appends one Step Record and is never overwritten; the Step Records are the append-only ground truth of the run.
The responder may act on Steps in the order the incident demands rather than being forced to follow the Version's order; the record captures the true order in which Steps were acted on.

An Execution is explicitly closed when the response is over.
A closed Execution accepts no further Step Records.

A Computed Review is derived from a closed Execution: it lays out, in order, what was done against the pinned Version's Steps — which were done, skipped, or failed, with their notes, who, and when.
The Computed Review reflects exactly the Step Records it derives from; it is never authored or edited by hand.

## Non-goals
The Objective could tempt an eager builder well past this slice; these are fenced out:
- **Changing how runbooks are authored.** Creating, editing, and publishing Runbooks and Runbook Versions is slice 01 and is unchanged; this slice adds no authoring capability.
- **Owning incidents.** The platform holds a reference to an incident from an external tool; it does not create, manage, list, or become the source of truth for incidents, and it does no paging, alerting, or on-call scheduling.
- **Correcting or deleting a Step Record.** Step Records are append-only; there is no edit, undo, or delete of a recorded outcome in this slice.
- **Re-pinning or upgrading an Execution.** A running or closed Execution is never moved to a different or newer Runbook Version.
- **Re-opening a closed Execution.** Once closed, an Execution takes no further Step Records.
- **Writing the postmortem.** The Computed Review is derived; authoring narrative analysis, root-cause, or action items on top of it is out of scope.
- **Editing the Computed Review.** It is computed from Step Records and cannot be hand-edited.
- **Assigning Steps to people, reminders, timers, or SLA tracking.** Out of scope.
- **Real-time multi-responder collaboration or live presence.** Out of scope.
- **Authentication, accounts, or permissions.** Running and recording are open, consistent with the prior slices.
- **Cross-Execution analytics or dashboards.** Reporting across many runs is out of scope.
- **Comparing or diffing Executions or Reviews.** Out of scope.
- **Notifications or integrations** beyond consuming a reference to an external incident.

## Metrics
Hypotheses with stated basis; the figures are illustrative, carried from discovery for a ~30-engineer SRE org.
- **Review preparation time:** producing the account of a response drops from ~45 minutes of manual reconstruction to ≤5 minutes to obtain a Computed Review — basis: the review is derived from records captured during the run rather than rebuilt from memory; treated as a hypothesis to correct, not a commitment.
- **Capture completeness:** ≥90% of the Steps acted on during an Execution carry a Step Record, versus lossy after-the-fact recall — basis: capturing in the moment beats reconstruction; assumed, not validated.
- **Review fidelity:** zero discrepancy between a Computed Review and the Step Records it derives from — basis: the review is mechanical, so this is a property to hold and measure, not a target to approach.
- **Pin integrity:** zero Executions whose pinned Runbook Version changed after the run started — basis: the unchanging pin is the core promise of this slice; a property to hold, not approach.
- **Adoption:** at least the ~4 reviewable incidents/month are run as Executions within the first month of use — basis: the illustrative discovery figure; assumed, to be corrected against real use.

## Open questions
- **Who closes an Execution and triggers the review compute** — a manual action by a responder or commander, or derived from the external incident being closed? [NEEDS CLARIFICATION: this is the carried-forward workshop open question and is not answered here.]
- **Enforce order, or only capture it?** The working stance is that responders act in any order and the record captures the true order (incidents are messy). [NEEDS CLARIFICATION: confirm the product does not block out-of-order Steps — this is hotspot H3, destined for an ADR at plan time.]
- **Mid-incident publish (H2).** The working assumption is that the pin holds for the life of the Execution and the responder keeps seeing the pinned Version. [NEEDS CLARIFICATION: confirm this is the desired responder experience and that no in-flight upgrade prompt is wanted.]
- **Incident reference shape (H5).** The platform holds a reference to an external incident rather than owning incident identity. [NEEDS CLARIFICATION: what minimal reference is needed for the responder and the review to be meaningful — an identifier alone, or also a human-readable title?]
- **Responder identity in a Step Record.** A Step Record names "who," but the prior slices have no authentication. [NEEDS CLARIFICATION: is the responder a free-text name entered at run time, a value carried from the external incident tool, or omitted in this slice?]
- **Appetite for this slice is not yet set.** Initiative 01's "couple of evenings" sized authoring only; execution is larger. [NEEDS CLARIFICATION: a scope ceiling for this slice before /speckit.plan, so scope is cut rather than extended if it overruns.]
- **Metrics are assumed, not validated.** The figures above remain hypotheses until the platform serves real responders.
