using System.Net;
using System.Net.Http.Json;

namespace RunbookPlatform.Api.Tests;

// FR-001/002/015 (US1): start an Execution, pin the current Version, refuse unpublished.
public class ExecutionStartTests : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;

    public ExecutionStartTests(ApiFixture fixture) => _client = fixture.CreateClient();

    private async Task<RunbookDto> CreatePublishedRunbook()
    {
        var rb = await _client.PostAsJsonAsync("/api/runbooks", new { name = "DB Failover" });
        var runbook = (await rb.Content.ReadFromJsonAsync<RunbookDto>())!;
        await _client.PutAsJsonAsync($"/api/runbooks/{runbook.Id}/steps",
            new { steps = new[] { new { text = "Page DBA" }, new { text = "Check replica" } } });
        await _client.PostAsync($"/api/runbooks/{runbook.Id}/publish", null);
        return (await _client.GetFromJsonAsync<RunbookDto>($"/api/runbooks/{runbook.Id}"))!;
    }

    [Fact] // FR-001: start pins the current published Version.
    public async Task FR001_start_execution_pins_current_version()
    {
        var runbook = await CreatePublishedRunbook();

        var response = await _client.PostAsJsonAsync("/api/executions", new
        {
            incidentId = "INC-001",
            incidentTitle = "Database outage",
            runbookId = runbook.Id,
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var exec = await response.Content.ReadFromJsonAsync<ExecutionDto>();
        Assert.NotNull(exec);
        Assert.Equal("INC-001", exec.IncidentId);
        Assert.Equal("Database outage", exec.IncidentTitle);
        Assert.Equal("Open", exec.Status);
        Assert.Equal(runbook.CurrentVersionNumber, exec.PinnedVersionNumber);
        Assert.Equal(2, exec.Steps.Count);
    }

    [Fact] // FR-002: refuse unpublished Runbook.
    public async Task FR002_refuse_unpublished_runbook()
    {
        var rb = await _client.PostAsJsonAsync("/api/runbooks", new { name = "Draft" });
        var runbook = (await rb.Content.ReadFromJsonAsync<RunbookDto>())!;

        var response = await _client.PostAsJsonAsync("/api/executions", new
        {
            incidentId = "INC-002",
            runbookId = runbook.Id,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ErrorDto>();
        Assert.Contains("published", body!.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact] // unknown Runbook → 400.
    public async Task unknown_runbook_returns_bad_request()
    {
        var response = await _client.PostAsJsonAsync("/api/executions", new
        {
            incidentId = "INC-003",
            runbookId = Guid.NewGuid(),
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact] // blank incidentId → 400.
    public async Task blank_incident_id_returns_bad_request()
    {
        var runbook = await CreatePublishedRunbook();

        var response = await _client.PostAsJsonAsync("/api/executions", new
        {
            incidentId = "  ",
            runbookId = runbook.Id,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
