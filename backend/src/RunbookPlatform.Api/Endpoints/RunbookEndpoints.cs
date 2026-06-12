using Microsoft.EntityFrameworkCore;
using RunbookPlatform.Api.Data;
using RunbookPlatform.Api.Domain;

namespace RunbookPlatform.Api.Endpoints;

public record CreateRunbookRequest(string Name);
public record SaveStepsRequest(List<SaveStepItem> Steps);
public record SaveStepItem(string Text);

public static class RunbookEndpoints
{
    public static void MapRunbookEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/runbooks");

        // FR-009: list all Runbooks, including those with nothing published.
        api.MapGet("/", async (AppDbContext db) =>
        {
            var runbooks = await db.Runbooks
                .Include(r => r.Versions)
                .ToListAsync();
            // Ordered in memory: SQLite cannot ORDER BY DateTimeOffset.
            return Results.Ok(runbooks.OrderBy(r => r.CreatedAt).Select(r => new
            {
                id = r.Id,
                name = r.Name,
                currentVersionNumber = CurrentVersionNumber(r),
            }));
        });

        // FR-001: create a Runbook with a non-empty name.
        api.MapPost("/", async (CreateRunbookRequest request, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { error = "A name is required." });

            var runbook = new Runbook
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                CreatedAt = DateTimeOffset.UtcNow,
            };
            db.Runbooks.Add(runbook);
            await db.SaveChangesAsync();

            return Results.Created($"/api/runbooks/{runbook.Id}", ToDetail(runbook));
        });

        // Detail: working Steps + published versions summary.
        api.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var runbook = await LoadRunbook(db, id);
            return runbook is null ? NotFound() : Results.Ok(ToDetail(runbook));
        });

        // FR-002 (research R5): full ordered replacement of the working Steps.
        // Never touches published Runbook Versions (FR-006).
        api.MapPut("/{id:guid}/steps", async (Guid id, SaveStepsRequest request, AppDbContext db) =>
        {
            var runbook = await LoadRunbook(db, id);
            if (runbook is null) return NotFound();

            if (request.Steps.Any(s => string.IsNullOrWhiteSpace(s.Text)))
                return Results.BadRequest(new { error = "A Step needs content." });

            var newSteps = request.Steps
                .Select((s, i) => new Step
                {
                    Id = Guid.NewGuid(),
                    RunbookId = runbook.Id,
                    Position = i + 1,
                    Text = s.Text.Trim(),
                })
                .ToList();
            db.Steps.RemoveRange(runbook.Steps);
            db.Steps.AddRange(newSteps);
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                steps = newSteps.Select(s => new { s.Position, s.Text }),
            });
        });

        api.MapPublishEndpoints();
        api.MapVersionEndpoints();
    }

    internal static async Task<Runbook?> LoadRunbook(AppDbContext db, Guid id) =>
        await db.Runbooks
            .Include(r => r.Steps.OrderBy(s => s.Position))
            .Include(r => r.Versions)
            .FirstOrDefaultAsync(r => r.Id == id);

    internal static IResult NotFound() =>
        Results.NotFound(new { error = "No such Runbook." });

    internal static object ToDetail(Runbook runbook) => new
    {
        id = runbook.Id,
        name = runbook.Name,
        steps = runbook.Steps.OrderBy(s => s.Position).Select(s => new { s.Position, s.Text }),
        currentVersionNumber = CurrentVersionNumber(runbook),
        versions = runbook.Versions.OrderBy(v => v.Number)
            .Select(v => new { v.Number, v.PublishedAt }),
    };

    // FR-008: "current" is the highest published number — derived, not stored.
    internal static int? CurrentVersionNumber(Runbook runbook) =>
        runbook.Versions.Count == 0 ? null : runbook.Versions.Max(v => v.Number);
}
