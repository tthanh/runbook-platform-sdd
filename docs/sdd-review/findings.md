# What's good, what's wrong, what to improve

Plain-English review of how this project runs SDD. Date: 2026-06-28.

## What's good

- **The constitution is a real contract, not decoration.** It only holds rules an
  agent can act on. It even removes its own clutter: the 2026-06-11 amendment cut
  human-only rules because they cost reading time. A rulebook that trims itself is
  a good sign.
- **You can't write a spec from nothing.** `/speckit.specify` refuses to run
  without an approved PRD. Every spec points back to a real, human-approved
  problem.
- **Decisions (ADRs) have a clean two-step.** First the proposal merges, marked
  "Proposed". Then a separate, human-signed commit marks it "Accepted". The agent
  never flips that status. And work is blocked while a decision is still only
  "Proposed". The decision and its approval live in the same place.
- **One word, one meaning.** New terms enter the glossary only when a workshop
  agrees AND a slice actually needs them — never "just in case". Each change has a
  date and a reason.
- **You can trace a line all the way through.** Requirement id → plain-English
  acceptance rule (EARS "WHEN … THE SYSTEM SHALL …") → a test named for that id →
  release notes. The "old tests still pass, untouched" check is a neat way to
  prove behaviour didn't change.

## What's wrong

1. **The constitution told us to do something, and it never got done.** The
   2026-06-15 amendment said two patterns "belong in the conflict register":
   ordering dates in memory (not in SQLite), and never mixing `EnsureCreated` with
   migrations. The date-ordering one was missing from the register. The other only
   got added today.
2. **The "finish before you start the next thing" rule had no teeth.** The
   constitution says the next initiative advances only after the current one is
   released. But initiatives 03 and 04 both sat unfinished (no architecture
   update, no tag, no retro), and 04 shipped while 03 was still unfinished. Nothing
   actually stopped it. It relied on someone remembering.
3. **Release numbers stopped making sense.** 03 was built before 04 but released
   after it, so it got a higher tag (v0.1.4 vs v0.1.3). And v0.1.2 had release
   notes but no git tag. The tags no longer line up with the order of work.
4. **The constitution lists rules for problems we don't have.** "No dual-writes /
   use an outbox" sits under "rules that never relax", but there is one database
   and nothing to sync. Every plan just writes "satisfied trivially". That is
   exactly the kind of "complexity we don't need yet" the constitution tells us to
   avoid.
5. **A stale register quietly weakens a real check.** Plans must check the conflict
   register. But one entry (C-002) was simply false for two slices — it said no
   migrations existed after they were already in use. A plan that "checked" it was
   checking a wrong fact.

## What to improve

- **Make finishing automatic, not a memory test.** Add a standard "closeout" step
  to every task list (update architecture, tag, write the retro). Optionally, a
  small check that complains if an initiative is left unfinished while a new one
  starts.
- **Update the architecture doc as part of building, not after.** A stale conflict
  register breaks the plan-time check that depends on it.
- **Write down the version rule and line the tags up.** Say plainly: tags follow
  release order. Add the missing v0.1.2 tag. Add a small table linking each
  initiative to its tag and date.
- **Move "future" rules out of "never relax".** Keep the principle, but put it in a
  clearly-marked "applies when we add a second store" note, so plans stop writing
  "satisfied trivially" and the constitution stops arguing with itself.
- **Keep all the deferred work in one place.** Today, "what's next" is scattered
  across many PRD "non-goals". One list makes it easy to see.

## The big picture

The *design* of the method here is strong — close to a model example. The misses
are all about **enforcement**: the rules that needed a person to remember a manual
step are the ones that slipped. The answer isn't more rules. It's making the rules
we have hard to skip.
