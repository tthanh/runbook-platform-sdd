# PRD Register

Single source of truth for initiative status. Every transition is a committed
edit to this file.

**Status legend:** Registered → Discovery (full / inherited) → Workshop /
Skip recorded → PRD → Specify → Plan → Tasks → Implement → Released

| # | Initiative | Objective | Status |
|---|------------|-----------|--------|
| 1 | 01-versioned-runbook-execution | Author versioned runbooks; an execution pins a version and records every step, so the post-incident review is computed, not reconstructed | Released |
| 2 | 02-rich-domain-model | Structural: enforce domain invariants inside aggregates (ADR-0003) | Tasks |

## Artifact ledger

| Initiative | Discovery | Workshop hotspots | ADRs | Spec folder | Tag | Retro |
|------------|-----------|-------------------|------|-------------|-----|-------|
| 01-versioned-runbook-execution | Finalized 2026-06-12 (Go) | Ratified 2026-06-12; first PRD scoped to authoring (3 terms in constitution; 5 deferred) | 0001, 0002 (Accepted) | specs/001-runbook-authoring | v0.1.0 (2026-06-12) | 18/18 tests green; 2 production bugs caught by TDD (EF Core entity tracking, SQLite DateTimeOffset ordering); `EnsureCreated` correctly deferred migration tooling; method respected appetite — all 3 user stories shipped in scope |
| 02-rich-domain-model | Inherited 2026-06-12 (Go) | Skipped with receipt 2026-06-12 | 0003 (Accepted) | specs/002-rich-domain-model | — | — |
