# Cleanup plan + status

Fixes from [findings.md](findings.md). Done 2026-06-28.

| # | Fix | Status | Where |
|---|-----|--------|-------|
| 1 | Add the two patterns the constitution mandated to the conflict register: SQLite date ordering done in memory (C-009), and never mix `EnsureCreated` with migrations (folded into C-002) | ✅ Done | docs/architecture.md |
| 2 | Write down the version rule ("tags follow release order") and add an initiative→tag→date table; note the out-of-order 03/04 release | ✅ Done | docs/prd-register.md → "Release tags" |
| 3 | Create the missing v0.1.2 git tag so notes and tags line up | ✅ Done | git tag v0.1.2 → commit 0d9c0da |
| 4 | Gather all deferred work into one backlog | ✅ Done | docs/deferred.md |
| 5 | Add a required "Closeout" phase to the tasks template so finishing can't be skipped | ✅ Done | .specify/templates/tasks-template.md |
| 6 | Trim the unused "outbox / no dual-writes" rule from the constitution | ⏸ Proposed — needs human acceptance | [proposed-constitution-amendment.md](proposed-constitution-amendment.md) |

## Notes

- Fixes 1–5 are doc / architecture / process changes — safe for the agent to make.
- Fix 6 touches the constitution. Per the constitution's own rule, the agent never
  changes constitution state; it is drafted as a proposal for a human to accept.
- A possible fix **not** done here: a script/CI check that fails when an initiative
  is left unfinished while a new one starts (the "closeout gate" with real teeth).
  The template change (fix 5) makes the steps visible; an automated gate would make
  them unskippable. Flagged for a future decision.
