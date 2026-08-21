namespace EkoaGym.Mobile.Services.Auth;

public enum BiometricUnlockOutcome
{
    Success,
    NotEnabled,
    NotAvailable,
    Cancelled,
    Failed
}

// Gates the already-stored refresh token behind a platform biometric check (M-1.7) — never issues or
// stores a separate credential of its own, just adds an explicit unlock step in front of ITokenStore.
public interface IBiometricLoginService
{
    Task<bool> IsAvailableAsync();
    Task<bool> IsEnabledAsync();
    Task EnableAsync();
    Task DisableAsync();
    Task<BiometricUnlockOutcome> TryUnlockAsync();

    // "Prompt once" (M-1.7): true only if hardware is available, not already enabled, and the
    // one-time post-login prompt hasn't been shown/dismissed before.
    Task<bool> ShouldPromptToEnableAsync();
    Task MarkPromptDismissedAsync();
}
