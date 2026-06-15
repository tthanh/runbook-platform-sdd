using Microsoft.EntityFrameworkCore;
using RunbookPlatform.Api.Data;
using RunbookPlatform.Api.Domain;

namespace RunbookPlatform.Api.Endpoints;

public record StartExecutionRequest(string IncidentId, string? IncidentTitle, Guid RunbookId);
public record RecordStepRequest(int StepPosition, string Outcome, string? Note);

public static class ExecutionEndpoints
{
    public static void MapExecutionEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/executions");

        // DomainException → 400 { error } (same pattern as RunbookEndpoints).
        api.AddEndpointFilter(async (context, next) =>
        {
            try
            {
                return await next(context);
            }
            catch (DomainException e)
            {
                return Results.BadRequest(new { error = e.Message });
            }
        });

        // FR-001/002/015: start or resume. Clarification 2026-06-13: open→200, closed→409, none→201.
        api.MapPost("/", async (StartExecutionRequest req, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(req.IncidentId))
                return Results.BadRequest(new { error = "An incident identifier is required." });

            // Check for an existing execution for this incident.
            var existing = await db.Executions
                .Include(e => e.StepRecords)
                .FirstOrDefaultAsync(e => e.IncidentId == req.IncidentId.Trim());

            if (existing is not null)
            {
                if (existing.Status == ExecutionStatus.Open)
                {
                    var existingPinned = await LoadPinnedVersion(db, existing.PinnedRunbookVersionId);
                    return Results.Ok(ToRunView(existing, existingPinned, existingPinned?.NameAtPublish ?? ""));
                }
                return Results.Conflict(new { error = "This incident already has a completed Execution." });
            }

            var runbook = await db.Runbooks
                .Include(r => r.Versions)
                .FirstOrDefaultAsync(r => r.Id == req.RunbookId);

            if (runbook is null)
                return Results.BadRequest(new { error = "No such Runbook." });

            var execution = Execution.Start(runbook, req.IncidentId, req.IncidentTitle);
            db.Executions.Add(execution);
            await db.SaveChangesAsync();

            var pinnedVersion = await LoadPinnedVersion(db, execution.PinnedRunbookVersionId);
            return Results.Created($"/api/executions/{execution.Id}", ToRunView(execution, pinnedVersion, runbook.Name));
        });

        // FR-003/ADR-0004: always shows the pinned Version (silent on newer).
        api.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var execution = await db.Executions
                .Include(e => e.StepRecords)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (execution is null)
                return Results.NotFound(new { error = "No such Execution." });

            var pinnedVersion = await LoadPinnedVersion(db, execution.PinnedRunbookVersionId);
            return Results.Ok(ToRunView(execution, pinnedVersion, pinnedVersion?.NameAtPublish ?? ""));
        });

        // FR-004/005/006/007: append a Step Record; 409 if closed; 400 for bad position/outcome.
        api.MapPost("/{id:guid}/records", async (Guid id, RecordStepRequest req, AppDbContext db) =>
        {
            var execution = await db.Executions
                .Include(e => e.StepRecords)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (execution is null)
                return Results.NotFound(new { error = "No such Execution." });

            if (execution.Status == ExecutionStatus.Closed)
                return Results.Conflict(new { error = "This Execution is closed." });

            if (!Enum.TryParse<StepOutcome>(req.Outcome, ignoreCase: true, out var outcome))
                return Results.BadRequest(new { error = $"Invalid outcome '{req.Outcome}'. Must be Done, Skipped, or Failed." });

            var pinnedVersion = await LoadPinnedVersion(db, execution.PinnedRunbookVersionId);
            var maxPosition = pinnedVersion?.Steps.Count ?? 0;

            var record = execution.RecordStep(req.StepPosition, outcome, req.Note, maxPosition);
            db.StepRecords.Add(record);
            await db.SaveChangesAsync();

            return Results.Created($"/api/executions/{id}/records/{record.Id}", new
            {
                id = record.Id,
                stepPosition = record.StepPosition,
                outcome = record.Outcome.ToString(),
                note = record.Note,
                recordedAt = record.RecordedAt,
            });
        });

        // FR-009: manual close; 409 if already closed.
        api.MapPost("/{id:guid}/close", async (Guid id, AppDbContext db) =>
        {
            var execution = await db.Executions
                .Include(e => e.StepRecords)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (execution is null)
                return Results.NotFound(new { error = "No such Execution." });

            if (execution.Status == ExecutionStatus.Closed)
                return Results.Conflict(new { error = "This Execution is already closed." });

            execution.Close();
            await db.SaveChangesAsync();

            var pinnedVersion = await LoadPinnedVersion(db, execution.PinnedRunbookVersionId);
            return Results.Ok(ToRunView(execution, pinnedVersion, pinnedVersion?.NameAtPublish ?? ""));
        });

        // FR-011/012/013: Computed Review — derived on read from closed Execution (R1). 409 if open.
        api.MapGet("/{id:guid}/review", async (Guid id, AppDbContext db) =>
        {
            var execution = await db.Executions
                .Include(e => e.StepRecords)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (execution is null)
                return Results.NotFound(new { error = "No such Execution." });

            if (execution.Status == ExecutionStatus.Open)
                return Results.Conflict(new { error = "The Execution must be closed to compute its review." });

            var pinnedVersion = await LoadPinnedVersion(db, execution.PinnedRunbookVersionId);

            return Results.Ok(ToComputedReview(execution, pinnedVersion!));
        });
    }

    // Helpers ---------------------------------------------------------------

    private static async Task<RunbookVersion?> LoadPinnedVersion(AppDbContext db, Guid versionId) =>
        await db.RunbookVersions
            .Include(v => v.Steps.OrderBy(s => s.Position))
            .FirstOrDefaultAsync(v => v.Id == versionId);

    private static object ToRunView(Execution execution, RunbookVersion? pinnedVersion, string runbookName) =>
        new
        {
            id = execution.Id,
            incidentId = execution.IncidentId,
            incidentTitle = execution.IncidentTitle,
            status = execution.Status.ToString(),
            runbookName,
            pinnedVersionNumber = pinnedVersion?.Number,
            steps = pinnedVersion?.Steps.Select(s => new { s.Position, s.Text }) ?? [],
            records = execution.StepRecords
                .OrderBy(r => r.RecordedAt).ThenBy(r => r.Sequence)
                .Select(r => new
                {
                    stepPosition = r.StepPosition,
                    outcome = r.Outcome.ToString(),
                    note = r.Note,
                    recordedAt = r.RecordedAt,
                }),
        };

    private static object ToComputedReview(Execution execution, RunbookVersion pinnedVersion)
    {
        // R6: order in memory (SQLite DateTimeOffset ordering pitfall from slice-01 retro).
        var timeline = execution.StepRecords
            .OrderBy(r => r.RecordedAt).ThenBy(r => r.Sequence)
            .Select(r =>
            {
                var step = pinnedVersion.Steps.FirstOrDefault(s => s.Position == r.StepPosition);
                return new
                {
                    stepPosition = r.StepPosition,
                    stepText = step?.Text ?? "",
                    outcome = r.Outcome.ToString(),
                    note = r.Note,
                    recordedAt = r.RecordedAt,
                };
            });

        // FR-012: per-step coverage — end state = most recent record; NotReached if none.
        var lastByPosition = execution.StepRecords
            .GroupBy(r => r.StepPosition)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.RecordedAt).ThenByDescending(r => r.Sequence).First());

        var coverage = pinnedVersion.Steps.Select(s =>
        {
            var endState = lastByPosition.TryGetValue(s.Position, out var last)
                ? last.Outcome.ToString()
                : "NotReached";
            return new { stepPosition = s.Position, endState };
        });

        return new
        {
            incident = execution.IncidentTitle ?? execution.IncidentId,
            runbookName = pinnedVersion.NameAtPublish,
            pinnedVersionNumber = pinnedVersion.Number,
            startedAt = execution.StartedAt,
            closedAt = execution.ClosedAt,
            timeline,
            coverage,
        };
    }
}
