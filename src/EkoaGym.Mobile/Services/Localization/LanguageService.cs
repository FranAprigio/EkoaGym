using System.Globalization;

namespace EkoaGym.Mobile.Services.Localization;

public interface ILanguageService
{
    string Current { get; }
    CultureInfo Culture { get; }
    event Action? Changed;

    // Applies the stored choice, or the device language when nothing has been stored yet.
    void Initialize();
    void SetLanguage(string languageCode);
}

public sealed class LanguageService(ILanguagePreferenceStore store) : ILanguageService
{
    public string Current { get; private set; } = AppLanguages.English;

    public CultureInfo Culture => AppLanguages.ToCulture(Current);

    public event Action? Changed;

    public void Initialize()
    {
        Current = AppLanguages.Normalize(store.Get()) ?? AppLanguages.Resolve(CultureInfo.CurrentUICulture.Name);
        ApplyCulture();
    }

    public void SetLanguage(string languageCode)
    {
        var normalized = AppLanguages.Normalize(languageCode)
            ?? throw new ArgumentException($"Unsupported language '{languageCode}'.", nameof(languageCode));

        if (normalized == Current)
        {
            return;
        }

        Current = normalized;
        store.Set(normalized);
        ApplyCulture();
        Changed?.Invoke();
    }

    private void ApplyCulture()
    {
        var culture = Culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }
}
