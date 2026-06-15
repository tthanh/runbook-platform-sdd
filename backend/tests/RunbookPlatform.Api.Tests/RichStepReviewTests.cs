using System.Net.Http.Json;

namespace RunbookPlatform.Api.Tests;

// 004 US3 (FR-011) — the Computed Review timeline carries each pinned Step's
// detail alongside its recorded outcome.
public class RichStepReviewTests : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;

    public RichStepReviewTests(ApiFixture fixture) => _client = fixture.CreateClient();

    [Fact] // FR-011
    public async Task FR011_review_timeline_includes_step_detail()
    {
        var rb = await _client.PostAsJsonAsync("/api/runbooks", new { name = "Runbook" });
        var runbookId = (await rb.Content.ReadFromJsonAsync<RichRunbookDto>())!.Id;
        await _client.PutAsJsonAsync($"/api/runbooks/{runbookId}/steps", new
        {
            steps = new[]
            {
                new { text = "Drain", instructions = "do **it**", command = "kubectl drain", expectedResult = "drained", type = "Action" },
            },
        });
        await _client.PostAsync($"/api/runbooks/{runbookId}/publish", null);

        var start = await _client.PostAsJsonAsync("/api/executions",
            new { incidentId = "INC-RICH-2", incidentTitle = "Rich", runbookId });
        var executionId = (await start.Content.ReadFromJsonAsync<RichExecutionDto>())!.Id;
        await _client.PostAsJsonAsync($"/api/executions/{executionId}/records",
            new { stepPosition = 1, outcome = "Done" });
        await _client.PostAsync($"/api/executions/{executionId}/close", null);

        var review = await _client.GetFromJsonAsync<RichReviewDto>($"/api/executions/{executionId}/review");

        var entry = Assert.Single(review!.Timeline);
        Assert.Equal("Drain", entry.StepText);
        Assert.Equal("do **it**", entry.Instructions);
        Assert.Equal("kubectl drain", entry.Command);
        Assert.Equal("drained", entry.ExpectedResult);
        Assert.Equal("Action", entry.Type);
        Assert.Equal("Done", entry.Outcome);
    }
}
