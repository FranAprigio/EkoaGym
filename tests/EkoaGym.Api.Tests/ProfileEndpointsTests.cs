using System.Net;
using System.Net.Http.Json;

namespace EkoaGym.Api.Tests;

// Regression coverage for a bug found during manual testing: enums in JSON request/response bodies
// (e.g. Sex, ActivityLevel) failed to deserialize until JsonStringEnumConverter was registered globally.
public class ProfileEndpointsTests(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Put_ValidProfileWithStringEnums_RoundTripsAsStrings()
    {
        var token = await TestAuthHelper.RegisterAndGetAccessTokenAsync(_client);
        TestAuthHelper.AuthorizeWith(_client, token);

        var request = new
        {
            name = "Test User",
            dateOfBirth = "1995-05-01",
            sex = "Male",
            heightCm = 180,
            currentWeightKg = 80.5,
            activityLevel = "ModeratelyActive"
        };

        var putResponse = await _client.PutAsJsonAsync("/api/profile", request);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        var putBody = await putResponse.Content.ReadFromJsonAsync<ProfileDto>();
        Assert.Equal("Male", putBody!.Sex);
        Assert.Equal("ModeratelyActive", putBody.ActivityLevel);

        var getResponse = await _client.GetAsync("/api/profile");
        var getBody = await getResponse.Content.ReadFromJsonAsync<ProfileDto>();
        Assert.Equal("Male", getBody!.Sex);
        Assert.Equal("ModeratelyActive", getBody.ActivityLevel);
    }

    [Fact]
    public async Task Get_BeforeProfileCreated_ReturnsNotFound()
    {
        var token = await TestAuthHelper.RegisterAndGetAccessTokenAsync(_client);
        TestAuthHelper.AuthorizeWith(_client, token);

        var response = await _client.GetAsync("/api/profile");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private record ProfileDto(string Name, string Sex, string ActivityLevel);
}
