# HTTP API Contract: Runbook Execution

The interface the React frontend consumes for running a Runbook and reviewing a
run. JSON over HTTP. No authentication (FR-017). No DELETE endpoints (Step
Records and Executions are never deleted). The authoring endpoints (slice 01)
are unchanged.

Base path: `/api`. Domain-rule violations return `400 { "error": "…" }` via the
existing `DomainException` seam; conflict cases use `409` as noted.

## POST /api/executions — start (or resume) an Execution (FR-001, FR-002, FR-015, US1)

Request:

```json
{ "incidentId": "INC-1234", "incidentTitle": "Database outage", "runbookId": "…" }
```

`incidentTitle` is optional (ADR-0005).

- `201` → a new open Execution (pins the Runbook's current published Version):

  ```json
  {
    "id": "…",
    "incidentId": "INC-1234",
    "incidentTitle": "Database outage",
    "status": "Open",
    "runbookName": "Database failover",
    "pinnedVersionNumber": 2,
    "steps": [ { "position": 1, "text": "Page the on-call DBA" }, { "position": 2, "text": "Verify replica lag" } ],
    "records": []
  }
  ```

- `200` → when an **open** Execution already exists for `incidentId`: the existing
  Execution is returned (resume), same shape as `201` (clarification 2026-06-13).
- `409` → when the incident's Execution is already **closed** → `{ "error": "This incident already has a completed Execution." }`
- `400` → when the chosen Runbook has no published Runbook Version → `{ "error": "The Runbook must be published before it can be run." }` (FR-002)
- `400` → when `incidentId` is empty/blank, or `runbookId` is unknown.

`steps` are the **pinned Version's** Steps (immutable), not the Runbook's working
Steps.

## GET /api/executions/{id} — the run view (US1, US3)

Response `200`:

```json
{
  "id": "…",
  "incidentId": "INC-1234",
  "incidentTitle": "Database outage",
  "status": "Open",
  "runbookName": "Database failover",
  "pinnedVersionNumber": 2,
  "steps": [ { "position": 1, "text": "Page the on-call DBA" } ],
  "records": [
    { "stepPosition": 1, "outcome": "Done", "note": "paged at 02:14", "recordedAt": "…" }
  ]
}
```

- Always shows the **pinned** Version's Steps, even after a newer Version is
  published (FR-003, ADR-0004 — silent, no "newer exists" signal).
- `404` when the Execution id is unknown.

## POST /api/executions/{id}/records — record a Step outcome (FR-004, FR-005, FR-006, FR-007, US1)

Request:

```json
{ "stepPosition": 1, "outcome": "Done", "note": "paged at 02:14" }
```

`outcome` ∈ `Done` | `Skipped` | `Failed`. `note` optional. Steps may be recorded
in any order (FR-007) and a Step may be recorded more than once (FR-006); each
call appends one Step Record.

- `201` → the appended Step Record.
- `409` → when the Execution is closed → `{ "error": "This Execution is closed." }` (FR-010, US2-5)
- `400` → unknown `stepPosition` for the pinned Version, or invalid `outcome`.
- `404` → unknown Execution.

## POST /api/executions/{id}/close — close the Execution (FR-009, US2)

Request: empty body.

- `200` → the Execution with `status: "Closed"` and `closedAt` set.
- `409` → when already closed → `{ "error": "This Execution is already closed." }`
- `404` → unknown Execution.

Closing is manual; there is no endpoint that closes an Execution from the
external incident (FR-009).

## GET /api/executions/{id}/review — the Computed Review (FR-011, FR-012, FR-013, US2)

Available only for a **closed** Execution. Response `200`:

```json
{
  "incident": "Database outage",
  "runbookName": "Database failover",
  "pinnedVersionNumber": 2,
  "startedAt": "…",
  "closedAt": "…",
  "timeline": [
    { "stepPosition": 1, "stepText": "Page the on-call DBA", "outcome": "Done",   "note": "paged at 02:14", "recordedAt": "…" },
    { "stepPosition": 1, "stepText": "Page the on-call DBA", "outcome": "Failed",  "note": "no answer",      "recordedAt": "…" }
  ],
  "coverage": [
    { "stepPosition": 1, "endState": "Done" },
    { "stepPosition": 2, "endState": "NotReached" },
    { "stepPosition": 3, "endState": "Skipped" }
  ]
}
```

- `timeline` is chronological (FR-011); a Step recorded more than once appears
  once per mark (FR-006).
- `coverage` gives each pinned-Version Step its end state — the most recent
  outcome, or `NotReached` when it has no Step Record (FR-012). `NotReached` is
  derived, never recorded.
- `incident` is `incidentTitle` if present, else `incidentId` (ADR-0005).
- `409` when the Execution is still open → `{ "error": "The Execution must be closed to compute its review." }`
- `404` when the Execution id is unknown.

The review is computed on read from the Step Records and the pinned Version
(research R1); it is never stored or editable (FR-013).
