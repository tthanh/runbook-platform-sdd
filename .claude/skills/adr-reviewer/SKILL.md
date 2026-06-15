---
name: adr-reviewer
description: Review an ADR before a human accepts it, using the six-pass decision-record lint. Use when asked to review, lint, critique, or check an ADR, or whether an ADR is ready to accept.
---

# ADR reviewer

Review an ADR (Architecture Decision Record) before it is accepted, using the
six-pass lint below. Produce a review report; never edit the ADR.

## Mindset
An ADR is a durable record a human is about to accept — and the constitution
makes it **immutable once Accepted** (reversals are new superseding ADRs). A
weak ADR locks in a decision whose rationale can't be reconstructed later, or
hides that no real alternative was ever weighed. Read for the future engineer
who finds this ADR in two years and must trust *why*, not just *what*. Hostile
on the reasoning, silent on the author. Three failure modes to hunt:
- **Inevitability** — the decision is presented as the only option (no genuine
  alternative considered, or only strawmen).
- **All upside** — costs and consequences are not honestly admitted.
- **No exit** — no concrete, observable condition that would flip the decision.

## Inputs
The ADR at `docs/adr/NNNN-*.md`, plus (context only, never edited): the
constitution (`.specify/memory/constitution.md` — its Decisions rules, the
binding glossary, and the conflict register in `docs/architecture.md`), the
`spec.md` / `plan.md` that motivated the decision, and sibling ADRs in
`docs/adr/`.

## The six passes, in order
1. **Context** — Is the problem and its forces stated *without presupposing the
   answer*? Is it grounded (a code reference, a spec FR, a workshop hotspot id,
   an appetite)? Flag context that smuggles in the chosen option, or that states
   no real tension (if there's no force, there's no decision).
2. **Options** — Are there ≥2 *genuine* alternatives, each with a one-line "why
   rejected"? The chosen option must appear here. Flag strawmen (options nobody
   would pick), a missing status-quo/do-nothing option, and any decision with
   only one option listed.
3. **Decision** — Is it singular, unambiguous, and actionable — does it actually
   answer the Context's question with no hedging? Could a reader implement it
   without guessing? Flag vagueness, multiple decisions smuggled into one, or a
   decision that doesn't match the option it claims to pick.
4. **Trade-offs & Consequences** — Are real costs admitted (not just benefits)?
   Are consequences *concrete* — what changes in the data model, the conflict
   register, sibling ADRs, FRs, or future slices? Flag an all-upside ADR or
   consequences so generic they say nothing.
5. **Flip condition** — Is there a concrete, *observable* trigger that would make
   a future team revisit or supersede this? The template requires one. Flag a
   missing flip condition or a vague one ("if it doesn't work out").
6. **Governance & consistency** — Status + date present and valid for the
   lifecycle (Proposed before acceptance; Accepted only via a human commit)?
   Number unique and sequential? Glossary terms used with their one binding
   meaning? Do the conflict-register entries it claims to touch actually exist
   and say what the ADR says? Does it contradict an Accepted ADR without
   superseding it? Was the constitution's process honored (human-context
   questions before the Decision; an Accepted ADR not edited semantically)?

## Output format
A review report, never an edited file:
1. Verdict: READY TO ACCEPT / ACCEPT WITH EDITS / NOT READY.
2. Findings by pass: quote (exact line) / problem (one sentence: what a future
   reader would misbelieve, or what the decision fails to settle) / proposal
   (replacement text, added option, admitted cost, flip condition, or fix).
3. Contradictions section: ADR vs constitution / glossary / conflict register /
   spec / sibling ADRs.
4. If a pass finds nothing: "Pass N: clean." A clean pass is information — do not
   manufacture findings to seem thorough.
Do not edit the ADR. Do not change its Status (acceptance is a human commit).
