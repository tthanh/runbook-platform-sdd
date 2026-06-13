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
| C-002 | Schema managed by `EnsureCreated` — no EF Core migrations exist | specs/001-runbook-authoring T005 | First schema change in any slice requires migrating to EF Core migrations before applying the change |
| C-003 | Frontend uses hash routing — no router package | specs/001-runbook-authoring (appetite) | Execution slice frontend must stay within hash routing or earn a router via ADR |
| C-004 | Domain invariants live inside aggregates, enforced through behavior methods; HTTP handlers stay thin (load → method → save → map) | ADR-0003 (specs/002-rich-domain-model) | New invariants in any slice belong in aggregate behavior, not in endpoints; bypassing the aggregate to mutate state requires an ADR |
