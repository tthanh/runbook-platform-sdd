# Quickstart: Runbook Authoring — run & validate

Validation guide proving the feature end-to-end. Details live in
[data-model.md](data-model.md) and [contracts/http-api.md](contracts/http-api.md);
this file only runs and checks.

## Prerequisites

- .NET 10 SDK
- Node.js 20+ (frontend)

## Run

```bash
# backend (serves the API on http://localhost:5000)
cd backend/src/RunbookPlatform.Api
dotnet run

# frontend (dev server proxying /api to the backend)
cd frontend
npm install
npm run dev
```

Open the printed frontend URL. The SQLite database file is created on first run;
delete it to reset.

## Validate — maps to user stories & FRs

### US1 — create and publish Version 1

1. Create a Runbook named "Database failover" (FR-001).
2. Add Steps "Page the on-call DBA", "Verify replica lag" (FR-002).
3. Publish → expect **Version 1** with those two Steps (FR-003–FR-005, US1-3).
4. Try publishing a fresh Runbook with no Steps → refused with "at least one
   Step" (US1-4); empty name on create → refused (US1-5).

### US2 — edit, republish, immutability

5. Edit the working Steps (reword one, add one), publish again → **Version 2**
   (US2-2).
6. Open **Version 1** → original content, unchanged (US2-1, FR-006).
7. Confirm the UI shows Version 2 as current by default (FR-008, US2-4).

### US3 — browse and view

8. Runbook list shows every Runbook; unpublished ones show no version number
   (FR-009, US3-1).
9. Open Version 1 by its number → exact published Steps (US3-2).
10. A Runbook with no published versions says so and shows editable Steps (US3-3).

### Edge cases

11. Publish twice with no edits → next number, identical content.
12. Request a non-existent version number → "no such Runbook Version".
13. Save a Step with blank text → refused.

## Automated check

```bash
cd backend
dotnet test   # xUnit integration tests; test names carry the FR ids they verify
```

Expected: all green; coverage includes FR-001…FR-011 (FR-010 asserted as
"no auth demanded"; FR-011 as "no DELETE route exists").
