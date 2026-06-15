using System.Net.Http.Json;

namespace RunbookPlatform.Api.Tests;

// 004 US1 — publish freezes the Step detail + type into the Runbook Version,
// and a later edit + re-publish never changes an earlier Version (FR-007/008).
public class RichStepFreezeTests : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;

    public RichStepFreezeTests(ApiFixture fixture) => _client = fixture.CreateClient();

    private async Task<Guid> CreateRunbook(string name = "Database failover")
    {
        var response = await _client.PostAsJsonAsync("/api/runbooks", new { name });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RichRunbookDto>())!.Id;
    }

    private Task SaveSteps(Guid id, object steps) =>
        _client.PutAsJsonAsync($"/api/runbooks/{id}/steps", new { steps });

    [Fact] // FR-007: publish freezes all detail + type into the version
    public async Task FR007_publish_freezes_detail_and_type()
    {
        var id = await CreateRunbook();
        await SaveSteps(id, new object[]
        {
            new { text = "Drain", instructions = "do **it**", command = "kubectl drain", expectedResult = "drained", type = "Action" },
            new { text = "Verify", type = "Check" },
        });

        await _client.PostAsync($"/api/runbooks/{id}/publish", null);

        var v1 = await _client.GetFromJsonAsync<RichVersionDto>($"/api/runbooks/{id}/versions/1");
        Assert.Collection(v1!.Steps,
            s =>
            {
                Assert.Equal("Drain", s.Text);
                Assert.Equal("do **it**", s.Instructions);
                Assert.Equal("kubectl drain", s.Command);
                Assert.Equal("drained", s.ExpectedResult);
                Assert.Equal("Action", s.Type);
            },
            s => { Assert.Equal("Verify", s.Text); Assert.Equal("Check", s.Type); });
    }

    [Fact] // FR-008: editing + re-publishing leaves the earlier Version untouched
    public async Task FR008_earlier_version_detail_is_immutable_after_republish()
    {
        var id = await CreateRunbook();
        await SaveSteps(id, new[]
        {
            new { text = "Drain", command = "kubectl drain node-1", type = "Action" },
        });
        await _client.PostAsync($"/api/runbooks/{id}/publish", null);

        // Edit the working step's command, then publish version 2.
        await SaveSteps(id, new[]
        {
            new { text = "Drain", command = "kubectl drain node-1 --force", type = "Action" },
        });
        await _client.PostAsync($"/api/runbooks/{id}/publish", null);

        var v1 = await _client.GetFromJsonAsync<RichVersionDto>($"/api/runbooks/{id}/versions/1");
        var v2 = await _client.GetFromJsonAsync<RichVersionDto>($"/api/runbooks/{id}/versions/2");
        Assert.Equal("kubectl drain node-1", v1!.Steps[0].Command);
        Assert.Equal("kubectl drain node-1 --force", v2!.Steps[0].Command);
    }
}
