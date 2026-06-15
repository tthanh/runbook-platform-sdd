# HTTP API contract deltas — 004-rich-steps (Phase 1)

Additive changes to existing endpoints only. No new route, no removed route. All
new per-Step fields are optional except `type` (defaults to `Action`). `text`
remains the required title (R1/R9). DomainException → `400 { "error": "…" }` at the
existing group seam, unchanged.

## Shared per-Step shape

Every place a Step is sent or returned gains the same four fields beside the existing
`position` / `text`:

```jsonc
{
  "position": 1,
  "text": "Drain the node",                 // required: the title
  "instructions": "Run the drain, then **wait** for pods to reschedule.",  // optional, markdown source
  "command": "kubectl drain node-1 --ignore-daemonsets",                    // optional, verbatim
  "expectedResult": "node-1 shows SchedulingDisabled",                      // optional, verbatim
  "type": "Action"                          // "Action" | "Check"; default "Action"
}
```

## Authoring (write)

### `PUT /api/runbooks/{id}/steps` — replace working steps

Request — `SaveStepItem` gains optional detail + type (`text` still required):

```jsonc
{
  "steps": [
    {
      "text": "Drain the node",
      "instructions": "Run the drain, then **wait**.",
      "command": "kubectl drain node-1",
      "expectedResult": "SchedulingDisabled",
      "type": "Action"
    },
    { "text": "Confirm traffic drained", "type": "Check" },
    { "text": "Title-only step still valid" }   // detail omitted → null; type → Action
  ]
}
```

Response: the saved working steps in the shared per-Step shape.

Errors (unchanged seam): empty/whitespace `text` → 400 "A Step needs content.";
`type` not in {Action, Check} → 400.

### `POST /api/runbooks/{id}/publish` — freeze a version

Unchanged request/response shape; the frozen version now carries each Step's detail
+ type (FR-007). No new fields in the publish request.

## Reads

### `GET /api/runbooks/{id}` — runbook detail

`steps[]` items use the shared per-Step shape (working steps with detail + type).
`versions[]` summary unchanged.

### `GET /api/runbooks/{id}/versions/{number}` — version view

`steps[]` items use the shared per-Step shape (frozen detail + type). Other fields
(`number`, `nameAtPublish`, `publishedAt`) unchanged. (FR-010)

### `GET /api/executions/{id}` — run view

`steps[]` (the pinned Version's steps) use the shared per-Step shape, so the
responder sees instructions/command/expected result/type before recording (FR-009).
`records[]` unchanged. The pinned-Version rule (ADR-0004) is unchanged.

### `GET /api/executions/{id}/review` — Computed Review

`timeline[]` and `coverage[]` are keyed by `stepPosition` as today; each gains the
pinned Step's detail for display alongside the outcome (FR-011). `timeline[]` already
carries `stepText`; it additionally carries `instructions`, `command`,
`expectedResult`, `type`. `coverage[]` (per-step end state) MAY carry the same detail
for rendering. Derivation is unchanged — review stays read-derived, never authored
(FR-013).

## Frontend typed client (`frontend/src/api/client.ts`)

`StepItem` gains `instructions: string | null`, `command: string | null`,
`expectedResult: string | null`, `type: 'Action' | 'Check'`. `saveSteps` accepts the
richer step objects instead of bare strings. `ReviewTimeline` gains the same detail
fields. No new endpoint methods.

## Out of scope (negative contract)

- No endpoint runs, validates, or compares `command` / `expectedResult` (FR-013).
- No per-Step owner/assignee field (FR-014).
- No branching/next-step, condition, or gate fields on any Step (PRD non-goals).
- No change to execution lifecycle or Step Record shape (FR-015).
