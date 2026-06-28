# Runbook Platform Constitution

## Core Principles

### I. Earned Complexity
Complexity is earned by a requirement in the spec, never anticipated. No layer,
pattern, dependency, or abstraction enters before a spec requirement forces it. A
rule that would guard a problem the system does not yet have is deferred with a flip
condition, not carried as dead weight.

### II. Spec-Driven, PRD-Gated Flow
Every feature flows specify → plan → tasks → implement. `/speckit.specify` runs ONLY
from an approved PRD at `docs/initiatives/NN-name/prd.md`, referenced by path in the
generated spec; no PRD path → do not specify, ask for one. The agent owns the
Specify → Plan → Tasks → Implement statuses in `docs/prd-register.md` and updates the
initiative's row as each phase completes, committed with the work.

### III. Ubiquitous Language
One meaning per word across specs, plans, tasks, code, events, and commits. The
binding glossary (see Additional Constraints) is the single source of term meaning.
New or changed terms enter only via a ratified workshop amendment — never ad hoc,
never speculatively: a term enters when a workshop ratifies it AND a slice actually
needs it.

### IV. Decisions Recorded and Human-Ratified
Every contested decision is recorded as an ADR, and its status is changed only by a
human — the agent drafts and proposes, a human accepts. Invariant: an ADR's written
status always states its true status; "Proposed" on main means genuinely awaiting
the human's verdict. (Procedure in Development Workflow; lifecycle in Governance.)

### V. Testable by Requirement
Acceptance criteria use EARS: WHEN <condition> THE SYSTEM SHALL <behavior>. Tests map
to requirement ids. CI gates (e.g. leak, replay, idempotency) stay green once
introduced.

## Additional Constraints

Project-specific constraints (the template's Section 2). Two apply here: the domain
language, and where cross-feature truth lives.

### Domain Language (Binding Glossary)
One meaning per word in specs, plans, tasks, code, events, commits:
- **Runbook** — a named, ordered procedure for responding to a recurring class
  of incident; the authored, evolving thing, not a single run of it.
- **Runbook Version** — an immutable snapshot of a runbook's steps, frozen and
  identified at publish; editing a published runbook produces a new version, and
  a published version's content never changes.
- **Step** — one ordered instruction within a runbook version; carries a required
  title and optional detail — instructions (lightweight markdown), the command to
  run, and the expected result — together with a Step Type. Detail may be empty,
  so a title-only Step is valid; a published version freezes a Step's title,
  detail, and type immutably.
- **Step Type** — the classification of a Step by the kind of work it represents;
  one of Action (do something), Check (verify a condition or observe a result), or
  Decision (choose among named options, each routing to a target step). Action and
  Check are descriptive only — they do not change a Step's required fields or how it
  is executed; Decision changes execution navigation, because the responder's choice
  selects the next step. Gate (manual approval) is deferred with the remaining
  branching work.
- **Option** — a named choice on a Decision step (e.g. "Database reachable" /
  "Database down"); selecting it during an execution routes to its target step. An
  Option is frozen into the runbook version at publish, alongside the step.
- **Target Step** — the step an Option routes to. The reference is frozen at publish
  and survives the version freeze unchanged, exactly as a Step's other content does.
- **Execution** — a single run of a runbook against one incident; it pins exactly
  one runbook version when it starts and never re-pins.
- **Version Pin** — the fixed binding of an execution to the one runbook version
  in effect when the execution started.
- **Step Record** — the append-only captured outcome of acting on a step during
  an execution: which step, when, outcome (done/skipped/failed), optional note;
  the ground-truth fact of what was done. The actor ("who") is deferred until
  authentication exists — no slice captures it yet.
- **Taken Path** — the ordered sequence of steps actually reached in an execution,
  determined by the Options chosen at Decision steps; for a runbook with no Decision
  steps it is simply every step in order. It is the denominator for Computed Review
  coverage.
- **Computed Review** — the post-incident timeline derived mechanically from an
  execution's step records against its pinned version; computed, never authored
  from memory. Coverage is computed over the taken path — every step actually
  reached — so steps on a branch not taken are reported as NotReached, not Skipped.
- **Incident** — the real-world event an execution responds to; a boundary term —
  identity is owned by an external incident-management tool, and we hold a
  reference, not the source of truth.

New or changed terms enter via a workshop amendment here, never ad hoc. Flip
condition: extract to docs/glossary.md when this section outgrows a screen or
per-term history is needed.

### Architecture Coherence
- docs/architecture.md holds only cross-feature truth: context map, NFRs, and the
  conflict register. /speckit.plan MUST check the conflict register and state which
  conflicts the feature touches (or "none").
- The test for what belongs there: does the fact span more than one feature folder?
  Yes → architecture.md. No → it lives in its specs/NNN/ folder.

## Development Workflow

The order of work and the gates between phases (the template's Section 3).

### Inputs and owned statuses
- /speckit.specify runs ONLY from an approved PRD at docs/initiatives/NN-name/prd.md,
  referenced by path in the generated spec. No PRD path → do not specify; ask for one.
- After completing each phase, update the initiative's row in docs/prd-register.md
  (the statuses this agent owns: Specify → Plan → Tasks → Implement) and commit the
  edit with the work.

### Planning and decisions
- During /speckit.plan, every contested decision (it affects multiple features, had
  real alternatives, or someone might question it later) is PROMOTED to
  docs/adr/NNNN-title.md as a DRAFT with Status: Proposed. Format: Context / Options
  considered / Decision / Trade-offs accepted / Consequences / Flip condition /
  Status + date.
- While drafting an ADR, if the choice between options depends on information the
  documents do not contain — priorities, risk tolerance, budget, preferences — ASK
  the human before writing the Decision field: present the options with their
  trade-offs and a recommendation, as numbered questions. Do not silently pick. But
  do not ask when the documents already answer it: if the spec, constitution, or an
  existing ADR settles the choice, decide and cite the source. Asking everything is
  offloading; asking nothing is guessing.
- The plan references ADR ids and carries no decision rationale itself; a plan is
  never the canonical record of a decision.
- After producing the plan and its Proposed ADRs, STOP. Do not run /speckit.tasks.
- /speckit.tasks is blocked while any ADR referenced by the plan is still Proposed.

### After implement (Spec Kit's loop ends; ours doesn't)
- Amend docs/architecture.md with what is now true across features.
- Tag a release; RELEASE_NOTES.md references the spec folder and ADRs.
- Write a retro line in the register's artifact ledger.
- Only then does the next initiative advance.

## Governance

This constitution supersedes other working practices; where a practice conflicts with
it, the constitution wins. Plans and reviews verify compliance with these principles.

### ADR acceptance and lifecycle
- Acceptance is two recorded events. (1) The plan PR merges with its ADRs honestly
  marked Proposed — this records that the proposals were made. (2) After review,
  acceptance of each ADR is its OWN commit on main, flipping the status line to
  "Accepted — <date>", authored or explicitly instructed by the human; the agent
  never initiates a status change. Rejected ADRs are superseded or marked Rejected
  the same way — by their own human commit.
- ADRs are immutable once Accepted. Implementation lessons are appended as dated
  notes; reversals are new superseding ADRs (the old one is marked Superseded-by,
  never rewritten). Proposed drafts are freely editable. Non-semantic corrections
  (typos, links, formatting) may be edited directly; the test: if the edit changes
  what a future reader would believe was decided or why, it supersedes instead.

### Amending this constitution
- The agent never changes constitution state on its own; amendments are
  human-authored or applied only on explicit human instruction, and recorded in the
  log below with a date and a reason. Glossary changes additionally require a ratified
  workshop.
- Versioning is semantic: MAJOR for a removed or redefined principle/governance rule,
  MINOR for a new principle/section or materially expanded guidance, PATCH for
  clarifications and non-semantic fixes.

### Amendment log
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
- 2026-06-13: Step Record narrowed — "who" deferred. During PRD-03 clarification
  the human reviewer chose not to capture the actor in the runbook-execution
  slice: no authentication exists in any slice, so "who" would be unverified free
  text. The Step Record definition drops "who" until accounts exist, keeping the
  binding glossary consistent with what the slice actually captures. Reason: the
  glossary must match the spec — deferring an unverifiable field beats recording
  noise or carrying a standing spec-vs-glossary mismatch.
- 2026-06-13: Glossary extended (5 terms) — initiative 03 workshop, ratified by
  the human reviewer: Execution, Version Pin, Step Record, Computed Review,
  Incident. These are the execution-slice terms held as proposed since the 01
  workshop; the runbook-execution slice is now being built and needs them, so
  they enter the binding glossary per the rule above (ratified + a slice needs
  them). Glossary now holds 8 terms — still under a screen, so the flip to
  docs/glossary.md does not trip. Reason: the deferred half of 01 is being
  delivered; the language enters exactly when the slice requires it.
- 2026-06-15: Step redefined + Step Type added (9 terms) — initiative 04 workshop.
  Step grows from "one ordered instruction" to carry a required title plus optional
  detail (instructions as lightweight markdown, command, expected result) and a
  Step Type; the freeze at publish now covers title, detail, and type. Step Type
  is a new term with a minimal set (Action, Check), descriptive only — Decision
  and Gate types are deferred with the branching initiative. The slice that needs
  this language (04-rich-steps) is being built, so the terms enter per the rule
  (ratified + a slice needs them). Glossary now holds 9 terms — still under a
  screen, so the flip to docs/glossary.md does not trip. Reason: a bare title is
  below the floor every comparable runbook tool sets; the language enters exactly
  when the slice requires richer Steps, and stays minimal (no control-flow types)
  to keep complexity earned.
- 2026-06-15: Initiative 03 reached Implement — two engineering rules validated
  in practice and now treated as standing patterns for future slices:
  (1) C-002 migration path: the initial EF Core migration reproducing the
  existing schema byte-for-byte (so existing databases and tests are unaffected)
  followed by a second migration for the new tables is the correct procedure for
  any schema-changing slice. EnsureCreated must never be mixed with migrations.
  (2) SQLite DateTimeOffset ordering: ORDER BY DateTimeOffset pushed to SQLite
  is unreliable (confirmed in slice-01 retro, mitigated successfully in slice-03
  via in-memory ordering + a per-Execution Sequence tiebreaker). Any slice that
  orders by DateTimeOffset MUST do so in memory after loading. Both patterns
  belong in docs/architecture.md's conflict register as standing constraints;
  this note records that they are empirically confirmed, not just theorised.
- 2026-06-28: Removed "No dual-writes; cross-store propagation rides the outbox"
  from the engineering rules. The app has one store and nothing to propagate, so
  every plan marked it "satisfied trivially" — anticipated complexity, which this
  constitution forbids elsewhere ("complexity is earned, never anticipated"). Flip
  condition: reintroduce it (via an ADR) the first time a slice adds a second store
  or async cross-store propagation — the moment the rule has a real problem to
  guard. Reason: a rule that never binds is noise; defer it until a slice needs it,
  exactly as the glossary defers terms. (The two confirmed patterns from the
  2026-06-15 note now live in the conflict register as C-009 and within C-002.)
- 2026-06-28: Restructured onto the standard Spec Kit constitution template —
  Core Principles (numbered I–V), Additional Constraints, Development Workflow,
  Governance, and a Version/Ratified/Last-Amended footer. Content was remapped,
  not changed: every prior rule, all nine glossary terms, and this amendment log
  are preserved; the restructure itself added or removed no principle. Adopted
  semantic versioning starting at 1.0.0. Reason: align with the shipped template so
  every section has a documented purpose and nothing is bespoke without reason.

- 2026-06-28: Branching language ratified — initiative 05 workshop, ratified by the
  human reviewer. Step Type gains a third member, **Decision** (choose among named
  options, each routing to a target step); unlike Action/Check it is *not*
  descriptive-only — it changes execution navigation, which is the point of the
  slice. **Gate** (manual approval) stays deferred, so the enum grows by exactly one.
  Three new terms enter: **Option**, **Target Step**, **Taken Path**. **Computed
  Review** is redefined so coverage is computed over the taken path (untaken-branch
  steps are NotReached, not Skipped) — additive in practice, since the execution
  slice already distinguishes NotReached from Skipped. The slice that needs this
  language (05-branching) is being built, so the terms enter per the rule (ratified
  + a slice needs them). Glossary now holds 12 terms — nearing but not past the flip
  to docs/glossary.md; monitor at the next amendment. Reason: a linear-only procedure
  cannot encode contingency; the language enters exactly when the branching slice
  requires it, and stays minimal (one new type, no condition-expressions, gates,
  loops, or parallel paths) to keep complexity earned.

**Version**: 1.1.0 | **Ratified**: 2026-06-11 | **Last Amended**: 2026-06-28
