# Tasks: Runbook Authoring

**Input**: Design documents from `/specs/001-runbook-authoring/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/http-api.md, quickstart.md — ADR-0001 and ADR-0002 are **Accepted** (tasks gate clear)

**Tests**: Included — the constitution requires tests mapping to requirement ids (research R6: xUnit integration tests at the API seam, test names carry FR ids). Write each story's tests first; they must fail before implementation.

**Organization**: Grouped by user story; each story is independently implementable and testable. Sized to the couple-of-evenings appetite — cut scope before extending.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: US1 / US2 / US3 from spec.md
- Exact file paths in every description

## Path Conventions

Web app per plan.md: `backend/src/RunbookPlatform.Api/`, `backend/tests/RunbookPlatform.Api.Tests/`, `frontend/src/`.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project skeletons matching plan.md's structure

- [X] T001 Create .NET solution with ASP.NET Core minimal API project in backend/src/RunbookPlatform.Api/ (net10.0, EF Core + SQLite packages) and solution file backend/RunbookPlatform.sln
- [X] T002 Create xUnit test project in backend/tests/RunbookPlatform.Api.Tests/ referencing the API project, with WebApplicationFactory + per-test SQLite database fixture in backend/tests/RunbookPlatform.Api.Tests/ApiFixture.cs
- [X] T003 [P] Scaffold Vite React TypeScript app in frontend/ with dev-server proxy of /api to the backend (frontend/vite.config.ts)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Domain + persistence + app bootstrap every story depends on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Domain entities with binding glossary names — Runbook, Step (working), RunbookVersion, frozen step copy — per data-model.md, in backend/src/RunbookPlatform.Api/Domain/Runbook.cs, Step.cs, RunbookVersion.cs, RunbookVersionStep.cs
- [X] T005 EF Core DbContext + SQLite wiring + migration, including unique index on (RunbookId, Number) for RunbookVersion (ADR-0001) and required/non-empty constraints from data-model.md, in backend/src/RunbookPlatform.Api/Data/AppDbContext.cs
- [X] T006 App bootstrap in backend/src/RunbookPlatform.Api/Program.cs — minimal API setup, /api base path, JSON error shape { "error": "…" } per contracts/http-api.md, DB created on startup; no auth middleware (FR-010)
- [X] T007 [P] Typed API client matching contracts/http-api.md in frontend/src/api/client.ts (types + fetch wrappers for all six endpoints)

**Checkpoint**: Foundation ready — user story implementation can now begin

---

## Phase 3: User Story 1 - Create a Runbook and publish its first Runbook Version (Priority: P1) 🎯 MVP

**Goal**: Author creates a named Runbook, adds ordered Steps, publishes → immutable Runbook Version 1

**Independent Test**: Create one Runbook with Steps, publish once, confirm Version 1 exists with exactly the published content (quickstart steps 1–4)

### Tests for User Story 1 (write first — must fail before implementation)

- [X] T008 [P] [US1] Integration tests FR-001/FR-002/FR-003/FR-004/FR-005 (create with name, empty-name 400, add/save steps, blank-step 400, publish → Version 1, publish-without-steps 400) in backend/tests/RunbookPlatform.Api.Tests/CreateAndPublishTests.cs

### Implementation for User Story 1

- [X] T009 [US1] POST /api/runbooks (create, name validation) and GET /api/runbooks/{id} (detail incl. working steps, versions, currentVersionNumber, 404) in backend/src/RunbookPlatform.Api/Endpoints/RunbookEndpoints.cs
- [X] T010 [US1] PUT /api/runbooks/{id}/steps — full ordered replacement, positions 1..n, blank-text 400, never touches published versions (research R5) in backend/src/RunbookPlatform.Api/Endpoints/RunbookEndpoints.cs
- [X] T011 [US1] POST /api/runbooks/{id}/publish — publish gate (FR-003), transactional number assignment max+1 (ADR-0001), frozen step copies + nameAtPublish (FR-004/FR-006) in backend/src/RunbookPlatform.Api/Endpoints/PublishEndpoints.cs
- [X] T012 [US1] Frontend Runbook detail page — create form, step editor (add/edit/remove/reorder + save), publish button with error display, in frontend/src/pages/RunbookDetail.tsx and frontend/src/components/StepEditor.tsx

**Checkpoint**: US1 fully functional — quickstart steps 1–4 pass

---

## Phase 4: User Story 2 - Edit and republish; earlier Runbook Versions stay intact (Priority: P2)

**Goal**: Each publish creates Version N+1; every earlier Runbook Version remains viewable and byte-identical

**Independent Test**: Publish V1, edit a Step, publish V2, confirm V1 unchanged and V2 current (quickstart steps 5–7)

### Tests for User Story 2 (write first — must fail before implementation)

- [X] T013 [P] [US2] Integration tests FR-006/FR-008 (edit leaves published versions unchanged; republish → N+1; identical republish → new number; latest is current; no mutation route exists for versions) in backend/tests/RunbookPlatform.Api.Tests/ImmutabilityTests.cs

### Implementation for User Story 2

- [X] T014 [US2] Current-version derivation (highest number) surfaced as currentVersionNumber in list/detail responses, and republish N+1 path hardened against the unique (RunbookId, Number) index, in backend/src/RunbookPlatform.Api/Endpoints/RunbookEndpoints.cs and PublishEndpoints.cs
- [X] T015 [US2] Frontend version history — versions list with current badge, republish flow reusing the publish button, in frontend/src/components/VersionHistory.tsx (wired into RunbookDetail.tsx)

**Checkpoint**: US1 and US2 both pass independently — quickstart steps 5–7 pass

---

## Phase 5: User Story 3 - Browse Runbooks and view any published Runbook Version (Priority: P3)

**Goal**: List all Runbooks (with or without published versions); open any Runbook Version by number, exactly as published

**Independent Test**: Two Runbooks (one with two versions): list shows both correctly; each version reachable by number (quickstart steps 8–10)

### Tests for User Story 3 (write first — must fail before implementation)

- [X] T016 [P] [US3] Integration tests FR-007/FR-008/FR-009 (list shows all incl. unpublished with null version; GET version by number returns exact published content; unknown number 404; unpublished runbook signalled) in backend/tests/RunbookPlatform.Api.Tests/BrowseAndViewTests.cs

### Implementation for User Story 3

- [X] T017 [US3] GET /api/runbooks (list with currentVersionNumber-or-null) and GET /api/runbooks/{id}/versions/{number} (frozen content, 404 on missing) in backend/src/RunbookPlatform.Api/Endpoints/RunbookEndpoints.cs and VersionEndpoints.cs
- [X] T018 [US3] Frontend Runbook list page (all runbooks, version badge or "nothing published yet") and read-only version viewer, in frontend/src/pages/RunbookList.tsx and frontend/src/pages/VersionView.tsx

**Checkpoint**: All three stories independently functional

---

## Phase 6: Polish & Cross-Cutting Concerns

- [X] T019 [P] Guard tests FR-010/FR-011 — assert no DELETE route responds for any resource, no PUT/PATCH route exists for versions, and no request requires auth, in backend/tests/RunbookPlatform.Api.Tests/GuardRailTests.cs
- [X] T020 Run the full quickstart.md validation walk (steps 1–13) against the running app and fix anything that fails; confirm `dotnet test` is green with FR ids visible in test names

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: none — start immediately; T003 parallel to T001–T002
- **Foundational (Phase 2)**: needs Phase 1; T007 parallel to T004–T006; **blocks all stories**
- **US1 (Phase 3)**: needs Phase 2 — MVP
- **US2 (Phase 4)**: needs Phase 2; builds on US1's publish path (T011) — run after US1
- **US3 (Phase 5)**: needs Phase 2; read-only over US1/US2 data — can run parallel to US2 if staffed
- **Polish (Phase 6)**: needs all desired stories

### Within Each Story

Tests first (fail) → endpoints → frontend. Models/persistence already exist from Phase 2.

### Parallel Opportunities

- T003 ∥ T001–T002 (frontend scaffold vs backend solution)
- T007 ∥ T004–T006 (API client vs domain/persistence)
- Each story's test task (T008, T013, T016) ∥ other stories' work — different files
- T012 ∥ T011 once T009–T010 are done (frontend vs publish endpoint)
- US3 ∥ US2 after US1 (different endpoint/page files; watch shared RunbookEndpoints.cs — coordinate T014/T017)

## Parallel Example: User Story 1

```bash
# After T009–T010 land, run together:
Task: "T011 publish endpoint in backend/src/RunbookPlatform.Api/Endpoints/PublishEndpoints.cs"
Task: "T012 frontend detail page in frontend/src/pages/RunbookDetail.tsx"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Phases 1–2 (setup + foundation)
2. Phase 3 (US1) → **stop and validate**: quickstart steps 1–4 + `dotnet test`
3. That alone delivers the slice's core promise: a citable, immutable Version 1

### Incremental Delivery

Evening 1: Phases 1–3 (MVP). Evening 2: Phases 4–6. Each story validates via its quickstart steps before the next begins; commit after each task or logical group.

### Notes

- Tests must fail before their story's implementation begins (constitution: tests map to requirement ids — keep FR ids in test method names)
- Glossary names (Runbook, RunbookVersion, Step) in all code identifiers
- No DELETE handlers, no auth, no version-mutation paths — guarded by T019
- Stop at any checkpoint; appetite rule: cut scope, don't extend
