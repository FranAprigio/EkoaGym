using Maui.Biometric;
using Microsoft.Maui.Storage;

namespace EkoaGym.Mobile.Services.Auth;

public class BiometricLoginService(ITokenStore tokenStore) : IBiometricLoginService
{
    private const string EnabledPreferenceKey = "ekoagym.biometric_login_enabled";
    private const string PromptedPreferenceKey = "ekoagym.biometric_prompt_shown";

    public async Task<bool> IsAvailableAsync()
    {
        var availability = await BiometricAuthentication.Current.CheckAvailabilityAsync();
        return availability.IsAvailable;
    }

    public Task<bool> IsEnabledAsync() => Task.FromResult(Preferences.Default.Get(EnabledPreferenceKey, false));

    public Task EnableAsync()
    {
        Preferences.Default.Set(EnabledPreferenceKey, true);
        return Task.CompletedTask;
    }

    public async Task DisableAsync()
    {
        Preferences.Default.Remove(EnabledPreferenceKey);

        // The stored refresh token was only safe to silently reuse because biometrics gated it —
        // once that guarantee is dropped, clear it so the next login requires a password again.
        await tokenStore.ClearAsync();
    }

    public async Task<BiometricUnlockOutcome> TryUnlockAsync()
    {
        if (!await IsEnabledAsync())
        {
            return BiometricUnlockOutcome.NotEnabled;
        }

        if (!await IsAvailableAsync())
        {
            return BiometricUnlockOutcome.NotAvailable;
        }

        var result = await BiometricAuthentication.Current.AuthenticateAsync(
            new AuthenticationRequest("Log in to EkoaGym", "Use your fingerprint or face to log in"));

        return result.Status switch
        {
            AuthenticationStatus.Success => BiometricUnlockOutcome.Success,
            AuthenticationStatus.Canceled => BiometricUnlockOutcome.Cancelled,
            _ => BiometricUnlockOutcome.Failed
        };
    }

    public async Task<bool> ShouldPromptToEnableAsync()
    {
        if (Preferences.Default.Get(PromptedPreferenceKey, false))
        {
            return false;
        }

        return !await IsEnabledAsync() && await IsAvailableAsync();
    }

    public Task MarkPromptDismissedAsync()
    {
        Preferences.Default.Set(PromptedPreferenceKey, true);
        return Task.CompletedTask;
    }
}
