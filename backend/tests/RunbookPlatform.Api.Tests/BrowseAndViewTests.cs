using System.Net;
using System.Net.Http.Json;

namespace RunbookPlatform.Api.Tests;

// US3 — browse Runbooks and view any published Runbook Version.
public class BrowseAndViewTests : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;

    public BrowseAndViewTests(ApiFixture fixture) => _client = fixture.CreateClient();

    private async Task<Guid> Create(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/runbooks", new { name });
        return (await response.Content.ReadFromJsonAsync<RunbookDto>())!.Id;
    }

    private async Task Publish(Guid id, params string[] steps)
    {
        await _client.PutAsJsonAsync($"/api/runbooks/{id}/steps", new
        {
            steps = steps.Select(t => new { text = t }).ToArray(),
        });
        (await _client.PostAsync($"/api/runbooks/{id}/publish", null)).EnsureSuccessStatusCode();
    }

    [Fact] // FR-009 (US3-1): list shows all, incl. unpublished with null version
    public async Task FR009_list_shows_every_runbook_with_current_version_or_null()
    {
        var published = await Create("Database failover");
        await Publish(published, "Page the on-call DBA");
        await Publish(published, "Page the on-call DBA", "Verify replica lag");
        var unpublished = await Create("Cache flush");

        var list = await _client.GetFromJsonAsync<List<RunbookListItemDto>>("/api/runbooks");

        var p = list!.Single(r => r.Id == published);
        var u = list.Single(r => r.Id == unpublished);
        Assert.Equal(2, p.CurrentVersionNumber);
        Assert.Null(u.CurrentVersionNumber);
    }

    [Fact] // FR-007 (US3-2): earlier version reachable, exact content
    public async Task FR007_view_published_version_by_number_returns_exact_content()
    {
        var id = await Create("Database failover");
        await Publish(id, "Page the on-call DBA");
        await Publish(id, "Different step now");

        var v1 = await _client.GetFromJsonAsync<VersionDto>($"/api/runbooks/{id}/versions/1");

        Assert.Equal(1, v1!.Number);
        Assert.Equal("Database failover", v1.NameAtPublish);
        Assert.Single(v1.Steps);
        Assert.Equal("Page the on-call DBA", v1.Steps[0].Text);
    }

    [Fact] // edge case: unknown version number → 404
    public async Task unknown_version_number_returns_not_found()
    {
        var id = await Create("Database failover");
        await Publish(id, "Page the on-call DBA");

        var response = await _client.GetAsync($"/api/runbooks/{id}/versions/99");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact] // FR-008/US3-3: unpublished runbook signals nothing published
    public async Task US3_unpublished_runbook_shows_editable_steps_and_no_versions()
    {
        var id = await Create("Cache flush");
        await _client.PutAsJsonAsync($"/api/runbooks/{id}/steps", new
        {
            steps = new[] { new { text = "Flush it" } },
        });

        var detail = await _client.GetFromJsonAsync<RunbookDto>($"/api/runbooks/{id}");

        Assert.Null(detail!.CurrentVersionNumber);
        Assert.Empty(detail.Versions);
        Assert.Single(detail.Steps);
    }
}
