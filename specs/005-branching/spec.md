# Feature Specification: Branching (Decision Steps)

**Feature Branch**: `005-branching`

**Created**: 2026-07-01

**Status**: Draft

**Input**: User description: "PRD at docs/initiatives/05-branching/prd.md — the minimal Decision-Flow model: an author types a Step as a Decision with named Options, each routing to a Target Step, frozen into the Runbook Version at publish; during an Execution, reaching a Decision Step presents its Options and the responder's choice routes the run to that Option's Target Step; the Computed Review reports over the Taken Path, with untaken-branch Steps shown as not reached rather than skipped. Existing runbooks with no Decision Steps behave exactly as before."

**Source PRD**: [docs/initiatives/05-branching/prd.md](../../docs/initiatives/05-branching/prd.md)

Glossary terms used (binding, per the constitution v1.1.0): **Runbook**, **Runbook Version**, **Step**, **Step Type**, **Decision**, **Option**, **Target Step**, **Execution**, **Version Pin**, **Step Record**, **Taken Path**, **Computed Review**, **Incident**.

> The branching language (Decision Step Type; Option, Target Step, Taken Path; Computed Review coverage over the taken path) was ratified into the constitution glossary on 2026-06-28 (v1.1.0). This spec uses those terms as binding. It enriches the *shape* of a procedure only — it adds no new way to author Step content (that shipped in rich-steps), no automation, and no condition logic the platform evaluates on the responder's behalf.

## Clarifications

### Session 2026-07-01

- Q: What content does a Decision Step carry, beyond its Options (FR-018 / H4)? → A: It **reuses the existing optional Step detail fields** (instructions / command / expected result) that every Step already has since rich-steps, plus its Options — no field is added or forbidden for the Decision type. Authors typically use the title + instructions to pose the choice; the fields stay optional.
- Q: Which conditions block publishing a branched Runbook Version (FR-017 / H5)? → A: **BLOCK** — any Option whose Target Step does not exist in the Version; any loop / back-edge (forward-only routing); any Decision Step with fewer than two Options. **WARN (non-blocking)** — a Step unreachable from the start. **ALLOW** — a path that simply ends (a reached Step with no continuation is a valid ending).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Author a branching procedure and freeze it at publish (Priority: P1)

An author writing a Runbook marks a Step as a **Decision** and gives it named **Options** — for example a Check-like question "Is the database reachable?" with Options "Database reachable" and "Database down" — each routing to a **Target Step**. When the Runbook is published, the Decision Step, its Options, and their routing freeze into the Runbook Version exactly as Step content already does.

**Why this priority**: This is the core of the slice — without an author being able to express a fork and have it frozen immutably, there is nothing branched to run or review. It is the minimum viable payoff: a published procedure that encodes "if X do A, else do B" instead of one flat list.

**Independent Test**: Author a Runbook with a Decision Step whose Options route to different Target Steps; publish it; reopen the published Runbook Version and confirm the Decision Step, its Options, and their Target Steps are present and correct. Edit the draft, publish again, and confirm the first Version's routing is unchanged.

**Acceptance Scenarios** (EARS):

1. WHEN an author sets a Step's Step Type to Decision, THE SYSTEM SHALL accept it as one of Action, Check, or Decision, and SHALL reject any other value.
2. WHEN an author adds Options to a Decision Step, THE SYSTEM SHALL let each Option carry a label the responder will read and a Target Step it routes to.
3. WHEN an author routes an Option to a Target Step, THE SYSTEM SHALL keep that routing pointing at the intended Step even if Steps are reordered or inserted while the draft is edited.
4. WHEN a Runbook is published, THE SYSTEM SHALL freeze each Decision Step, its Options, and their Target-Step routing into the resulting Runbook Version.
5. WHEN an author edits a Runbook after a Runbook Version has been published and publishes again, THE SYSTEM SHALL leave the earlier Version's Decision Steps, Options, and routing unchanged and SHALL capture the edits only in the new Version.
6. WHEN any caller attempts to modify the Options or routing of a published Runbook Version, THE SYSTEM SHALL provide no path to do so.
7. WHEN a Runbook has no Decision Steps, THE SYSTEM SHALL author, publish, and freeze it exactly as before this feature.

---

### User Story 2 - Follow one path through a branched run (Priority: P2)

A responder running an Execution against a pinned branched Runbook Version reaches a Decision Step, is shown its Options, chooses the one that matches what they found, and the run continues from that Option's Target Step — so the responder walks only the path their situation calls for, not every contingency.

**Why this priority**: The branching payoff only lands if a responder can actually take one path at run time; this is what removes the under-pressure "mentally skip the steps that don't apply" improvising. It depends on P1 having frozen the routing into the Version.

**Independent Test**: Start an Execution against a published branched Runbook Version; at the Decision Step, confirm the Options are presented; choose one and confirm the run continues from that Option's Target Step and does not present the Steps on the branch not taken. Repeat choosing the other Option and confirm the alternate path is followed.

**Acceptance Scenarios** (EARS):

1. WHILE an Execution is open, WHEN a responder reaches a Decision Step of the pinned Runbook Version, THE SYSTEM SHALL present that Step's Options for the responder to choose from.
2. WHEN a responder selects an Option at a Decision Step, THE SYSTEM SHALL continue the Execution from that Option's Target Step.
3. WHEN a responder completes a Decision Step, THE SYSTEM SHALL treat the completion as the choice of a named Option, not as a done/skipped/failed outcome, and SHALL record which Option was chosen.
4. WHEN a responder is on a branched run, THE SYSTEM SHALL NOT present the Steps that lie only on a branch the run did not take.
5. WHEN an Execution routes at a Decision Step, THE SYSTEM SHALL resolve the Options and their Target Steps against the Runbook Version the Execution pinned at start, never a later Version.
6. WHEN a responder runs a Runbook Version that has no Decision Steps, THE SYSTEM SHALL walk every Step in order and record outcomes exactly as before this feature.

---

### User Story 3 - Review a branched run over its Taken Path (Priority: P3)

A reviewer reads the Computed Review of a closed branched Execution and sees coverage reported over the **Taken Path** — the Steps the run actually reached — with Steps on a branch the run did not take shown as **not reached**, clearly distinct from a Step the responder deliberately **skipped**, and with the Option chosen at each Decision Step visible.

**Why this priority**: The review is the platform's reason for capturing runs; "coverage = every step" misreads a branched run. Distinguishing a not-taken branch from a deliberate skip is what keeps the post-incident review trustworthy. It layers on top of authoring (P1) and running (P2).

**Independent Test**: Run and close a branched Execution down one path; produce its Computed Review and confirm reached Steps show their recorded outcomes, the Option chosen at the Decision Step is shown, and Steps on the branch not taken are reported as not reached — never as skipped.

**Acceptance Scenarios** (EARS):

1. WHEN a Computed Review is rendered for a branched Execution, THE SYSTEM SHALL compute coverage over the Taken Path — the Steps the run actually reached.
2. WHEN a Computed Review reports a Step that lies only on a branch the run did not take, THE SYSTEM SHALL report it as not reached and SHALL NOT report it as skipped.
3. WHEN a Computed Review is rendered for a branched Execution, THE SYSTEM SHALL show, at each Decision Step, the Option that was chosen.
4. WHEN a Computed Review is rendered for an Execution of a Runbook Version with no Decision Steps, THE SYSTEM SHALL report coverage over every Step exactly as before this feature.

---

### Edge Cases

- **A runbook with no Decision Steps**: authored, run, and reviewed exactly as before; its Taken Path is every Step in order, and coverage is every Step.
- **A runbook authored before this feature**: loads, edits, publishes, runs, and reviews without change; it has no Decision Steps.
- **Two Options routing to the same Target Step**: allowed; the run simply continues from that Step. No special "branches converge / merge" behavior is defined beyond continuing.
- **A path that ends** (a Step, or an Option's Target Step, with no continuation): a valid ending — publishing is allowed; the run simply completes at that Step (H5).
- **A Step unreachable from the start**: allowed to publish but the author is warned (non-blocking); it is not a broken route (H5).
- **An Option that points at no Step, or at a Step removed before publish**: publishing is blocked — an Option with no valid Target Step is a broken route (H5).
- **Reordering or inserting Steps while drafting**: an Option keeps pointing at the Step the author meant; routing never silently re-points to a different Step.
- **A Decision Step reached but the run closed before choosing**: the Decision Step is not completed; in the Computed Review it is neither a skip nor a taken branch — it was reached but unresolved, and steps beyond it are not reached.
- **Backward routing / loops**: not expressible — an Option routes forward only; the author cannot create a route back to an earlier Step (loops are out of scope, enforced at publish).
- **A Decision Step with a single Option, or none**: publishing is blocked — a Decision Step must have at least two Options to be publishable (H5).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST let an author set a Step's Step Type to Decision, accepting only Action, Check, or Decision and rejecting any other value.
- **FR-002**: The system MUST let an author give a Decision Step named Options, where each Option carries a label the responder reads and routes to a Target Step.
- **FR-003**: The system MUST keep an Option's routing pointing at the Step the author intended even when Steps are reordered or inserted while the draft is edited (routing MUST NOT silently re-point to a different Step).
- **FR-004**: WHEN a Runbook is published, the system MUST freeze each Decision Step, its Options, and their Target-Step routing into the resulting Runbook Version, and MUST provide no path to modify them afterward.
- **FR-005**: The system MUST keep an earlier Runbook Version's Decision Steps, Options, and routing unchanged when the Runbook is later edited and re-published; edits MUST appear only in the new Version.
- **FR-006**: WHILE an Execution is open, WHEN a responder reaches a Decision Step of the pinned Runbook Version, the system MUST present that Step's Options for the responder to choose.
- **FR-007**: WHEN a responder selects an Option at a Decision Step, the system MUST continue the Execution from that Option's Target Step.
- **FR-008**: The system MUST treat completing a Decision Step as choosing a named Option — not as a done/skipped/failed outcome — and MUST record which Option was chosen.
- **FR-009**: The system MUST NOT present, during a run, the Steps that lie only on a branch the run did not take.
- **FR-010**: The system MUST resolve a Decision Step's Options and Target Steps against the Runbook Version the Execution pinned at start, never a later Version.
- **FR-011**: The system MUST determine an Execution's Taken Path as the ordered Steps actually reached, following the Options chosen at Decision Steps; for a Runbook Version with no Decision Steps the Taken Path MUST be every Step in order.
- **FR-012**: WHEN a Computed Review is rendered, the system MUST compute coverage over the Taken Path, reporting reached Steps with their recorded outcomes (done/skipped/failed).
- **FR-013**: The system MUST report a Step that lies only on a branch the run did not take as not reached, and MUST NOT report it as skipped.
- **FR-014**: WHEN a Computed Review is rendered for a branched Execution, the system MUST show the Option chosen at each Decision Step.
- **FR-015**: The system MUST NOT evaluate any condition, run any command, read any metric, or otherwise choose a branch automatically — a human always selects the Option.
- **FR-016**: The system MUST NOT allow an Option to route backward or create a loop; routing is forward-only, enforced at publish.
- **FR-017**: WHEN a Runbook is published, the system MUST reject (block) a Version if any Option's Target Step does not exist in the Version, if any Option routes backward or forms a loop, or if any Decision Step has fewer than two Options. The system MUST warn (without blocking) when a Step is unreachable from the start, and MUST allow a path that simply ends (a reached Step with no continuation is a valid ending).
- **FR-018**: The system MUST let a Decision Step carry the same optional detail fields as any other Step (instructions, command, expected result) in addition to its Options; it MUST NOT require any of those fields and MUST NOT forbid them for the Decision type.
- **FR-019**: The system MUST leave a Runbook with no Decision Steps behaving exactly as before this feature across authoring, publish, run, and review.
- **FR-020**: The system MUST leave Runbook Version immutability, the Version Pin, one-open-Execution-per-Incident, and no-restart behavior unchanged; branching adds routing within a Version only.

### Key Entities

- **Step** (from slice 04): one ordered instruction within a Runbook Version; now its Step Type may be Decision in addition to Action/Check. Action and Check remain descriptive only; Decision changes execution navigation.
- **Step Type** (extended): the closed set grows by one to Action, Check, or Decision. Decision is the only member that affects navigation.
- **Decision Step**: a Step whose Step Type is Decision; carries the same optional detail fields as any Step (instructions/command/expected-result, all optional) plus at least two Options. Completing it means choosing an Option, which routes the run.
- **Option**: a named choice on a Decision Step; carries a label the responder reads and routes to a Target Step. Frozen into the Runbook Version at publish alongside the Step.
- **Target Step**: the Step an Option routes to; the reference is frozen at publish and survives the Version freeze unchanged.
- **Taken Path**: the ordered sequence of Steps actually reached in an Execution, determined by the Options chosen at Decision Steps; for a Runbook with no Decision Steps it is every Step in order. It is the denominator for Computed Review coverage.
- **Step Record** (from slice 03): the append-only captured outcome of acting on a Step; for a Decision Step the recorded fact is which Option was chosen.
- **Computed Review** (redefined): coverage is computed over the Taken Path; Steps on a branch not taken are reported as not reached, not skipped.
- **Runbook / Runbook Version / Version Pin / Execution** (from slices 01/03): unchanged except that a Version now freezes Decision routing and an Execution now walks a Taken Path.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An author can publish a runbook that encodes at least one genuine fork (a Decision Step whose Options lead to different Steps) as a single runbook, rather than flattening it into one list or maintaining near-duplicate runbooks (demonstrated in the demo; adoption hypothesis from the PRD, to be corrected against real use).
- **SC-002**: A procedure that previously required near-duplicate runbooks for its variations can be rebuilt as one branched runbook in the demo, with the duplicates retired.
- **SC-003**: In a walkthrough, a responder running a branched runbook is presented only the Steps on their chosen path and never the Steps on a branch not taken.
- **SC-004**: For a branched run, the Computed Review makes clear which Option was taken at each Decision and reports every not-taken-branch Step as not reached, with zero cases where a not-taken Step is shown as skipped.
- **SC-005**: 100% of runbooks with no Decision Steps author, publish, run, and review identically to before this feature, verified by the prior slices' tests remaining green and unmodified.
- **SC-006**: A published branched Runbook Version's Options and routing read identically every time and never change after any later edit or re-publish of the Runbook.

## Assumptions

- **Language ratification**: the constitution glossary amendment (v1.1.0) that adds the Decision Step Type and the routing terms (Option, Target Step, Taken Path) and redefines Computed Review coverage is ratified and in force; this spec uses those binding terms.
- **Minimal Decision-Flow appetite**: the slice builds explicit human-chosen decision routing only. Out of scope (PRD non-goals): condition-expression branching, automated branch selection, loops/back-edges, Gate/approval Steps, parallel paths, automatic merge/rejoin semantics, a visual flow-builder, new per-Step content fields, and any change to Version immutability, pinning, deletion, or one-open-Execution-per-Incident.
- **Routing reference mechanism (H1)** — how an Option's Target Step is referenced so it survives reordering and the Version freeze (by stable Step identity vs. by position) is a planning decision (ADR); the spec requires only that routing never silently re-points (FR-003) and freezes immutably (FR-004).
- **How a chosen Option is recorded (H2)** — whether resolving a Decision is captured as a Step Record outcome or a distinct event is a planning decision (ADR); the spec requires only that which Option was chosen is recorded (FR-008) and shown in the review (FR-014).
- **Not-reached uses the existing state (H3)** — untaken-branch Steps are reported using the existing not-reached state distinct from skipped, per the ratified Computed Review redefinition (FR-013); confirmed at plan.
- **Decision Step content (H4)** and **publish-time validation rules (H5)** were resolved in the `/speckit.clarify` session 2026-07-01 (see Clarifications): a Decision Step reuses the existing optional Step detail fields plus Options (FR-018); publish blocks dangling targets, loops, and Decisions with <2 Options, warns on unreachable Steps, and allows path endings (FR-017).
- **Authoring/run UI (H6)** stays within the existing hash-routing navigation with no visual flow-builder (conflict register C-003); how modest the read/edit surface is settles in the spec/plan.
- **Conflict register** entries this feature is expected to touch — C-001 (sequential Version numbering vs. routing references), C-004 (routing invariants live in the aggregate), C-006 (the closed Step Type enum grows by Decision), and C-007 (routing resolves through the Version Pin) — are checked and resolved during `/speckit.plan`, per the constitution. C-002 (additive migration) applies to the schema change; C-005 (markdown rendering) is unaffected.
- **No authentication exists**, so no per-Step or per-Decision actor is captured (consistent with the actor deferred until accounts exist).
