using System.Net;
using System.Net.Http.Json;

namespace RunbookPlatform.Api.Tests;

// FR-009/010 (US2): close an Execution; refuse recording and double-close after close.
public class ExecutionCloseTests : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;

    public ExecutionCloseTests(ApiFixture fixture) => _client = fixture.CreateClient();

    private async Task<ExecutionDto> StartExecution(string incident = "INC-300")
    {
        var rb = await _client.PostAsJsonAsync("/api/runbooks", new { name = "Playbook" });
        var runbook = (await rb.Content.ReadFromJsonAsync<RunbookDto>())!;
        await _client.PutAsJsonAsync($"/api/runbooks/{runbook.Id}/steps",
            new { steps = new[] { new { text = "Step 1" } } });
        await _client.PostAsync($"/api/runbooks/{runbook.Id}/publish", null);
        var exec = await _client.PostAsJsonAsync("/api/executions",
            new { incidentId = incident, runbookId = runbook.Id });
        return (await exec.Content.ReadFromJsonAsync<ExecutionDto>())!;
    }

    [Fact] // FR-009: manual close succeeds.
    public async Task FR009_close_execution_sets_status_closed()
    {
        var execution = await StartExecution("INC-301");

        var response = await _client.PostAsync($"/api/executions/{execution.Id}/close", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var closed = await response.Content.ReadFromJsonAsync<ExecutionDto>();
        Assert.Equal("Closed", closed!.Status);
    }

    [Fact] // FR-010: recording on a closed Execution → 409.
    public async Task FR010_recording_after_close_refuses_409()
    {
        var execution = await StartExecution("INC-302");
        await _client.PostAsync($"/api/executions/{execution.Id}/close", null);

        var response = await _client.PostAsJsonAsync($"/api/executions/{execution.Id}/records",
            new { stepPosition = 1, outcome = "Done" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact] // double-close → 409.
    public async Task double_close_is_refused_with_409()
    {
        var execution = await StartExecution("INC-303");
        await _client.PostAsync($"/api/executions/{execution.Id}/close", null);

        var response = await _client.PostAsync($"/api/executions/{execution.Id}/close", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
