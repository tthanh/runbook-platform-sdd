using System.Net.Http.Json;

namespace RunbookPlatform.Api.Tests;

// 004 US2 (FR-009) — the run view carries each pinned Step's detail + type, so a
// responder reads how to act before recording an outcome.
public class RichStepRunViewTests : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;

    public RichStepRunViewTests(ApiFixture fixture) => _client = fixture.CreateClient();

    [Fact] // FR-009
    public async Task FR009_run_view_includes_step_detail()
    {
        var rb = await _client.PostAsJsonAsync("/api/runbooks", new { name = "Runbook" });
        var runbookId = (await rb.Content.ReadFromJsonAsync<RichRunbookDto>())!.Id;
        await _client.PutAsJsonAsync($"/api/runbooks/{runbookId}/steps", new
        {
            steps = new object[]
            {
                new { text = "Drain", instructions = "do **it**", command = "kubectl drain", expectedResult = "drained", type = "Action" },
                new { text = "Verify", type = "Check" },
            },
        });
        await _client.PostAsync($"/api/runbooks/{runbookId}/publish", null);

        var start = await _client.PostAsJsonAsync("/api/executions",
            new { incidentId = "INC-RICH-1", incidentTitle = "Rich", runbookId });
        var execution = (await start.Content.ReadFromJsonAsync<RichExecutionDto>())!;

        Assert.Collection(execution.Steps,
            s =>
            {
                Assert.Equal("Drain", s.Text);
                Assert.Equal("do **it**", s.Instructions);
                Assert.Equal("kubectl drain", s.Command);
                Assert.Equal("drained", s.ExpectedResult);
                Assert.Equal("Action", s.Type);
            },
            s => { Assert.Equal("Verify", s.Text); Assert.Equal("Check", s.Type); });
    }
}
