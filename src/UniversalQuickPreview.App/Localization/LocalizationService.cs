using System.Globalization;
using System.Windows;

namespace UniversalQuickPreview.App.Localization;

public sealed class LocalizationService
{
    public const string English = "en";
    public const string TraditionalChinese = "zh-TW";

    private readonly LanguagePreferenceStore _preferenceStore = new();

    public event EventHandler? LanguageChanged;

    public CultureInfo CurrentCulture { get; private set; } = CultureInfo.GetCultureInfo(English);

    public void Initialize()
    {
        var preferredLanguage = _preferenceStore.ReadLanguage();
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
            _preferenceStore.WriteLanguage(language);
        }

        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    private static bool IsSupported(string? language)
    {
        return language is English or TraditionalChinese;
    }
}
