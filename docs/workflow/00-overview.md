# SpecKit workflow — overview

This is a structured way to control what an AI coding tool produces. It splits a feature into ordered steps — describe intent, write a spec, plan, break into tasks, implement — and puts a human review between each one. It is not "the AI writes everything". The human owns the intent, the trade-offs, the review, and the final decisions. The AI drafts, structures, compares options, and implements, but only inside scope a human has agreed to. The steps exist so the AI's output is small and inspectable at each stage, instead of one large change you have to trust or reject whole.

## The workflow at a glance

```mermaid
flowchart TD
    A[Idea / PRD] --> B[/specify]
    B --> C[Spec review and clarification]
    C --> D[/plan]
    D --> E[Plan review]
    E --> F[/tasks]
    F --> G[Task review]
    G --> H[Implementation]
    H --> I[Testing / validation]
    I --> J[Pull request / delivery]
    C -. revise .-> B
    E -. revise .-> D
    G -. revise .-> F
    I -. revise .-> H
```

The solid arrows are the forward path. The dotted arrows are the normal case: a review sends a step back to be redone before you move on. Looping back is the point, not a failure.

## How the steps connect

Each step's output is the next step's input, and nothing skips ahead.

- The **idea or PRD** is plain intent — the *what* and *why*, not the technology. A PRD is not a SpecKit command or artifact; it is just a fuller written form of that same intent. SpecKit's `/specify` can also take the intent straight from a short natural-language description.
- The **spec** sits between the PRD and the technical plan. It turns business intent into clear, testable requirements (EARS form helps here: `WHEN <condition>, THE SYSTEM SHALL <behavior>`). The plan and tasks are only as good as the spec they come from.
- The **plan** makes technical decisions — and only *after* the requirements are settled. It reads the spec, not the PRD.
- **Tasks** break the plan into units small enough to implement, test, and review one at a time.
- **Implementation** carries out the tasks; **testing** checks the result against the spec's requirements; the **pull request** is where the whole change is delivered and reviewed in context.

## Where human review happens

Review is required between major steps — it is the control, not an optional polish.

- **Spec review** — before `/plan`. Is the scope right, are the requirements testable, are gaps flagged rather than filled in silently? (`/clarify` helps surface open questions.)
- **Plan review** — before `/tasks`. Are the technical choices sound, do they match the spec, are contested ones written down (this project records them as ADRs)?
- **Task review** — before implementation. Are the tasks small, ordered, and complete?
- **Pull request** — at delivery. The standard code review, with the spec and plan as context for *why* the change looks the way it does.

Mapping tests back to requirements, recording ADRs, and a post-implementation closeout are conventions some projects (including this one) layer on top. SpecKit does not enforce them.

## Where Claude Code / AI coding tools are useful

Treat the AI as a collaborator that is fast at drafting and tireless at consistency checks — not as the source of truth.

- Drafting the first version of a spec, plan, or task list from your intent, so you edit instead of starting blank.
- Generating implementation code and tests against requirements you have already agreed.
- Comparing options ("EF Core migration vs. raw SQL here, with trade-offs") so you choose with the trade-offs laid out.
- Running checks: building, running the test suite, and cross-checking the spec, plan, and tasks for contradictions.

## Where they are dangerous

The same speed that helps in drafting hurts when nobody checks the output.

- **Inventing missing scope.** A requirement the PRD never stated gets quietly added. A gap should become a question or a written assumption, never a hidden decision.
- **Making silent decisions.** A technology, data shape, or edge-case behavior gets chosen mid-implementation with no record of why.
- **Plausible but wrong output.** A spec or a function that reads cleanly and is still incorrect. Confident phrasing is not evidence; that is what the reviews are for.
- **Being treated as the source of truth.** Once people stop reading the drafts and accept them on sight, every guarantee in this workflow is gone.

## The steps in detail

Each step has its own file in this set, numbered `01` through `10`. Read them in order the first time; after that, jump to the step you are on.
