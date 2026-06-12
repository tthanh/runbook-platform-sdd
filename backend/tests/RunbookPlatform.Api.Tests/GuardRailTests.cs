using System.Net;
using System.Net.Http.Json;

namespace RunbookPlatform.Api.Tests;

// Cross-cutting fences: FR-010 (no auth) and FR-011 (nothing deletable).
public class GuardRailTests : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;

    public GuardRailTests(ApiFixture fixture) => _client = fixture.CreateClient();

    [Fact] // FR-010: no request requires authentication
    public async Task FR010_no_auth_required_for_any_authoring_action()
    {
        // No Authorization header anywhere in these tests; a full authoring
        // round-trip succeeding proves no sign-in gate exists.
        var create = await _client.PostAsJsonAsync("/api/runbooks", new { name = "Open access" });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var runbook = await create.Content.ReadFromJsonAsync<RunbookDto>();
        var steps = await _client.PutAsJsonAsync($"/api/runbooks/{runbook!.Id}/steps", new
        {
            steps = new[] { new { text = "Anyone can author" } },
        });
        Assert.Equal(HttpStatusCode.OK, steps.StatusCode);

        var publish = await _client.PostAsync($"/api/runbooks/{runbook.Id}/publish", null);
        Assert.Equal(HttpStatusCode.Created, publish.StatusCode);
    }

    [Fact] // FR-011: no DELETE route responds for any resource
    public async Task FR011_delete_routes_do_not_exist()
    {
        var create = await _client.PostAsJsonAsync("/api/runbooks", new { name = "Permanent" });
        var runbook = await create.Content.ReadFromJsonAsync<RunbookDto>();
        await _client.PutAsJsonAsync($"/api/runbooks/{runbook!.Id}/steps", new
        {
            steps = new[] { new { text = "Forever" } },
        });
        await _client.PostAsync($"/api/runbooks/{runbook.Id}/publish", null);

        var deleteRunbook = await _client.DeleteAsync($"/api/runbooks/{runbook.Id}");
        var deleteVersion = await _client.DeleteAsync($"/api/runbooks/{runbook.Id}/versions/1");

        Assert.True(deleteRunbook.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed);
        Assert.True(deleteVersion.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed);

        // And the data is still there.
        var detail = await _client.GetFromJsonAsync<RunbookDto>($"/api/runbooks/{runbook.Id}");
        Assert.Equal(1, detail!.CurrentVersionNumber);
    }
}
