# Discovery — 02-rich-domain-model (inherited form)

Structural initiative: implements a consequence of shipped work, so discovery shrinks to three lines.

Problem inherited from: code review of 001 — the domain invariants are enforced by endpoint discipline, not by the domain types, in three facets:
(a) publish rules live outside the aggregate — the publish gate (`PublishEndpoints.cs:16-19`), the sequential version-number assignment that is ADR-0001's core rule (`PublishEndpoints.cs:27`: `Number = runbook.Versions.Count == 0 ? 1 : runbook.Versions.Max(v => v.Number) + 1`), and the step-freeze (`PublishEndpoints.cs:31-40`) are all in the HTTP handler, so any future code path creating a Runbook Version could skip the gate;
(b) validation duplicated across endpoints — the non-empty-name invariant appears at `RunbookEndpoints.cs:35` (create) and again at `PublishEndpoints.cs:16` (publish), with the blank-Step check separate at `RunbookEndpoints.cs:64`, so the same rule can drift;
(c) entities are settable property bags — all four domain classes are public get/set with no behavior (`Runbook.cs:6-11`, `RunbookVersion.cs:7-14`), so FR-006 immutability holds only because no endpoint happens to mutate a Runbook Version; the type itself permits `version.Number = 5` or `version.Steps.Clear()` from anywhere.

Appetite: two evenings — refactor only, behavior unchanged; the 18 existing integration tests are the safety net.

Go/No-go: Go — 2026-06-12.
