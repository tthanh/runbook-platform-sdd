# Data Model: Runbook Authoring

Entities use the binding glossary names (Runbook, Runbook Version, Step).
Identity scheme per ADR-0001; storage per ADR-0002.

## Runbook

The authored, evolving thing (glossary). Holds the working state an author edits.

| Field | Type | Rules |
|-------|------|-------|
| id | unique identifier | system-assigned at create |
| name | text | non-empty (FR-001); not required unique (spec assumption) |
| createdAt | timestamp | system-assigned |

Relationships: has an ordered list of **Steps** (working set, editable); has zero
or more **Runbook Versions**.

## Step (working)

One ordered instruction in a Runbook's editable working set (FR-002).

| Field | Type | Rules |
|-------|------|-------|
| id | unique identifier | system-assigned |
| runbookId | reference → Runbook | required |
| position | integer ≥ 1 | contiguous per Runbook; defines order |
| text | text | non-empty on save (edge case: empty Step refused) |

Mutability: fully editable (add/edit/remove/reorder) — editing never touches any
published Runbook Version (FR-006, US2-1).

## Runbook Version

An immutable snapshot frozen at publish (glossary, FR-004–FR-007).

| Field | Type | Rules |
|-------|------|-------|
| runbookId | reference → Runbook | required |
| number | integer ≥ 1 | per-Runbook sequential, starts at 1, +1 per publish (FR-005, ADR-0001); **unique (runbookId, number)** enforced by the database |
| nameAtPublish | text | copy of the Runbook's name at publish (FR-006) |
| publishedAt | timestamp | system-assigned at publish |

Relationships: contains an ordered list of **frozen Step copies**.

Mutability: **none after insert** — no update or delete code path exists
(research R4, FR-006, FR-011).

## Frozen Step copy (within a Runbook Version)

The Step content captured at publish. Not a fourth domain concept — it is the
Step as it exists inside a published Runbook Version (FR-004).

| Field | Type | Rules |
|-------|------|-------|
| runbookId + versionNumber | reference → Runbook Version | required |
| position | integer ≥ 1 | order as it stood at publish |
| text | text | copied verbatim at publish |

## State & transitions

```
Runbook (working, editable)
   │  publish — allowed only if name non-empty AND ≥1 Step with non-empty text (FR-003)
   ▼
Runbook Version N  (immutable forever; N = previous max + 1, or 1 if none)
```

- "Current" Runbook Version = the one with the highest number (FR-008); derived,
  not stored.
- Publishing with no edits since the last publish still creates N+1 (spec
  assumption/edge case).
- No transition ever deletes or alters a Runbook or a published Runbook Version
  (FR-011).

## Validation summary (maps to FRs)

| Rule | FR |
|------|----|
| Create requires non-empty name | FR-001 |
| Saved Step text non-empty | FR-002 (edge case) |
| Publish gate: non-empty name + ≥1 non-empty Step | FR-003 |
| Version number sequential per Runbook from 1 | FR-005 |
| Published content never changes | FR-006 |
| Nothing is deletable | FR-011 |
