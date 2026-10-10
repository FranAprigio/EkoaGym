using System.Net;
using System.Text;
using EkoaGym.Mobile.Services.Api;
using EkoaGym.Mobile.Services.Localization;

namespace EkoaGym.Mobile.Tests.Localization;

public class ProfileLanguageSyncTests
{
    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public List<(HttpMethod Method, string Body)> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add((request.Method, body));
            return respond(request);
        }
    }

    private const string ProfileJson =
        """{"name":"Ana","dateOfBirth":"1990-05-01","sex":"Female","heightCm":170,"currentWeightKg":62.5,"activityLevel":"LightlyActive","preferredLanguage":"en"}""";

    private static HttpResponseMessage Json(HttpStatusCode status, string json) =>
        new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private static ApiClient Client(RecordingHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://api.test/") });

    [Fact]
    public async Task Push_ServerLanguageDiffers_PutsFullProfileWithNewLanguage()
    {
        var handler = new RecordingHandler(request => request.Method == HttpMethod.Get
            ? Json(HttpStatusCode.OK, ProfileJson)
            : new HttpResponseMessage(HttpStatusCode.NoContent));

        await ProfileLanguageSync.PushAsync(Client(handler), "pt-BR");

        var put = Assert.Single(handler.Requests, r => r.Method == HttpMethod.Put);
        Assert.Contains("\"preferredLanguage\":\"pt-BR\"", put.Body);
        Assert.Contains("\"name\":\"Ana\"", put.Body);
        Assert.Contains("62.5", put.Body);
    }

    [Fact]
    public async Task Push_ServerLanguageAlreadyMatches_DoesNotPut()
    {
        var handler = new RecordingHandler(_ => Json(HttpStatusCode.OK, ProfileJson));

        await ProfileLanguageSync.PushAsync(Client(handler), "en");

        Assert.DoesNotContain(handler.Requests, r => r.Method == HttpMethod.Put);
    }

    [Fact]
    public async Task Push_NoProfileYet_DoesNothingAndDoesNotThrow()
    {
        var handler = new RecordingHandler(_ => Json(HttpStatusCode.NotFound, """{"code":"profile_not_found","message":"Profile not found for this user."}"""));

        await ProfileLanguageSync.PushAsync(Client(handler), "pt-BR");

        Assert.DoesNotContain(handler.Requests, r => r.Method == HttpMethod.Put);
    }

    [Fact]
    public async Task Push_PutFails_DoesNotThrow()
    {
        var handler = new RecordingHandler(request => request.Method == HttpMethod.Get
            ? Json(HttpStatusCode.OK, ProfileJson)
            : new HttpResponseMessage(HttpStatusCode.InternalServerError));

        await ProfileLanguageSync.PushAsync(Client(handler), "pt-BR");

        Assert.Single(handler.Requests, r => r.Method == HttpMethod.Put);
    }
}
