# 0005 — Incident is a foreign reference, not an owned entity

## Context

Workshop hotspot **H5** (initiative 01, carried into 03). The glossary makes
*Incident* a **boundary term**: identity is owned by an external
incident-management tool (incident.io-style); this platform holds a reference,
not the source of truth. The execution slice must fix what that reference *is*
and how the system holds it.

Two facts bound the decision: (a) there is no live integration with the external
tool in this slice — incident-driven close is deferred (FR-009), so the system
cannot fetch or sync incident data; and (b) the Computed Review names the
incident, so the reference must carry enough to be legible to a human reader.

## Options considered

1. **Own an Incident entity/table.** Model Incident as a first-class persisted
   entity with its own lifecycle. Rejected: it duplicates a source of truth the
   platform explicitly does not own, invites drift between our copy and the
   tool's, and buys nothing this slice needs (no querying or reacting to
   incidents is in scope).
2. **Foreign reference value on the Execution: identifier + optional title.**
   Store, directly on the Execution, a required free-text identifier (the
   external incident's id or URL) and an optional human-readable title, entered
   manually at start, kept verbatim, never validated against or synced with the
   external tool.
3. **Identifier only.** As (2) but no title. Rejected: the Computed Review would
   show an opaque id with no human context, weakening the very artifact the
   slice exists to produce.
4. **Full integration / sync with the external tool.** Rejected: out of scope
   and far past the appetite; it would turn the boundary into a live upstream
   dependency this slice deliberately avoids.

## Decision

Adopt **option 2**. There is **no Incident entity or table**. The Execution
carries a foreign reference as plain values:

- `IncidentId` — required free-text string (the external incident's id or URL),
  supplied by the responder at Execution start, stored verbatim.
- `IncidentTitle` — optional free-text string, a human-readable label for the
  Computed Review.

Neither is validated against, nor synchronised with, the external tool. The
one-Execution-per-incident rule (FR-015) keys on `IncidentId`.

## Trade-offs accepted

- **The identifier is unverified** — a responder can mistype or paste the wrong
  id, and the system cannot tell. Accepted: there is no integration to validate
  against, and the appetite does not fund one.
- **Deduplication is only as good as the typed id.** Because FR-015 keys on
  `IncidentId`, a mistyped id could permit an unintended second Execution for
  what is really the same incident. Accepted at this appetite.
- **The title can drift** from the external tool's title over time, since it is
  a manual snapshot. Accepted — it is a label for the review, not a source of
  truth.
- **A shared id collides.** Because `IncidentId` is free-text and uniquely
  indexed (data-model.md), two genuinely different incidents accidentally given
  the same id will collide — the unique constraint wrongly refuses the second
  incident's legitimate Execution. Accepted: unvalidated input is the cost of
  holding no integration; reliable deduplication is the flip trigger.

## Consequences

- The data model gains no Incidents table; `IncidentId` (required) and
  `IncidentTitle` (optional) are columns on the Execution.
- Confirms the context boundary: one bounded context (**Runbook Execution**)
  with incident management as an **external** context the platform only
  references. This **reconciles `docs/architecture.md`**, whose context-map line
  currently says the execution slice "will introduce a second bounded context" —
  it does not; authoring and execution remain one context, and the only boundary
  is the external Incident. (The architecture.md line is corrected at the
  post-implement amendment, per the constitution's release rhythm.)
- The Computed Review renders `IncidentTitle` when present, else `IncidentId`.

## Flip condition

If the platform needs to *react to* incident state — incident-driven close, or
reliable deduplication — supersede this ADR to integrate the external tool as a
real upstream context behind an anti-corruption layer. That is the point at which
Incident stops being a stored value and becomes a synced reference.

## Status + date

Proposed — 2026-06-13
