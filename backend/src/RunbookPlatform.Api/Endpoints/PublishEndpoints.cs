using RunbookPlatform.Api.Data;

namespace RunbookPlatform.Api.Endpoints;

public static class PublishEndpoints
{
    public static void MapPublishEndpoints(this RouteGroupBuilder api)
    {
        // The publish gate (FR-003), freeze (FR-004), and sequential number
        // (FR-005, ADR-0001) live inside Runbook.Publish() (ADR-0003); the
        // unique (RunbookId, Number) index guards races.
        api.MapPost("/{id:guid}/publish", async (Guid id, AppDbContext db) =>
        {
            var runbook = await RunbookEndpoints.LoadRunbook(db, id);
            if (runbook is null) return RunbookEndpoints.NotFound();

            var version = runbook.Publish();
            // EF treats navigation-discovered entities with client-set Guid
            // keys as existing rows; mark the new version graph as inserts.
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
