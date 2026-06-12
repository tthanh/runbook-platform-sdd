# Feature Specification: Runbook Authoring

**Feature Branch**: `001-runbook-authoring`

**Created**: 2026-06-12

**Status**: Draft

**Input**: User description: "PRD at docs/initiatives/01-versioned-runbook-execution/prd.md — first slice, authoring only: create a Runbook and publish immutable Runbook Versions identified by sequential version numbers."

**Source PRD**: [docs/initiatives/01-versioned-runbook-execution/prd.md](../../docs/initiatives/01-versioned-runbook-execution/prd.md)

Glossary terms used (binding, per the constitution): **Runbook**, **Runbook Version**, **Step**.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create a Runbook and publish its first Runbook Version (Priority: P1)

An author creates a new Runbook, gives it a name, adds one or more Steps in order, and publishes.
Publishing freezes the current Steps into Runbook Version 1 — an immutable snapshot the organisation can cite.

**Why this priority**: This is the core promise of the slice — a stable, citable version of a procedure. Without it nothing else has value.

**Independent Test**: Can be fully tested by creating one Runbook with Steps and publishing once, then confirming Version 1 exists with exactly the published content.

**Acceptance Scenarios** (EARS):

1. WHEN an author creates a Runbook with a name, THE SYSTEM SHALL store the Runbook with that name and no published Runbook Versions.
2. WHEN an author adds a Step to a Runbook, THE SYSTEM SHALL append the Step at the end of the Runbook's ordered Step list.
3. WHEN an author publishes a Runbook that has a name and at least one Step, THE SYSTEM SHALL create Runbook Version 1 containing the Runbook's current Steps in their current order.
4. WHEN an author attempts to publish a Runbook with no Steps, THE SYSTEM SHALL refuse to publish and tell the author at least one Step is required.
5. WHEN an author attempts to create or publish a Runbook with an empty name, THE SYSTEM SHALL refuse and tell the author a name is required.

---

### User Story 2 - Edit and republish; earlier Runbook Versions stay intact (Priority: P2)

After publishing, the author edits the Runbook's Steps (add, remove, reorder, reword) and publishes again.
Each publish creates the next sequential Runbook Version; every earlier Runbook Version remains viewable and unchanged.

**Why this priority**: Immutability across edits is what distinguishes this from an editable document; it depends on P1 existing first.

**Independent Test**: Publish Version 1, change a Step, publish Version 2, then confirm Version 1 still shows its original content and Version 2 the new content.

**Acceptance Scenarios** (EARS):

1. WHEN an author edits the Steps of a Runbook that has published Runbook Versions, THE SYSTEM SHALL leave every published Runbook Version unchanged.
2. WHEN an author publishes a Runbook whose latest published Runbook Version is N, THE SYSTEM SHALL create Runbook Version N+1 containing the Runbook's current Steps.
3. WHEN any actor attempts to modify the contents of a published Runbook Version, THE SYSTEM SHALL refuse the change.
4. WHEN an author opens a Runbook with published Runbook Versions, THE SYSTEM SHALL show the most recently published Runbook Version as the current one by default.

---

### User Story 3 - Browse Runbooks and view any published Runbook Version (Priority: P3)

An author sees a list of all Runbooks, opens one, and can view its current Runbook Version or any earlier one by its version number.

**Why this priority**: Viewing makes the published versions usable and citable; it is read-only on top of P1/P2.

**Independent Test**: With two Runbooks (one having two published Runbook Versions), confirm the list shows both, and each Runbook Version is reachable and displays its number and Steps.

**Acceptance Scenarios** (EARS):

1. WHEN an author views the Runbook list, THE SYSTEM SHALL show every Runbook with its name and, where one exists, its current Runbook Version number.
2. WHEN an author selects an earlier Runbook Version by its version number, THE SYSTEM SHALL display that Runbook Version's Steps exactly as published.
3. WHEN an author opens a Runbook with no published Runbook Versions, THE SYSTEM SHALL show its editable Steps and indicate nothing is published yet.

---

### Edge Cases

- Publishing twice with no edits in between: allowed; it creates the next sequential Runbook Version with identical content (see Assumptions).
- A Step with empty text: refused on save, the author is told a Step needs content.
- A Runbook name that duplicates an existing Runbook's name: allowed; Runbooks are distinct even when names collide (see Assumptions).
- Reordering Steps before first publish: the publish captures the order as it stands at publish time.
- Viewing a version number that does not exist for that Runbook: the author is told there is no such Runbook Version.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST let an author create a Runbook with a non-empty name.
- **FR-002**: The system MUST let an author add, edit, remove, and reorder the Steps of a Runbook; each Step is a single plain-text instruction.
- **FR-003**: The system MUST refuse to publish a Runbook unless it has a non-empty name and at least one Step with non-empty content.
- **FR-004**: WHEN an author publishes a Runbook, the system SHALL create a new Runbook Version capturing the Runbook's Steps and their order at that moment.
- **FR-005**: The system MUST identify each published Runbook Version by a sequential version number per Runbook, starting at 1 and increasing by 1 with each publish.
- **FR-006**: The system MUST treat every published Runbook Version as immutable: its name-at-publish, Steps, and order can never change after publishing.
- **FR-007**: The system MUST keep every published Runbook Version available for viewing; none are deleted, retired, or archived.
- **FR-008**: The system MUST present the most recently published Runbook Version as the Runbook's current version by default, while keeping earlier Runbook Versions reachable by version number.
- **FR-009**: The system MUST list all Runbooks, including those with no published Runbook Versions.
- **FR-010**: The system MUST NOT require sign-in, accounts, or permissions for any authoring action in this slice.
- **FR-011**: The system MUST NOT allow deleting a Runbook or a published Runbook Version.

### Key Entities

- **Runbook**: A named, ordered procedure for responding to a recurring class of incident; the authored, evolving thing. Holds a name and an ordered list of Steps; has zero or more published Runbook Versions.
- **Runbook Version**: An immutable snapshot of a Runbook's Steps frozen at publish, identified by a per-Runbook sequential version number starting at 1.
- **Step**: One ordered instruction within a Runbook (and, once published, within a Runbook Version); a single plain-text instruction.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A new author can create a Runbook with ~10 Steps and publish its first Runbook Version in 10 minutes or less.
- **SC-002**: At least 80% of created Runbooks reach at least one published Runbook Version.
- **SC-003**: Zero published Runbook Versions ever change content after publishing — verified by comparing each Runbook Version's content over time.
- **SC-004**: Roughly 5 Runbooks are authored in the first month of use in a ~30-engineer organisation (illustrative hypothesis from the PRD, to be corrected against real use).
- **SC-005**: 100% of published Runbook Versions are reachable by their version number at any later time.

## Assumptions

- Publishing with no changes since the last publish still creates the next Runbook Version: the PRD sets no uniqueness rule, and refusing would add complexity no requirement has earned (constitution: complexity is earned).
- Runbook names are not required to be unique: the PRD names no uniqueness constraint; identity comes from the Runbook itself, not its name.
- A Step is plain text only — rich formatting, attachments, and media are fenced out by the PRD's non-goals.
- Single-author, open access: authentication, accounts, and permissions are PRD non-goals; concurrent multi-author editing is out of scope.
- Versions accumulate indefinitely: retiring/archiving is a PRD non-goal, so no retention or cleanup behavior exists in this slice.
- Per the PRD, executing a Runbook during a live response, recording responses, timelines, external integrations, search/tags/folders, import/export/clone, diffing versions, and approval workflows are all out of scope.
- How version numbers are assigned internally (e.g., concurrency guarantees) is a planning decision routed to an ADR (workshop hotspot H1 direction: sequential numbers).
