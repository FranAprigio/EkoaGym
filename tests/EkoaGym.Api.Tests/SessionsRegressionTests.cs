using System.Net.Http.Json;

namespace EkoaGym.Api.Tests;

// Regression coverage for a bug found during manual testing: DateTime values reloaded from the database
// lost their Utc Kind, so a value returned fresh vs. one re-read from storage serialized inconsistently
// (missing trailing "Z"). This is exactly the idempotent-complete scenario below.
public class SessionsRegressionTests(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CompleteSession_CalledTwice_ReturnsIdenticallyFormattedEndedAt()
    {
        var token = await TestAuthHelper.RegisterAndGetAccessTokenAsync(_client);
        TestAuthHelper.AuthorizeWith(_client, token);

        var exercises = await _client.GetFromJsonAsync<PagedResultDto>("/api/exercises?page=1&pageSize=1");
        var exerciseId = exercises!.Items[0].Id;

        var planRequest = new
        {
            name = "Test Plan",
            exercises = new[] { new { exerciseId, sets = 3, reps = 10, weightKg = 20, restSeconds = 90 } }
        };
        var planResponse = await _client.PostAsJsonAsync("/api/plans", planRequest);
        planResponse.EnsureSuccessStatusCode();
        var plan = await planResponse.Content.ReadFromJsonAsync<PlanDto>();

        var sessionResponse = await _client.PostAsJsonAsync("/api/sessions", new { planId = plan!.Id });
        sessionResponse.EnsureSuccessStatusCode();
        var session = await sessionResponse.Content.ReadFromJsonAsync<SessionDto>();

        var firstComplete = await _client.PutAsync($"/api/sessions/{session!.SessionId}/complete", null);
        var firstBody = await firstComplete.Content.ReadFromJsonAsync<CompleteDto>();

        var secondComplete = await _client.PutAsync($"/api/sessions/{session.SessionId}/complete", null);
        var secondBody = await secondComplete.Content.ReadFromJsonAsync<CompleteDto>();

        Assert.Equal(firstBody!.EndedAt, secondBody!.EndedAt);
        Assert.EndsWith("Z", firstBody.EndedAt, StringComparison.Ordinal);
        Assert.EndsWith("Z", secondBody.EndedAt, StringComparison.Ordinal);
    }

    private record PagedResultDto(List<ExerciseSummaryDto> Items);
    private record ExerciseSummaryDto(Guid Id);
    private record PlanDto(Guid Id);
    private record SessionDto(Guid SessionId);
    private record CompleteDto(Guid Id, string StartedAt, string EndedAt);
}
