# Proposed constitution amendment — trim a rule we don't use

**Status: Proposed — awaiting human acceptance.**
The agent does not change the constitution on its own. This is a draft for a human
to accept (by editing the constitution) or reject. It is written the same way ADRs
are proposed.

## The question

You asked: if a rule isn't used anywhere, can we remove it to keep the constitution
cleaner? Short answer: yes — for one specific line — and removing it is actually
*more* in line with this constitution's own values.

## What this is about

Under **"Engineering rules that never relax"** the constitution says:

> - No dual-writes; cross-store propagation rides the outbox.

There is one database and nothing to sync between stores. So every plan just writes
"satisfied trivially". That is exactly the "complexity we don't need yet" the same
constitution tells us to avoid ("Complexity is earned by a requirement in the spec,
never anticipated"). The rule is a solution waiting for a problem we don't have.

Two near-by lines should stay — they are not the same case:

- *"Tests map to requirement ids"* — actually used (tests are named for FR ids).
  **Keep.**
- *"CI gates (leak, replay, idempotency) stay green once introduced"* — already
  guards itself with "once introduced", so it binds nothing until such gates exist.
  Harmless. **Keep** (optional: move it next to the outbox note for tidiness).

## Recommended change

Don't just delete the line and lose the idea. Do it the way the glossary defers
terms — remove it from the active rules, but record why and when to bring it back.

1. In **"Engineering rules that never relax"**, remove:
   > - No dual-writes; cross-store propagation rides the outbox.

2. Add an **Amendments** entry (dated, with a reason and a flip condition):

   > - 2026-06-28: Removed "No dual-writes; cross-store propagation rides the
   >   outbox" from the engineering rules. The app has one store and nothing to
   >   propagate, so every plan marked it "satisfied trivially" — anticipated
   >   complexity, which this constitution forbids elsewhere. Flip condition:
   >   reintroduce it (via an ADR) the first time a slice adds a second store or
   >   async cross-store propagation — the moment the rule has a real problem to
   >   guard. Reason: a rule that never binds is noise; defer it until a slice
   >   needs it, exactly as the glossary defers terms.

## Why a flip condition instead of a clean delete

"No dual-writes" is a good guardrail to have written down *before* someone adds a
second store. The flip condition keeps that safety: the idea is not lost, it is
parked with a clear trigger to bring it back. If you'd rather delete it outright and
trust the normal "earned + ADR" path to reintroduce it, that also works — your call.

## How to accept

Per the constitution, acceptance is a human edit to `constitution.md` in its own
commit. Tell me to apply it and I'll prepare that edit for you to commit, or you can
make the edit yourself.
