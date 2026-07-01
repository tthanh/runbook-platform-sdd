# Feature Specification: Run History and Replay

**Feature Branch**: `006-run-history-replay`

**Created**: 2026-07-01

**Status**: Draft

**Input**: User description: "PRD at docs/initiatives/06-run-history-and-replay/prd.md — a browsable, searchable history of past Executions across incidents, plus single-run replay as a play/pause/seek transport (even-pace, seek indexed by moment) over every recorded moment of one Execution (start, each Step Record with outcome, each Decision with the chosen Option, close), following the Taken Path for branched runs with untaken-branch steps shown as not reached. Read-only; records nothing new. Build gated behind 05-branching's release."

**Source PRD**: [docs/initiatives/06-run-history-and-replay/prd.md](../../docs/initiatives/06-run-history-and-replay/prd.md)

Glossary terms used (binding, per the constitution): **Execution**, **Step Record**, **Taken Path**, **Computed Review**, **Incident**, **Runbook**, **Runbook Version**, **Step**, **Decision** (Step Type), **Option**, **Version Pin**.

> This feature is **read-only**: it surfaces facts already captured by the execution slice (03) and enriched by branching (05). It introduces no new domain events, no new captured outcome, and no new glossary terms — "run history" and "replay" are capability names, not domain terms. Build is **gated behind 05-branching's release**, because replay presents the Taken Path and the Option chosen at each Decision.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Browse a history of past runs across incidents (Priority: P1)

A post-incident reviewer opens a history view and sees past Executions from across all incidents in one list, most recent first, each entry showing enough to recognise the run — the Runbook it ran, the Incident it responded to, when it ran, and a summary of its Computed Review coverage — and can open any one of them.

**Why this priority**: This is the floor of the slice and the last thing that would be cut (per the appetite: drop search before replay, drop replay before shipping nothing). Today a closed Execution is a write-only island reachable only if you already know it exists; a cross-incident list is itself the first value — you can find and revisit prior runs instead of re-deriving them from memory. It stands alone: even with no replay and no search, a browsable history closes part of the gap.

**Independent Test**: Close two or more Executions against different Incidents, open the history view, and confirm all closed runs appear most-recent-first with their Runbook, Incident reference, time, and coverage summary; open one and confirm it resolves to that run.

**Acceptance Scenarios** (EARS):

1. WHEN a reviewer opens the history view, THE SYSTEM SHALL list closed Executions across all Incidents, ordered most recent first.
2. WHEN the history view lists an Execution, THE SYSTEM SHALL show, for each entry, the Runbook it ran, the Incident it responded to (by the held reference), when it ran, and a summary of its Computed Review coverage.
3. WHEN a reviewer selects an Execution from the history, THE SYSTEM SHALL open that Execution.
4. WHILE an Execution is still open (not closed), THE SYSTEM SHALL NOT present it as a completed run in the history.
5. WHEN a reviewer browses, opens, or reads any run from the history, THE SYSTEM SHALL record no new Step Record, outcome, or other change to the run.

---

### User Story 2 - Replay a single run with play, pause, and seek (Priority: P2)

A reviewer opens one closed Execution and replays it: a transport with play, pause, and seek lets them play through the run's recorded moments in the order they happened — the run's start, each Step Record with its outcome, and its close — pausing on any moment and seeking to any point. Playback advances at an even pace, and the seek control is indexed by moment, not by real elapsed time.

**Why this priority**: Replay is the headline capability and the second-most essential (cut only after search). Stepping through what the team actually did, in order, is the review experience the single static Computed Review cannot give. It builds on the recorded run and can be reached from the history (US1) or the existing single-run view.

**Independent Test**: Take a closed Execution with several recorded outcomes, open its replay, press play and confirm the moments advance in recorded order at an even pace; pause and confirm it holds on a moment; seek forward and back and confirm it lands on the corresponding moment; confirm nothing about the run changes.

**Acceptance Scenarios** (EARS):

1. WHEN a reviewer opens the replay of a closed Execution, THE SYSTEM SHALL present that run's recorded moments in the exact order they were recorded: its start, each Step Record with its outcome (done / skipped / failed), and its close.
2. WHEN a reviewer presses play, THE SYSTEM SHALL advance through the moments one after another at an even pace — a fixed interval between moments — independent of the real elapsed time between the recorded events.
3. WHEN a reviewer pauses, THE SYSTEM SHALL hold on the current moment until play or seek is next used.
4. WHEN a reviewer seeks, THE SYSTEM SHALL move to the selected moment, with the seek control indexed by moment rather than by wall-clock time.
5. WHEN replay resolves a run's Step content, THE SYSTEM SHALL resolve it against the Runbook Version the Execution pinned at start, never a later Version.
6. WHEN replay displays a Step's authored detail, THE SYSTEM SHALL render it with the same safe rendering the platform applies to Step detail elsewhere.
7. WHEN a run has been replayed, THE SYSTEM SHALL leave every Step Record and the run itself unchanged — replay adds nothing, loses nothing, and reorders nothing.
8. WHEN two recorded moments share a timestamp, THE SYSTEM SHALL order them the same way every time the run is replayed.
9. WHEN replay presents a run's moments, THE SYSTEM SHALL show what happened and in what order, and SHALL NOT attribute any moment to a specific actor.

---

### User Story 3 - Search the history to locate a specific run (Priority: P3)

A reviewer with a specific past run in mind searches the history to find it directly, rather than scrolling the full list.

**Why this priority**: Search refines browsing (US1) and is the first capability cut if the appetite overruns. It adds convenience once a history exists, but the history is usable without it.

**Independent Test**: With several closed Executions in the history, search by a supported attribute and confirm only matching runs are shown, and that a known run can be located without scrolling the whole list.

**Acceptance Scenarios** (EARS):

1. WHEN a reviewer searches the history, THE SYSTEM SHALL show only the closed Executions that match the search and SHALL preserve the most-recent-first ordering among them.
2. WHEN a search matches no runs, THE SYSTEM SHALL indicate that no runs match, without error.
3. WHEN a reviewer clears the search, THE SYSTEM SHALL restore the full most-recent-first history.

---

### Edge Cases

- **Empty history**: with no closed Executions, the history view shows an empty state without error; an in-progress-only environment shows nothing to replay.
- **A branched run** (once 05 has shipped): replay follows the **Taken Path** — at each **Decision** moment it shows the **Option** that was chosen and continues down that Option's path; Steps on branches the run did not take are shown as **not reached**, visibly distinct from a Step the responder deliberately **skipped**.
- **A run with no Decision Steps**: replays as a single straight sequence of moments in recorded order — the Taken Path is simply every Step in order.
- **A run recorded before branching existed**: replays as a straight sequence with no Decision moments; unaffected by the branching capability.
- **Simultaneous moments**: two Step Records (or a Decision and a Step Record) sharing a timestamp appear in a stable, repeatable order on every replay.
- **A Step whose authored detail contains unsafe or active markup**: rendered safely during replay — replaying a run never executes or injects active content.
- **A very long run** (many moments): browses and replays without breaking the history or replay surfaces; even-pace playback does not attempt to reproduce a multi-hour real duration.
- **Seeking past the end or before the start**: the transport clamps to the first or last moment rather than erroring.
- **The pinned Runbook Version underlying a run**: replay always reads the pinned Version's Step content; a newer Version published since does not change what the replay shows.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST present a history of closed Executions drawn from across all Incidents, ordered most recent first.
- **FR-002**: The system MUST show, for each history entry, the Runbook the Execution ran, the Incident it responded to (by the held reference), when it ran, and a summary of its Computed Review coverage.
- **FR-003**: The system MUST let a reviewer open any Execution listed in the history.
- **FR-004**: The system MUST include only closed Executions in the history and in replay; an Execution that is still open MUST NOT be presented as a completed run.
- **FR-005**: The system MUST be read-only in this feature — browsing, opening, searching, or replaying a run MUST NOT create a Step Record, capture an outcome, or edit or delete any Execution, Step Record, or Computed Review.
- **FR-006**: The system MUST let a reviewer replay a closed Execution through a transport offering play, pause, and seek.
- **FR-007**: The system MUST present, as the moments of a replay, the run's start, each Step Record with its recorded outcome (done / skipped / failed), each Decision with the Option that was chosen, and the run's close — in the exact order they were recorded.
- **FR-008**: WHEN playing, the system MUST advance through moments at an even pace — a fixed interval between moments — and MUST NOT reproduce the real elapsed time between recorded events, stretch out real gaps, offer variable-speed playback, or present a wall-clock time axis.
- **FR-009**: The system MUST index the seek control by moment (jumping between recorded moments), not by real elapsed time, and MUST clamp seeking to the first and last moment.
- **FR-010**: WHEN a run is branched, the system MUST replay along its Taken Path: it MUST show the Option chosen at each Decision, continue down that Option's path, and mark Steps on branches not taken as not reached — distinct from Steps that were skipped.
- **FR-011**: WHEN a run has no Decision Steps, the system MUST replay it as a single straight sequence of moments in recorded order.
- **FR-012**: The system MUST resolve all Step content shown during replay against the Runbook Version the Execution pinned at start, never a later Version.
- **FR-013**: The system MUST render authored Step detail shown during replay with the same safe rendering applied to Step detail elsewhere, so that replaying a run cannot execute or inject active content.
- **FR-014**: The system MUST order moments that share a timestamp deterministically, so that the same run replays in the same order every time.
- **FR-015**: The system MUST present replay moments as what happened and in what order, and MUST NOT attribute any moment to a specific actor.
- **FR-016**: The system MUST let a reviewer search the history to locate a specific run, showing only matching closed Executions while preserving most-recent-first ordering, and MUST restore the full history when the search is cleared. [NEEDS CLARIFICATION: which attributes are searchable — the Runbook, the Incident reference, a date/time range, coverage/outcome — and are any of these only displayed rather than searchable?]
- **FR-017**: The system MUST display and group history entries by the Incident reference it holds, without modeling, storing, or owning Incident data beyond that reference.
- **FR-018**: The system MUST NOT provide a compare/diff of two runs, cross-run analytics or reporting, postmortem narrative authoring, live or multi-user replay, or an export/print/shareable-link surface.
- **FR-019**: The system MUST reuse the existing run outcome states (done / skipped / failed / not reached) and MUST NOT introduce a new outcome state for history or replay.

### Key Entities

- **Execution** (from slice 03): a single run of a Runbook against one Incident; here it is the unit browsed in history and replayed. Read-only in this feature.
- **Step Record** (from slice 03): the append-only captured outcome of acting on a Step during an Execution; the ground-truth facts replay presents in order. No actor is captured.
- **Taken Path** (from slice 05): the ordered sequence of Steps actually reached in an Execution, determined by the Options chosen at Decision Steps; the path replay follows. For a run with no Decision Steps it is every Step in order.
- **Decision / Option** (from slice 05): a Decision Step's completion is choosing among named Options; replay surfaces which Option was chosen at each Decision as a moment on the timeline.
- **Computed Review** (from slice 03/05): the mechanically derived timeline and coverage over the Taken Path; its coverage summary appears on history entries, and replay is the moment-by-moment view of the same recorded facts.
- **Incident** (boundary term): the real-world event a run responded to; identity is owned externally and the platform holds only a reference, by which history displays and groups runs.
- **Runbook Version / Version Pin** (from slices 01/03): the immutable published snapshot a run pinned at start; replay resolves all Step content through the pinned Version.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A reviewer can locate a specific known past run from the history — by browsing, and where supported by search — without re-deriving it from memory or consulting anything outside the tool (observational target from the PRD, to be corrected against real use).
- **SC-002**: Replaying a run reproduces its recorded moments and outcomes exactly — same moments, same order — as the run's own Step Records, with zero added, lost, or reordered moments, verified against a known run.
- **SC-003**: For a branched run, a reviewer watching the replay can state which Option was taken at each Decision and can tell a not-reached branch from a deliberately skipped Step, with no case where the two are shown the same way.
- **SC-004**: The same run replays in the same order every time it is replayed, including when moments share a timestamp.
- **SC-005**: 100% of runs recorded before this feature (including runs with no Decision Steps) browse and replay without modification and without error.
- **SC-006**: No history or replay action changes any run — the count and content of every Execution's Step Records is identical before and after browsing, searching, and replaying.

## Assumptions

- **Sequencing / dependency on 05**: replay presents the Taken Path and the Option chosen at each Decision, so this feature is built after **05-branching** releases; the branching glossary (Decision, Option, Target Step, Taken Path) is ratified and in force. A run with no Decision Steps is the degenerate straight-line case and works identically.
- **Read-only over existing records**: history and replay read the append-only Step Records and Computed Review the execution slice already produces; whether they are served by reading those records directly or by another mechanism is a planning decision (PRD open question H1) — the appetite does not mandate a storage rewrite, and if replay is cheaply satisfiable on today's records it must not grow one.
- **Even-pace playback**: playback uses a single fixed interval between moments; the exact interval is a UX/planning detail. Real incident durations (minutes to hours) are deliberately not reproduced.
- **No actor**: no authentication exists, so no run captures a "who"; replay shows what happened and in what order only (consistent with the actor deferred until accounts exist).
- **Only closed runs**: replay and history target closed Executions; watching an in-progress run is out of scope.
- **Deterministic ordering** reuses the established rule that moments are ordered by their recorded time with a per-Execution sequence tiebreaker (conflict register C-009); confirmed and applied at `/speckit.plan`.
- **Conflict register** entries this feature is expected to touch — C-003 (hash routing, for the new views), C-004 (read surface adds no aggregate mutation), C-005 (safe rendering of replayed Step detail), C-007 (replay resolves through the Version Pin), C-008 (Incident stays a foreign reference), and C-009 (deterministic ordering) — are checked and resolved during `/speckit.plan`, per the constitution.
- **Per the PRD non-goals**, out of scope: compare/diff of two runs; cross-run analytics, dashboards, or reporting; postmortem narrative authoring; live or multi-user replay; real-time reconstruction or variable-speed playback; export/print/shareable links; new capture during replay; editing or deleting history; replay of an in-progress run; and any new run outcome state.
