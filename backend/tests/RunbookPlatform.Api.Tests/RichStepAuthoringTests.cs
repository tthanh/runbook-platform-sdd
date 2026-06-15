using System.Net;
using System.Net.Http.Json;

namespace RunbookPlatform.Api.Tests;

// 004 US1 — authoring rich Step detail + Step Type on the working Runbook.
public class RichStepAuthoringTests : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;

    public RichStepAuthoringTests(ApiFixture fixture) => _client = fixture.CreateClient();

    private async Task<Guid> CreateRunbook(string name = "Database failover")
    {
        var response = await _client.PostAsJsonAsync("/api/runbooks", new { name });
        response.EnsureSuccessStatusCode();
        var rb = await response.Content.ReadFromJsonAsync<RichRunbookDto>();
        return rb!.Id;
    }

    [Fact] // FR-001: detail + type round-trips on save/reload
    public async Task FR001_step_detail_and_type_round_trip()
    {
        var id = await CreateRunbook();

        var save = await _client.PutAsJsonAsync($"/api/runbooks/{id}/steps", new
        {
            steps = new object[]
            {
                new
                {
                    text = "Drain the node",
                    instructions = "Run the drain, then **wait** for pods.",
                    command = "kubectl drain node-1",
                    expectedResult = "node-1 shows SchedulingDisabled",
                    type = "Action",
                },
                new { text = "Confirm traffic drained", type = "Check" },
            },
        });
        Assert.Equal(HttpStatusCode.OK, save.StatusCode);

        var detail = await _client.GetFromJsonAsync<RichRunbookDto>($"/api/runbooks/{id}");
        Assert.Collection(detail!.Steps,
            s =>
            {
                Assert.Equal("Drain the node", s.Text);
                Assert.Equal("Run the drain, then **wait** for pods.", s.Instructions);
                Assert.Equal("kubectl drain node-1", s.Command);
                Assert.Equal("node-1 shows SchedulingDisabled", s.ExpectedResult);
                Assert.Equal("Action", s.Type);
            },
            s =>
            {
                Assert.Equal("Confirm traffic drained", s.Text);
                Assert.Null(s.Instructions);
                Assert.Null(s.Command);
                Assert.Null(s.ExpectedResult);
                Assert.Equal("Check", s.Type);
            });
    }

    [Fact] // FR-003: a title-only Step is valid; detail null, type defaults to Action
    public async Task FR003_title_only_step_is_valid_with_defaults()
    {
        var id = await CreateRunbook();

        var save = await _client.PutAsJsonAsync($"/api/runbooks/{id}/steps", new
        {
            steps = new[] { new { text = "Title only, no detail" } },
        });
        Assert.Equal(HttpStatusCode.OK, save.StatusCode);

        var detail = await _client.GetFromJsonAsync<RichRunbookDto>($"/api/runbooks/{id}");
        var step = Assert.Single(detail!.Steps);
        Assert.Equal("Title only, no detail", step.Text);
        Assert.Null(step.Instructions);
        Assert.Null(step.Command);
        Assert.Null(step.ExpectedResult);
        Assert.Equal("Action", step.Type);
    }

    [Fact] // FR-002: a Step Type outside {Action, Check} is rejected
    public async Task FR002_invalid_step_type_is_refused()
    {
        var id = await CreateRunbook();

        var response = await _client.PutAsJsonAsync($"/api/runbooks/{id}/steps", new
        {
            steps = new[] { new { text = "Branch somewhere", type = "Decision" } },
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ErrorDto>();
        Assert.Contains("type", body!.Error, StringComparison.OrdinalIgnoreCase);
    }
}
