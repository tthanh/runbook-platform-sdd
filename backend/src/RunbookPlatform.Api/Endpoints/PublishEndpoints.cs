using RunbookPlatform.Api.Data;
using RunbookPlatform.Api.Domain;

namespace RunbookPlatform.Api.Endpoints;

public static class PublishEndpoints
{
    public static void MapPublishEndpoints(this RouteGroupBuilder api)
    {
        // FR-003 publish gate; FR-004 freeze; FR-005 sequential number (ADR-0001).
        api.MapPost("/{id:guid}/publish", async (Guid id, AppDbContext db) =>
        {
            var runbook = await RunbookEndpoints.LoadRunbook(db, id);
            if (runbook is null) return RunbookEndpoints.NotFound();

            if (string.IsNullOrWhiteSpace(runbook.Name))
                return Results.BadRequest(new { error = "A name is required." });
            if (runbook.Steps.Count == 0)
                return Results.BadRequest(new { error = "At least one Step is required." });

            // ADR-0001: number assigned inside the save transaction as max+1;
            // the unique (RunbookId, Number) index guards races.
            var version = new RunbookVersion
            {
                Id = Guid.NewGuid(),
                RunbookId = runbook.Id,
                Number = runbook.Versions.Count == 0 ? 1 : runbook.Versions.Max(v => v.Number) + 1,
                NameAtPublish = runbook.Name,
                PublishedAt = DateTimeOffset.UtcNow,
            };
            version.Steps = runbook.Steps
                .OrderBy(s => s.Position)
                .Select(s => new RunbookVersionStep
                {
                    Id = Guid.NewGuid(),
                    RunbookVersionId = version.Id,
                    Position = s.Position,
                    Text = s.Text,
                })
                .ToList();

            db.RunbookVersions.Add(version);
            await db.SaveChangesAsync();

            return Results.Created(
                $"/api/runbooks/{runbook.Id}/versions/{version.Number}",
                new
                {
                    number = version.Number,
                    nameAtPublish = version.NameAtPublish,
                    publishedAt = version.PublishedAt,
                    steps = version.Steps.Select(s => new { s.Position, s.Text }),
                });
        });
    }
}
