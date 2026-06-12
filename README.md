# SDD Method Starter

**A domain-agnostic spec-driven-development starter: a documented pipeline that
carries an idea from a one-page discovery to shipped code, wrapped around
GitHub Spec Kit — where the process is the product.**

This repo is a reusable *method*, not a product. Every feature earns its way
from a one-page idea to shipped code through a documented pipeline; the commit
history, the specs, and the decision records are the artifact. Start a project
on it and the same path runs for every feature.

## How a feature gets built here

This is the centerpiece. Every feature travels the same path:

```
  Discovery* → (Workshop)** → PRD → [Spec Kit: Specify → Plan → Tasks → Implement] → Release → Retro
                                                                                          │
  └───────────────────────────────────────────────────────────────────────────────────────┘
                          the loop closes: specs & decision records are amended
```

*Full page for a new problem; for a consequence of an already-decided one
it shrinks to a pointer, an appetite, and a go/no-go — never to nothing.

**Runs when an initiative introduces new language, new events, or crosses
context boundaries; otherwise skipped with a recorded reason.

- **Discovery** — answers a single question: *does this problem deserve to
  exist?* No solution welded on yet. A full page for a new problem; for a
  consequence of an already-decided one it shrinks to a pointer, an appetite,
  and a go/no-go — never to nothing.
- **Domain Workshop** — event storming, language decisions, and hotspots; the
  shared vocabulary is fixed here before any code can blur it.
- **PRD** — outcomes, explicit non-goals, and metrics stated as falsifiable
  hypotheses, not vanity targets.
- **Specify → Plan → Tasks → Implement** — GitHub Spec Kit drives the middle;
  the agent implements against the spec.
- **Release → Retro** — the cumulative architecture doc and decision records
  are amended with what's now true, and the loop closes.

## Two layers

**`docs/` — the human layer.** Not part of the Spec Kit flow — it lives in
this repo for convenience, so the whole journey of each feature (discovery,
workshop, PRD, decisions) is visible in one place rather than scattered
across wikis and boards. Discovery notes, workshop records, and PRDs live
in `docs/initiatives/NN-name/`. Decisions live in `docs/adr/` (each with
the condition that would reverse it). Cross-feature truth lives in
`docs/architecture.md`. Status lives in `docs/prd-register.md`.

**Everything else — the SDD engine.** From the PRD onward, GitHub Spec Kit
drives: `/speckit.specify` consumes the PRD into `specs/NNN-*/spec.md`,
then plan → tasks → implement. The agent's rules live in
`.specify/memory/constitution.md` — deliberately minimal, because it is
injected into every command.

The boundary in one sentence: **docs/ shows how an idea becomes a PRD;
Spec Kit takes it from there.**

## The rules of the game

The [constitution](.specify/memory/constitution.md) is binding, not decorative:

- **One meaning per word.** A binding glossary means the same thing in specs,
  code, events, and commits — and it starts **empty**: the first workshop
  introduces terms by amendment.
- **Complexity is earned, never anticipated** — a requirement must justify it.
- **ADRs are accepted in their own commit** — proposed during planning, accepted
  by a separate human-authored commit, and immutable once accepted.
- **Acceptance criteria use EARS** — *WHEN ‹condition› THE SYSTEM SHALL ‹behavior›.*
- **No dual-writes** — cross-store propagation rides the outbox.

## Prior art, credited

- **[GitHub Spec Kit](https://github.com/github/spec-kit)** — the engine driving
  Specify → Plan → Tasks → Implement.
- **Kubernetes KEPs** — initiative folders and chain of custody.
- **Kiro** — EARS notation for acceptance criteria.

**One deliberate deviation:** the popular SDD tooling starts at *"describe the
feature"* and ends at *"implement."* This starter adds a stage **before** the
spec (Discovery + Workshop — how an idea earns the right to enter the path) and
a cumulative doc **after** it (architecture — what the system remembers across
every trip through the loop).

## Start a project on this

1. Register your first initiative in [`docs/prd-register.md`](docs/prd-register.md)
   (status `Registered`).
2. Create `docs/initiatives/NN-name/` from the forms in
   [`docs/templates/`](docs/templates/).
3. Run **Discovery → (Workshop) → PRD** before any `/speckit.*` command — the
   constitution requires an approved PRD as the input to `/speckit.specify`.

The glossary starts empty; your first workshop fills it.
