# Research — 004-rich-steps (Phase 0)

Resolves the design unknowns behind the plan. Each item: Decision / Rationale /
Alternatives considered. Contested items that affect multiple surfaces or could be
questioned later are promoted to ADRs (R4 → ADR-0006, R3 → ADR-0007).

## R1 — Keep `Step.Text` as the required title (no rename)

**Decision**: Keep the existing `Step.Text` / `RunbookVersionStep.Text` column as
the Step's required **title**; do not rename it to `Title`. Add the new fields
alongside it.

**Rationale**: `Text` already means "one ordered instruction" — the glossary's
title. Renaming would force a column-rename migration plus edits to every existing
authoring/execution test and the frontend `text` field, for zero behavior gain. The
constitution forbids unearned complexity; an additive change keeps slice-01/03 tests
green and the migration purely additive.

**Alternatives**: Rename `Text` → `Title` (rejected — churn across tests, API, and a
rename migration with no functional benefit); expose `text` in code but `title` in
the API (rejected — domain/API naming mismatch).

## R2 — Model detail as flat properties, not a value object

**Decision**: Add `Instructions`, `Command`, `ExpectedResult` (nullable strings) and
`StepType` as flat properties on `Step` and `RunbookVersionStep`, constructed only
via `Runbook.ReplaceSteps` and `RunbookVersion.Freeze` (ADR-0003).

**Rationale**: Encapsulation (construction through aggregate behavior) is already
satisfied by the existing pattern. A `StepDetail` owned value object adds EF
owned-type configuration and indirection without an invariant that spans the three
fields. Complexity is earned by a requirement; none requires grouping them.

**Alternatives**: A `StepDetail` value object owning the three text fields + type
(rejected for this slice — no cross-field invariant justifies it; revisit if later
slices add per-detail rules).

## R3 — Step Type is a closed enum {Action, Check}, stored as text → ADR-0007

**Decision**: `StepType` is a closed C# enum with two values, **Action** and
**Check**, stored as a string column (default `Action`), descriptive only.

**Rationale**: Settled by the ratified glossary (2026-06-15) and the PRD: minimal
set now, Decision/Gate deferred with branching. Stored as text for readability and
consistency with how `StepOutcome` is handled at the seam. Default `Action` backfills
existing rows so they stay valid (R8). Promoted to **ADR-0007** because the workshop
routed H3 there and "why closed / why two" is worth recording against future
re-litigation.

**Alternatives**: Open/user-extensible type set (rejected — unearned, and the glossary
fixes the set); include Decision/Gate now (rejected — they imply control flow, which
is a separate initiative); store as int (rejected — opaque in the DB and at the API).

## R4 — Markdown rendering & sanitization: hand-rolled, escape-first → ADR-0006

**Decision**: Render the instructions body with a small in-repo frontend module
(`frontend/src/lib/markdown.ts`) that **escapes all HTML first**, then applies a
fixed whitelist of markdown rules, then emits a safe HTML string. No markdown or
sanitizer dependency. (Human decision, 2026-06-15.)

**Rationale**: The frontend has zero runtime dependencies beyond React and no router
package (C-003) — a deliberate minimal-deps stance. The platform is single-user,
local, and demonstrative (author and responder are the same trusted user), so the
threat model is low; escape-first construction makes injection impossible regardless.
Owning a tiny renderer fits the ethos and keeps the bundle and supply-chain surface
flat. See ADR-0006 for the full trade-off and the supported subset.

**Alternatives**: Vetted libraries (markdown-it + DOMPurify) — most robust, rejected
as the first runtime deps beyond React against the established grain; server-side
render to sanitized HTML at publish — rejected for storing rendered HTML (muddies the
"freeze the source markup" model) and adding a backend dependency.

## R5 — Supported markdown subset

**Decision**: Support a "lightweight" subset: bold, italic, inline code, fenced code
blocks, links, unordered and ordered lists, and paragraph/line breaks. Exclude raw
HTML, images, tables, and headings.

**Rationale**: Covers what an incident instruction needs (emphasis, a command inline
or in a block, a link to a dashboard, a short list) while keeping the hand-rolled
renderer small and safe. Images/raw-HTML are the main injection vectors and are out
(ADR-0006). The exact grammar is an implementation detail for tasks.

**Alternatives**: Full CommonMark (rejected — needs a library, R4); plain text only
(rejected — the PRD chose markdown).

## R6 — Command and expected result render verbatim

**Decision**: The **command** and **expected result** are plain text, displayed
verbatim with whitespace preserved (monospaced for the command). They are **not**
parsed as markdown.

**Rationale**: A command rendered as markdown would mangle special characters
(asterisks, backticks, underscores). Verbatim display is correct and avoids a second
render path. Matches FR-005.

**Alternatives**: Markdown for all three fields (rejected — corrupts commands, widens
the safe-render surface).

## R7 — One additive migration (C-002)

**Decision**: Add a single EF Core migration that adds `Instructions`, `Command`,
`ExpectedResult` (nullable TEXT) and `StepType` (TEXT not null, default `Action`) to
both `Steps` and `RunbookVersionSteps`. No table is added or dropped.

**Rationale**: Slice 03 already replaced `EnsureCreated` with migrations (constitution
2026-06-15), so the procedure exists; this is a normal additive migration. Nullable
detail + a defaulted enum column means existing rows migrate without data loss and
stay valid (R8). Mixing `EnsureCreated` is forbidden (C-002).

**Alternatives**: Recreate the schema (rejected — destroys data, violates C-002); a
separate table for detail (rejected — no need; detail belongs to the Step row).

## R8 — Backward compatibility

**Decision**: All detail is optional and Step Type defaults to `Action`. Steps
authored before this slice (title only) load, edit, publish, run, and review
unchanged; their detail reads as empty and their type as Action.

**Rationale**: FR-003 / SC-004 require existing Steps to stay valid. Nullable columns
+ a defaulted enum guarantee it at the storage layer; the read surfaces render a
title-only Step by its title alone (spec edge cases).

**Alternatives**: Require detail (rejected — breaks every existing Step and the
"title-only is valid" rule).

## R9 — API contract evolution (additive)

**Decision**: `SaveStepItem` gains optional `instructions`, `command`,
`expectedResult` and a `type`; the keep-named `text` remains the required title.
Read responses (runbook detail, version view, run view, computed review) gain the
same fields per Step. The typed client and its interfaces are extended to match.

**Rationale**: Additive fields keep the contract backward compatible; retaining
`text` avoids a breaking rename (R1). Full shapes in `contracts/http-api.md`.

**Alternatives**: A nested `detail` object per step (rejected — flat fields match the
existing flat `{ position, text }` shape and the data model R2).

## R10 — Frontend read/write surfaces, no new dependency

**Decision**: Add authoring inputs (instructions, command, expected result, a type
selector) to the Runbook detail editor; render the frozen detail in the version view,
the Execution run view (before recording), and the Computed Review. Instructions go
through `lib/markdown.ts`; command/expected result render verbatim. All within hash
routing; no package added.

**Rationale**: Matches the three read surfaces in the spec (US2, US3) and FR-009/010/011.
Keeps C-003 and the minimal-deps ethos (ADR-0006).

**Alternatives**: A shared step-detail component vs inline per page — an implementation
choice left to tasks; either stays dependency-free.
