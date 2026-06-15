# Tasks: Rich Steps

**Input**: Design documents in `specs/004-rich-steps/` — [plan.md](plan.md), [spec.md](spec.md), [research.md](research.md), [data-model.md](data-model.md), [contracts/http-api.md](contracts/http-api.md), [quickstart.md](quickstart.md)

**Tests**: Included. The constitution requires tests to map to requirement ids. Backend uses xUnit + `WebApplicationFactory` (slice 01/03). A small frontend unit-test runner (**vitest**, dev-only) is added to cover ADR-0006's markdown safety invariants — no runtime dependency (C-003 holds). The **slice-01/03 tests must stay green and unmodified**.

**Gate**: ADR-0006 and ADR-0007 are **Accepted** (2026-06-15) — implementation is unblocked.

**Appetite**: a couple of evenings. MVP = Phase 3 (US1). Cut from the bottom (US3, then US2) before extending.

## Format: `[ID] [P?] [Story] Description`
- **[P]**: parallelizable (different file, no incomplete dependency)
- **[Story]**: US1 / US2 / US3 for user-story phases only

## Path conventions
Web app: `backend/src/RunbookPlatform.Api/`, `backend/tests/RunbookPlatform.Api.Tests/`, `frontend/src/`.

---

## Phase 1: Setup

- [X] T001 [P] Add a dev-only test runner to the frontend: add `vitest` to `devDependencies`, a `"test": "vitest run"` script, and a minimal config in `frontend/package.json` (+ `frontend/vitest.config.ts` if needed), so the markdown renderer's safety invariants can be unit-tested (ADR-0006). No runtime dependency is added (C-003). Backend needs no new package — migration tooling is already present from slice 03.

---

## Phase 2: Foundational (blocking prerequisites — domain, schema, shared frontend)

**⚠️ CRITICAL**: every user story reads or writes the enriched Step; no story work begins until this phase is complete and the regression checkpoint passes.

### Domain (invariants inside the aggregate — ADR-0003 / C-004)
- [X] T002 [P] Add `StepType` enum (`Action`, `Check`) in `backend/src/RunbookPlatform.Api/Domain/StepType.cs` (ADR-0007).
- [X] T003 Enrich `Step` in `backend/src/RunbookPlatform.Api/Domain/Step.cs`: add `Instructions`, `Command`, `ExpectedResult` (nullable strings) and `StepType` (default `Action`); keep `Text` as the required title (R1); construct only via `ReplaceSteps`.
- [X] T004 Enrich `RunbookVersionStep` in `backend/src/RunbookPlatform.Api/Domain/RunbookVersionStep.cs`: same four fields, frozen; construct only via `RunbookVersion.Freeze`.
- [X] T005 Update `Runbook.ReplaceSteps` (and the `RunbookVersion.Freeze` copy) in `backend/src/RunbookPlatform.Api/Domain/Runbook.cs` + `RunbookVersion.cs`: accept structured step inputs (title + optional instructions/command/expectedResult + type), validate title non-empty (FR-003) and `StepType ∈ {Action, Check}` (FR-002), and copy all four new fields into each frozen `RunbookVersionStep` at publish (FR-007). Depends on T002/T003/T004.

### Data
- [X] T006 Map the new columns in `backend/src/RunbookPlatform.Api/Data/AppDbContext.cs`: `StepType` enum↔string conversion (default `Action`) and the three nullable text columns, for both `Step` and `RunbookVersionStep`.
- [X] T007 Add one additive EF Core migration in `backend/src/RunbookPlatform.Api/Data/Migrations/` adding `Instructions`/`Command`/`ExpectedResult` (TEXT NULL) + `StepType` (TEXT NOT NULL DEFAULT 'Action') to `Steps` and `RunbookVersionSteps` (C-002 / R7). No table added/dropped. Depends on T005/T006.
- [X] T008 Regression checkpoint: run `dotnet test`; the slice-01/03 suites pass **unmodified** against the migrated schema (`git diff --stat backend/tests/` empty). Blocks all story work.

### Shared frontend
- [X] T009 [P] Add the hand-rolled markdown renderer `frontend/src/lib/markdown.ts` (ADR-0006): escape all HTML first, then apply the whitelist (bold, italic, inline code, fenced code block, links with http/https/mailto only, ordered/unordered lists, paragraphs/line breaks); return a safe HTML string. No images, tables, or raw HTML.
- [X] T010 [P] Unit tests `frontend/src/lib/markdown.test.ts` for the safety invariants (ADR-0006): raw HTML is escaped, `<script>` is neutralised, `javascript:` links are blocked, and bold/list/link render correctly. Depends on T001/T009.
- [X] T011 Extend the typed client `frontend/src/api/client.ts`: `StepItem` gains `instructions`/`command`/`expectedResult` (nullable) and `type` (`'Action' | 'Check'`); `saveSteps` accepts structured steps; `ReviewTimeline` gains the same detail fields (R9, contracts/http-api.md).

### Foundational domain tests
- [X] T012 [P] Backend tests `backend/tests/RunbookPlatform.Api.Tests/RichStepFreezeTests.cs`: a title-only step is valid (FR-003); a non-`{Action,Check}` type is rejected (FR-002); publish freezes all detail + type; editing the working Runbook and re-publishing leaves an earlier Version's detail unchanged (FR-007/008). Depends on T005/T007.

**Checkpoint**: enriched domain + schema persist, existing tests green, renderer + client ready. User stories can begin.

---

## Phase 3: User Story 1 — Author rich detail and freeze it at publish (P1) 🎯 MVP

**Goal**: An author adds instructions/command/expected result + a Step Type to each Step; publishing freezes it into the Runbook Version; the version view shows it.

**Independent Test**: Author a Step with detail + type, publish, reopen the Version (detail present, markdown rendered); edit + re-publish and confirm the earlier Version is unchanged; a title-only Step still works.

### Endpoints (thin: load → method → save → map)
- [X] T013 [US1] In `backend/src/RunbookPlatform.Api/Endpoints/RunbookEndpoints.cs`: extend `SaveStepItem` with optional `Instructions`/`Command`/`ExpectedResult` and `Type`; pass them through `ReplaceSteps`; include the four fields per step in the `PUT /steps` response and in `ToDetail` (FR-001/003, contracts).
- [X] T014 [US1] In `backend/src/RunbookPlatform.Api/Endpoints/VersionEndpoints.cs`: return `instructions`/`command`/`expectedResult`/`type` per frozen step from `GET /api/runbooks/{id}/versions/{number}` (FR-010).

### Tests (map to FR ids)
- [X] T015 [P] [US1] Integration tests `backend/tests/RunbookPlatform.Api.Tests/RichStepAuthoringTests.cs`: save+reload round-trips all detail + type (FR-001); a title-only step is accepted (FR-003); an invalid `type` returns 400 (FR-002).
- [X] T016 [P] [US1] FR-010 (version view returns frozen detail + type) and FR-007/008 (edit + re-publish leaves Version 1 unchanged) are covered in `backend/tests/RunbookPlatform.Api.Tests/RichStepFreezeTests.cs`, which asserts against the `GET /versions/{n}` endpoint — folded there rather than a separate file to avoid a redundant fixture.

### Frontend (hash-routed — C-003; no new runtime dep)
- [X] T017 [US1] In `frontend/src/pages/RunbookDetail.tsx`: add per-step authoring inputs — instructions (multi-line), command, expected result, and an Action/Check type selector — and send them via `saveSteps`. Title remains the only required field.
- [X] T018 [US1] In `frontend/src/pages/VersionView.tsx`: render each frozen step's instructions through `lib/markdown`, the command and expected result verbatim (monospaced), and the Step Type label (FR-010).

**Checkpoint**: US1 fully functional — the MVP. A published procedure now says how, not just what.

---

## Phase 4: User Story 2 — Read a Step's detail while running an Execution (P2)

**Goal**: A responder sees each Step's detail (instructions rendered, command, expected result, type) in the run view before recording an outcome.

**Independent Test**: Start an Execution against a published Version with detail; open a Step and see the rendered detail before marking it; recording behaves as slice 03.

- [X] T019 [US2] In `backend/src/RunbookPlatform.Api/Endpoints/ExecutionEndpoints.cs` (`ToRunView`): include `instructions`/`command`/`expectedResult`/`type` for each pinned-Version step (FR-009). Recording/lifecycle unchanged (FR-015).
- [X] T020 [P] [US2] Integration test `backend/tests/RunbookPlatform.Api.Tests/RichStepRunViewTests.cs`: the run view returns each pinned step's detail (FR-009).
- [X] T021 [US2] In `frontend/src/pages/ExecutionRun.tsx`: show each step's instructions (via `lib/markdown`), command, expected result, and type before the Done/Skipped/Failed controls; a title-only step shows its title alone (FR-009).

**Checkpoint**: US1 + US2 both work independently — the responder reads how to act as they act.

---

## Phase 5: User Story 3 — See a Step's detail in the Computed Review (P3)

**Goal**: The Computed Review shows each pinned Step's detail alongside its recorded outcome.

**Independent Test**: Run and close an Execution, open its Computed Review, and confirm each timeline step shows its detail next to its outcome; coverage still lists every step incl. "not reached".

- [X] T022 [US3] In `backend/src/RunbookPlatform.Api/Endpoints/ExecutionEndpoints.cs` (`ToComputedReview`): include each pinned step's detail (instructions/command/expectedResult/type) in the `timeline` items (FR-011). Derivation stays read-only (FR-013).
- [X] T023 [P] [US3] Integration test `backend/tests/RunbookPlatform.Api.Tests/RichStepReviewTests.cs`: the Computed Review timeline includes step detail alongside outcomes (FR-011).
- [X] T024 [US3] In the frontend Computed Review rendering (`frontend/src/pages/ExecutionRun.tsx` or its review component): show each step's detail (instructions via `lib/markdown`, command/expected verbatim, type) next to its outcome (FR-011).

**Checkpoint**: all three stories independently functional.

---

## Phase 6: Polish & Cross-Cutting

- [X] T025 [P] Run `specs/004-rich-steps/quickstart.md` Scenarios A–E, including safety Scenario D (FR-005/006) and backward-compatibility Scenario E (SC-004).
- [X] T026 Full green: `dotnet test` (slice 01/03 unmodified) + `npm run test` (markdown safety) + `npm run build`.
- [X] T027 Note for release (NOT this phase): `docs/architecture.md` is amended at the post-implement release step per the constitution — record the enriched frozen Step fields and correct the stale "second bounded context" line then, not now.

---

## Dependencies & Execution Order

### Phase dependencies
- **Setup (P1)**: no dependencies.
- **Foundational (P2)**: depends on Setup; **blocks all user stories**. T008 (regression) gates story work.
- **User Stories (P3–P5)**: each depends on Foundational. US1 is the MVP; US2 and US3 can each start after Foundational and are independently testable (US2/US3 only add read surfaces; no cross-story code dependency).
- **Polish (P6)**: after the desired stories.

### Within each story
- Tests and implementation for a story touch different files and can interleave; backend endpoint before its integration test run; frontend after its endpoint returns the fields.

### Parallel opportunities
- T002, T009, T010 (different files) are [P] within Foundational; T009/T010 (frontend) run in parallel with T002–T007 (backend).
- Per story, the [P] test files (T015/T016, T020, T023) are independent of each other.
- With capacity, US1/US2/US3 backend+frontend can proceed in parallel once Foundational is done.

---

## Parallel Example: Foundational

```bash
# Backend domain/data and the frontend renderer are independent tracks:
Task: "T002 Add StepType enum (Domain/StepType.cs)"
Task: "T009 Add markdown renderer (frontend/src/lib/markdown.ts)"
Task: "T010 Markdown safety unit tests (frontend/src/lib/markdown.test.ts)"
```

---

## Implementation Strategy

### MVP first (US1 only)
1. Phase 1 Setup → 2. Phase 2 Foundational (regression green at T008) → 3. Phase 3 US1 → **STOP and validate** (quickstart Scenario A) → demo: a published runbook that says how.

### Incremental delivery
Foundation → US1 (MVP, demo) → US2 (read while running, demo) → US3 (review detail, demo). Each adds a read surface without breaking the prior.

---

## Notes
- [P] = different files, no incomplete dependency. [Story] maps a task to its user story.
- `Text` stays the required title (no rename) — keeps slice-01/03 tests green (R1).
- The only new dependency is dev-only `vitest`; no runtime dependency (C-003 / ADR-0006).
- Commit after each task or logical group; stop at any checkpoint to validate a story independently.
