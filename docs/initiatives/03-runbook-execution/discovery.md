# Discovery — 03-runbook-execution (inherited form)

Inherited initiative: this is the deferred payoff half of initiative 01, not a new problem.
Discovery shrinks to a pointer plus what carries forward.

Problem inherited from: [01 discovery](../01-versioned-runbook-execution/discovery.md).
The post-incident review — what happened, in what order, what was done — is reconstructed afterward from memory, chat scrollback, and scattered notes; it is slow, lossy, and unreliable.
Slice 01 shipped the authoring foundation (create and publish an immutable Runbook Version).
This slice delivers the payoff that foundation was built for: carrying a published procedure out during a live response and capturing the record as it happens, so the review is computed, not reconstructed.

Whose problem (carried from 01): on-call / incident responders running procedures under pressure, the incident commander, and the reviewer who writes the postmortem.
Payer: the engineering / reliability org carrying downtime cost and audit obligations (demo assumption: a ~30-engineer SRE org).

Evidence: unchanged from 01 — assumed demand, not validated; illustrative figures (~4 reviewable incidents/month, ~45 min/timeline reconstructed) carried as hypotheses.

Appetite: a couple of evenings (set during PRD-03 clarification, 2026-06-13), matching slice 01 — cut scope rather than extend. Scope was trimmed to fit: closing an Execution is a manual action (incident-driven close deferred), and the Step Record's actor ("who") is deferred until accounts exist.

Go/No-go: Go — 2026-06-13. The authoring slice is Released (v0.1.0); v0.1.1 hardened the domain model (ADR-0003). The foundation this slice stands on is in place.
