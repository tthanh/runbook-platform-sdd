# 0001 — Runbook Version identity: per-Runbook sequential integers

## Context

Publishing freezes a Runbook's Steps into an immutable Runbook Version, and the
whole product promise — "which version was in effect" — hangs on how a version
is identified (workshop hotspot H1). The PRD fixes the author-facing behavior:
"the first published version is 1, the next 2, and so on" (author decision,
2026-06-12). This ADR formalizes the scheme and records the alternatives, since
the choice outlives this slice: a later execution slice will pin these
identifiers permanently.

## Options considered

1. **Per-Runbook sequential integer** (1, 2, 3, …) assigned at publish.
   Human-readable, naturally ordered, matches how authors talk ("v3 of the
   failover runbook").
2. **Content hash** of the published Steps. Self-verifying and global, but
   opaque to authors, unordered, and collides intentionally when two versions
   have identical content — yet the spec requires republishing unchanged
   content to create a *new* version.
3. **Semantic versioning** (major.minor.patch). Implies a semantic judgement
   (breaking vs. minor) that no requirement asks authors to make.

## Decision

Each published Runbook Version is identified by a **per-Runbook sequential
integer**, starting at 1 and increasing by 1 with each publish. The number is
assigned inside the publish transaction as `max(existing) + 1`, and the database
enforces uniqueness of (Runbook, number). Numbers are never reused — which,
combined with the no-delete rule (FR-011), keeps the sequence gapless this
slice.

## Trade-offs accepted

- Numbers carry no content fingerprint; integrity of a version's content is
  guaranteed by immutability-by-construction (no update path), not by the
  identifier.
- Two identical publishes get different numbers — accepted deliberately; the
  spec treats every publish as a new Runbook Version.
- Sequence assignment requires a transaction; trivial on the chosen single
  store (ADR-0002).

## Consequences

- Authors and the UI refer to "Version N" everywhere; "current" is simply the
  highest N (derived, not stored).
- The unique (Runbook, number) pair is the stable foreign identity the future
  execution slice will pin against.
- API contract paths use the number directly: `/runbooks/{id}/versions/{number}`.

## Flip condition

If a later slice requires globally unique, location-independent version
identity (e.g., cross-system references where a Runbook id + number pair can't
travel), introduce a supplementary global identifier via a superseding ADR —
the author-facing sequential number stays regardless.

## Status + date

Proposed — 2026-06-12
