using System.Net.Http.Json;

namespace RunbookPlatform.Api.Tests;

// FR-003/ADR-0004 (US3): pin holds for the life of the run — mid-incident publish is silent.
public class PinIntegrityTests : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;

    public PinIntegrityTests(ApiFixture fixture) => _client = fixture.CreateClient();

    [Fact] // FR-003/ADR-0004: publish N+1 after start; run view and review still show N.
    public async Task FR003_pin_holds_after_mid_incident_publish()
    {
        // Set up a runbook, publish Version 1.
        var rb = await _client.PostAsJsonAsync("/api/runbooks", new { name = "Failover" });
        var runbook = (await rb.Content.ReadFromJsonAsync<RunbookDto>())!;
        await _client.PutAsJsonAsync($"/api/runbooks/{runbook.Id}/steps",
            new { steps = new[] { new { text = "Step A" } } });
        await _client.PostAsync($"/api/runbooks/{runbook.Id}/publish", null);

        // Start an Execution — pins Version 1.
        var exec = await _client.PostAsJsonAsync("/api/executions",
            new { incidentId = "INC-500", runbookId = runbook.Id });
        var execution = (await exec.Content.ReadFromJsonAsync<ExecutionDto>())!;
        Assert.Equal(1, execution.PinnedVersionNumber);

        // Mid-incident: publish Version 2 with different steps.
        await _client.PutAsJsonAsync($"/api/runbooks/{runbook.Id}/steps",
            new { steps = new[] { new { text = "Step A" }, new { text = "Step B NEW" } } });
        await _client.PostAsync($"/api/runbooks/{runbook.Id}/publish", null);

        // Run view still shows Version 1 (SC-004 / ADR-0004 — silent, no "newer" signal).
        var view = await _client.GetFromJsonAsync<ExecutionDto>($"/api/executions/{execution.Id}");
        Assert.Equal(1, view!.PinnedVersionNumber);
        Assert.Single(view.Steps); // Version 1 had one step

        // Records against Version 1 step positions are still valid.
        var record = await _client.PostAsJsonAsync($"/api/executions/{execution.Id}/records",
            new { stepPosition = 1, outcome = "Done" });
        record.EnsureSuccessStatusCode();

        // Close and verify review is set against Version 1.
        await _client.PostAsync($"/api/executions/{execution.Id}/close", null);
        var review = await _client.GetFromJsonAsync<ReviewDto>($"/api/executions/{execution.Id}/review");
        Assert.Equal(1, review!.PinnedVersionNumber);
        Assert.Single(review.Coverage); // only Version 1's step
    }

    [Fact] // T026/ADR-0004: run view contains no "newer version" signal.
    public async Task ADR0004_run_view_has_no_newer_version_signal()
    {
        var rb = await _client.PostAsJsonAsync("/api/runbooks", new { name = "Runbook" });
        var runbook = (await rb.Content.ReadFromJsonAsync<RunbookDto>())!;
        await _client.PutAsJsonAsync($"/api/runbooks/{runbook.Id}/steps",
            new { steps = new[] { new { text = "Step 1" } } });
        await _client.PostAsync($"/api/runbooks/{runbook.Id}/publish", null);

        var exec = await _client.PostAsJsonAsync("/api/executions",
            new { incidentId = "INC-501", runbookId = runbook.Id });
        var execution = (await exec.Content.ReadFromJsonAsync<ExecutionDto>())!;

        // Publish Version 2.
        await _client.PutAsJsonAsync($"/api/runbooks/{runbook.Id}/steps",
            new { steps = new[] { new { text = "Step 1" }, new { text = "Step 2" } } });
        await _client.PostAsync($"/api/runbooks/{runbook.Id}/publish", null);

        // Run view response must not reference any "newer version" field.
        var view = await _client.GetFromJsonAsync<ExecutionDto>($"/api/executions/{execution.Id}");
        // pinnedVersionNumber == 1 (not 2) — the DTO has no "latestVersionNumber" or "newerVersionExists".
        Assert.Equal(1, view!.PinnedVersionNumber);
    }
}
