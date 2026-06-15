# Quickstart — 004-rich-steps validation walk

End-to-end check that a Step's detail is authored, frozen, run, and reviewed, and
that title-only Steps still work. Validates the spec's user stories and FRs. Not an
implementation guide — see tasks.md for that.

## Prerequisites

```bash
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet/tools:$HOME/.dotnet:$PATH"
```

- Apply the new migration before first run (C-002): `dotnet ef database update`
  (or run the API, which calls `Database.Migrate()` on startup).
- Backend: `dotnet run --project backend/src/RunbookPlatform.Api --launch-profile http`
- Frontend: `cd frontend && npm run dev` (proxies `/api` → localhost:5000)
- Tests: `dotnet test backend/tests/RunbookPlatform.Api.Tests`

## Scenario A — Author rich detail and freeze it (US1 / P1)

1. Create a Runbook; add a Step with a title, instructions containing markdown
   (e.g. `**bold**`, a `- list`, a `[link](https://example.com)`), a command, an
   expected result, and Step Type `Action`. Add a second Step typed `Check`. Add a
   third Step with a title only.
2. Save → reload the Runbook. **Expect**: all three Steps return; Step 1's detail and
   type are intact; Step 3 has empty detail and type `Action`.
3. Publish. Open the published Version. **Expect**: each Step's detail + type are
   frozen and shown; Step 1's markdown renders (bold, list, link), the command and
   expected result show verbatim.
4. Edit Step 1's command on the working Runbook and publish again. Open Version 1.
   **Expect**: Version 1's Step 1 command is unchanged (FR-008); Version 2 has the
   edit.

## Scenario B — Read detail while running (US2 / P2)

1. Start an Execution against the published Runbook for an incident id.
2. Open a Step in the run view. **Expect**: instructions (markdown rendered), command,
   and expected result are visible **before** marking the Step; a title-only Step
   shows just its title.
3. Mark Steps done/skipped/failed. **Expect**: recording behaves exactly as slice 03
   (Step Records appended; detail does not change recording).

## Scenario C — Read detail in the Computed Review (US3 / P3)

1. Close the Execution and open its Computed Review.
2. **Expect**: each recorded Step in the timeline shows its detail (rendered
   instructions, command, expected result, type) alongside its outcome; coverage
   still lists every pinned Step with its end state, "not reached" included.

## Scenario D — Safety and verbatim rendering (FR-005/006)

1. Author instructions containing raw HTML / a `javascript:` link / a `<script>` tag.
   View the Step. **Expect**: nothing executes; markup is shown escaped or stripped to
   the safe whitelist; links use only http/https/mailto.
2. Author a command containing `*`, `_`, and backticks. **Expect**: shown verbatim,
   not interpreted as markdown.

## Scenario E — Backward compatibility (SC-004)

1. Against a database created before this slice (title-only Steps), load, edit,
   publish, run, and review. **Expect**: no error; old Steps display by title; detail
   empty; type `Action`.

## Pass criteria

- All scenarios behave as stated; the slice-01/03 test suites stay green and
  unmodified; new tests cover FR-001…FR-015 (mapped at /speckit.tasks).
