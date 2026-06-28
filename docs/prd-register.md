# PRD Register

Single source of truth for initiative status. Every transition is a committed
edit to this file.

**Status legend:** Registered → Discovery (full / inherited) → Workshop /
Skip recorded → PRD → Specify → Plan → Tasks → Implement → Released

| # | Initiative | Objective | Status |
|---|------------|-----------|--------|
| 1 | 01-versioned-runbook-execution | Author versioned runbooks; an execution pins a version and records every step, so the post-incident review is computed, not reconstructed | Released |
| 2 | 02-rich-domain-model | Structural: enforce domain invariants inside aggregates (ADR-0003) | Released |
| 3 | 03-runbook-execution | Run a published version against an incident: pin the version, record every step as it happens, compute the review from the records | Released |
| 4 | 04-rich-steps | Enrich a Step beyond a single title — structured detail (instructions, command, expected result) and a step type — so a published runbook is executable, not a list of vague titles. Branching deferred to a later initiative. | Released |

## Artifact ledger

| Initiative | Discovery | Workshop hotspots | ADRs | Spec folder | Tag | Retro |
|------------|-----------|-------------------|------|-------------|-----|-------|
| 01-versioned-runbook-execution | Finalized 2026-06-12 (Go) | Ratified 2026-06-12; first PRD scoped to authoring (3 terms in constitution; 5 deferred) | 0001, 0002 (Accepted) | specs/001-runbook-authoring | v0.1.0 (2026-06-12) | 18/18 tests green; 2 production bugs caught by TDD (EF Core entity tracking, SQLite DateTimeOffset ordering); `EnsureCreated` correctly deferred migration tooling; method respected appetite — all 3 user stories shipped in scope |
| 02-rich-domain-model | Inherited 2026-06-12 (Go) | Skipped with receipt 2026-06-12 | 0003 (Accepted) | specs/002-rich-domain-model | v0.1.1 (2026-06-13) | 18/18 tests green and unmodified — refactor commit touched zero test files, proving behavior held; schema byte-identical (C-002 untouched); invariants relocated into the Runbook aggregate with no new endpoints, fields, or `Rename` path (complexity stayed earned); pragmatic encapsulation per ADR-0003 honored the two-evening appetite |
| 03-runbook-execution | Inherited 2026-06-13 (Go) — deferred payoff half of 01; appetite a couple of evenings | Inherited from 01; 5 terms ratified into the constitution 2026-06-13 (glossary now 8 terms; Step Record "who" deferred); PRD clarified 2026-06-13 (manual close only, capture-not-enforce order); H2/H5 → ADRs at plan, H4 → spike, H3 stance set | 0004 (H2, Accepted), 0005 (H5, Accepted); H4 resolved no-dual-write | specs/003-runbook-execution (Draft, 2026-06-13) | v0.1.4 (2026-06-28) | Released 2026-06-28 as v0.1.4 — T030 closeout (the only task left open after implement). Code was complete (T001–T029) and dogfooding it surfaced slice 04. Backend execution suites green; 18 authoring tests unmodified against the migrated schema. Migrated slice 01's `EnsureCreated` → EF Core migrations (initial migration byte-identical, C-002 corrected). Stayed **one** bounded context — Incident is a foreign reference (ADR-0005, C-008), correcting the slice-01 "second context" expectation. Version pin holds mid-incident (ADR-0004, C-007). New NFRs: append-only records, manual-close-only, review-computed-on-read. Released out of implementation order (after 04, which took v0.1.3) — tag reflects release order. See "Release tags" below. |
| 04-rich-steps | Finalized 2026-06-15 (Go) — new problem (thin Step surfaced by dogfooding); appetite a couple of evenings, branching explicitly OUT | RUN 2026-06-15 (compiled) — redefines **Step** + new term **Step Type**; ratified into the constitution glossary 2026-06-15 (9 terms); no new events; H1–H5 carried (H2 markdown/XSS, H3 Step Type taxonomy, H4 C-002 migration, H5 branching-deferred boundary) | 0006 (H2, Accepted 2026-06-15), 0007 (H3, Accepted 2026-06-15) | specs/004-rich-steps (Draft, 2026-06-15) | v0.1.3 (2026-06-28) | PRD finalized 2026-06-15: six-pass review clean; scope set to instructions+command+expected-result, minimal Action/Check type, detail in run + Computed Review; formatting resolved to markdown. Spec 2026-06-15: quality checklist clean first pass, zero NEEDS CLARIFICATION. Plan 2026-06-15: no constitution violations; ADR-0006 (hand-rolled escape-first markdown, no new dep) + ADR-0007 (closed Action/Check enum) Proposed; H1/H4/H5 settled in research; one additive migration (C-002). ADRs 0006/0007 Accepted 2026-06-15 (separate commits); tasks unblocked. Tasks 2026-06-15: 27 tasks over 6 phases (Setup, Foundational, US1 MVP, US2, US3, Polish); tests included; only new dep is dev-only vitest for ADR-0006 markdown safety. Implement 2026-06-15: all 27 tasks done; backend 46 tests green (39 prior unmodified + 7 new), frontend build green + 8 markdown safety tests green; one additive migration (AddStepDetail); Text kept as title. Pre-existing lint debt unchanged (1 set-state-in-effect from repo idiom). Released 2026-06-28 as v0.1.3 (see "Release tags" below) — closeout re-verified green (backend 46, frontend 8 + build); architecture.md amended (C-002 corrected to EF migrations, +C-005 markdown sanitization / +C-006 closed StepType enum, rich-step bounded-context note); appetite held — two evenings, branching stayed OUT. Carryover: architecture.md still owes a slice-003 execution bounded-context write-up, and 03-runbook-execution remains at Implement (untagged, no retro) — both since resolved (03 released 2026-06-28 as v0.1.4). |

## Release tags

**Rule:** git tags follow **release order**, not implementation order. A tag is
created at closeout (constitution "After implement"), so an initiative built
earlier but released later gets a higher tag.

| Tag | Date | What |
|-----|------|------|
| v0.1.0 | 2026-06-12 | 01-versioned-runbook-execution (authoring) |
| v0.1.1 | 2026-06-13 | 02-rich-domain-model (structural refactor) |
| v0.1.2 | 2026-06-15 | Frontend visual refresh — not an initiative (no PRD, no spec folder) |
| v0.1.3 | 2026-06-28 | 04-rich-steps |
| v0.1.4 | 2026-06-28 | 03-runbook-execution (built before 04, released after it) |

Note: 03 and 04 were released out of order — 04 reached closeout first and took
v0.1.3, so 03 took v0.1.4. v0.1.2 is a non-initiative refresh; its git tag was
added retroactively on 2026-06-28 to match its long-standing RELEASE_NOTES entry.
