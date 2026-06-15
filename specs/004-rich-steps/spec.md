# Feature Specification: Rich Steps

**Feature Branch**: `004-rich-steps`

**Created**: 2026-06-15

**Status**: Draft

**Input**: User description: "PRD at docs/initiatives/04-rich-steps/prd.md — enrich a Step beyond a single title with optional structured detail (instructions as lightweight markdown, a command, an expected result) and a Step Type (Action or Check), frozen into the Runbook Version at publish and shown while running an Execution and in the Computed Review. Branching deferred."

**Source PRD**: [docs/initiatives/04-rich-steps/prd.md](../../docs/initiatives/04-rich-steps/prd.md)

Glossary terms used (binding, per the constitution): **Runbook**, **Runbook Version**, **Step**, **Step Type**, **Execution**, **Step Record**, **Computed Review**.

> Depends on the constitution glossary amendment (redefine **Step**, add **Step Type**) drafted from the initiative-04 workshop; this spec uses those terms as binding.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Author a Step with real detail and freeze it at publish (Priority: P1)

An author writing a Runbook gives each Step more than a title: instructions for what to do (written in lightweight markdown), the exact command to run, the expected result that confirms it worked, and a Step Type that says whether the Step is an Action or a Check. When the Runbook is published, all of that detail is frozen into the Runbook Version exactly as the title already is.

**Why this priority**: This is the core of the slice — without an author being able to record real detail and have it frozen immutably, there is nothing richer to execute or review. It is the minimum viable payoff: a published procedure that says how, not just what.

**Independent Test**: Author a Step with instructions, a command, an expected result, and a Step Type; publish the Runbook; reopen the published Runbook Version and confirm every field is present. Then edit the draft, publish again, and confirm the first Runbook Version's detail is unchanged.

**Acceptance Scenarios** (EARS):

1. WHEN an author edits a Step, THE SYSTEM SHALL let them add or change an instructions body (lightweight markdown), a command, and an expected result, in addition to the required title.
2. WHEN an author assigns a Step a Step Type, THE SYSTEM SHALL accept only Action or Check and SHALL reject any other value.
3. WHEN an author saves a Step that has a title but no instructions, command, or expected result, THE SYSTEM SHALL treat the Step as valid.
4. WHEN a Runbook is published, THE SYSTEM SHALL freeze each Step's title, instructions, command, expected result, and Step Type into the resulting Runbook Version.
5. WHEN an author edits a Runbook's Steps after a Runbook Version has been published and publishes again, THE SYSTEM SHALL leave the earlier Runbook Version's Step detail unchanged and SHALL capture the edits only in the new Runbook Version.
6. WHEN any caller attempts to modify the Step detail of a published Runbook Version, THE SYSTEM SHALL provide no path to do so.

---

### User Story 2 - Read a Step's detail while running an Execution (Priority: P2)

A responder running an Execution against a pinned Runbook Version sees each Step's detail — the instructions rendered, the command, and the expected result — in front of them as they work, before they mark the Step done, skipped, or failed.

**Why this priority**: The detail only delivers value if the responder can read it at the moment they act; this is the payoff that lowers time-to-act and improvisation. It depends on P1 having frozen the detail into the Runbook Version.

**Independent Test**: Start an Execution against a published Runbook Version whose Steps carry detail, open a Step, and confirm the rendered instructions, command, and expected result are shown before recording an outcome; the marking of outcomes behaves exactly as in the prior slice.

**Acceptance Scenarios** (EARS):

1. WHILE an Execution is open, WHEN a responder views a Step of the pinned Runbook Version, THE SYSTEM SHALL display that Step's instructions (with markdown rendered), command, and expected result before the responder records an outcome.
2. WHEN a responder views a Step that carries no detail beyond its title, THE SYSTEM SHALL display the Step by its title alone without error.
3. WHEN a responder records a Step outcome, THE SYSTEM SHALL capture the Step Record exactly as in the prior slice, unaffected by the Step's added detail.

---

### User Story 3 - See a Step's detail when a published Version and its Computed Review are read (Priority: P3)

The detail appears wherever a published Runbook Version is read: when an author or reviewer views the Version, and in the Computed Review of a closed Execution, so the record shows not just that a Step was marked but what doing it meant.

**Why this priority**: Surfacing detail in the read views makes the published procedure and the post-incident review legible. It is layered on top of authoring (P1) and execution (P2); the everyday author-and-run loop works without it.

**Independent Test**: View a published Runbook Version and confirm each Step's detail is shown; run and close an Execution, produce its Computed Review, and confirm each pinned-Version Step's detail is shown alongside its recorded outcome.

**Acceptance Scenarios** (EARS):

1. WHEN a published Runbook Version is viewed, THE SYSTEM SHALL display each Step's instructions (rendered), command, expected result, and Step Type.
2. WHEN a Computed Review is rendered, THE SYSTEM SHALL display, for each Step of the pinned Runbook Version, that Step's detail alongside its recorded outcome.

---

### Edge Cases

- A Step with a title but no detail: valid; displayed by its title alone in authoring, execution, and the Computed Review, with no error or empty placeholders.
- A Step authored before this feature (title only): loads, edits, and publishes without error; its display is unchanged.
- Instructions containing unsafe or active markup: rendered safely — displaying the Step never executes or injects active content.
- A command or expected result containing markup-like characters (asterisks, backticks): shown verbatim, never interpreted as formatting.
- An author edits a Step's detail after its Runbook Version is published: a new Runbook Version captures the change; the earlier Version's detail is untouched.
- Very long instructions or command text: displayed without breaking the run or review surfaces.
- A Step typed Check versus Action: both valid; the Step Type changes only how the Step is labeled, nothing about its fields or how it is recorded.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST let an author add to each Step, in addition to its required title, an optional instructions body, an optional command, and an optional expected result.
- **FR-002**: The system MUST let an author assign each Step a Step Type of either Action or Check, and MUST reject any other Step Type value.
- **FR-003**: The system MUST treat a Step that has a title but no instructions, command, or expected result as valid, so that Steps authored before this feature remain valid and unchanged.
- **FR-004**: The system MUST interpret the instructions body as lightweight markdown and render its formatting when the Step is read.
- **FR-005**: The system MUST present the command and the expected result as plain text exactly as entered, without interpreting markup.
- **FR-006**: The system MUST render author-supplied markup safely, so that displaying a Step cannot execute or inject active content.
- **FR-007**: WHEN a Runbook is published, the system MUST freeze each Step's title, instructions, command, expected result, and Step Type into the resulting Runbook Version, and MUST provide no path to modify them afterward.
- **FR-008**: The system MUST keep the Step detail of a published Runbook Version unchanged when the underlying Runbook is later edited and re-published; edits MUST appear only in the new Runbook Version.
- **FR-009**: WHILE an Execution is open, the system MUST display the pinned Runbook Version Step's instructions (rendered), command, and expected result to the responder before an outcome is recorded.
- **FR-010**: WHEN a published Runbook Version is viewed, the system MUST display each Step's instructions (rendered), command, expected result, and Step Type.
- **FR-011**: WHEN a Computed Review is rendered, the system MUST display each pinned-Version Step's detail alongside its recorded outcome.
- **FR-012**: The system MUST treat Step Type as descriptive only — it MUST NOT change which Step fields are required, nor alter how a Step is recorded or executed.
- **FR-013**: The system MUST NOT run, validate, or compare a Step's command or expected result; they are reference text for the responder to act on and judge.
- **FR-014**: The system MUST NOT capture a per-Step owner or assignee in this feature.
- **FR-015**: The system MUST leave the Execution lifecycle and Step Record capture from the prior slice unchanged; this feature only enriches the content a Step carries and where that content is displayed.

### Key Entities

- **Step** (enriched): one ordered instruction within a Runbook Version. Carries a required title and optional detail — instructions (lightweight markdown), a command (plain text), and an expected result (plain text) — together with a Step Type. Detail may be empty, so a title-only Step is valid.
- **Step Type**: the classification of a Step by the kind of work it represents — Action (do something) or Check (verify a condition or observe a result). Descriptive only; does not change a Step's required fields or how it is executed. Decision and Gate types are deferred with branching.
- **Runbook / Runbook Version** (from slices 01/03): the authored procedure and its immutable published snapshot; unchanged except that a Runbook Version now freezes the enriched Step detail and Step Type.
- **Execution / Step Record / Computed Review** (from slice 03): a run, its append-only captured outcomes, and the mechanically derived timeline; unchanged, and they consume the enriched Step for display.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: At least 60% of Steps in Runbook Versions published after this feature carry detail beyond the title (adoption hypothesis from the PRD, to be corrected against real use).
- **SC-002**: Among Steps typed Action, a majority carry both a command and an expected result.
- **SC-003**: In a walkthrough, a responder completes each sampled Step that carries detail using only the on-screen detail, without consulting any source outside the tool (observational target).
- **SC-004**: 100% of Steps authored before this feature remain valid and viewable without modification.
- **SC-005**: A published Runbook Version's Step detail reads identically every time it is read, with zero changes after any later edit or re-publish of the Runbook.

## Assumptions

- **Language ratification**: the constitution glossary amendment that redefines **Step** and adds **Step Type** (initiative-04 workshop) is ratified; this spec uses those binding terms.
- **"Lightweight markdown" means basic text formatting** (emphasis, lists, inline code, links). Full HTML, embedded media, and scripts are out; the exact supported subset is a planning detail (workshop H2).
- **Safe rendering of author-supplied markup** (escaping / sanitization) is required (FR-006); the mechanism is a planning decision (workshop H2).
- **Title remains the only required Step field**; all added detail is optional, which keeps every previously authored Step valid.
- **No authentication exists**, so no per-Step owner is captured (consistent with the actor deferred until accounts exist).
- **No new domain events or lifecycle moments**: this feature enriches Step content within the existing authoring and execution flows (slices 01/03); a Step is still authored, frozen at publish, and marked done/skipped/failed exactly as today.
- Per the PRD non-goals, the following are out of scope: branching and decision steps; between-step control flow (conditions, gates, approvals, delays, loops); automation and integration (running commands, connecting to external tools); automated verification of outcomes; reference links, attachments, and rollback / on-failure fields; a WYSIWYG / visual editor; Step Types beyond Action and Check; and any change to Runbook Version immutability or deletion.
- The conflict register entries this feature is expected to touch — C-001 and C-004 (immutability / aggregate-enforced invariants) and C-002 (the established schema-change procedure) — are checked and resolved during `/speckit.plan`, per the constitution; routing (C-003) is unaffected.
