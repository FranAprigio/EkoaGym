using EkoaGym.Mobile.Services.Api;
using EkoaGym.Mobile.Services.Auth;
using EkoaGym.Mobile.Services.Connectivity;
using EkoaGym.Mobile.Services.Data;
using Microsoft.Extensions.Logging;

namespace EkoaGym.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

		// Auth / token storage
		builder.Services.AddSingleton<ITokenStore, SecureStorageTokenStore>();
		builder.Services.AddSingleton<AuthState>();
		builder.Services.AddTransient<AuthDelegatingHandler>();

		// Connectivity + offline cache
		builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
		builder.Services.AddSingleton<LocalDatabaseService>();
		builder.Services.AddSingleton<ISyncService, SyncService>();

		// Typed API client — carries the bearer token and retries once on 401 via AuthDelegatingHandler.
		builder.Services
			.AddHttpClient<ApiClient>(ApiClientRegistration.ApiClientName, client =>
			{
				client.BaseAddress = new Uri(ApiEndpointOptions.BaseUrl);
			})
			.AddHttpMessageHandler<AuthDelegatingHandler>();

		// Plain client used only for the silent refresh call, so it never recurses into itself.
		builder.Services.AddHttpClient(ApiClientRegistration.RefreshClientName, client =>
		{
			client.BaseAddress = new Uri(ApiEndpointOptions.BaseUrl);
		});

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
