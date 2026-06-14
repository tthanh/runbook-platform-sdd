# Feature Specification: Runbook Execution

**Feature Branch**: `003-runbook-execution`

**Created**: 2026-06-13

**Status**: Draft

**Input**: User description: "PRD at docs/initiatives/03-runbook-execution/prd.md — execution slice: a responder runs a published Runbook Version against an external incident (pinning the current published Version), records each Step outcome as a Step Record, manually closes the Execution, and obtains a Computed Review."

**Source PRD**: [docs/initiatives/03-runbook-execution/prd.md](../../docs/initiatives/03-runbook-execution/prd.md)

Glossary terms used (binding, per the constitution): **Runbook**, **Runbook Version**, **Step**, **Execution**, **Version Pin**, **Step Record**, **Computed Review**, **Incident**.

## Clarifications

### Session 2026-06-13

- Q: When a responder starts an Execution for an Incident that already has one, what happens? → A: Resume the existing open Execution if one is open (the responder continues recording); refuse if the Incident's Execution is already closed (re-opening is a non-goal). This also serves as the way a responder returns to an in-flight Execution.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Run a published procedure and capture each Step outcome (Priority: P1)

A responder, facing a live incident, starts an Execution against a chosen Runbook. The Execution pins that Runbook's current (most recently published) Runbook Version. As the responder works, they mark each Step done, skipped, or failed — optionally with a note — and each mark is captured as a Step Record the moment it happens.

**Why this priority**: Capturing the response as it happens is the foundation of the whole slice — without the ground-truth Step Records there is nothing to compute a review from. This is the minimum viable payoff: the record is no longer reconstructed from memory.

**Independent Test**: Publish a Runbook (slice 01), start an Execution against it, mark a few Steps with mixed outcomes and a note, and confirm a Step Record exists for each mark with the right Step, outcome, note, and time.

**Acceptance Scenarios** (EARS):

1. WHEN a responder starts an Execution against a Runbook that has at least one published Runbook Version, THE SYSTEM SHALL create an open Execution that pins the Runbook's current (most recently published) Runbook Version.
2. WHEN a responder attempts to start an Execution against a Runbook with no published Runbook Version, THE SYSTEM SHALL refuse and tell the responder the Runbook must be published first.
3. WHILE an Execution is open, WHEN a responder marks a Step of the pinned Runbook Version with an outcome of done, skipped, or failed (optionally with a note), THE SYSTEM SHALL append exactly one Step Record capturing that Step, the outcome, the note, and the time.
4. WHEN a responder marks Steps in an order other than the pinned Runbook Version's order, THE SYSTEM SHALL accept the marks and capture the order in which the Step Records were recorded.
5. WHEN a responder marks a Step that already has a Step Record (for example, failed earlier and now done), THE SYSTEM SHALL append a new Step Record and treat the most recent outcome as that Step's end state.
6. WHEN a responder starts an Execution for an Incident that already has an open Execution, THE SYSTEM SHALL return that open Execution rather than create a second, so the responder continues recording against the same run.
7. WHEN a responder starts an Execution for an Incident whose Execution is already closed, THE SYSTEM SHALL refuse and tell the responder the Incident already has a completed Execution.

---

### User Story 2 - Close the Execution and obtain its Computed Review (Priority: P2)

When the response is over, a responder or the incident commander closes the Execution. From the closed Execution's Step Records, set against its pinned Runbook Version, the system produces the Computed Review — the timeline of what was done, in the order it happened, derived mechanically rather than written from memory.

**Why this priority**: This is the payoff the slice exists for — the post-incident review computed from facts. It depends on the capture in P1 existing first.

**Independent Test**: Run and record an Execution (P1), close it, and confirm the Computed Review lists the recorded outcomes in chronological order, marks untouched Steps as "not reached," and matches the Step Records exactly.

**Acceptance Scenarios** (EARS):

1. WHEN a responder or incident commander closes an open Execution, THE SYSTEM SHALL mark the Execution closed and refuse any further Step Records against it.
2. WHEN a Computed Review is produced for a closed Execution, THE SYSTEM SHALL present the recorded outcomes in the chronological order they were recorded, each with its Step, outcome, note, and time.
3. WHEN the Computed Review is produced, THE SYSTEM SHALL represent any Step of the pinned Runbook Version that has no Step Record as "not reached," distinct from a Step explicitly marked skipped.
4. WHEN the Computed Review is produced, THE SYSTEM SHALL reflect exactly the Execution's Step Records — every recorded outcome appears and nothing else — and SHALL NOT allow the review to be authored or edited by hand.
5. WHEN a responder attempts to record a Step outcome against a closed Execution, THE SYSTEM SHALL refuse the change.

---

### User Story 3 - The pinned Version holds for the life of the run (Priority: P3)

While an Execution is in flight, an author may publish a new Runbook Version of the same Runbook. The running Execution stays bound to the Version it pinned at start; it is never re-pinned, so the responder keeps working — and recording — against one stable procedure.

**Why this priority**: Version-pin integrity is what lets a Computed Review truthfully say "this is the procedure we followed." It is a guarantee layered on top of P1/P2; the everyday run does not depend on a mid-incident publish occurring.

**Independent Test**: Start an Execution pinning Version N, publish Version N+1 of the same Runbook, then confirm the open Execution still shows and records against Version N, and its Computed Review is set against Version N.

**Acceptance Scenarios** (EARS):

1. WHEN a newer Runbook Version of an Execution's Runbook is published while that Execution is open, THE SYSTEM SHALL keep the Execution pinned to its original Runbook Version and SHALL NOT re-pin it.
2. WHEN a responder records or views Steps in an Execution after a newer Runbook Version exists, THE SYSTEM SHALL present the pinned Runbook Version's Steps, not the newer Version's.
3. WHEN a Computed Review is produced for such an Execution, THE SYSTEM SHALL set it against the pinned Runbook Version.

---

### Edge Cases

- Starting an Execution against a Runbook that exists but has never been published: refused; the responder is told the Runbook must have a published Runbook Version first.
- A newer Runbook Version is published mid-Execution: the open Execution stays on its pinned Version (the responder-facing detail — whether a newer Version is surfaced at all — is deferred to the H2 ADR; see Assumptions).
- A Step marked failed and later marked done: both Step Records are kept; the Computed Review shows both in sequence and the Step's end state is done.
- A Step never acted on during the Execution: shown in the Computed Review as "not reached," never as skipped.
- An Execution closed with no Step Records, or only some Steps recorded: the Computed Review shows every unrecorded pinned-Version Step as "not reached."
- Recording a Step outcome against an already-closed Execution: refused.
- Starting an Execution for an incident that already has an **open** one: returns the existing open Execution so the responder can continue (the path back to an in-flight run).
- Starting an Execution for an incident whose Execution is already **closed**: refused; the responder is told the incident already has a completed Execution (re-opening is a non-goal).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST let a responder start an Execution from a reference to an external Incident and a chosen Runbook, pinning that Runbook's current (most recently published) Runbook Version at the moment the Execution starts.
- **FR-002**: The system MUST refuse to start an Execution against a Runbook that has no published Runbook Version.
- **FR-003**: The system MUST keep an Execution's pinned Runbook Version fixed for the entire life of the Execution, and MUST never re-pin it — even when a newer Runbook Version of the same Runbook is published while the Execution is open.
- **FR-004**: WHILE an Execution is open, the system MUST let the responder mark any Step of the pinned Runbook Version with an outcome of done, skipped, or failed, and optionally attach a note; each mark MUST append exactly one Step Record.
- **FR-005**: The system MUST treat Step Records as append-only: a recorded Step Record is never overwritten, edited, or deleted.
- **FR-006**: The system MUST allow a Step to be marked more than once within an Execution; each mark appends a new Step Record, and the most recently recorded outcome is that Step's end state.
- **FR-007**: The system MUST NOT enforce the pinned Runbook Version's Step order; the responder MAY mark Steps in any order, and the system MUST capture the order in which Step Records were recorded.
- **FR-008**: Each Step Record MUST capture which Step it refers to, when it was recorded, its outcome, and its optional note; it MUST NOT capture the actor ("who") in this slice.
- **FR-009**: The system MUST let a responder or incident commander manually close an open Execution, and MUST NOT close an Execution automatically from the external Incident.
- **FR-010**: WHEN an Execution is closed, the system MUST refuse any further Step Records against it.
- **FR-011**: The system MUST produce a Computed Review from a closed Execution that presents the recorded outcomes in the chronological order they were recorded, set against the pinned Runbook Version's Steps, and that reflects exactly the Execution's Step Records.
- **FR-012**: In the Computed Review, the system MUST represent a Step of the pinned Runbook Version that has no Step Record as "not reached," distinct from a Step explicitly marked skipped.
- **FR-013**: The system MUST NOT allow a Computed Review to be authored or edited by hand; it is derived solely from the Step Records and the pinned Runbook Version.
- **FR-014**: The system MUST hold only a reference to the external Incident and MUST NOT create, own, or manage Incident identity.
- **FR-015**: The system MUST associate at most one Execution with a given Incident reference in this slice. WHEN a responder starts an Execution for an Incident that already has an open Execution, the system MUST return that open Execution instead of creating a second; WHEN the Incident's Execution is already closed, the system MUST refuse to start a new Execution.
- **FR-016**: The system MUST treat an Execution as driven by a single person and MUST NOT provide concurrent multi-responder recording in this slice.
- **FR-017**: The system MUST NOT require sign-in, accounts, or permissions for any execution action in this slice.

### Key Entities

- **Execution**: A single run of a Runbook against one Incident. Holds a reference to the Incident, the pinned Runbook Version, an open/closed state, and an ordered, append-only set of Step Records.
- **Version Pin**: The fixed binding of an Execution to the one Runbook Version current when the Execution started; set once at start and never changed.
- **Step Record**: The append-only captured outcome of acting on a Step during an Execution — which Step, when, the outcome (done/skipped/failed), and an optional note. The actor ("who") is not captured in this slice.
- **Computed Review**: The timeline of a closed Execution, derived mechanically from its Step Records against its pinned Runbook Version; chronological, distinguishes "not reached" from skipped, and is never hand-authored or edited.
- **Incident**: The real-world event an Execution responds to; identity is owned by an external incident-management tool, and the system holds only a reference to it.
- **Runbook / Runbook Version / Step** (from slice 01): the authored procedure, its immutable published snapshot, and one ordered instruction within it; consumed here unchanged.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: After a responder closes an Execution, a reviewer obtains the Computed Review in under 5 minutes, versus roughly 45 minutes to reconstruct an incident timeline by hand today.
- **SC-002**: At least 90% of the Steps a responder acts on during an Execution have a corresponding Step Record (observational target — the system cannot count actions that were never recorded).
- **SC-003**: In 100% of closed Executions, the Computed Review matches the Execution's Step Records with zero discrepancy — every recorded outcome appears, in the order recorded, and nothing is invented.
- **SC-004**: Zero Executions end with a pinned Runbook Version different from the Version that was current when the Execution started.
- **SC-005**: At least the ~4 reviewable incidents per month (illustrative, for a ~30-engineer organisation) are run as Executions within the first month of use — a hypothesis from the PRD, to be corrected against real use.

## Assumptions

- **Incident reference shape (workshop H5)** is deferred to an ADR at `/speckit.plan`. The spec assumes the system holds at least a stable identifier that ties one Execution to one external Incident; whether it also holds a human-readable title, and how the reference is supplied, is the ADR's decision.
- **Mid-incident publish (workshop H2)**: the pin holds for the life of the Execution (FR-003). The precise responder-facing experience — whether and how the responder is told a newer Runbook Version exists — is deferred to the H2 ADR at `/speckit.plan`.
- **The actor ("who") is not captured** in this slice: no authentication exists, so recording an actor would be unverified. The Step Record glossary term was amended on 2026-06-13 to defer "who" until accounts exist.
- **Closing is manual** (a responder or commander acts); deriving close from the external Incident, and the integration that would require, are out of scope.
- **One Execution per Incident** and **single-driver recording**: multiplicity and concurrent multi-responder recording are out of scope, upholding the architecture's single-user NFR.
- **An Execution may be closed with zero or partial Step Records**; unrecorded pinned-Version Steps appear in the Computed Review as "not reached."
- Per the PRD non-goals, the following are out of scope: authoring changes; owning or managing Incidents; editing/deleting a Step Record; re-pinning, upgrading, or re-opening an Execution; running a Runbook outside an Incident (drills); browsing/searching past Executions; cross-Execution analytics; writing or editing the postmortem; comparing Executions or Reviews; notifications or integrations beyond consuming an Incident reference.
- **Persistence, concurrency, and how the Computed Review is derived and stored** are planning decisions; the constitution's no-dual-writes rule (workshop H4 spike) is handled at `/speckit.plan`, not here.
- The conflict register entries this slice is expected to touch — C-001, C-002, C-003, C-004 — are checked and resolved during `/speckit.plan`, per the constitution.
