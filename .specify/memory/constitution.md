# Project Constitution

## Inputs and status
- /speckit.specify runs ONLY from an approved PRD at
  docs/initiatives/NN-name/prd.md, referenced by path in the generated
  spec. No PRD path provided → do not specify; ask for one.
- After completing each phase, update the initiative's row in
  docs/prd-register.md (the statuses this agent owns: Specify → Plan →
  Tasks → Implement) and commit the edit with the work.

## Decisions
- During /speckit.plan, every contested decision (it affects multiple
  features, had real alternatives, or someone might question it later)
  is PROMOTED to docs/adr/NNNN-title.md as a DRAFT with Status: Proposed.
  Format: Context / Options considered / Decision / Trade-offs accepted /
  Consequences / Flip condition / Status + date.
- While drafting an ADR, if the choice between options depends on
  information the documents do not contain — priorities, risk tolerance,
  budget, preferences — ASK the human before writing the Decision field:
  present the options with their trade-offs and a recommendation, as
  numbered questions. Do not silently pick. But do not ask when the
  documents already answer it: if the spec, constitution, or an existing
  ADR settles the choice, decide and cite the source. Asking everything
  is offloading; asking nothing is guessing.
- The plan references ADR ids and carries no decision rationale itself;
  a plan is never the canonical record of a decision.
- After producing the plan and its Proposed ADRs, STOP. Do not run
  /speckit.tasks.
- Acceptance is two recorded events. (1) The plan PR merges with its
  ADRs honestly marked Proposed — this records that the proposals were
  made. (2) After review, acceptance of each ADR is its OWN commit on
  main, flipping the status line to "Accepted — <date>", authored or
  explicitly instructed by the human; the agent never initiates a status
  change. Rejected ADRs are superseded or marked Rejected the same way —
  by their own human commit.
- Invariant: an ADR's written status always states its true status.
  Proposed on main means genuinely awaiting the human's verdict.
- /speckit.tasks is blocked while any ADR referenced by the plan is
  still Proposed.
- ADRs are immutable once Accepted. Implementation lessons are appended
  as dated notes; reversals are new superseding ADRs (the old one is
  marked Superseded-by, never rewritten). Proposed drafts are freely
  editable. Non-semantic corrections (typos, links, formatting) may be
  edited directly; the test: if the edit changes what a future reader
  would believe was decided or why, it supersedes instead.

## Language (binding glossary)
One meaning per word in specs, plans, tasks, code, events, commits:
- **Runbook** — a named, ordered procedure for responding to a recurring class
  of incident; the authored, evolving thing, not a single run of it.
- **Runbook Version** — an immutable snapshot of a runbook's steps, frozen and
  identified at publish; editing a published runbook produces a new version, and
  a published version's content never changes.
- **Step** — one ordered instruction within a runbook version.
New or changed terms enter via a workshop amendment here, never ad hoc.
Flip condition: extract to docs/glossary.md when this section outgrows a
screen or per-term history is needed.

## Architecture coherence
- docs/architecture.md holds only cross-feature truth: context map, NFRs,
  and the conflict register. /speckit.plan MUST check the conflict register
  and state which conflicts the feature touches (or "none").
- The test for what belongs there: does the fact span more than one feature
  folder? Yes → architecture.md. No → it lives in its specs/NNN/ folder.

## Engineering rules that never relax
- Complexity is earned by a requirement in the spec, never anticipated.
- Acceptance criteria use EARS: WHEN <condition> THE SYSTEM SHALL <behavior>.
- No dual-writes; cross-store propagation rides the outbox.
- Tests map to requirement ids. CI gates (leak, replay, idempotency) stay
  green once introduced.

## After implement (Spec Kit's loop ends; ours doesn't)
- Amend docs/architecture.md with what is now true across features.
- Tag a release; RELEASE_NOTES.md references the spec folder and ADRs.
- Write a retro line in the register's artifact ledger.
- Only then does the next initiative advance.

## Amendments
- 2026-06-11: Process section reduced to the agent contract
  (inputs, owned statuses). Human pipeline rules moved to templates and
  README — they are not agent-actionable and were paying context tax in
  the hot path.
- 2026-06-12: Decisions section finalized — the section now *requires*
  that ADRs be drafted Proposed during /speckit.plan with human-context
  questions asked before the Decision field; acceptance split into two
  recorded events (proposal merges as Proposed; acceptance is a separate
  human-authored status commit on main); tasks blocked until acceptance;
  immutability scoped to Accepted with a non-semantic correction valve.
  (This records the rule being written; no ADR has been drafted yet.)
  Reason: approval and
  the state it approves must never live in different places — making
  both the proposal and the acceptance their own commits gives each its
  own timestamp and author, and the written status is true at every
  point in history.
- 2026-06-12: Glossary seeded (3 terms) — initiative 01 workshop, ratified by
  the human reviewer: Runbook, Runbook Version, Step. The first PRD is scoped to
  authoring only (create and publish a Runbook), so only these three land now.
  The execution-slice terms (Execution, Version Pin, Step Record, Computed
  Review, Incident) stay proposed in the workshop file and enter on a later
  ratified workshop when that slice is built. Reason: terms enter the binding
  glossary only when a real workshop ratifies them and a slice actually needs
  them — never speculatively.
