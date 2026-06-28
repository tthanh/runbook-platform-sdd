# Tasks: Runbook Execution

**Input**: Design documents in `specs/003-runbook-execution/` — [plan.md](plan.md), [spec.md](spec.md), [research.md](research.md), [data-model.md](data-model.md), [contracts/http-api.md](contracts/http-api.md), [quickstart.md](quickstart.md)

**Tests**: Included. The constitution requires tests to map to requirement ids, and slice 01 established the integration-test contract (xUnit + `WebApplicationFactory`). The **18 authoring tests must stay green and unmodified**.

**Gate**: ADR-0004 and ADR-0005 are **Accepted** — implementation is unblocked.

**Appetite**: a couple of evenings. MVP = Phase 3 (US1). Cut from the bottom (US3 first, then US2 polish) before extending.

## Format: `[ID] [P?] [Story] Description`
- **[P]**: parallelizable (different file, no incomplete dependency)
- **[Story]**: US1 / US2 / US3 for user-story phases only

## Path conventions
Web app: `backend/src/RunbookPlatform.Api/`, `backend/tests/RunbookPlatform.Api.Tests/`, `frontend/src/`.

---

## Phase 1: Setup

- [x] T001 Add EF Core migrations tooling: add the `Microsoft.EntityFrameworkCore.Design` package reference to `backend/src/RunbookPlatform.Api/RunbookPlatform.Api.csproj` (C-002 / research R2); confirm `dotnet ef` is available.

## Phase 2: Foundational (blocking — migrate off `EnsureCreated` before any schema change)

- [x] T002 Generate the **initial** EF Core migration capturing the *existing* 001/002 schema (Runbooks, Steps, RunbookVersions, RunbookVersionSteps) in `backend/src/RunbookPlatform.Api/Data/Migrations/` — must reproduce the current `EnsureCreated` schema byte-for-byte (C-002 / R2).
- [x] T003 Switch startup from `Database.EnsureCreated()` to `Database.Migrate()` in `backend/src/RunbookPlatform.Api/Program.cs`.
- [x] T004 Regression checkpoint: run `dotnet test`; the **18 authoring tests pass unmodified** against the migrated schema (`git diff --stat backend/tests/` empty). Blocks all further work.

---

## Phase 3: User Story 1 — Run a published procedure and capture each Step outcome (P1) 🎯 MVP

**Goal**: A responder starts (or resumes) an Execution against a published Runbook, pins its current Version, and records each Step outcome as an append-only Step Record.

**Independent test**: Publish a Runbook, start an Execution, mark Steps with mixed outcomes out of order, re-mark one, resume by starting again — confirm Step Records exist per mark with correct Step/outcome/note/time.

### Domain (invariants inside the aggregate — ADR-0003 / C-004)
- [x] T005 [P] [US1] Add `StepOutcome` enum (Done/Skipped/Failed) in `backend/src/RunbookPlatform.Api/Domain/StepOutcome.cs`.
- [x] T006 [P] [US1] Add `StepRecord` in `backend/src/RunbookPlatform.Api/Domain/StepRecord.cs` — `Id`, `ExecutionId`, `StepPosition`, `Outcome`, `Note?`, `RecordedAt`, `Sequence`; private ctor for EF + internal ctor used only by `Execution`; no public mutation (FR-005, FR-008).
- [x] T007 [US1] Add `Execution` aggregate root in `backend/src/RunbookPlatform.Api/Domain/Execution.cs`: `static Start(Runbook runbook, string incidentId, string? incidentTitle)` — requires a published Version (FR-002, else `DomainException`), pins `PinnedRunbookVersionId` once (FR-001, ADR-0004), `Status=Open`; `RecordStep(int stepPosition, StepOutcome outcome, string? note)` — open-only, position must exist in the pinned Version, appends one Step Record (FR-004/006/007); `IncidentId` trimmed/non-empty; `IReadOnlyList<StepRecord>` over a private list.

### Data
- [x] T008 [US1] Map the execution model in `backend/src/RunbookPlatform.Api/Data/AppDbContext.cs`: `DbSet<Execution>`, `DbSet<StepRecord>`; backing-field/navigation config; **unique index on `Execution.IncidentId`** (FR-015).
- [x] T009 [US1] Add the migration creating the `Executions` and `StepRecords` tables (+ the unique index) in `backend/src/RunbookPlatform.Api/Data/Migrations/` (depends on T002/T008).

### Endpoints (thin: load → method → save → map)
- [x] T010 [US1] Add `ExecutionEndpoints.cs` in `backend/src/RunbookPlatform.Api/Endpoints/` and map the group in `Program.cs`; reuse the `DomainException → 400 { error }` filter, add `409` mapping for conflict cases.
- [x] T011 [US1] `POST /api/executions` — start/resume: pin current Version; if an **open** Execution exists for `incidentId` return it `200`, if **closed** refuse `409`, else `201`; `400` if the Runbook has no published Version (FR-001/002/015, clarification 2026-06-13). Per contracts/http-api.md.
- [x] T012 [P] [US1] `GET /api/executions/{id}` — run view: pinned Version's Steps + records + status; `404` unknown.
- [x] T013 [US1] `POST /api/executions/{id}/records` — append a Step Record; `409` if closed; `400` unknown position/outcome (FR-004/005/006/007/008).

### Tests (map to FR ids)
- [x] T014 [P] [US1] Integration tests in `backend/tests/RunbookPlatform.Api.Tests/ExecutionStartTests.cs`: start pins current Version (FR-001); refuse unpublished Runbook (FR-002); start against unknown Runbook 404/400.
- [x] T015 [P] [US1] Integration tests in `backend/tests/RunbookPlatform.Api.Tests/StepRecordingTests.cs`: record out of order (FR-007); optional note + no actor captured (FR-008); records append-only (FR-005); re-mark appends and end-state = latest (FR-006).
- [x] T016 [P] [US1] Integration test in `backend/tests/RunbookPlatform.Api.Tests/ExecutionResumeTests.cs`: second start for an incident with an open Execution resumes it, not a second run (FR-015).

### Frontend (hash-routed — C-003 / R7)
- [x] T017 [US1] Add the execution **run view** to `frontend/src/` following the slice-01 view/hash-routing pattern: start/resume a run, show the pinned Version's Steps, mark each Done/Skipped/Failed with an optional note. No router package.

**Checkpoint**: US1 is independently demoable — capture works end to end.

---

## Phase 4: User Story 2 — Close the Execution and obtain its Computed Review (P2)

**Goal**: Manually close an Execution and get the Computed Review derived on read.

**Independent test**: Run/record an Execution (US1), close it, confirm the review lists records chronologically, marks untouched Steps "not reached," and refuses post-close recording.

### Domain
- [x] T018 [US2] Add `Execution.Close()` in `Domain/Execution.cs` — `Open → Closed`, set `ClosedAt`; already-closed throws `DomainException` (FR-009/010).
- [x] T019 [US2] Add the Computed Review **read model + derivation** (computed on read, no persisted projection — R1) in `backend/src/RunbookPlatform.Api/` (e.g. `Domain/ComputedReview.cs` or an endpoint-level mapper): chronological timeline (R6 in-memory ordering by `RecordedAt`,`Sequence`), per-Step coverage with end state, `NotReached` derived for Steps with no record (FR-011/012/013).

### Endpoints
- [x] T020 [US2] `POST /api/executions/{id}/close` — close manually; `409` already closed; no incident-driven close path (FR-009).
- [x] T021 [US2] `GET /api/executions/{id}/review` — Computed Review for a closed Execution; `409` if still open; header shows title else id (FR-011/012/013, ADR-0005). Per contracts/http-api.md.

### Tests
- [x] T022 [P] [US2] Integration tests in `backend/tests/RunbookPlatform.Api.Tests/ExecutionCloseTests.cs`: close succeeds; recording after close refused `409` (FR-010); double-close refused.
- [x] T023 [P] [US2] Integration tests in `backend/tests/RunbookPlatform.Api.Tests/ComputedReviewTests.cs`: timeline chronological incl. multiple marks (FR-011); untouched Step = `NotReached` ≠ `Skipped` (FR-012); review reflects records exactly, only when closed (FR-013).

### Frontend
- [x] T024 [US2] Add the **review view** + a Close action to `frontend/src/` (hash-routed): timeline + coverage, title/id header.

**Checkpoint**: US1 + US2 = the full payoff — computed review from a real run.

---

## Phase 5: User Story 3 — The pinned Version holds for the life of the run (P3)

**Goal**: A mid-incident publish never moves a running Execution off its pinned Version, silently (ADR-0004).

**Independent test**: Start an Execution pinning Version N, publish Version N+1 of the same Runbook, confirm the Execution still shows/records against N and its review is set against N.

- [x] T025 [US3] Integration test in `backend/tests/RunbookPlatform.Api.Tests/PinIntegrityTests.cs`: start pins N → publish N+1 → run view still N, recording targets N's Steps, review set against N (FR-003, ADR-0004, SC-004). (Largely already satisfied by T007's design; this proves it.)
- [x] T026 [P] [US3] Confirm the run view (T017) never surfaces a "newer Version exists" signal — silent (ADR-0004).

**Checkpoint**: pin integrity proven.

---

## Phase 6: Polish & cross-cutting

- [x] T027 Walk [quickstart.md](quickstart.md) end-to-end against the running app (start/record/pin/close/review/resume/refuse paths).
- [x] T028 Final regression: `dotnet test` all green; **18 authoring tests unmodified** (`git diff --stat backend/tests/` shows only new execution test files).
- [x] T029 [P] Tidy execution response DTOs / mapping at the endpoint seam (no entity leakage), consistent with slice-01 style.
- [x] T030 Record the post-implement obligation: at release, amend `docs/architecture.md` — add the execution entities/NFRs, add Execution-context truth, and **correct the "second bounded context" line** to one context (ADR-0005); write the retro line + tag (constitution "after implement"). Done 2026-06-28: released as v0.1.4.

---

## Dependencies & order

- **Setup (T001) → Foundational (T002–T004)** block everything. T004 is a hard gate (authoring regression).
- **US1 (T005–T017)**: T005/T006 [P] → T007 → T008 → T009; endpoints T010 → T011/T012/T013; tests T014–T016 [P] after their endpoints; T017 frontend after endpoints.
- **US2 (T018–T024)** depends on US1 (Execution + records exist). T018 → T019 → T020/T021 → tests T022/T023 [P] → T024 frontend.
- **US3 (T025–T026)** depends on US1 (pin) and benefits from US2 (review against pin); mostly verification.
- **Polish (T027–T030)** last.

## Parallel execution examples

- After T007: `T005`, `T006` were already parallel; within US1 tests, `T014` / `T015` / `T016` run in parallel (separate test files).
- US2 tests `T022` / `T023` in parallel after their endpoints land.

## Implementation strategy

- **MVP = Phase 3 (US1)**: capture as it happens — demoable alone.
- **Then US2** for the computed-review payoff, **then US3** (cheap verification).
- If the couple-of-evenings appetite runs short: ship US1 + US2; US3 is mostly a test and can follow. Do **not** extend scope — cut.
- Migrations (T001–T003) are unavoidable groundwork, not optional.
