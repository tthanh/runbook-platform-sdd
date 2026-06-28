# Deferred work

One place for things we chose **not** to build yet. Each slice pushes ideas here
through its PRD "non-goals" — this list gathers them so "what's next" is easy to
see. It is a backlog, not a promise; picking the next initiative is still a human
call that starts with discovery + a PRD.

Last gathered: 2026-06-28 (from the four shipped PRDs and the constitution).

## Most likely next

- **Branching / decision steps** — let a run take different paths instead of one
  straight line. The `Step Type` enum was kept closed (`Action`, `Check`) on
  purpose; the `Decision` and `Gate` types wait for this. Needs its own ADRs
  because it breaks "a run is linear" and "coverage = every step".
  Source: [04 discovery](initiatives/04-rich-steps/discovery.md), [04 PRD](initiatives/04-rich-steps/prd.md), constitution glossary (Step Type).

## Needs accounts first

- **Capture who did a step.** The Step Record has no "who" because there is no
  login anywhere. Deferred until accounts exist.
  Source: [03 PRD](initiatives/03-runbook-execution/prd.md), constitution glossary (Step Record).
- **Accounts / authentication** itself — the thing the above waits on.
- **Approval / sign-off before publishing.** Source: [01 PRD](initiatives/01-versioned-runbook-execution/prd.md).

## Around execution

- **Browse / search past executions** — today you see one run and its review, not a
  history. Source: [03 PRD](initiatives/03-runbook-execution/prd.md).
- **Reporting across many runs** (dashboards, analytics). Source: [03 PRD](initiatives/03-runbook-execution/prd.md).
- **Compare / diff two executions or reviews.** Source: [03 PRD](initiatives/03-runbook-execution/prd.md).
- **Close a run from the external incident** (instead of closing by hand). Needs an
  incident-tool integration. Source: [03 PRD](initiatives/03-runbook-execution/prd.md).
- **Run a runbook outside an incident** (drills, rehearsals). Source: [03 PRD](initiatives/03-runbook-execution/prd.md).
- **More than one run per incident / restart a run.** Source: [03 PRD](initiatives/03-runbook-execution/prd.md).
- **Several people on one run, live.** Conflicts with the single-user NFR. Source: [03 PRD](initiatives/03-runbook-execution/prd.md).
- **Write the postmortem narrative** on top of the Computed Review (root cause,
  action items). Source: [03 PRD](initiatives/03-runbook-execution/prd.md).
- **Assign steps to people; reminders, timers, SLAs.** Source: [03 PRD](initiatives/03-runbook-execution/prd.md).
- **Enforce step order** during a run. Today we capture order, not enforce it.
  Source: [01 deferred capability](initiatives/01-versioned-runbook-execution/prd.md).

## Around authoring

- **Organise runbooks** — search, tags, folders, categories. Source: [01 PRD](initiatives/01-versioned-runbook-execution/prd.md).
- **Import / export / clone runbooks.** Source: [01 PRD](initiatives/01-versioned-runbook-execution/prd.md).
- **Real-time multi-author editing.** Source: [01 PRD](initiatives/01-versioned-runbook-execution/prd.md).
- **More step fields** — owner, expected duration, a rollback field. Source: [04 PRD](initiatives/04-rich-steps/prd.md).
