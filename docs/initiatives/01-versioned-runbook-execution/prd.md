# PRD — 01-versioned-runbook-execution (first slice: authoring)

## Context
This is a demonstration build of the SDD method applied to a realistic problem.
Teams that respond to recurring problems rely on written procedures.
Those procedures drift: they are edited in place, and no one can later say which wording was in effect at a given time.
This initiative gives a procedure a stable shape.
An author writes a Runbook — a named, ordered procedure made of Steps — and publishes it as a Runbook Version, an immutable snapshot that never changes once published.
A later edit produces a new Runbook Version rather than altering a published one.

This PRD covers the **first slice only: authoring**.
The scope is creating a Runbook and publishing a Runbook Version.
The eventual payoff — carrying a published procedure out during a live response, and producing an after-the-fact account of what was done — is deliberately deferred to a later slice; this slice lays the authoring foundation that later work stands on.

Users are **authors**: the people who write and maintain these procedures.
Demand is an accepted assumption for the demonstration, carried with illustrative figures rather than validated evidence.
The build appetite for this slice is deliberately small — a couple of evenings — which is itself why the scope is authoring only.
This initiative touches no entries in the architecture conflict register (it is currently empty).

## Objective
Let an author create a Runbook and publish it as an immutable Runbook Version, so the organisation holds a stable, citable version of a procedure.
A Runbook is a named, ordered list of Steps.
An author can publish a Runbook Version only once the Runbook has a name and at least one Step.
Publishing freezes the current Steps into a Runbook Version whose content never changes afterwards.
Editing a published Runbook produces a new Runbook Version; earlier Runbook Versions remain available and unchanged.
An author edits the Runbook's Steps directly, and each publish captures the Runbook's current Steps as a new Runbook Version; a Runbook may exist before its first Runbook Version is published.
The most recently published Runbook Version is the current one, shown to authors by default; earlier Runbook Versions stay viewable.

## Non-goals
The Objective could tempt an eager builder into the whole product; these are fenced out of this slice:
- **Carrying out a Runbook during a live response.** Running a published procedure is out of scope.
- **Recording what happens during a response.** Capturing who did what, and when, is out of scope.
- **Any after-the-fact account or timeline of a response.** Producing a computed record of a response is out of scope.
- **Connecting to external response, paging, or alerting tools.** No integration in this slice.
- **Approval or sign-off workflow before publishing.** An author publishes a Runbook Version directly; review gates are out of scope.
- **Rich formatting, attachments, or media inside a Step.** A Step is a single plain instruction.
- **Real-time multi-author or collaborative editing.** Out of scope.
- **Comparing or diffing Runbook Versions inside the product.** A comparison or diff view is not part of this slice.
- **Deleting a Runbook or a published Runbook Version.** Published Runbook Versions are immutable and are not removed; whole Runbooks are not deletable in this slice either.
- **Organising Runbooks — search, tags, folders, or categories.** A simple list is enough for this slice; richer organisation is out of scope.
- **Importing, exporting, or cloning Runbooks.** Out of scope.
- **Authentication, accounts, or permissions.** Authoring is open in this slice — no sign-in and no access control over who can create, edit, or publish.
- **Retiring or archiving a Runbook Version.** Runbook Versions accumulate; there is no outdated or retired state in this slice.

## Metrics
Hypotheses with stated basis.
- **Time to first published version:** a new author creates and publishes a Runbook Version for a ~10-Step procedure in ≤10 minutes — basis: comparable to writing a short checklist document; treated as a hypothesis to correct, not a commitment.
- **Authoring reaches publish:** ≥80% of created Runbooks reach at least one published Runbook Version, rather than remaining unpublished — basis: if publishing feels heavy, authors leave Runbooks unpublished; assumed for the demo, not validated.
- **Version integrity:** zero changes to any Runbook Version after it is published, across the demo — basis: an unchanging published version is the core promise of this slice; treated as a property to hold and measure, not a target to approach.
- **Demo adoption:** roughly 5 Runbooks authored in the first month for a ~30-engineer organisation — basis: a team that size has a handful of common procedures; illustrative and assumed, to be corrected against real use.

## Open questions
- **Referring to a published version.** How does an author identify a specific published Runbook Version — a human-meaningful label, or a system-assigned identifier? Open; to be settled before build. [NEEDS CLARIFICATION]
- **Metrics are assumed, not validated.** The figures above remain hypotheses until the platform serves real authors.
- **Deferred capability.** Whether a published procedure must be followed in its given order when later carried out, and who concludes a response, are out of scope for this slice — recorded here so they are not lost, and intentionally not answered now.
