using EkoaGym.Mobile.Models;
using EkoaGym.Mobile.Services.Api;
using EkoaGym.Mobile.Services.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace EkoaGym.Mobile.Services.Localization;

// Keeps the server-side preferredLanguage in step with the device choice (the device always wins).
public sealed class ProfileLanguageSync
{
    private readonly IServiceProvider services;
    private readonly ILanguageService language;
    private readonly AuthState authState;

    public ProfileLanguageSync(IServiceProvider services, ILanguageService language, AuthState authState)
    {
        this.services = services;
        this.language = language;
        this.authState = authState;
        language.Changed += () => _ = SyncAsync();
    }

    public async Task SyncAsync()
    {
        if (!authState.IsAuthenticated)
        {
            return;
        }

        // ApiClient is a transient typed client, so resolve per call instead of capturing it in this singleton.
        await PushAsync(services.GetRequiredService<ApiClient>(), language.Current);
    }

    public static async Task PushAsync(ApiClient api, string languageCode, CancellationToken cancellationToken = default)
    {
        ProfileSummary? profile;
        try
        {
            profile = await api.GetAsync<ProfileSummary>("api/profile", cancellationToken);
        }
        catch (ApiException)
        {
            // 404 means no profile yet (the language travels with profile setup); other failures retry on next launch.
            return;
        }

        if (profile is null || string.Equals(profile.PreferredLanguage, languageCode, StringComparison.Ordinal))
        {
            return;
        }

        try
        {
            await api.PutAsync("api/profile", new ProfileSetupRequest(
                profile.Name, profile.DateOfBirth, profile.Sex, profile.HeightCm,
                profile.CurrentWeightKg, profile.ActivityLevel, languageCode), cancellationToken);
        }
        catch (ApiException)
        {
            // Best effort: the mismatch is detected again on the next sync.
        }
    }
}
