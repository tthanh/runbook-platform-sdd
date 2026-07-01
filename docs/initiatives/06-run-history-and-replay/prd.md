# PRD — 06-run-history-and-replay

> Inputs: [discovery.md](discovery.md), [workshop.md](workshop.md).
> Appetite: medium — browse/search past executions + single-run replay
> (cut scope, don't extend; drop search before replay, drop replay before
> shipping nothing).
> Language gate: **cleared with no amendment** — the workshop proposed no new terms.
> This PRD is expressible entirely in ratified glossary words (Execution, Step
> Record, Taken Path, Computed Review, Incident); "run history" and "replay" are
> capability names, not domain terms.
> Sequencing: **build is gated behind 05-branching's release** — replay must
> present the Taken Path and the Options chosen at Decision steps, so it is designed
> against the branched run shape, not one that is about to change.

## Context

Every Execution the platform records is, once closed, a write-only island.
A reviewer can open one run and read its Computed Review, but there is no way to
see runs across incidents as a history, and no way to move through a single run in
the order it actually happened.

The facts that would make both possible already exist.
Every Execution keeps an append-only Step Record of what was done, when, and with
what outcome, and — once branching ships — which path the run took at each Decision.
Nothing surfaces those facts as a browsable list or a time-ordered playback.

So the learning the platform exists to produce — *how did we respond, and how do we
respond better next time* — stays locked inside individual runs no one can navigate
to or step through.
The reviewer named this an essential capability, not a supplement: a runbook exists
to reduce decision variability, and you cannot reduce what you cannot review across
runs.

This initiative turns those closed runs into something a reviewer can use.
It adds a **history** — a browsable, searchable list of past Executions across
incidents — and a **replay** — a playback of one Execution's recorded timeline with
familiar play, pause, and seek controls, moving through the moments in the order they
happened: the run's start and close, each Step Record's outcome, and — for a branched
run — each Decision and the Option that was chosen.
It reads facts already captured; it records nothing new and changes no run.

Constraints touched (pointers for the plan, stated at product level):
history and replay are **read-only** — they consume the append-only, no-deletion
guarantee the execution slice already provides and must never relax it, nor capture
any new outcome (C-004 aggregate behavior is not extended by a read surface).
Replay must resolve a run against the version it pinned at start, never the latest
(C-007), and present a run's timeline in the exact order it was recorded (C-009
ordering rule).
Replayed step content is shown with the same safe rendering the platform already
applies to step detail everywhere else (C-005).
History displays each run's Incident as the external reference the platform holds,
not as data it owns (C-008).
Everything stays inside the existing product surface and navigation approach (C-003).

## Objective

Let a reviewer find any past run and step through what actually happened, so the
record the platform already keeps becomes reviewable across runs and moment by moment.

In scope:

- A **history** view lists past Executions across incidents, most recent first (the
  last incident before older ones), and lets a reviewer **search** it to locate a
  specific run rather than re-deriving it from memory.
- Each entry shows enough to recognise the run — the runbook it ran, the incident it
  responded to (by the reference the platform holds), when it ran, and a summary of
  its Computed Review coverage.
- Opening a run offers a **replay** with play, pause, and seek controls: the reviewer
  can play the run's timeline, pause on any moment, and seek to any point by moving
  between moments.
- The timeline's moments are every recorded moment, in the order they happened: the
  run's start and close, each Step Record with its outcome (done / skipped / failed),
  and — for a branched run — each Decision with the Option that was chosen.
- Playback advances at an **even pace** — a fixed interval between moments — so the
  seek control is indexed by moment, not by real elapsed time; a long real gap between
  two events is not stretched out.
- For a branched run, replay follows the **Taken Path**: it plays the Decisions and
  their chosen Options in sequence and marks steps on a branch the run did not take as
  not reached — distinct from a step the responder skipped.
- A run with no Decision steps replays as a single straight sequence of moments,
  exactly the order it was recorded.
- Replay is a faithful reconstruction: it shows only what was recorded, adds nothing,
  loses nothing, and reorders nothing.
- History and replay change no run: opening, searching, or stepping through a run
  records no new outcome and never edits or deletes what happened.

## Non-goals

Fencing for an eager reader — each is plausibly buildable from the Objective and is
explicitly out, holding the medium appetite:

- **No compare or diff of two runs.**
  A reviewer replays one run at a time; the platform does not show two runs side by
  side or compute what differs between them.
- **No cross-run analytics, dashboards, or reporting.**
  History is a list to browse and search, not a source of aggregate metrics, trends,
  charts, or rollups across many runs.
- **No postmortem narrative authoring.**
  The platform surfaces the recorded facts; it does not add a place to write up,
  annotate, or edit an incident story on top of the Computed Review.
- **No live or multi-user replay.**
  Replay reconstructs a closed run after the fact; it is not a live shared session
  multiple people drive together in real time.
- **No real-time reconstruction or variable-speed playback.**
  Playback runs at a single even pace between moments; it does not replay the real
  elapsed time of the incident, stretch out real gaps, or offer speed multipliers or
  a wall-clock time axis.
- **No export, print, or shareable-link surface.**
  History and replay are viewed in the product only; the platform does not download,
  print, or produce a shareable link to a run or the history list.
- **No new capture during replay.**
  Stepping through a run adds no note, outcome, or record; replay is strictly a
  reader of what already exists.
- **No editing or deleting history.**
  A reviewer cannot alter, hide, or remove past runs or their records; the history is
  as immutable as the records it reads.
- **No replay of a run whose shape is still changing.**
  Replay targets closed Executions; it is not a way to watch an in-progress run.
- **No new run outcome states.**
  Replay reuses the outcomes runs already have (done / skipped / failed / not reached);
  it coins none.

## Metrics

Hypotheses with a stated basis, to correct rather than commit to.
This is a demonstration platform, so targets are illustrative, carried from the
discovery assumptions (~30-engineer SRE org, ~4 reviewable incidents/month).

- **Findability: a reviewer can locate a specific past run from history without
  re-deriving it from memory.**
  Basis: the discovery pain is that prior incidents can only be re-derived, not
  revisited; if a reviewer still cannot find a known past run, history has not closed
  that gap.
  Demonstrated by locating a named run in the demo; treated as a hypothesis to
  correct, not a commitment.

- **Replay fidelity: playing or seeking through a run reproduces the order and
  outcomes exactly as they were recorded.**
  Basis: replay is worthless if it reorders or omits what happened; a playback must
  match the run's own record moment for moment, whether played straight through or
  seeked to a point.
  Measured by observation against a known run in the demo; treated as a hypothesis to
  correct, not a commitment.

- **Branched-run legibility: replaying a branched run makes clear which Option was
  chosen at each Decision and does not show unreached branches as skipped.**
  Basis: the highest-value moment to review once branching ships is *which path did we
  take*; a replay that blurs a not-taken branch with a deliberate skip misreads the
  run.
  Treated as a hypothesis to correct, not a commitment.

- **Learning-loop closure: reviewers actually navigate to and step through past runs
  rather than working from memory.**
  Basis: the initiative exists to make review-across-runs happen; if the history and
  replay go unused in the demo walkthrough, the capability has not changed behavior.
  Treated as a hypothesis to correct, not a commitment.

## Open questions

Carried from the workshop; each must be resolved before or during the relevant
downstream phase.
The hotspot ids (H1–H6) are the workshop's; their technical resolution belongs to
/speckit.plan as ADRs or spikes.

- **Where replay and history read their facts from (H1).**
  The product requirement is only that history and replay are faithful to what was
  recorded — no run appears with a step it did not have, and none loses a step it did.
  Whether that is served by reading the existing append-only records or by re-founding
  how runs are kept on a new mechanism is a plan-time decision (ADR under the
  earn-your-complexity principle); the appetite does not mandate a rewrite, and if
  replay is cheaply satisfiable on today's records it must not grow one.

- **How a branched run replays (H2).**
  Settled at product level: each Decision appears as its own moment on the timeline,
  showing the Option that was chosen, and the run plays down the Taken Path with steps
  on branches not taken shown as not reached.
  This depends on 05-branching having shipped; how the chosen Option is read back for
  the timeline is a plan-time decision (ADR).

- **What a reviewer can search history by (H3).**
  History must be searchable enough to find a specific run without dragging in
  cross-run analytics (a non-goal).
  [NEEDS CLARIFICATION: which attributes are searchable — the runbook, the incident
  reference, a date range, how the run closed / its coverage — and which are just
  displayed?]

- **How replay makes its order stable and what it can show about "who" (H4).**
  Playback advances at an even pace between moments and does not reconstruct the real
  elapsed time of the incident (settled at product level); it still presents the moments
  in the exact order they were recorded, and that order must be reproducible for the
  same run every time — including when two records share a timestamp.
  The product requirement is a stable, faithful order; the tie-breaking rule is a
  plan-time detail (C-009).
  Replay shows *what happened and in what order*, not *who did it* — no run captures an
  actor yet, so replay must not imply one.

- **How far the history and replay surfaces go (H5).**
  Settled for replay: a play / pause / seek transport over the run's moments, within the
  existing navigation approach (C-003) and without new heavy interface machinery. Still
  open: what the search surface over history covers (see H3), and the cut line — if the
  medium appetite overruns, search is dropped before replay, and replay before shipping
  nothing.

- **How history presents runs across incidents when Incident is external (H6).**
  History spans incidents, but the platform holds only a reference to each Incident, not
  its data.
  The product requirement is that history can show and group runs by that reference
  without modeling or owning Incident data (C-008).

- **Demonstration demand, not validated.**
  Demand for history and replay is an accepted assumption carried from discovery, with
  illustrative figures from slice 01; the metrics above are hypotheses to correct, not
  commitments.
