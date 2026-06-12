# Implementation Plan: Runbook Authoring

**Branch**: `claude/distracted-brahmagupta-013312` | **Date**: 2026-06-12 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-runbook-authoring/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

An author creates a Runbook (named, ordered Steps), edits it, and publishes immutable Runbook Versions identified by per-Runbook sequential version numbers; the latest published version is current, all earlier ones stay viewable.
Approach: a small web application — ASP.NET Core API with SQLite storage and a React single-page frontend — sized to the "couple of evenings" appetite.
Decisions with real alternatives are recorded in ADR-0001 (version identity) and ADR-0002 (technology stack & storage); this plan references those ids and carries no decision rationale itself.

## Technical Context

**Language/Version**: C# / .NET 10 (LTS) backend; TypeScript / React frontend (per ADR-0002)

**Primary Dependencies**: ASP.NET Core minimal APIs, EF Core (SQLite provider); React + Vite (per ADR-0002)

**Storage**: SQLite, single database file (per ADR-0002)

**Testing**: xUnit integration tests against the HTTP API via WebApplicationFactory; each test names the FR id(s) it covers (constitution: tests map to requirement ids)

**Target Platform**: Local developer machine (cross-platform .NET + a browser); no hosting/deployment this slice

**Project Type**: Web application (backend + frontend)

**Performance Goals**: Single-author interactive use; page and API responses comfortably under 1s — no load targets (none earned by the spec)

**Constraints**: No authentication/accounts (FR-010); no deletion of Runbooks or published Runbook Versions (FR-011); published Runbook Versions immutable (FR-006); appetite "a couple of evenings" — cut scope before extending

**Scale/Scope**: ~30-engineer organisation, a handful of Runbooks (~5 in the first month per SC-004), 3 user stories, 11 functional requirements

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Gate | Status | Note |
|------|--------|------|
| Approved PRD as input, referenced by path | PASS | Spec references `docs/initiatives/01-versioned-runbook-execution/prd.md` |
| Binding glossary respected | PASS | Only Runbook, Runbook Version, Step used as domain names — in spec, this plan, data model, contracts, and (during implement) code |
| Complexity earned, never anticipated | PASS | One backend project, one frontend app, one SQLite file; no layers, queues, or patterns beyond what FRs require. No execution-slice scaffolding (no outbox — see dual-writes gate) |
| Acceptance criteria use EARS | PASS | All spec scenarios are WHEN ... THE SYSTEM SHALL ... |
| No dual-writes; cross-store propagation rides the outbox | PASS (trivially) | Single store (one SQLite database); no cross-store propagation exists in this slice, so no outbox is built — adding one would be unearned complexity |
| Tests map to requirement ids | PASS (planned) | Integration tests named/annotated with FR ids; quickstart.md maps scenarios to FRs |
| Conflict register checked | PASS | `docs/architecture.md` conflict register is empty (pending) — this feature touches **none** |
| Contested decisions promoted to ADRs, Proposed | PASS | ADR-0001 (version identity), ADR-0002 (stack & storage) — both Status: Proposed, awaiting human acceptance |
| Stop after plan; tasks blocked while ADRs Proposed | NOTED | /speckit.tasks must not run until ADR-0001 and ADR-0002 are Accepted by human commit |

**Post-design re-check (after Phase 1)**: PASS — the data model adds no entities beyond the three glossary terms (plus the frozen Step copy inside a Runbook Version, which is the immutability requirement FR-006 made concrete); contracts expose only the operations the FRs name; no new violations introduced.

## Project Structure

### Documentation (this feature)

```text
specs/001-runbook-authoring/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
│   └── http-api.md
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

```text
backend/
├── src/
│   └── RunbookPlatform.Api/      # ASP.NET Core minimal API
│       ├── Domain/               # Runbook, RunbookVersion, Step (glossary names)
│       ├── Data/                 # EF Core DbContext + SQLite migrations
│       └── Endpoints/            # HTTP endpoints per contracts/http-api.md
└── tests/
    └── RunbookPlatform.Api.Tests/  # xUnit integration tests, named by FR id

frontend/
├── src/
│   ├── api/                      # typed client for the HTTP API
│   ├── components/               # list, editor, version viewer
│   └── pages/                    # Runbook list, Runbook detail (edit + versions)
└── tests/                        # (thin; behavior is covered at the API seam)
```

**Structure Decision**: Web application layout — `backend/` (single ASP.NET Core project + one test project) and `frontend/` (single Vite React app). One project per side; no additional projects until a requirement earns them.

## Decisions referenced (ADRs)

- **ADR-0001** — Runbook Version identity: per-Runbook sequential integers. `docs/adr/0001-runbook-version-identity.md` (Proposed)
- **ADR-0002** — Technology stack & storage: .NET + React, SQLite. `docs/adr/0002-technology-stack.md` (Proposed)

## Complexity Tracking

> No constitution violations — table intentionally empty.
