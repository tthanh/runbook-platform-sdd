# SDD Step Guide — write it, review it

> **Audience: humans (and the agent's reviewers).** Practical practices for
> each step of the pipeline. Pairs with the constitution, which is the
> authority — this guide never overrides it, it just says how to do the work
> well and how to check it.

For every step there are two jobs: **write it** and **review it**. The reviewer
is not a rubber stamp — each step has a small number of things that, if wrong,
poison every step after it. Catch those.

A rule that holds at every step: **trust the agent's output, not its
confidence.** The cleaner and more certain a draft looks, the harder you poke
its edges. Ask it to point at the source for any claim ("show me where this is
defined"), and check the cases it skipped — empty, existing data, the duplicate.

---

## 1. Constitution

*The rulebook. The fixed rules every later step must obey.*

**Write it**
- Keep it short — it is injected into every command, so every line pays a
  context tax. If a rule is not agent-actionable, move it to a guide or README.
- State rules as constraints, not advice: "criteria use EARS," "no dual-writes,"
  not "try to keep things consistent."
- One meaning per word. The glossary starts empty and grows only by amendment.
- Record *why* a rule exists in the amendment note, not in the rule itself.

**Review it**
- Is every rule something the agent can actually act on or be checked against?
- Does any new rule contradict an existing one? Two rules pulling apart is worse
  than no rule.
- Did a new word enter the glossary without a workshop ratifying it? If so, stop.
- Is the amendment dated, with a reason? A rule with no recorded "why" gets
  re-argued later.

---

## 2. Spec

*The what. The behavior the system must show, in testable sentences.*

**Write it**
- Acceptance criteria in EARS: `WHEN <condition> THE SYSTEM SHALL <behavior>`.
  One trigger, one behavior per line — never compound.
- Use only glossary words. New concept with no agreed word = stop and define it.
- Write what the system *refuses*, not just what it does. Half the value is in
  the "SHALL NOT" lines.
- Give every requirement an id (FR-001…) so tests can map back to it.
- State non-goals out loud. Unwritten scope is where features quietly grow.

**Review it**
- Could a tester turn each line into a test as written? If not, it's prose, not
  a requirement.
- Are the edges covered — empty input, existing rows, duplicates, the case
  nobody mentioned?
- Any implementation detail leaking in (a framework, an endpoint, a table)? That
  belongs in the plan, not here.
- Any `[NEEDS CLARIFICATION]` left, or a vague word ("handle," "support,"
  "correctly")? Vague = unfinished.

---

## 3. Plan

*The how, plus proof it's allowed. The approach and the rules it touches.*

**Write it**
- Say the approach in a few sentences, then show it's legal: which constitution
  rules and which existing constraints it touches.
- Promote any contested decision to its own decision record (ADR) — a choice
  that affects more than one feature, had real alternatives, or someone might
  question later. The plan *references* the decision; it never *is* the decision.
- Ask the human before deciding anything that depends on risk, budget, or
  preference the documents don't contain. Don't silently pick.
- State the appetite/scope and what gets cut first if time runs out.

**Review it**
- Does the plan re-argue a decision that should be its own record? Move it out.
- Is every contested choice backed by an accepted decision record — not still
  "proposed" — before work starts?
- Does it name what it touches that isn't in the diff: existing data, other
  services, shared interfaces, a migration?
- Does the approach match how this repo already does things? If it breaks a
  pattern, is the reason stated out loud?
- Did it check for conflicts with cross-feature constraints, and say which ones?

---

## 4. Tasks

*The to-do list. Small ordered steps, each pointing at one file.*

**Write it**
- One change per task, with the actual file path. If a task can't name its file,
  it's too big.
- Order by dependency. Put a blocking checkpoint where one must hold before the
  rest can proceed (e.g. "tests pass before any new work").
- Mark which tasks can run in parallel (different file, no pending dependency).
- Map test tasks to requirement ids, so coverage is visible, not assumed.
- Group by user story / priority so the lowest-value work can be cut from the
  bottom without unpicking the rest.

**Review it**
- Does every task name a file and a single change? Vague tasks hide work.
- Is the order right — does anything depend on a step that comes later?
- Is the "old data / existing rows" task present? Any "compute once" or schema
  change has a hidden backfill job the agent usually forgets.
- Do tests trace to requirement ids, and is anything untested?
- Are the parallel markers honest, or will two "parallel" tasks fight over the
  same file?

---

## 5. Implement

*The build. Turning tasks into commits.*

**Write it (i.e. let the agent build, on a leash)**
- Work task by task, in order. Tick each off as it lands; don't batch silently.
- Keep the engineering rules that never relax: tests map to ids, no dual-writes,
  invariants live where the design said, CI gates stay green once green.
- For schema changes, reproduce the existing schema exactly first, then add the
  new part — never mix one-shot creation with versioned migrations.
- Stop and ask when reality contradicts the plan, rather than quietly improvising
  a different design.

**Review it (this is where confidence lies most)**
- Run the tests and read the output yourself. "Tests pass" from the agent is a
  claim, not a result.
- Spot-check invented facts: a function, field, or flag it "used" that may not
  exist. Make it point at the real code.
- Re-check the edges in the running app, not just in unit tests — empty case,
  existing rows, the duplicate, the closed/blocked state.
- Did existing behavior stay intact? Old tests green and unmodified is the
  strongest proof a refactor held.
- Does the diff do only what the task said? Surprise extra changes get their own
  review or get reverted.
- After it works: update the cross-feature docs and write a one-line retro of
  what was learned. The loop isn't closed until the shared truth is updated.

---

## 6. Review the code PR

*The gate. The first check of real code against the intent — after implement,
before it lands on `main`.*

**Open it (write the PR)**
- Keep the PR scoped to one slice's tasks. A PR that does more than its tasks is
  hard to review and hides risk.
- In the description, link the spec and the tasks, and say which FR ids and ADRs
  this change satisfies — give the reviewer the map.
- Call out the risky bits yourself: the migration, the backfill of existing
  rows, anything that touches a shared interface or the conflict register.
- Make sure CI is green *before* asking for review — don't outsource that check.

**Review it**
- **Trace the FRs.** Every FR has a test, and the tests are green. An FR with no
  test is the gap, not a detail.
- **Check the never-relax rules.** No dual-writes; invariants in the aggregate,
  not the endpoint; migrations reproduce the old schema first; behavior matches
  the EARS lines.
- **Check decision honesty.** The code matches the Accepted ADRs; nothing
  silently contradicts one; ADR statuses are still true.
- **Check scope.** The diff does *only* what the tasks said. Surprise extra
  changes get questioned or reverted.
- **Check the edges in the running app**, not just unit tests — empty input,
  existing rows, the backfill, the closed/blocked state.
- **Read the CI output yourself.** "Tests pass" is a claim until you've seen it.
- **Trust the code, not the confidence.** The cleaner the diff looks, the harder
  you poke the cases it smoothed over; make every claim point at real code.
- The merge is a recorded acceptance — a named, timestamped sign-off, not an
  automated pass. Someone owns it.

---

## The short version

| Step | Write: get this right | Review: catch this |
|------|----------------------|--------------------|
| Constitution | short, actionable rules; words by amendment | contradictions; undefined new words |
| Spec | EARS, testable, glossary words, non-goals | untestable lines; missing edges; leaked design |
| Plan | approach + legality; decisions to ADRs | re-argued decisions; unseen blast radius |
| Tasks | one file per task; dependency order | vague tasks; missing backfill; untested ids |
| Implement | task by task; rules hold | the agent's confidence — verify, don't trust |
| Review PR | scoped diff; link FRs/ADRs; CI green | untested FRs; scope creep; broken rules; unseen edges |

**One line:** at every step, write it to be *checkable*, and review it by *poking
the edges the agent smoothed over* — trust its code, never its confidence.
