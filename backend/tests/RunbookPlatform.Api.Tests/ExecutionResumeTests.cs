using System.Net;
using System.Net.Http.Json;

namespace RunbookPlatform.Api.Tests;

// FR-015 (US1): one Execution per incident — resume if open, refuse if closed.
public class ExecutionResumeTests : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;

    public ExecutionResumeTests(ApiFixture fixture) => _client = fixture.CreateClient();

    private async Task<(Guid RunbookId, ExecutionDto Execution)> StartExecution(string incident)
    {
        var rb = await _client.PostAsJsonAsync("/api/runbooks", new { name = "Runbook" });
        var runbook = (await rb.Content.ReadFromJsonAsync<RunbookDto>())!;
        await _client.PutAsJsonAsync($"/api/runbooks/{runbook.Id}/steps",
            new { steps = new[] { new { text = "Step 1" } } });
        await _client.PostAsync($"/api/runbooks/{runbook.Id}/publish", null);

        var exec = await _client.PostAsJsonAsync("/api/executions",
            new { incidentId = incident, runbookId = runbook.Id });
        var execution = (await exec.Content.ReadFromJsonAsync<ExecutionDto>())!;
        return (runbook.Id, execution);
    }

    [Fact] // FR-015: second start with same incidentId + open execution → 200 (resume).
    public async Task FR015_second_start_open_execution_resumes_it()
    {
        var (runbookId, first) = await StartExecution("INC-200");

        var response = await _client.PostAsJsonAsync("/api/executions", new
        {
            incidentId = "INC-200",
            runbookId,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var resumed = await response.Content.ReadFromJsonAsync<ExecutionDto>();
        Assert.Equal(first.Id, resumed!.Id); // same Execution, not a new one
    }

    [Fact] // FR-015 (clarification 2026-06-13): closed execution for same incident → 409.
    public async Task FR015_second_start_closed_execution_refuses_409()
    {
        var (runbookId, execution) = await StartExecution("INC-201");
        await _client.PostAsync($"/api/executions/{execution.Id}/close", null);

        var response = await _client.PostAsJsonAsync("/api/executions", new
        {
            incidentId = "INC-201",
            runbookId,
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
