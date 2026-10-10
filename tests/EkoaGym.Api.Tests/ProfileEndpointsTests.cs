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

    private record ProfileDto(string Name, string Sex, string ActivityLevel, string PreferredLanguage);

    private static object ProfileBody(string? preferredLanguage) => new
    {
        name = "Lang User",
        dateOfBirth = "1995-05-01",
        sex = "Female",
        heightCm = 170,
        currentWeightKg = 60,
        activityLevel = "LightlyActive",
        preferredLanguage
    };

    [Fact]
    public async Task Put_WithPreferredLanguage_IsPersistedAndReturned()
    {
        TestAuthHelper.AuthorizeWith(_client, await TestAuthHelper.RegisterAndGetAccessTokenAsync(_client));

        var put = await _client.PutAsJsonAsync("/api/profile", ProfileBody("pt-BR"));
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);

        var get = await _client.GetFromJsonAsync<ProfileDto>("/api/profile");
        Assert.Equal("pt-BR", get!.PreferredLanguage);
    }

    [Fact]
    public async Task Put_WithoutPreferredLanguage_KeepsExistingValue()
    {
        TestAuthHelper.AuthorizeWith(_client, await TestAuthHelper.RegisterAndGetAccessTokenAsync(_client));
        await _client.PutAsJsonAsync("/api/profile", ProfileBody("pt-BR"));

        await _client.PutAsJsonAsync("/api/profile", ProfileBody(null));

        var get = await _client.GetFromJsonAsync<ProfileDto>("/api/profile");
        Assert.Equal("pt-BR", get!.PreferredLanguage);
    }

    [Fact]
    public async Task Put_WithUnsupportedLanguage_ReturnsBadRequest()
    {
        TestAuthHelper.AuthorizeWith(_client, await TestAuthHelper.RegisterAndGetAccessTokenAsync(_client));

        var put = await _client.PutAsJsonAsync("/api/profile", ProfileBody("fr"));

        Assert.Equal(HttpStatusCode.BadRequest, put.StatusCode);
    }

    [Fact]
    public async Task Register_WithPreferredLanguage_IsStoredBeforeAnyProfileExists()
    {
        var email = $"lang-{Guid.NewGuid():N}@example.com";
        var register = await _client.PostAsJsonAsync("/api/auth/register",
            new { email, password = "Password1", name = "Lang", preferredLanguage = "pt-BR" });
        register.EnsureSuccessStatusCode();

        TestAuthHelper.AuthorizeWith(_client, (await register.Content.ReadFromJsonAsync<TokenDto>())!.AccessToken);
        await _client.PutAsJsonAsync("/api/profile", ProfileBody(null));

        var get = await _client.GetFromJsonAsync<ProfileDto>("/api/profile");
        Assert.Equal("pt-BR", get!.PreferredLanguage);
    }

    private record TokenDto(string AccessToken);
}
