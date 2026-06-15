# Data Model — 004-rich-steps (Phase 1)

Additive change to two existing entities plus one new enum. No new table, no new
aggregate, no new domain event. Construction stays inside the Runbook aggregate
(ADR-0003 / C-004).

## StepType (new enum)

`Action | Check` — the classification of a Step (ADR-0007).

- **Action** — do something.
- **Check** — verify a condition or observe a result.
- Descriptive only: it does not change which fields are required or how a Step is
  recorded/executed (FR-012). Decision and Gate are **not** members (deferred with
  branching).
- Stored as a string column; default `Action` (R3).

## Step (enriched — working set)

One ordered instruction in a Runbook's editable working set. Constructed only via
`Runbook.ReplaceSteps` (ADR-0003).

| Field | Type | Rules |
|-------|------|-------|
| Id | Guid | unchanged |
| RunbookId | Guid | unchanged |
| Position | int | unchanged; 1-based, set by `ReplaceSteps` |
| Text | string (required) | **the title**; non-empty after trim (unchanged rule, FR-003) |
| Instructions | string? | optional; lightweight markdown source (R5); may be null/empty |
| Command | string? | optional; plain text, stored verbatim (R6) |
| ExpectedResult | string? | optional; plain text, stored verbatim (R6) |
| StepType | StepType | required; default `Action`; must be a member of the enum (FR-002) |

Validation (in the aggregate, FR-001/002/003):
- Title (`Text`) required and non-empty — unchanged.
- Instructions / Command / ExpectedResult optional; empty is valid.
- StepType must parse to `Action` or `Check`; any other value is rejected.

## RunbookVersionStep (enriched — frozen copy)

The Step as frozen inside a published Runbook Version. Constructed only by
`RunbookVersion.Freeze` (ADR-0003); no mutation afterward (FR-007, immutability).

| Field | Type | Rules |
|-------|------|-------|
| Id | Guid | unchanged |
| RunbookVersionId | Guid | unchanged |
| Position | int | unchanged |
| Text | string (required) | frozen title |
| Instructions | string? | frozen markdown source |
| Command | string? | frozen verbatim |
| ExpectedResult | string? | frozen verbatim |
| StepType | StepType | frozen; default `Action` |

`Freeze` copies all five content fields from each working `Step` into a
`RunbookVersionStep`. Editing the working Runbook and re-publishing produces a new
Version; earlier Versions' frozen detail is untouched (FR-008).

## Aggregate behavior changes

- **`Runbook.ReplaceSteps`** — signature changes from `IEnumerable<string>` to a
  structured input carrying title + optional detail + type per step (see
  `contracts/http-api.md`). Still a full ordered replacement of the working set;
  never touches published Versions (FR-006/008). Title-required validation retained.
- **`RunbookVersion.Freeze`** — copies the four new content fields in addition to
  title and position.

## Relationships

Unchanged. A Runbook has many Steps (working) and many Runbook Versions; a Runbook
Version has many RunbookVersionSteps. Execution / StepRecord / Computed Review
(slice 03) are unchanged — a `StepRecord` still references a Step by `Position`; the
run view and Computed Review read the enriched `RunbookVersionStep` by position for
display only (FR-009/011/015).

## Persistence (R7)

One additive EF Core migration adds to **both** `Steps` and `RunbookVersionSteps`:
`Instructions` (TEXT NULL), `Command` (TEXT NULL), `ExpectedResult` (TEXT NULL),
`StepType` (TEXT NOT NULL DEFAULT 'Action'). No table added/dropped; existing rows
backfill to empty detail + `Action` (R8). `EnsureCreated` is not used (C-002).
