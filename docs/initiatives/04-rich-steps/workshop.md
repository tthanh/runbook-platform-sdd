# Workshop — 04-rich-steps

Level: big-picture (language, events, contexts, hotspots).
Design-level detail belongs in /speckit.plan, where contested decisions
are promoted to ADRs. Process: see docs/workshop-guide.md.

**Workshop test result: RUN (not skipped).** The glossary is not empty, but this
initiative **changes the meaning of a binding term** — *Step* — and proposes one
new term — *Step Type*. Changing binding language is a glossary amendment, which
is human-gated and may never happen ad hoc. One leg of the test is a "no" → a
workshop runs, sized narrowly to the Step language change.

> **Compile note.** Demonstration build, no live human board. Acting as scribe,
> this file was compiled from the finalized [discovery one-pager](discovery.md)
> and the field research below. The proposed language is **NOT yet ratified** and
> is **NOT in the constitution** — it awaits the human reviewer's verdict before
> the PRD is written (the human gate that the prior slices' terms passed).

## Attendees & date
- Date: 2026-06-15
- Facilitator: (demo) — compiled by AI scribe
- Participants: none live (demonstration); framing from [discovery.md](discovery.md)
- Reviewer: human — **pending** (must ratify the Step redefinition + Step Type,
  or amend them, before PRD-04)

## Event timeline
This slice adds **no new domain events**. Enriching a Step changes the *content*
of an existing entity within events already ratified by slices 01/03:

1. **Runbook drafted** — an author writes/edits steps (now: title + detail).
2. **Runbook version published** — the draft is frozen, including the new fields.

Execution events (3–7: execution started, step marked done/skipped/failed,
closed, review computed) are **unchanged** — a responder still marks each Step
done/skipped/failed; only what they *read* on the step gets richer. Confirming
"no new events" is itself a scope guard: if the slice grows an event, it has
drifted toward branching and must stop.

## Language decisions → proposed glossary amendments (await ratification)
New or changed terms land in the constitution glossary, never ad hoc. The
following are **proposed here only** — they enter the binding glossary on a
ratified workshop, exactly as slices 01/03 did.

### Proposed amendment — **Step** (redefine the existing term)
| Current (binding) | Proposed |
|-------------------|----------|
| one ordered instruction within a runbook version. | one ordered instruction within a runbook version, carrying a **title** and optional **structured detail** — instructions, the command to run, and the expected result — classified by a **Step Type**. The title stays required; detail is additive and may be empty (so every existing Step remains valid). |

### Proposed new term — **Step Type**
| Term | Proposed meaning |
|------|------------------|
| **Step Type** | the classification of a Step by the kind of work it represents. Initial set proposed minimal: **Action** (do something) and **Check** (verify a condition / observe a result). Richer types that imply control flow — **Decision** (branch) and **Gate** (manual approval) — are **deferred with branching**, not coined now. |

> Step Type is proposed but borderline against "complexity is earned" — the human
> reviewer decides at PRD/plan whether it is in slice 04 or deferred. Captured
> here so the option isn't lost; not asserted as in-scope.

## Hotspots
Each becomes a spike or an ADR at /speckit.plan — none die in notes.

| # | Hotspot | Why it's contested | Destination | Owner |
|---|---------|--------------------|-------------|-------|
| H1 | **Rich-content freeze** — the new fields must freeze into the immutable Runbook Version at publish, exactly like `text` today | Touches version immutability (ADR-0001); confirm the freeze covers *all* new fields, not just title | Note (likely confirms ADR-0001, not a new ADR) | planner |
| H2 | **Markdown vs plain multi-line text** for the instructions body | Storing is cheap; rendering user content safely (XSS) is the real decision and adds frontend surface | ADR or spike | planner |
| H3 | **Step Type taxonomy** — fixed enum {Action, Check} vs open/extensible; in slice 04 or deferred | Determines schema and whether the slice earns the concept now | ADR + human decision | planner |
| H4 | **Schema migration for existing Steps** — new columns nullable/empty on existing rows | Conflict register C-002; must follow the confirmed two-migration pattern (constitution 2026-06-15), never EnsureCreated | Note (apply standing pattern) | planner |
| H5 | **Branching is OUT — record the deferral as a named non-goal** | Without an explicit boundary, decision/gate steps creep in and blow the appetite + immutability/coverage invariants | Boundary note (below) | facilitator |

## Boundary notes
- **Bounded context unchanged: `Runbook Execution`.** Authoring and running still
  live together; no new external context. Enriching a Step adds no upstream/
  downstream dependency.
- **Branching / decision flow is a deferred initiative, not this one.** A
  Decision Step (option → target step, à la incident.io Decision Flows / AWS SSM
  `aws:branch`) would break two shipped invariants — execution as a linear walk
  of positions, and Computed Review "coverage = every step." Those need their own
  ADRs and their own appetite. This slice does the high-leverage, low-risk half
  (rich content) and parks branching as the next workshop's subject.

## Open questions carried into the PRD
- **Which structured fields are in scope for two evenings?** Proposed floor:
  a markdown/multi-line **instructions** body (kills the "vague title" problem
  alone). Likely: **command** (copy-pasteable) and **expected result**. Probably
  deferred: reference links, per-step owner/role, rollback note. The PRD picks
  the line; cut fields before extending time.
- **Is Step Type in slice 04 or deferred with branching?** (H3 — human decision.)
- **Markdown or plain multi-line text** for the body? (H2 — security vs effort.)
- **Frontend scope:** does the run view show command + expected result inline as
  the responder works the step, and does VersionView/Computed Review surface the
  new detail? Where the read surfaces stop is part of the appetite.

## Field research (input to the above)

Captured from this session's investigation; full sources listed per item. Used
as the discovery Evidence and to shape the proposed Step anatomy. The recurring
finding: **manual, human-followed runbooks** (SRE playbooks, FireHydrant
freeform steps, incident.io Decision-Flow prompts) are the right analogue — not
the *automation* workflow engines (PagerDuty/incident.io Workflows = typed
machine actions), which model a different problem.

**What a step contains across tools**
- **Title** — universal (we have it).
- **Rich instructions / description body (markdown)** — FireHydrant "Freeform
  Text" step; incident.io Decision-Flow *Prompt*; SRE playbook entries. *The core
  gap.*
- **Command / code snippet (copy-pasteable)** — the hallmark of a "production-
  grade" SRE runbook step.
- **Expected result / verification** ("how you know it worked") — SRE workbook; AWS SSM.
- **Reference links** (dashboard/query/doc) — FireHydrant, SRE.
- **Owner / responsible role** — incident.io follow-ups (user *or team*), SRE.
- **Step type/kind** (action / check / decision / gate) — explicit in AWS SSM
  (`aws:approve`, `aws:branch`); implicit elsewhere.
- **Rollback / on-failure note** — SRE; AWS SSM `onFailure`; Ansible `rescue`.

**Branching (deferred — recorded for the next initiative)** — two paradigms in
the field: (1) explicit goto/choice — AWS SSM `aws:branch` (choice → `NextStep` +
`Default`), incident.io **Decision Flows** (node = prompt + options, each routing
to a next node; human-navigated tree); (2) condition-gated — per-step run/skip
conditions (`field op value`, AND/OR/NOT), e.g. PagerDuty `Condition` block
(AND-only, **no else, no jump**), FireHydrant/Rootly/Ansible `when`/GitHub `if`.
Even the big players keep in-procedure branching deliberately narrow. The
human-runbook-appropriate model is the Decision-Flow tree — to be designed in its
own slice.

**Sources** (selected, from the session's research):
- PagerDuty Incident Workflows / Logic actions — https://support.pagerduty.com/main/docs/incident-workflows ; https://support.pagerduty.com/actions/docs/condition
- incident.io Workflows, Expressions, Decision Flows — https://docs.incident.io/workflows ; https://docs.incident.io/workflows/expressions ; https://help.incident.io/articles/9192553935-decision-flows
- FireHydrant Runbooks & conditions — https://docs.firehydrant.com/docs/intro-to-runbooks ; https://docs.firehydrant.com/docs/runbook-conditions
- Rootly Workflows — https://docs.rootly.com/workflows/workflows
- AWS SSM `aws:branch` — https://docs.aws.amazon.com/systems-manager/latest/userguide/automation-action-branch.html
- Ansible blocks (block/rescue/always) — https://docs.ansible.com/projects/ansible/latest/playbook_guide/playbooks_blocks.html
- Google SRE Workbook, On-call (playbooks) — https://sre.google/workbook/on-call/
