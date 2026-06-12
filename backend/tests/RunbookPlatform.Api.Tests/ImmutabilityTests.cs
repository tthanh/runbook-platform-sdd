using System.Net;
using System.Net.Http.Json;

namespace RunbookPlatform.Api.Tests;

// US2 — edit and republish; earlier Runbook Versions stay intact.
public class ImmutabilityTests : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;

    public ImmutabilityTests(ApiFixture fixture) => _client = fixture.CreateClient();

    private async Task<Guid> CreatePublished(string name, params string[] steps)
    {
        var create = await _client.PostAsJsonAsync("/api/runbooks", new { name });
        var runbook = (await create.Content.ReadFromJsonAsync<RunbookDto>())!;
        await SaveSteps(runbook.Id, steps);
        var publish = await _client.PostAsync($"/api/runbooks/{runbook.Id}/publish", null);
        publish.EnsureSuccessStatusCode();
        return runbook.Id;
    }

    private async Task SaveSteps(Guid id, params string[] steps)
    {
        var response = await _client.PutAsJsonAsync($"/api/runbooks/{id}/steps", new
        {
            steps = steps.Select(t => new { text = t }).ToArray(),
        });
        response.EnsureSuccessStatusCode();
    }

    [Fact] // FR-006 (US2-1)
    public async Task FR006_editing_steps_leaves_published_version_unchanged()
    {
        var id = await CreatePublished("Database failover", "Page the on-call DBA");

        await SaveSteps(id, "Completely different step", "And another");

        var v1 = await _client.GetFromJsonAsync<VersionDto>($"/api/runbooks/{id}/versions/1");
        Assert.Single(v1!.Steps);
        Assert.Equal("Page the on-call DBA", v1.Steps[0].Text);
    }

    [Fact] // FR-005, FR-004 (US2-2)
    public async Task FR005_republish_creates_version_N_plus_1()
    {
        var id = await CreatePublished("Database failover", "Page the on-call DBA");
        await SaveSteps(id, "Page the on-call DBA", "Verify replica lag");

        var response = await _client.PostAsync($"/api/runbooks/{id}/publish", null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var v2 = await response.Content.ReadFromJsonAsync<VersionDto>();
        Assert.Equal(2, v2!.Number);
        Assert.Equal(2, v2.Steps.Count);
    }

    [Fact] // FR-005 edge case: identical republish still creates a new number
    public async Task FR005_identical_republish_still_creates_next_number()
    {
        var id = await CreatePublished("Database failover", "Page the on-call DBA");

        var response = await _client.PostAsync($"/api/runbooks/{id}/publish", null);

        var v2 = await response.Content.ReadFromJsonAsync<VersionDto>();
        Assert.Equal(2, v2!.Number);
        var v1 = await _client.GetFromJsonAsync<VersionDto>($"/api/runbooks/{id}/versions/1");
        Assert.Equal(v1!.Steps.Count, v2.Steps.Count);
    }

    [Fact] // FR-008 (US2-4)
    public async Task FR008_latest_published_version_is_current_by_default()
    {
        var id = await CreatePublished("Database failover", "Page the on-call DBA");
        await _client.PostAsync($"/api/runbooks/{id}/publish", null);

        var detail = await _client.GetFromJsonAsync<RunbookDto>($"/api/runbooks/{id}");

        Assert.Equal(2, detail!.CurrentVersionNumber);
        Assert.Equal(2, detail.Versions.Count);
    }

    [Fact] // FR-006 (US2-3): no mutation route exists for published versions
    public async Task FR006_no_mutation_path_exists_for_published_versions()
    {
        var id = await CreatePublished("Database failover", "Page the on-call DBA");

        var put = await _client.PutAsJsonAsync($"/api/runbooks/{id}/versions/1", new { steps = Array.Empty<object>() });
        var patch = await _client.PatchAsJsonAsync($"/api/runbooks/{id}/versions/1", new { });

        Assert.True(put.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed);
        Assert.True(patch.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed);
    }
}
