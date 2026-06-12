# Tasks: Rich Domain Model (structural)

**Input**: spec.md (pointer), plan.md, ADR-0003 (Accepted)

**Tests**: No new tests. The 18 existing integration tests in `backend/tests/RunbookPlatform.Api.Tests/` are the acceptance contract and must pass **unmodified** — any test edit fails the initiative (ADR-0003 Consequences).

**Organization**: One phase — this is a single-seam refactor; tasks are ordered by dependency, not by user story (there are no new stories).

## Phase 1: Move invariants into the aggregates

- [ ] T001 Add DomainException in backend/src/RunbookPlatform.Api/Domain/DomainException.cs — single exception type for invariant violations (ADR-0003)
- [ ] T002 Make Runbook the aggregate root in backend/src/RunbookPlatform.Api/Domain/Runbook.cs — static Create(name) (FR-001), ReplaceSteps(texts) (FR-002, blank-Step rule), Publish() (FR-003 gate, FR-004 freeze, FR-005 sequential number per ADR-0001), derived CurrentVersionNumber (FR-008); private setters, IReadOnlyList collections over private backing lists
- [ ] T003 Make RunbookVersion construct-only in backend/src/RunbookPlatform.Api/Domain/RunbookVersion.cs — created solely by Runbook.Publish() via internal factory, no public mutation after construction (FR-006 at the type level); frozen steps in RunbookVersionStep.cs likewise
- [ ] T004 Tighten Step in backend/src/RunbookPlatform.Api/Domain/Step.cs — internal construction, private setters; construction only via Runbook.ReplaceSteps
- [ ] T005 Map the encapsulated model in backend/src/RunbookPlatform.Api/Data/AppDbContext.cs — backing-field/navigation config only where convention fails; resulting schema must be byte-identical (conflict register C-002)
- [ ] T006 Shrink endpoints to load → call method → save → map in backend/src/RunbookPlatform.Api/Endpoints/RunbookEndpoints.cs and PublishEndpoints.cs — remove the duplicated name/step checks and the gate/number/freeze logic; add one DomainException → 400 { error } endpoint filter on the /api/runbooks group
- [ ] T007 Validate: `dotnet test` green with all 18 tests unmodified (`git diff --stat backend/tests/` must be empty); quickstart smoke of publish/republish/view against the running app

## Dependencies & Execution Order

T001 → T002/T003/T004 (domain, together) → T005 (mapping) → T006 (endpoints) → T007 (validation).
No parallel markers — every task after T001 touches files the next task depends on.

## Notes

- Behavior, HTTP contract, and schema unchanged — this is the whole definition of done.
- Stop rule (appetite): if EF Core mapping of the encapsulated model forces a schema change or eats the second evening, stop and cut encapsulation depth, not behavior (C-002; ADR-0003 chose pragmatic).
