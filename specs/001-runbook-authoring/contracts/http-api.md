# HTTP API Contract: Runbook Authoring

The interface the React frontend consumes. JSON over HTTP. No authentication
(FR-010). No DELETE endpoints exist anywhere (FR-011). Published Runbook
Versions have no mutation endpoints (FR-006, research R4).

Base path: `/api`.

## GET /api/runbooks — list Runbooks (FR-009, US3-1)

Response `200`:

```json
[
  { "id": "…", "name": "Database failover", "currentVersionNumber": 2 },
  { "id": "…", "name": "Cache flush",       "currentVersionNumber": null }
]
```

`currentVersionNumber` is `null` when nothing is published yet (US3-1 "where one
exists").

## POST /api/runbooks — create a Runbook (FR-001, US1-1)

Request: `{ "name": "Database failover" }`

- `201` → `{ "id": "…", "name": "…", "steps": [], "currentVersionNumber": null }`
- `400` when name is empty/blank → `{ "error": "A name is required." }` (US1-5)

## GET /api/runbooks/{id} — Runbook detail (US3-3)

Response `200`:

```json
{
  "id": "…",
  "name": "Database failover",
  "steps": [ { "position": 1, "text": "Page the on-call DBA" } ],
  "currentVersionNumber": 2,
  "versions": [ { "number": 1, "publishedAt": "…" }, { "number": 2, "publishedAt": "…" } ]
}
```

`steps` is the editable working set. `versions` lists published Runbook Versions
(empty array + `currentVersionNumber: null` when none — the UI states nothing is
published yet, US3-3).

- `404` when the Runbook id is unknown.

## PUT /api/runbooks/{id}/steps — replace the working Steps (FR-002, research R5)

Request (full ordered replacement; covers add/edit/remove/reorder):

```json
{ "steps": [ { "text": "Page the on-call DBA" }, { "text": "Verify replica lag" } ] }
```

- `200` → the updated working `steps` with positions assigned 1..n
- `400` when any Step text is empty/blank → `{ "error": "A Step needs content." }` (edge case)
- `404` unknown Runbook
- Never touches published Runbook Versions (US2-1).

## POST /api/runbooks/{id}/publish — publish a Runbook Version (FR-003–FR-005, US1-3/4, US2-2)

No request body.

- `201` → `{ "number": 3, "publishedAt": "…", "nameAtPublish": "…", "steps": [ … ] }`
  — number = previous max + 1, or 1 for the first publish
- `400` when the Runbook has no Steps → `{ "error": "At least one Step is required." }` (US1-4)
- `400` when the name is empty → `{ "error": "A name is required." }` (US1-5)
- `404` unknown Runbook

## GET /api/runbooks/{id}/versions/{number} — view a published Runbook Version (FR-007/008, US3-2)

Response `200`:

```json
{
  "number": 1,
  "nameAtPublish": "Database failover",
  "publishedAt": "…",
  "steps": [ { "position": 1, "text": "Page the on-call DBA" } ]
}
```

- `404` when that version number does not exist for the Runbook (edge case:
  "no such Runbook Version").

## Deliberately absent

| Not present | Why |
|-------------|-----|
| DELETE (any resource) | FR-011 |
| PUT/PATCH on versions | FR-006 — immutable by construction |
| Auth headers/endpoints | FR-010 |
| Rename endpoint | No FR requires rename this slice; name is set at create |
| Search/tags/import/export | PRD non-goals |
