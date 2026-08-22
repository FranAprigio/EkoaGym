using System.Net;
using EkoaGym.Mobile.Services.Api;

namespace EkoaGym.Mobile.Tests.Api;

public class ApiClientTests
{
    private sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
    }

    private sealed class ThrowingHttpMessageHandler(Exception exception) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            throw exception;
    }

    private static ApiClient CreateSut(HttpMessageHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://api.test/") });

    private record Widget(string Name);

    [Fact]
    public async Task GetAsync_SuccessWithBody_DeserializesResponse()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"name":"gadget"}""", System.Text.Encoding.UTF8, "application/json")
        });
        var sut = CreateSut(handler);

        var result = await sut.GetAsync<Widget>("widgets/1");

        Assert.Equal("gadget", result!.Name);
    }

    [Fact]
    public async Task GetAsync_SuccessWithEmptyBody_ReturnsDefault()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        var sut = CreateSut(handler);

        var result = await sut.GetAsync<Widget>("widgets/1");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_ErrorWithCodeMessageShape_ThrowsApiExceptionWithParsedMessage()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = new StringContent("""{"code":"email_in_use","message":"That email is already registered."}""",
                System.Text.Encoding.UTF8, "application/json")
        });
        var sut = CreateSut(handler);

        var ex = await Assert.ThrowsAsync<ApiException>(() => sut.GetAsync<Widget>("widgets/1"));

        Assert.Equal("That email is already registered.", ex.Message);
        Assert.Equal("email_in_use", ex.ErrorCode);
        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task GetAsync_ValidationProblemDetailsShape_JoinsAllFieldMessages()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(
                """{"errors":{"Email":["Email is required."],"Password":["Password is too short."]}}""",
                System.Text.Encoding.UTF8, "application/json")
        });
        var sut = CreateSut(handler);

        var ex = await Assert.ThrowsAsync<ApiException>(() => sut.GetAsync<Widget>("widgets/1"));

        Assert.Equal("Email is required. Password is too short.", ex.Message);
        Assert.Equal("validation_error", ex.ErrorCode);
    }

    [Fact]
    public async Task GetAsync_NonJsonErrorBody_FallsBackToFriendlyMessageForStatusCode()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("not json")
        });
        var sut = CreateSut(handler);

        var ex = await Assert.ThrowsAsync<ApiException>(() => sut.GetAsync<Widget>("widgets/1"));

        Assert.Equal("Your session has expired. Please log in again.", ex.Message);
        Assert.Equal(401, ex.StatusCode);
    }

    [Fact]
    public async Task GetAsync_NotFoundWithNoBody_FallsBackToFriendlyMessage()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var sut = CreateSut(handler);

        var ex = await Assert.ThrowsAsync<ApiException>(() => sut.GetAsync<Widget>("widgets/1"));

        Assert.Equal("We couldn't find what you were looking for.", ex.Message);
    }

    [Fact]
    public async Task GetAsync_ServerError_FallsBackToGenericServerMessage()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var sut = CreateSut(handler);

        var ex = await Assert.ThrowsAsync<ApiException>(() => sut.GetAsync<Widget>("widgets/1"));

        Assert.Equal("Something went wrong on our end. Please try again later.", ex.Message);
    }

    [Fact]
    public async Task GetAsync_NetworkFailure_ThrowsApiExceptionWithNetworkErrorCode()
    {
        var handler = new ThrowingHttpMessageHandler(new HttpRequestException("boom"));
        var sut = CreateSut(handler);

        var ex = await Assert.ThrowsAsync<ApiException>(() => sut.GetAsync<Widget>("widgets/1"));

        Assert.Equal("network_error", ex.ErrorCode);
    }

    [Fact]
    public async Task GetAsync_Timeout_ThrowsApiExceptionWithTimeoutCode()
    {
        var handler = new ThrowingHttpMessageHandler(new TaskCanceledException("timed out"));
        var sut = CreateSut(handler);

        var ex = await Assert.ThrowsAsync<ApiException>(() => sut.GetAsync<Widget>("widgets/1"));

        Assert.Equal("timeout", ex.ErrorCode);
    }
}
