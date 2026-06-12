# Runbook Platform

> Author versioned runbooks; an execution pins a version and records every step,
> so the post-incident review is **computed, not reconstructed**.
> Built using spec-driven development (SDD).

This repository is two things at once:

- **The product** — a runbook platform, built one feature at a time.
- **The method** — a documented SDD pipeline that carries each feature from a
  one-page discovery to shipped code, wrapped around
  [GitHub Spec Kit](https://github.com/github/spec-kit). The commit history,
  specs, and decision records are the deliverable as much as the code.

**Status:** the first feature, `01-versioned-runbook-execution`, is in flight —
see [`docs/prd-register.md`](docs/prd-register.md) for the live status board.

## How a feature gets built

Every feature travels the same path:

```
Discovery* → (Workshop)** → PRD → [Spec Kit: Specify → Plan → Tasks → Implement] → Release → Retro
                                                                                       │
└──────────────────────────────────────────────────────────────────────────────────────┘
                        the loop closes: specs and decision records are amended
```

\* Full page for a new problem; for a consequence of an already-decided one it
shrinks to a pointer, an appetite, and a go/no-go — never to nothing.

\*\* Runs when an initiative introduces new language, new events, or crosses
context boundaries; otherwise skipped with a recorded reason.

- **Discovery** — answers one question: does this problem deserve to exist? No
  solution attached yet.
- **Workshop** — event storming, language decisions, and hotspots; the shared
  vocabulary is fixed here before any code can blur it.
- **PRD** — outcomes, explicit non-goals, and metrics stated as falsifiable
  hypotheses, not vanity targets.
- **Specify → Plan → Tasks → Implement** — GitHub Spec Kit drives the middle;
  the agent implements against the spec.
- **Release → Retro** — the architecture doc and decision records are amended
  with what is now true, and the loop closes.

## Two layers

**`docs/` — the human layer.** Not part of the Spec Kit flow; it lives here so
the full journey of each feature (discovery, workshop, PRD, decisions) is visible
in one place rather than scattered across wikis and boards. Discovery notes,
workshop records, and PRDs live in `docs/initiatives/NN-name/`. Decisions live in
`docs/adr/`, each with the condition that would reverse it. Cross-feature truth
lives in `docs/architecture.md`. Status lives in `docs/prd-register.md`.

**Everything else — the SDD engine.** From the PRD onward, GitHub Spec Kit
drives: `/speckit.specify` consumes the PRD into `specs/NNN-*/spec.md`, then
plan → tasks → implement. The agent's rules live in
[`.specify/memory/constitution.md`](.specify/memory/constitution.md) —
deliberately minimal, because it is injected into every command.

In one sentence: **`docs/` shows how an idea becomes a PRD; Spec Kit takes it
from there.**

## Binding rules

The [constitution](.specify/memory/constitution.md) is enforced, not decorative:

- **One meaning per word.** A binding glossary means the same thing in specs,
  code, events, and commits — and it starts **empty**; the first workshop
  introduces terms by amendment.
- **Complexity is earned, never anticipated** — a requirement must justify it.
- **ADRs are accepted in their own commit** — proposed during planning, accepted
  by a separate human-authored commit, and immutable once accepted.
- **Acceptance criteria use EARS** — *WHEN ‹condition› THE SYSTEM SHALL ‹behavior›.*
- **No dual-writes** — cross-store propagation rides the outbox.

## Built on

- **[GitHub Spec Kit](https://github.com/github/spec-kit)** — drives
  Specify → Plan → Tasks → Implement.
- **Kubernetes KEPs** — initiative folders and chain of custody.
- **Kiro** — EARS notation for acceptance criteria.

This method adds a stage **before** the spec — Discovery and Workshop, where an
idea earns its way onto the path — and a cumulative architecture doc **after**
it, holding what the system remembers across every pass through the loop.
Standard SDD tooling runs only from "describe the feature" to "implement."

## Use this method for your own project

1. Register your first initiative in
   [`docs/prd-register.md`](docs/prd-register.md) (status `Registered`).
2. Create `docs/initiatives/NN-name/` from the forms in
   [`docs/templates/`](docs/templates/).
3. Run **Discovery → (Workshop) → PRD** before any `/speckit.*` command — the
   constitution requires an approved PRD as the input to `/speckit.specify`.

The glossary starts empty; your first workshop fills it.
