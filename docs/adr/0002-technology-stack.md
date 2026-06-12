# 0002 — Technology stack & storage: .NET + React, SQLite

## Context

The platform has no code yet; the first slice (runbook authoring,
specs/001-runbook-authoring) needs a stack. The choice spans every future
feature — exactly the "affects multiple features, had real alternatives"
test for an ADR. The appetite is a couple of evenings, the slice is a small
web app (an authoring UI over a versioned store), and no compliance or
hosting constraints bind it (PRD). Stack direction was chosen by the author
when asked (2026-06-12), per the constitution's ask-before-deciding rule.

## Options considered

1. **TypeScript full-stack** — one language end to end, light framework,
   SQLite. Smallest cognitive surface.
2. **Python + FastAPI** + server-rendered pages, SQLite.
3. **Go** + server-rendered HTML, SQLite. Single binary, more upfront code.
4. **.NET (ASP.NET Core) + React** — typed backend with first-class
   transactional tooling (EF Core), separate typed SPA frontend.

Storage within any option: SQLite vs. PostgreSQL vs. JSON files.

## Decision

**Backend:** .NET 10 (LTS), ASP.NET Core minimal APIs, EF Core.
**Frontend:** React with TypeScript on Vite, talking to the HTTP API
(contract: specs/001-runbook-authoring/contracts/http-api.md).
**Storage:** SQLite, one database file, accessed only through the backend.

Chosen on the author's direction (.NET + React). SQLite over the storage
alternatives because the slice is single-author and local: it needs
transactions (version-number assignment, ADR-0001) but no server database
ops; JSON files would hand-roll exactly the guarantees SQLite gives for free.

## Trade-offs accepted

- Two languages (C# + TypeScript) and two toolchains — heavier than a
  single-language stack for a two-evening slice; accepted as the author's
  standing preference, and the seam (one JSON HTTP contract) keeps the cost
  contained.
- SQLite is single-writer; fine for single-author use, a known limit if the
  platform ever hosts concurrent teams (see flip condition).
- A SPA needs an API contract test seam rather than server-rendered
  simplicity; the constitution's tests-map-to-FR-ids rule is satisfied at
  that seam (research R6).

## Consequences

- Repository layout gains `backend/` (one API project + one test project)
  and `frontend/` (one Vite app) — see plan.md Project Structure.
- Domain code uses the binding glossary names (Runbook, RunbookVersion, Step).
- Integration tests run against the real API with a per-test SQLite file.
- Later slices inherit this stack by default; departures need a superseding
  ADR.

## Flip condition

If the platform moves beyond single-author local use (hosted, concurrent
writers, or retention/compliance constraints arrive), revisit storage —
PostgreSQL behind the same EF Core seam is the expected successor, via a
superseding ADR. If frontend weight proves unearned by the authoring UI,
collapsing to server-rendered pages is the simplification path.

## Status + date

Proposed — 2026-06-12
