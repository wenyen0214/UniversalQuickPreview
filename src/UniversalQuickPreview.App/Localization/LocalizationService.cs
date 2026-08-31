using System.Globalization;
using System.Windows;
using UniversalQuickPreview.App.Settings;

namespace UniversalQuickPreview.App.Localization;

public sealed class LocalizationService
{
    public const string English = "en";
    public const string TraditionalChinese = "zh-TW";

    private readonly SettingsService _settings;

    public LocalizationService(SettingsService settings)
    {
        _settings = settings;
    }

    public event EventHandler? LanguageChanged;

    public CultureInfo CurrentCulture { get; private set; } = CultureInfo.GetCultureInfo(English);

    public void Initialize()
    {
        var preferredLanguage = _settings.Current.Language;
        var language = IsSupported(preferredLanguage)
            ? preferredLanguage!
            : CultureInfo.CurrentUICulture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
                ? TraditionalChinese
                : English;

        ApplyLanguage(language, persist: false);
    }

    public void ApplyLanguage(string language)
    {
        ApplyLanguage(language, persist: true);
    }

    public void SynchronizeLanguage(string language)
    {
        ApplyLanguage(language, persist: false);
    }

    public string GetString(string key)
    {
        return System.Windows.Application.Current.TryFindResource(key) as string ?? key;
    }

    private void ApplyLanguage(string language, bool persist)
    {
        if (!IsSupported(language))
        {
            throw new ArgumentOutOfRangeException(nameof(language));
        }

        CurrentCulture = CultureInfo.GetCultureInfo(language);
        CultureInfo.CurrentUICulture = CurrentCulture;

        var dictionaries = System.Windows.Application.Current.Resources.MergedDictionaries;
        var existing = dictionaries.FirstOrDefault(dictionary =>
            dictionary.Source?.OriginalString.Contains("Resources/Strings.", StringComparison.OrdinalIgnoreCase) == true);
        var replacement = new ResourceDictionary
        {
            Source = new Uri($"Resources/Strings.{language}.xaml", UriKind.Relative)
        };

        if (existing is null)
        {
            dictionaries.Add(replacement);
        }
        else
        {
            dictionaries[dictionaries.IndexOf(existing)] = replacement;
        }

        if (persist)
        {
            _settings.Update(settings => settings with { Language = language });
        }

        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    private static bool IsSupported(string? language)
    {
        return language is English or TraditionalChinese;
    }
}
