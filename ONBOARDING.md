# START HERE

Onboarding for anyone — human or a fresh Claude Code session — picking up this
repo cold.

## What this is

**runbook-platform** — a project built on a spec-driven-development (SDD) method.
Product idea: *author versioned runbooks; an execution pins a version and records
every step, so the post-incident review is computed, not reconstructed.*

The repo is two things at once: a reusable **method** (the SDD pipeline) and the
**product** being built through it. The method is the point as much as the product.

## Read these first (they encode the rules)

1. [`CLAUDE.md`](CLAUDE.md) — agent orientation: the two-layer boundary and which
   parts of `docs/` the agent touches.
2. [`.specify/memory/constitution.md`](.specify/memory/constitution.md) — the
   binding contract: inputs/status, the two-commit ADR rule, engineering rules
   (EARS, complexity-earned, tests-map-to-req-ids), and the **Amendments** log
   (the "why" behind the method's shape).
3. [`README.md`](README.md) — the method framing and the pipeline diagram.

## The pipeline (every feature travels this)

```
Discovery* → (Workshop)** → PRD → [Spec Kit: Specify → Plan → Tasks → Implement] → Release → Retro
```
\*Discovery shrinks for a consequence of an already-decided problem, never to nothing.
\**Workshop runs only when an initiative needs new language, new events, or crosses contexts; otherwise a recorded skip.

Non-negotiables worth knowing up front:
- The **glossary starts empty** ([constitution](.specify/memory/constitution.md)) — the first workshop introduces terms by amendment.
- **No `/speckit.*` command runs until an approved PRD exists** at `docs/initiatives/NN-name/prd.md`.
- **ADRs are accepted in their own human-authored commit** (proposed during planning, immutable once accepted).
- Every **status transition is a committed edit** to [`docs/prd-register.md`](docs/prd-register.md).

## Where things stand right now

- **Initiative 01 — `versioned-runbook-execution`**: status **Registered**, Discovery **in progress (DRAFT)**.
- Live status board: [`docs/prd-register.md`](docs/prd-register.md).

## How to resume

Open [`docs/initiatives/01-versioned-runbook-execution/discovery.md`](docs/initiatives/01-versioned-runbook-execution/discovery.md).
It holds a strawman framing plus **four `[PENDING author input]` items** that must
be answered before Discovery can clear:

1. **Demand** — is the post-incident reconstruction pain actually felt, or assumed?
2. **Appetite** — how much for the first slice?
3. **Constraints** — team capacity, compliance/retention, tools it must coexist with.
4. **Go / No-go** + date.

Answer those → finalize `discovery.md` → flip the register to `Discovery` (committed)
→ decide Workshop-or-skip → write the PRD → only then run `/speckit.specify`.

## Repo map

```
.specify/            SDD engine (scripts, templates, extensions) + the constitution (memory/)
.claude/skills/      prd-writer, prd-reviewer, and the speckit-* command skills
docs/templates/      blank forms: discovery, workshop, prd, meta
docs/workshop-guide.md   human-only guide for running a workshop
docs/initiatives/    one folder per initiative (discovery → workshop → prd)
docs/adr/            decision records (template.md only, so far)
docs/architecture.md cross-feature truth: context map / NFRs / conflict register
docs/prd-register.md the status board (single source of truth)
specs/               Spec Kit output (spec → plan → tasks), created per feature
```
