using Microsoft.EntityFrameworkCore;
using RunbookPlatform.Api.Data;
using RunbookPlatform.Api.Domain;

namespace RunbookPlatform.Api.Endpoints;

public static class VersionEndpoints
{
    // Shared frozen-Step projection: title + frozen detail + Step Type (004).
    // Reused by the version view and the Execution run view / Computed Review.
    internal static object ToVersionStepDto(RunbookVersionStep s) => new
    {
        s.Position,
        s.Text,
        instructions = s.Instructions,
        command = s.Command,
        expectedResult = s.ExpectedResult,
        type = s.Type.ToString(),
    };

    public static void MapVersionEndpoints(this RouteGroupBuilder api)
    {
        // FR-007: every published Runbook Version stays viewable, by number.
        // Read-only — no mutation routes exist for versions (FR-006).
        api.MapGet("/{id:guid}/versions/{number:int}", async (Guid id, int number, AppDbContext db) =>
        {
            var version = await db.RunbookVersions
                .Include(v => v.Steps.OrderBy(s => s.Position))
                .FirstOrDefaultAsync(v => v.RunbookId == id && v.Number == number);

            return version is null
                ? Results.NotFound(new { error = "No such Runbook Version." })
                : Results.Ok(new
                {
                    number = version.Number,
                    nameAtPublish = version.NameAtPublish,
                    publishedAt = version.PublishedAt,
                    steps = version.Steps.OrderBy(s => s.Position).Select(ToVersionStepDto),
                });
        });
    }
}
