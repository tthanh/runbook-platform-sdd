# HTTP API contract deltas — 005-branching (Phase 1)

Additive to the existing `/api/*` contract. Only deltas shown; unchanged endpoints keep
their slice-01/03/04 shape. Errors keep the existing seam: `DomainException` → `400
{ "error": "…" }`; conflicts → `409`.

## Authoring — save Steps with Options

`PUT /api/runbooks/{id}/steps` — `SaveStepItem` gains an optional `Options` list; a Step
with `Type: "Decision"` must carry ≥2 Options (validated in the aggregate).

```jsonc
// SaveStepsRequest.Steps[i] (SaveStepItem) — added field:
{
  "text": "Is the database reachable?",
  "instructions": "Check the primary DB health endpoint.",   // existing optional detail
  "command": null,
  "expectedResult": null,
  "type": "Decision",                                          // Action | Check | Decision
  "options": [                                                 // NEW — required (≥2) iff type=Decision, else omitted/empty
    { "ordinal": 1, "label": "Database reachable", "targetPosition": 4 },
    { "ordinal": 2, "label": "Database down",      "targetPosition": 6 }
  ]
}
```

Validation errors (400) — from the aggregate, surfaced at save or publish:
- a Decision Step with fewer than 2 Options
- an Option with an empty label
- (deferred to publish for full-graph checks; save may accept an in-progress draft)

## Publish — validation gate (FR-017)

`POST /api/runbooks/{id}/publish` — publish now runs the branching validation gate and
**rejects** (400) a broken flow:
- any Option whose `targetPosition` does not exist in the Version
- any Option that is not forward-only (`targetPosition` ≤ the Decision Step's position) — loop/back-edge
- any Decision Step with fewer than 2 Options

Non-blocking **warnings** (published, returned alongside the created Version) for Steps
unreachable from the first Step. A path that simply ends is allowed. Success response is
the created Version as today, optionally carrying `warnings: string[]`.

## Version read — frozen Options

`GET /api/runbooks/{id}` and the version view return, for each Decision Step, its frozen
`options` (`ordinal`, `label`, `targetPosition`). Non-Decision Steps omit `options`.

## Execution — present and record a Decision

`GET /api/executions/{id}` — for the Step currently reached, if it is a Decision Step the
payload includes its `options` (from the pinned Version; C-007) so the UI can present the
choice.

`POST /api/executions/{id}/records` — `RecordStepRequest` gains an optional
`chosenOptionOrdinal`; required when the recorded Step is a Decision, omitted otherwise.

```jsonc
// RecordStepRequest — added field:
{
  "stepPosition": 3,
  "outcome": "Decided",           // Decision resolution marker (see data-model); Action/Check keep Done|Skipped|Failed
  "note": null,
  "chosenOptionOrdinal": 2         // NEW — required iff the Step at stepPosition is a Decision
}
```

Errors: recording a `chosenOptionOrdinal` that is not an Option of that Decision Step →
400; recording a Decision without a choice → 400; recording after close → 409 (existing).

## Review — over the Taken Path (FR-011/013/014)

`GET /api/executions/{id}/review` — the Computed Review is unchanged in shape but:
- coverage is computed over the **Taken Path** (Steps actually reached)
- each Decision Step entry includes the **chosen Option** (label + ordinal)
- Steps on a branch not taken are reported with state **`NotReached`** (never `Skipped`)

A run of a Version with no Decision Steps returns the same review as before this slice.
