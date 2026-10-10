using EkoaGym.Mobile.Services.Api;
using EkoaGym.Mobile.Services.Auth;
using EkoaGym.Mobile.Services.Connectivity;
using EkoaGym.Mobile.Services.Data;
using EkoaGym.Mobile.Services.Localization;
using EkoaGym.Mobile.Services.Motion;
using EkoaGym.Mobile.Services.Notifications;
using Maui.Biometric;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;

namespace EkoaGym.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseLocalNotification()
			.UseBiometricAuthentication()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

		// Localization: IStringLocalizer<SharedResources> backed by SharedResources(.pt-BR).resx.
		builder.Services.AddLocalization();
		builder.Services.AddSingleton<ILanguagePreferenceStore, PreferencesLanguageStore>();
		builder.Services.AddSingleton<ILanguageService, LanguageService>();
		builder.Services.AddSingleton<ProfileLanguageSync>();
		// Auth / token storage
		builder.Services.AddSingleton<ITokenStore, SecureStorageTokenStore>();
		builder.Services.AddSingleton<AuthState>();
		builder.Services.AddSingleton<IBiometricLoginService, BiometricLoginService>();
		builder.Services.AddTransient<AuthDelegatingHandler>();

		// Connectivity + offline cache
		builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
		builder.Services.AddSingleton<LocalDatabaseService>();
		builder.Services.AddSingleton<ISyncService, SyncService>();
		builder.Services.AddSingleton<IRestTimerNotifier, RestTimerNotifier>();
		builder.Services.AddSingleton<IMotionService, MotionService>();

		// Typed API client — carries the bearer token and retries once on 401 via AuthDelegatingHandler.
		builder.Services
			.AddHttpClient<ApiClient>(ApiClientRegistration.ApiClientName, client =>
			{
				client.BaseAddress = new Uri(ApiEndpointOptions.BaseUrl);
			})
#if DEBUG
			.ConfigurePrimaryHttpMessageHandler(ApiEndpointOptions.CreateDevHttpMessageHandler)
#endif
			.AddHttpMessageHandler<AuthDelegatingHandler>();

		// Plain client used only for the silent refresh call, so it never recurses into itself.
		builder.Services.AddHttpClient(ApiClientRegistration.RefreshClientName, client =>
		{
			client.BaseAddress = new Uri(ApiEndpointOptions.BaseUrl);
		})
#if DEBUG
			.ConfigurePrimaryHttpMessageHandler(ApiEndpointOptions.CreateDevHttpMessageHandler)
#endif
			;

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();

		// Must run before any UI renders so the first frame already uses the right culture.
		app.Services.GetRequiredService<ILanguageService>().Initialize();
		// Constructing it subscribes to language changes so the profile is updated whenever the user switches.
		_ = app.Services.GetRequiredService<ProfileLanguageSync>();

		return app;
	}
}
