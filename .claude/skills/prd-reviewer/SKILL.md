---
name: prd-reviewer
description: Review a PRD before /speckit.specify using the six-pass lint framework. Use when asked to review, lint, critique, or check a PRD, or whether a PRD is ready for specify.
---

# PRD reviewer

Review a PRD before `/speckit.specify` using the six-pass lint framework.
Produce a review report; never edit the PRD.

## Mindset
The PRD is machine-consumed input. An agent resolves ambiguity by silently
inventing something plausible. Find every place that could happen.
Hostile on the text, silent on the author.

## Inputs
The PRD at `docs/initiatives/{{NN-name}}/prd.md`, plus (context only, never
edited): `discovery.md`, `workshop.md`, `docs/architecture.md`, the
constitution glossary.

## The six passes, in order
1. Ambiguity — flag vague adjectives/adverbs (fast, easy, secure, robust,
   intuitive, seamless, properly...); propose a measurable replacement
   with a stated basis, or a [NEEDS CLARIFICATION] marker. Never propose
   a number without its basis.
2. Testability — per requirement: state the test that could fail it; if
   none is imaginable, flag as a wish; propose an EARS rewrite
   (WHEN <condition> THE SYSTEM SHALL <behavior>) or removal.
3. Boundaries — list what an eager agent could plausibly build that the
   author probably does not want; each becomes a proposed non-goal; check
   existing non-goals actually fence the Objective.
4. Constraints — verify appetite, relevant NFRs, and conflict-register
   entries are carried or referenced; flag what's missing.
5. Unknowns — verify every workshop open question appears; find smooth
   sentences hiding unresolved decisions; propose [NEEDS CLARIFICATION]
   markers.
6. Altitude — flag every implementation word as a proposed deletion
   (belongs to /speckit.plan and ADRs); flag glossary violations.

## Output format
A review report, never an edited file:
1. Verdict: READY / READY WITH EDITS / NOT READY for /speckit.specify.
2. Findings by pass: quote (exact line) / problem (one sentence: what the
   agent would invent or break) / proposal (replacement, marker, non-goal,
   or deletion).
3. Contradictions section: PRD vs discovery / workshop / glossary.
4. If a pass finds nothing: "Pass N: clean." A clean pass is information —
   do not manufacture findings to seem thorough.
Do not edit the PRD. Do not run /speckit.specify.
