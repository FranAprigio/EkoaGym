using Microsoft.Maui.Storage;

namespace EkoaGym.Mobile.Services.Localization;

public interface ILanguagePreferenceStore
{
    string? Get();
    void Set(string languageCode);
}

// Preferences (not SecureStorage): a language choice isn't a secret.
public class PreferencesLanguageStore : ILanguagePreferenceStore
{
    private const string Key = "ekoagym.language";

    public string? Get() => Preferences.Default.Get<string?>(Key, null);

    public void Set(string languageCode) => Preferences.Default.Set(Key, languageCode);
}
