# Quickstart — 003 Runbook Execution

End-to-end validation that the execution slice works against the running app.
Assumes the slice-01 authoring API/UI and the slice-03 execution endpoints are
running. References [contracts/http-api.md](contracts/http-api.md) and
[data-model.md](data-model.md) rather than repeating shapes.

## Prerequisites

- Backend running (`dotnet run` in `backend/`), SQLite DB created via **EF Core
  migrations** (research R2) — `Database.Migrate()` runs at startup.
- At least one Runbook **published** (slice 01) — call it *Database failover*,
  Version 2 current.

## Validation walk

1. **Start an Execution** — `POST /api/executions` with an `incidentId`, optional
   `incidentTitle`, and the published Runbook's id. Expect `201`, `status: Open`,
   `pinnedVersionNumber: 2`, and the pinned Version's Steps. *(US1, FR-001)*
2. **Refuse starting against an unpublished Runbook** — create a fresh Runbook
   with no published Version, try to start an Execution against it. Expect `400`
   "must be published". *(FR-002, edge case)*
3. **Record Step outcomes out of order** — `POST …/records` for step 2 (`Done`),
   then step 1 (`Failed`, with a note). Expect `201` each; the run view lists both
   records in the order recorded. *(US1, FR-004, FR-007)*
4. **Re-mark a Step** — record step 1 again as `Done`. Expect `201`; the run view
   now shows two records for step 1. *(FR-006)*
5. **Pin holds across a mid-incident publish** — in the authoring UI, publish a
   new Version 3 of the same Runbook. Re-fetch the Execution: it still shows
   `pinnedVersionNumber: 2` and Version 2's Steps, with **no** "newer version"
   signal. *(US3, FR-003, ADR-0004)*
6. **Resume an open Execution** — `POST /api/executions` again with the **same**
   `incidentId`. Expect `200` returning the existing open Execution (not a second
   one). *(clarification 2026-06-13, FR-015)*
7. **Close the Execution** — `POST …/close`. Expect `200`, `status: Closed`. *(US2, FR-009)*
8. **Recording after close is refused** — `POST …/records`. Expect `409` "closed". *(US2-5, FR-010)*
9. **Refuse a new Execution for the now-closed incident** — `POST /api/executions`
   with the same `incidentId`. Expect `409` "already has a completed Execution".
   *(clarification 2026-06-13)*
10. **Get the Computed Review** — `GET …/review`. Verify:
    - `timeline` is chronological and includes every record (incl. both step-1
      marks). *(FR-011)*
    - `coverage` shows step 1 end state `Done` (most recent mark), step 2 `Done`
      (or its outcome), and any Step never recorded as `NotReached` — distinct
      from a `Skipped` step. *(FR-006, FR-012)*
    - the header shows `incidentTitle` (or `incidentId` if none). *(ADR-0005)*
11. **Review reflects records exactly** — confirm no entry appears that has no
    Step Record, and the review offers no edit affordance. *(FR-013, SC-003)*

## Regression guard

- The 18 slice-01 authoring tests still pass unmodified — the initial EF Core
  migration reproduced the existing schema (R2 / C-002), so authoring behavior is
  unchanged.

## Maps to success criteria

- SC-001 (review in <5 min): steps 1–10 are a few minutes of interaction.
- SC-003 (fidelity): steps 10–11.
- SC-004 (pin integrity): step 5.
