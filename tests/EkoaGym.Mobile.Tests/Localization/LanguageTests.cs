using System.Globalization;
using EkoaGym.Mobile.Resources;
using EkoaGym.Mobile.Services.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace EkoaGym.Mobile.Tests.Localization;

// Culture is process-wide state (DefaultThread*), so these tests must not run in parallel with each other.
[CollectionDefinition("Culture", DisableParallelization = true)]
public class CultureCollection;

[Collection("Culture")]
public class LanguageTests : IDisposable
{
    private readonly CultureInfo originalCulture = CultureInfo.CurrentCulture;
    private readonly CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
    private readonly CultureInfo? originalDefaultCulture = CultureInfo.DefaultThreadCurrentCulture;
    private readonly CultureInfo? originalDefaultUiCulture = CultureInfo.DefaultThreadCurrentUICulture;

    public void Dispose()
    {
        CultureInfo.CurrentCulture = originalCulture;
        CultureInfo.CurrentUICulture = originalUiCulture;
        CultureInfo.DefaultThreadCurrentCulture = originalDefaultCulture;
        CultureInfo.DefaultThreadCurrentUICulture = originalDefaultUiCulture;
    }

    private sealed class FakeStore(string? initial = null) : ILanguagePreferenceStore
    {
        public string? Value { get; private set; } = initial;
        public string? Get() => Value;
        public void Set(string languageCode) => Value = languageCode;
    }

    [Theory]
    [InlineData("pt", "pt-BR")]
    [InlineData("pt-BR", "pt-BR")]
    [InlineData("pt-PT", "pt-BR")]
    [InlineData("PT-br", "pt-BR")]
    [InlineData("en-US", "en")]
    [InlineData("es-ES", "en")]
    [InlineData("", "en")]
    [InlineData(null, "en")]
    public void Resolve_MapsAnyPortugueseLocaleToBrazilian(string? deviceCulture, string expected) =>
        Assert.Equal(expected, AppLanguages.Resolve(deviceCulture));

    [Fact]
    public void Initialize_WithoutStoredChoice_FollowsDevice()
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("pt-PT");
        var service = new LanguageService(new FakeStore());

        service.Initialize();

        Assert.Equal("pt-BR", service.Current);
        Assert.Equal("pt-BR", CultureInfo.CurrentCulture.Name);
    }

    [Fact]
    public void Initialize_StoredChoiceWinsOverDevice()
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("pt-BR");
        var service = new LanguageService(new FakeStore("en"));

        service.Initialize();

        Assert.Equal("en", service.Current);
        Assert.Equal("en-US", CultureInfo.CurrentUICulture.Name);
    }

    [Fact]
    public void SetLanguage_PersistsAppliesCultureAndRaisesChanged()
    {
        var store = new FakeStore("en");
        var service = new LanguageService(store);
        service.Initialize();
        var raised = 0;
        service.Changed += () => raised++;

        service.SetLanguage("pt-BR");

        Assert.Equal("pt-BR", store.Value);
        Assert.Equal("pt-BR", CultureInfo.CurrentCulture.Name);
        Assert.Equal("pt-BR", CultureInfo.DefaultThreadCurrentUICulture?.Name);
        Assert.Equal(1, raised);
    }

    [Fact]
    public void SetLanguage_SameLanguage_DoesNotRaiseChanged()
    {
        var service = new LanguageService(new FakeStore("en"));
        service.Initialize();
        var raised = 0;
        service.Changed += () => raised++;

        service.SetLanguage("en");

        Assert.Equal(0, raised);
    }

    [Fact]
    public void SetLanguage_UnsupportedLanguage_Throws()
    {
        var service = new LanguageService(new FakeStore());

        Assert.Throws<ArgumentException>(() => service.SetLanguage("fr"));
    }

    private static IStringLocalizer CreateLocalizer() =>
        new ResourceManagerStringLocalizerFactory(Options.Create(new LocalizationOptions()), NullLoggerFactory.Instance)
            .Create(typeof(SharedResources));

    [Fact]
    public void Localizer_ReturnsPortugueseForPtBr()
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("pt-BR");

        Assert.Equal("Voltar", CreateLocalizer()["Common.Back"].Value);
    }

    [Fact]
    public void Localizer_UnsupportedCulture_FallsBackToEnglish()
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");

        Assert.Equal("Back", CreateLocalizer()["Common.Back"].Value);
    }

    [Theory]
    [InlineData("en", 0, false)]
    [InlineData("en", 1, true)]
    [InlineData("en", 2, false)]
    [InlineData("pt-BR", 0, true)]
    [InlineData("pt-BR", 1, true)]
    [InlineData("pt-BR", 2, false)]
    public void PluralRules_FollowCultureRules(string culture, int count, bool singular) =>
        Assert.Equal(singular, PluralRules.IsSingular(CultureInfo.GetCultureInfo(culture), count));

    [Fact]
    public void Plural_PicksTheRightForm()
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("pt-BR");
        var localizer = CreateLocalizer();

        Assert.Equal("1 série", localizer.Plural("Plural.Set", 1));
        Assert.Equal("2 séries", localizer.Plural("Plural.Set", 2));
    }
}
