# Workshop Guide

> **Audience: humans.** Operating procedure for the team — not part of any
> agent context. Do not load into prompts, except the compile rule in
> Phase 3, which is pasted into the scribe prompt when used.

> How workshops are run and how `workshop.md` gets written.
> Format and timings follow Example Mapping (Wynne), Three Amigos guidance,
> and remote Event Storming practice. The core pattern: **diverge async and
> silent; spend live time only on disagreement; AI compiles, never invents.**

---

## When a workshop happens

Apply the workshop test from the constitution first. If the initiative needs
no new language, no new events, and stays in one context — skip it, leave the
receipt in `workshop.md`, and stop reading.

Schedule the workshop **no more than 1–2 weeks before the PRD is written** —
earlier, and the details fade; the output feeds directly into the PRD.

## Roles

- **Facilitator** — owns the clock and the board. Does not win arguments;
  parks them. Rotates per workshop.
- **Participants** — everyone touching the initiative (engineers + PM).
  Five people is the comfortable ceiling for one board.
- **Scribe (AI)** — compiles the board into `workshop.md` afterward.
  See the compile rule below.
- **Reviewer** — one named person, **not** the facilitator. Checks the
  compiled file against the board within 24h. Fresh eyes are the point.

## Phase 1 — Before (async, ~15 min per person, 24–48h ahead)

1. Facilitator posts the board link, the discovery page, and the PRD draft
   (if one exists), and seeds **one event from the middle of the story**
   to ignite contributions.
2. Each participant, on their own time, silently adds to the board:
   - **events** (orange) — things that happen, past tense:
     "Item created", "Request submitted"
   - **terms** (yellow) — words that need a meaning, or whose meaning
     feels contested
   - **questions** (blue) — anything unclear
3. No discussion in this phase. Duplicates are fine; silence is the tool —
   it prevents the loudest voice from writing the domain model.

A participant who adds nothing async attends the live session as a reviewer
of others' stickies, which is also a contribution.

## Phase 2 — Live (45 minutes, hard stop)

| Block | Time | What happens |
|---|---|---|
| Silent sort | 10 min | Everyone reads and rearranges the merged board into one timeline. **No talking.** Duplicates merged, gaps marked. |
| Timeline walk | 10 min | Facilitator reads the timeline aloud once. Anyone says "stop" where it's wrong or missing. Fixes that take <30 seconds happen now; anything longer becomes a sticky. |
| Hotspot harvest | 10 min | Every disagreement, unknown, or "it depends" becomes a **pink sticky with a name on it**. Zero hotspots are solved in the room. Solving is not this meeting's job. |
| Language + routing | 15 min | Ratify or amend contested terms (decisions go to the constitution's glossary). Then route **every** pink sticky: each gets an owner and a destination — a spike in the plan, or an ADR. None die on the board. |

**Hard stop means hard stop.** If the board isn't done when the clock is,
the initiative is too big — split it. Meeting overrun is a scope smell,
not a scheduling problem.

## Phase 3 — After (async)

1. **AI compiles** the board (plus any decision notes from the live session)
   into `workshop.md` using the template. One hard constraint, stated in the
   compile prompt:

   > Every event, term, hotspot, and decision in this file must trace to a
   > sticky on the board or a recorded decision from the session. Compile
   > and structure only. Do not originate content. If a section has no
   > source material, write "(none raised)".

2. **Reviewer checks fidelity** within 24h — against the board, not against
   their own opinion of the domain. Found an invention? Delete it. Found a
   missing sticky? Add it. Disagree with a decision? That's a new hotspot
   for the next session, not an edit.
3. Glossary amendments are committed to the constitution **in the same
   commit** as the workshop file. Hotspot routings appear in the plan or as
   ADR stubs.
4. Facilitator updates the register status. Done.

Total cost: ~15 async minutes per person, 45 live minutes, one 10-minute
review.

## What the file is — and is not

`workshop.md` is a **routing record**, not a transcript. It stays short
because everything in it points somewhere else: language → constitution,
hotspots → spikes/ADRs, open questions → PRD. A long workshop file means
the routing didn't happen — that is the document failing, not succeeding.

## Anti-patterns

- **The marathon.** Two-hour storms drain the room and produce worse boards
  than two 45-minute sessions a week apart.
- **Solving hotspots live.** The room's job is to *find and route*
  disagreements, not resolve them. Resolution happens in ADRs, where the
  options and trade-offs get written down.
- **AI from the void.** Letting the AI "fill in" a sparse board produces a
  beautiful file describing a domain nobody discussed. A sparse board is
  information: it means the initiative needed less workshop than scheduled.
- **The facilitator-reviewer.** The person who ran the session reviews their
  own memory, not the file. Fidelity needs fresh eyes.
- **Skipping the silent phases.** Open discussion from minute zero hands the
  model to whoever talks first.
