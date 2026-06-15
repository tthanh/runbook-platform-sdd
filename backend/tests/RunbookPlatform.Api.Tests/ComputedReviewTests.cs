using System.Net;
using System.Net.Http.Json;

namespace RunbookPlatform.Api.Tests;

// FR-011/012/013 (US2): Computed Review — chronological timeline, NotReached, closed-only.
public class ComputedReviewTests : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;

    public ComputedReviewTests(ApiFixture fixture) => _client = fixture.CreateClient();

    private async Task<ExecutionDto> StartAndRecord(string incident, params (int pos, string outcome)[] records)
    {
        var rb = await _client.PostAsJsonAsync("/api/runbooks", new { name = "Runbook" });
        var runbook = (await rb.Content.ReadFromJsonAsync<RunbookDto>())!;
        await _client.PutAsJsonAsync($"/api/runbooks/{runbook.Id}/steps", new
        {
            steps = new[]
            {
                new { text = "Step A" },
                new { text = "Step B" },
                new { text = "Step C" },
            },
        });
        await _client.PostAsync($"/api/runbooks/{runbook.Id}/publish", null);

        var exec = await _client.PostAsJsonAsync("/api/executions",
            new { incidentId = incident, incidentTitle = "Test incident", runbookId = runbook.Id });
        var execution = (await exec.Content.ReadFromJsonAsync<ExecutionDto>())!;

        foreach (var (pos, outcome) in records)
        {
            await _client.PostAsJsonAsync($"/api/executions/{execution.Id}/records",
                new { stepPosition = pos, outcome });
        }

        return execution;
    }

    [Fact] // FR-011: timeline is chronological; a multiply-marked Step appears once per mark.
    public async Task FR011_timeline_is_chronological_and_shows_all_marks()
    {
        var execution = await StartAndRecord("INC-400",
            (1, "Failed"), (2, "Done"), (1, "Done")); // step 1 re-marked

        await _client.PostAsync($"/api/executions/{execution.Id}/close", null);
        var review = await _client.GetFromJsonAsync<ReviewDto>($"/api/executions/{execution.Id}/review");

        Assert.NotNull(review);
        Assert.Equal(3, review.Timeline.Count); // all 3 marks
        Assert.Equal(1, review.Timeline[0].StepPosition);   // first: step 1 Failed
        Assert.Equal("Failed", review.Timeline[0].Outcome);
        Assert.Equal(2, review.Timeline[1].StepPosition);   // second: step 2 Done
        Assert.Equal(1, review.Timeline[2].StepPosition);   // third: step 1 Done (re-mark)
        Assert.Equal("Done", review.Timeline[2].Outcome);
    }

    [Fact] // FR-012: untouched Step appears in coverage as NotReached (not Skipped).
    public async Task FR012_untouched_step_is_not_reached_not_skipped()
    {
        var execution = await StartAndRecord("INC-401", (1, "Done")); // step 2 and 3 untouched
        await _client.PostAsync($"/api/executions/{execution.Id}/close", null);
        var review = await _client.GetFromJsonAsync<ReviewDto>($"/api/executions/{execution.Id}/review");

        var step2 = review!.Coverage.Single(c => c.StepPosition == 2);
        var step3 = review.Coverage.Single(c => c.StepPosition == 3);
        Assert.Equal("NotReached", step2.EndState);
        Assert.Equal("NotReached", step3.EndState);

        // End-state for step 1 is Done (not NotReached).
        var step1 = review.Coverage.Single(c => c.StepPosition == 1);
        Assert.Equal("Done", step1.EndState);
    }

    [Fact] // FR-013: review only available when closed; 409 if open.
    public async Task FR013_review_unavailable_on_open_execution()
    {
        var execution = await StartAndRecord("INC-402", (1, "Done"));
        // NOT closed.

        var response = await _client.GetAsync($"/api/executions/{execution.Id}/review");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact] // review header uses incidentTitle when present, else incidentId (ADR-0005).
    public async Task review_header_uses_title_when_present()
    {
        var execution = await StartAndRecord("INC-403");
        await _client.PostAsync($"/api/executions/{execution.Id}/close", null);
        var review = await _client.GetFromJsonAsync<ReviewDto>($"/api/executions/{execution.Id}/review");

        Assert.Equal("Test incident", review!.Incident);
    }
}
