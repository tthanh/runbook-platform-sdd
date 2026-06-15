using System.Net;
using System.Net.Http.Json;

namespace RunbookPlatform.Api.Tests;

// FR-004/005/006/007/008 (US1): record step outcomes — out of order, notes, append-only, re-marking.
public class StepRecordingTests : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;

    public StepRecordingTests(ApiFixture fixture) => _client = fixture.CreateClient();

    private async Task<(Guid RunbookId, ExecutionDto Execution)> StartExecution(string incident = "INC-100")
    {
        var rb = await _client.PostAsJsonAsync("/api/runbooks", new { name = "Playbook" });
        var runbook = (await rb.Content.ReadFromJsonAsync<RunbookDto>())!;
        await _client.PutAsJsonAsync($"/api/runbooks/{runbook.Id}/steps",
            new { steps = new[] { new { text = "Step A" }, new { text = "Step B" }, new { text = "Step C" } } });
        await _client.PostAsync($"/api/runbooks/{runbook.Id}/publish", null);

        var exec = await _client.PostAsJsonAsync("/api/executions",
            new { incidentId = incident, runbookId = runbook.Id });
        var execution = (await exec.Content.ReadFromJsonAsync<ExecutionDto>())!;
        return (runbook.Id, execution);
    }

    [Fact] // FR-007: Steps may be recorded in any order.
    public async Task FR007_record_steps_out_of_order()
    {
        var (_, execution) = await StartExecution("INC-101");

        await _client.PostAsJsonAsync($"/api/executions/{execution.Id}/records",
            new { stepPosition = 3, outcome = "Done" });
        await _client.PostAsJsonAsync($"/api/executions/{execution.Id}/records",
            new { stepPosition = 1, outcome = "Failed" });

        var view = await _client.GetFromJsonAsync<ExecutionDto>($"/api/executions/{execution.Id}");
        Assert.Equal(2, view!.Records.Count);
    }

    [Fact] // FR-008: optional note captured; no actor captured.
    public async Task FR008_optional_note_captured()
    {
        var (_, execution) = await StartExecution("INC-102");

        var response = await _client.PostAsJsonAsync($"/api/executions/{execution.Id}/records",
            new { stepPosition = 1, outcome = "Done", note = "paged at 02:14" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var record = await response.Content.ReadFromJsonAsync<StepRecordDto>();
        Assert.Equal("paged at 02:14", record!.Note);
    }

    [Fact] // FR-005: records are append-only (each call appends a new record).
    public async Task FR005_records_are_append_only()
    {
        var (_, execution) = await StartExecution("INC-103");

        await _client.PostAsJsonAsync($"/api/executions/{execution.Id}/records",
            new { stepPosition = 1, outcome = "Failed" });
        await _client.PostAsJsonAsync($"/api/executions/{execution.Id}/records",
            new { stepPosition = 1, outcome = "Done" });

        var view = await _client.GetFromJsonAsync<ExecutionDto>($"/api/executions/{execution.Id}");
        Assert.Equal(2, view!.Records.Count);
    }

    [Fact] // FR-006: re-marking appends; most recent = end state.
    public async Task FR006_remark_appends_and_end_state_is_latest()
    {
        var (_, execution) = await StartExecution("INC-104");

        await _client.PostAsJsonAsync($"/api/executions/{execution.Id}/records",
            new { stepPosition = 2, outcome = "Failed" });
        await _client.PostAsJsonAsync($"/api/executions/{execution.Id}/records",
            new { stepPosition = 2, outcome = "Done" });

        var view = await _client.GetFromJsonAsync<ExecutionDto>($"/api/executions/{execution.Id}");
        var posTwo = view!.Records.Where(r => r.StepPosition == 2).ToList();
        Assert.Equal(2, posTwo.Count); // both records present
        Assert.Equal("Done", posTwo.Last().Outcome); // most recent = Done
    }

    [Fact] // FR-004/FR-008: invalid outcome → 400.
    public async Task invalid_outcome_returns_bad_request()
    {
        var (_, execution) = await StartExecution("INC-105");

        var response = await _client.PostAsJsonAsync($"/api/executions/{execution.Id}/records",
            new { stepPosition = 1, outcome = "Completed" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact] // bad step position → 400.
    public async Task invalid_step_position_returns_bad_request()
    {
        var (_, execution) = await StartExecution("INC-106");

        var response = await _client.PostAsJsonAsync($"/api/executions/{execution.Id}/records",
            new { stepPosition = 99, outcome = "Done" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
