# 0003 — Rich domain model: invariants enforced inside the aggregates

## Context

Code review of the shipped 001 slice (initiative 02-rich-domain-model, inherited
discovery) found that the domain invariants hold by endpoint discipline, not by
the domain types:

- **Publish rules live outside the aggregate.** The publish gate
  (`PublishEndpoints.cs:16-19`), the sequential number assignment that is
  ADR-0001's core rule (`PublishEndpoints.cs:27`:
  `Number = runbook.Versions.Count == 0 ? 1 : runbook.Versions.Max(v => v.Number) + 1`),
  and the step-freeze (`PublishEndpoints.cs:31-40`) all sit in the HTTP handler.
  Any future code path that creates a `RunbookVersion` — the execution slice
  will add several callers — can skip the gate entirely.
- **Validation is duplicated.** The non-empty-name invariant appears at
  `RunbookEndpoints.cs:35` (create) and again at `PublishEndpoints.cs:16`
  (publish); the blank-Step check sits separately at `RunbookEndpoints.cs:64`.
  The same rule written in multiple places can drift.
- **Entities are settable property bags.** All four domain classes are public
  get/set with no behavior (`Runbook.cs:6-11`, `RunbookVersion.cs:7-14`).
  FR-006 immutability holds only because no endpoint happens to mutate a
  `RunbookVersion`; the type itself permits `version.Number = 5` or
  `version.Steps.Clear()` from anywhere.

Appetite (discovery): two evenings, refactor only, behavior unchanged; the 18
existing integration tests are the safety net.

## Options considered

1. **Status quo — anemic model, logic in the endpoints.** Keep entities as
   property bags; rely on the endpoint code and the integration tests to hold
   the invariants. Rejected because the protection is positional, not
   structural: the moment the execution slice adds a second caller that
   touches Runbooks or creates Runbook Versions, every invariant must be
   re-remembered at that call site, and the compiler offers no help. The
   evidence above shows the drift has already started (the name rule exists
   twice today, with one caller validating the request DTO and the other the
   entity).

2. **Rich domain model — invariants enforced inside the aggregates.** `Runbook`
   becomes the aggregate root: `Create(name)`, `Rename(name)`,
   `ReplaceSteps(texts)`, and `Publish()` carry the rules; `Publish()` computes
   the next sequential number and produces the frozen `RunbookVersion`
   (ADR-0001's rule moves to one place); setters become private/init where EF
   Core tolerates it cheaply; collections are exposed as `IReadOnlyList`
   backed by private lists. Violations throw a single `DomainException`
   mapped once at the endpoint seam to the existing `{ error }` 400 shape.

3. **Middle — validation objects only, entities stay anemic.** Shared
   validator classes remove the duplication (facet b) but nothing else: the
   publish gate remains skippable by any caller that forgets to invoke the
   validator (facet a unsolved), and the types stay fully settable (facet c
   unsolved). Rejected because it spends a refactor's worth of churn on one
   facet of three while leaving the structural hole — invariants the caller
   must remember to check — exactly where it is.

## Decision

Adopt a **pragmatic rich domain model** (option 2) for the Runbook Authoring
context:

- `Runbook` is the aggregate root. All state changes go through behavior
  methods — `Create`, `Rename`, `ReplaceSteps`, `Publish` — and each method
  enforces its own invariants (non-empty name, non-blank Steps, publish gate,
  sequential numbering per ADR-0001).
- `RunbookVersion` and its frozen Steps are constructed only by
  `Runbook.Publish()` and expose no mutation after construction.
- Encapsulation is pragmatic, not total: private/init setters and
  `IReadOnlyList` collections where EF Core maps them without ceremony;
  no factory hierarchies, no backing-field metadata gymnastics beyond what
  the mapping requires.
- Invariant violations throw `DomainException`; the endpoint seam catches it
  once and returns the existing `{ error }` 400 shape. HTTP behavior is
  unchanged — the 18 integration tests are the acceptance contract.

## Trade-offs accepted

- **Serialization friction.** EF Core and JSON serializers prefer settable
  properties; private setters and read-only collections require mapping
  configuration and response DTOs where today the entity shape leaks straight
  into the response. This is real, recurring cost.
- **Steeper pattern for contributors.** "Find the property, set it" no longer
  works; a contributor must find (or add) the behavior method. The pattern
  pays off only if future slices actually route through it.
- **Harder mapping at API boundaries.** Request DTOs can no longer be poured
  into entities; each endpoint translates explicitly into method calls.
  More code at the seam, in exchange for the seam being the only place rules
  could be bypassed before.
- **Pragmatic means not airtight.** Reflection, EF materialization, and
  same-assembly code can still mutate state that full encapsulation would
  seal. We accept structural protection against *accidental* misuse, not
  against deliberate circumvention.

## Consequences

- The publish gate, sequential numbering, and freeze exist in exactly one
  place (`Runbook.Publish()`); the execution slice consumes them instead of
  re-implementing them.
- The name and Step invariants exist once; the duplicated checks at
  `RunbookEndpoints.cs:35`, `PublishEndpoints.cs:16`, and
  `RunbookEndpoints.cs:64` collapse into the aggregate.
- Endpoints shrink to: load aggregate, call method, save, map response.
- The 18 integration tests must pass unmodified — any test edit means the
  refactor changed behavior and has gone wrong.
- `docs/architecture.md` conflict register entries C-001 (version uniqueness)
  and C-002 (EnsureCreated) are untouched; if EF mapping of the encapsulated
  model forces a schema change, C-002 requires migrating to EF Core
  migrations first.

## Flip condition

If, after the execution slice is built, the behavior methods are being
bypassed in practice — observable as any invariant check (re)appearing in an
endpoint or service instead of inside the aggregate, or as a second
`DomainException`-equivalent error path growing at the seam — the pattern is
not carrying its weight: supersede this ADR and either commit to full
encapsulation (if the bypasses were accidental) or return to explicit
service-level validation (if the model proved hostile to the codebase's
actual shape).

## Status + date

Proposed — 2026-06-12
