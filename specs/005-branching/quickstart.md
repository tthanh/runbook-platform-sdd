# Quickstart — 005-branching validation walk

End-to-end validation that branching works and that non-branched runbooks are
unchanged. Assumes the backend and frontend run per the repo README.

Prerequisites:
```bash
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet/tools:$HOME/.dotnet:$PATH"
dotnet test backend/tests/RunbookPlatform.Api.Tests   # all prior + new tests green
dotnet run --project backend/src/RunbookPlatform.Api --launch-profile http
cd frontend && npm run dev                              # proxies /api → localhost:5000
```

## Scenario A — author, publish, and freeze a branched runbook (US1)

1. Create a Runbook; add Steps where one is `Type: Decision` (e.g. "Is the database
   reachable?") with two Options routing to different later Steps.
2. Publish. Expect success and a frozen Version whose Decision Step carries its Options
   and target positions.
3. Edit the draft (reorder/insert Steps) and confirm the editor keeps each Option pointing
   at the intended Step; publish again and confirm the **first** Version's routing is
   unchanged (immutability).

**Validation gate (FR-017):**
- Publish a Decision Step with only one Option → **rejected** (400).
- Point an Option at a non-existent or earlier Step → **rejected** (loop/dangling).
- Leave a Step unreachable → **published with a warning**.
- Let a path simply end → **allowed**.

## Scenario B — follow one path at run time (US2)

1. Start an Execution against the branched Version (pins the Version; C-007).
2. Reach the Decision Step; confirm both Options are presented.
3. Choose one Option; confirm the run continues from that Option's Target Step and the
   Steps on the branch not taken are **not** presented.
4. Repeat with the other Option (new Execution) and confirm the alternate path is walked.

## Scenario C — review over the Taken Path (US3)

1. Close the branched Execution from Scenario B.
2. Open its Computed Review; confirm:
   - reached Steps show their recorded outcomes,
   - the **chosen Option** is shown at the Decision Step,
   - Steps on the branch not taken show **NotReached**, never **Skipped**.

## Scenario D — backward compatibility (US regression)

1. Author/run/review a Runbook with **no Decision Steps**.
2. Confirm authoring, publish, run, and review behave exactly as before this slice
   (Taken Path = every Step in order), and that the slice-01/03/04 test suites are green
   and unmodified.

## Expected outcomes (maps to Success Criteria)

- SC-001/002: a genuine fork ships as one runbook (Scenario A).
- SC-003: only the chosen path is presented (Scenario B).
- SC-004: chosen Option shown; not-taken Steps are NotReached, never Skipped (Scenario C).
- SC-005: no-Decision runbooks unchanged; prior tests green (Scenario D).
- SC-006: frozen Options read identically and never change after re-publish (Scenario A).
