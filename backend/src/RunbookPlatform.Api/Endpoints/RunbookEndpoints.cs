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

        // ADR-0003: invariants live in the aggregate and surface as
        // DomainException — mapped to the { error } 400 shape in exactly
        // one place: this group filter.
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
                currentVersionNumber = r.CurrentVersionNumber,
            }));
        });

        // FR-001: create a Runbook with a non-empty name.
        api.MapPost("/", async (CreateRunbookRequest request, AppDbContext db) =>
        {
            var runbook = Runbook.Create(request.Name);
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

            runbook.ReplaceSteps(request.Steps.Select(s => s.Text));
            // EF treats navigation-discovered entities with client-set Guid
            // keys as existing rows; mark the replacements as inserts.
            db.Steps.AddRange(runbook.Steps);
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                steps = runbook.Steps.Select(s => new { s.Position, s.Text }),
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
        currentVersionNumber = runbook.CurrentVersionNumber,
        versions = runbook.Versions.OrderBy(v => v.Number)
            .Select(v => new { v.Number, v.PublishedAt }),
    };
}
