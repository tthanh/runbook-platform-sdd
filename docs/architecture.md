# Architecture

Cross-feature truth only; amended at release time per the constitution.

## Context map

**System boundary**: local developer machine (cross-platform .NET + browser).
No cloud hosting, no external integrations, no auth provider in scope.

### Components

| Component | Path | Role |
|-----------|------|------|
| Runbook Authoring API | `backend/` | ASP.NET Core minimal API; owns all domain logic and persistence; exposes HTTP endpoints at `/api/*` |
| Web UI | `frontend/` | React SPA (Vite / TypeScript); consumes the API via `/api` proxy; hash-routed; renders all three authoring user stories |
| SQLite database | `runbook-platform.db` (runtime, gitignored) | Single persistent store; owned by the API process; one file, one schema |

### Bounded context (slice 001)

One bounded context: **Runbook Authoring**.
The Runbook, Runbook Version, and Step entities (ratified glossary, constitution) live here.
No other bounded contexts exist yet; the execution slice will introduce a second one when built.

**Runbook is the aggregate root** (slice 002, ADR-0003). Every domain invariant — the publish
gate, the version freeze, and sequential numbering — is enforced inside aggregate behavior
(`Runbook.Create` / `ReplaceSteps` / `Publish`), not in HTTP handlers. Runbook Version and its
frozen Steps are constructed only by `Runbook.Publish()` and expose no mutation afterward.
Endpoints are load → call method → save → map; a single `DomainException` is mapped once at the
endpoint seam to the `{ "error": "…" }` 400 shape. Behavior, HTTP contract, and schema are
identical to slice 001 — this was a structural refactor, no functional change.

**Step carries structured detail** (slice 004, ADR-0006/0007). A Step is no longer a bare title:
it adds optional `Instructions`, `Command`, and `ExpectedResult` (markdown text) plus a required
`StepType` (`Action` | `Check`, default `Action`). All four fields are validated and frozen inside
the aggregate (`Runbook.ReplaceSteps` → `RunbookVersion.Freeze`) exactly as the title is, so C-004
holds; publish copies all four into each frozen `RunbookVersionStep`, leaving earlier Versions'
detail unchanged. Detail is rendered in the frontend through a hand-rolled escape-first markdown
renderer (`frontend/src/lib/markdown.ts`, ADR-0006) — HTML is escaped before a fixed whitelist is
applied — so no markdown or sanitizer package is added. Branching between steps is explicitly out
of scope.

## NFRs

Established by slice 001 (`specs/001-runbook-authoring`); binding on future slices unless explicitly superseded by an ADR.

| NFR | Statement | Source |
|-----|-----------|--------|
| No authentication | No sign-in, accounts, or permissions are required for any authoring action | FR-010 |
| Immutability | Published Runbook Versions cannot be modified or deleted; no code path exists to do so | FR-006, FR-011 |
| No deletion | Runbooks and Runbook Versions are never deleted; no DELETE handlers exist in the API | FR-011 |
| Single-user interactive | No concurrent-write guarantees beyond SQLite serialized writes; no multi-user session support | Slice 001 scope |
| Local only | No hosting, deployment, or cloud targets in this slice | Appetite |

## Conflict register

Entries here record constraints that future initiatives must not violate without an ADR.

| ID | Constraint | Established by | Impact on future slices |
|----|------------|----------------|------------------------|
| C-001 | `(RunbookId, Number)` unique index on `RunbookVersion` — version numbers are immutable sequential integers per Runbook | ADR-0001 | Execution slice: version-pinning reads this index; must not alter the uniqueness rule |
| C-002 | Schema managed by EF Core migrations in `backend/src/RunbookPlatform.Api/Data/Migrations/` (the `EnsureCreated` mechanism of slice 001 has been retired) | adopted specs/003-runbook-execution; specs/004-rich-steps adds the additive `AddStepDetail` migration | Every schema change ships as an additive EF Core migration; no table is dropped, and migrations must apply cleanly over the existing store |
| C-003 | Frontend uses hash routing — no router package | specs/001-runbook-authoring (appetite) | Execution slice frontend must stay within hash routing or earn a router via ADR |
| C-004 | Domain invariants live inside aggregates, enforced through behavior methods; HTTP handlers stay thin (load → method → save → map) | ADR-0003 (specs/002-rich-domain-model) | New invariants in any slice belong in aggregate behavior, not in endpoints; bypassing the aggregate to mutate state requires an ADR |
| C-005 | Step detail is rendered through the escape-first whitelist renderer (`frontend/src/lib/markdown.ts`): raw HTML, images, tables, and non-`http(s)`/`mailto` links are neutralised | ADR-0006 (specs/004-rich-steps) | Any change to how step detail is displayed must preserve the escape-first sanitization invariants (covered by `markdown.test.ts`); adding a markdown or sanitizer dependency requires an ADR |
| C-006 | `StepType` is a closed enum `{ Action, Check }` (default `Action`), validated in the aggregate and persisted as a string | ADR-0007 (specs/004-rich-steps) | Adding a step type requires an ADR amending the taxonomy; the enum↔string mapping and aggregate validation must stay in sync |
