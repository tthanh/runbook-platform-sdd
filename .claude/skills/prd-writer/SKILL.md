---
name: prd-writer
description: Write or update a PRD for an initiative in docs/initiatives/NN-name/. Use when asked to draft, write, create, or revise a PRD, or to turn discovery/workshop outputs into a PRD.
---

# PRD writer

Write or update a PRD for an initiative under `docs/initiatives/NN-name/`.
Read every input first, then write the PRD at PM altitude — what, why, and
how it is measured, never how it is built.

## Inputs (read before writing)
- `docs/initiatives/{{NN-name}}/discovery.md` — appetite, payer hypothesis,
  evidence flags carry into the PRD
- `docs/initiatives/{{NN-name}}/workshop.md` — ratified language, hotspots,
  open questions; every workshop open question MUST appear in the PRD
- `docs/architecture.md` — conflict register: state which entries this
  initiative touches, or "none"
- The glossary in `.specify/memory/constitution.md` — terms are binding

## Structure (the template, exactly)
Context / Objective / Non-goals / Metrics / Open questions

## Rules
- PM altitude only: what, why, how-measured. ZERO implementation
  vocabulary — no technology names, storage choices, service names,
  endpoint shapes, schema details. If the source material contains them,
  they belong to /speckit.plan; leave them out.
- Metrics are hypotheses with a stated basis: "<target> — basis:
  <reasoning or comparison>; treated as a hypothesis to correct, not a
  commitment." Never a bare number.
- Non-goals are fencing for a machine reader: list what an eager agent
  could plausibly build from the Objective that is not wanted, and fence
  each one explicitly.
- Unknowns are marked, never smoothed over: use
  [NEEDS CLARIFICATION: ...] inline. A visible hole is better input than
  a confident sentence hiding one.
- Plain English, short sentences, no buzzwords. One sentence per line
  (semantic line breaks) so diffs review cleanly.
- The document is in-world: it describes the product and its users only.
- Output: the PRD file content. Do not run /speckit.specify. Do not
  update the register — the author commits and transitions status.
