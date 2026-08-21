using System.Net;
using System.Net.Http.Json;

namespace EkoaGym.Api.Tests;

public class ExercisesEndpointsTests(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetAll_NoAuthRequired_ReturnsSeededExercises()
    {
        var response = await _client.GetAsync("/api/exercises?page=1&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PagedResultDto>();
        Assert.Equal(42, body!.TotalCount);
        Assert.Equal(5, body.Items.Count);
    }

    [Fact]
    public async Task GetById_UnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/exercises/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_KnownId_ReturnsDetailWithStringEnums()
    {
        var list = await _client.GetFromJsonAsync<PagedResultDto>("/api/exercises?page=1&pageSize=1");
        var id = list!.Items[0].Id;

        var response = await _client.GetAsync($"/api/exercises/{id}");
        var body = await response.Content.ReadFromJsonAsync<ExerciseDetailDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(body!.MuscleGroup));
    }

    private record PagedResultDto(List<ExerciseSummaryDto> Items, int TotalCount);
    private record ExerciseSummaryDto(Guid Id, string Name);
    private record ExerciseDetailDto(Guid Id, string Name, string MuscleGroup);
}
