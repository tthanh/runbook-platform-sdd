using System.Net;
using System.Net.Http.Json;

namespace RunbookPlatform.Api.Tests;

// US1 — create a Runbook and publish its first Runbook Version.
public class CreateAndPublishTests : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;

    public CreateAndPublishTests(ApiFixture fixture) => _client = fixture.CreateClient();

    private async Task<RunbookDto> CreateRunbook(string name = "Database failover")
    {
        var response = await _client.PostAsJsonAsync("/api/runbooks", new { name });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RunbookDto>())!;
    }

    [Fact] // FR-001 (US1-1)
    public async Task FR001_create_runbook_with_name_stores_it_with_no_published_versions()
    {
        var response = await _client.PostAsJsonAsync("/api/runbooks", new { name = "Database failover" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var runbook = await response.Content.ReadFromJsonAsync<RunbookDto>();
        Assert.NotNull(runbook);
        Assert.Equal("Database failover", runbook.Name);
        Assert.Null(runbook.CurrentVersionNumber);
        Assert.Empty(runbook.Steps);
    }

    [Fact] // FR-001 (US1-5)
    public async Task FR001_create_with_empty_name_is_refused()
    {
        var response = await _client.PostAsJsonAsync("/api/runbooks", new { name = "  " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ErrorDto>();
        Assert.Contains("name", body!.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact] // FR-002 (US1-2)
    public async Task FR002_saving_steps_appends_in_order_with_positions()
    {
        var runbook = await CreateRunbook();

        var response = await _client.PutAsJsonAsync($"/api/runbooks/{runbook.Id}/steps", new
        {
            steps = new[] { new { text = "Page the on-call DBA" }, new { text = "Verify replica lag" } },
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var detail = await _client.GetFromJsonAsync<RunbookDto>($"/api/runbooks/{runbook.Id}");
        Assert.Collection(detail!.Steps,
            s => { Assert.Equal(1, s.Position); Assert.Equal("Page the on-call DBA", s.Text); },
            s => { Assert.Equal(2, s.Position); Assert.Equal("Verify replica lag", s.Text); });
    }

    [Fact] // FR-002 edge case: blank Step refused
    public async Task FR002_blank_step_text_is_refused()
    {
        var runbook = await CreateRunbook();

        var response = await _client.PutAsJsonAsync($"/api/runbooks/{runbook.Id}/steps", new
        {
            steps = new[] { new { text = "" } },
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact] // FR-003, FR-004, FR-005 (US1-3)
    public async Task FR003_FR004_FR005_publish_creates_version_1_with_current_steps()
    {
        var runbook = await CreateRunbook();
        await _client.PutAsJsonAsync($"/api/runbooks/{runbook.Id}/steps", new
        {
            steps = new[] { new { text = "Page the on-call DBA" } },
        });

        var response = await _client.PostAsync($"/api/runbooks/{runbook.Id}/publish", null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var version = await response.Content.ReadFromJsonAsync<VersionDto>();
        Assert.Equal(1, version!.Number);
        Assert.Equal("Database failover", version.NameAtPublish);
        Assert.Single(version.Steps);
        Assert.Equal("Page the on-call DBA", version.Steps[0].Text);
    }

    [Fact] // FR-003 (US1-4)
    public async Task FR003_publish_without_steps_is_refused()
    {
        var runbook = await CreateRunbook();

        var response = await _client.PostAsync($"/api/runbooks/{runbook.Id}/publish", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ErrorDto>();
        Assert.Contains("Step", body!.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact] // contract: unknown Runbook → 404
    public async Task unknown_runbook_returns_not_found()
    {
        var response = await _client.GetAsync($"/api/runbooks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
