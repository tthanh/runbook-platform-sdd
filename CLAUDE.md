# Repo orientation

Your contract during Spec Kit commands is .specify/memory/constitution.md
(injected by Spec Kit) — it is the authority; nothing here repeats it.

Two layers:
- docs/ is NOT part of the Spec Kit SDD flow. It lives in this repo for
  convenience — to demonstrate, in one place, the work that happens before
  and around the pipeline (discovery notes, workshop records, PRDs, ADRs,
  the register). Treat it as normal working territory: when asked, draft,
  compile, edit, or review anything in it like any other file.
- specs/ and src/ are the SDD engine's workspace: the Spec Kit pipeline
  (specify → plan → tasks → implement) operates here, per the constitution.

During Spec Kit commands specifically, your touchpoints into docs/ are:
- docs/initiatives/NN-name/prd.md — required input for /speckit.specify
  (no PRD path → ask, don't specify)
- docs/prd-register.md — update the statuses you own
  (Specify → Plan → Tasks → Implement), committed with the work
- docs/adr/ — promote contested decisions from plans here
- docs/architecture.md — check its conflict register during /speckit.plan;
  amend it after implement

<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan
at specs/004-rich-steps/plan.md
<!-- SPECKIT END -->

Shell notes (required before running dotnet or dotnet-ef):
  export DOTNET_ROOT="$HOME/.dotnet"
  export PATH="$HOME/.dotnet/tools:$HOME/.dotnet:$PATH"
dotnet-ef is installed globally at $HOME/.dotnet/tools/dotnet-ef.
Run the backend: dotnet run --project backend/src/RunbookPlatform.Api --launch-profile http
Run all tests:   dotnet test backend/tests/RunbookPlatform.Api.Tests
Run the frontend: cd frontend && npm run dev   (proxies /api → localhost:5000)
