using System.Windows;
using System.Windows.Controls;
using UniversalQuickPreview.App.Localization;
using UniversalQuickPreview.Core.Models;

namespace UniversalQuickPreview.App.Settings;

public partial class SettingsWindow : Window
{
    private readonly SettingsService _settings;
    private readonly LocalizationService _localization;

    public SettingsWindow(SettingsService settings, LocalizationService localization)
    {
        _settings = settings;
        _localization = localization;
        InitializeComponent();
        PopulateChoices();
        LoadValues();
    }

    private void PopulateChoices()
    {
        AddChoice(ShortcutComboBox, PreviewShortcutGesture.Space, "Space");
        AddChoice(ShortcutComboBox, PreviewShortcutGesture.AltSpace, "Alt + Space");
        AddChoice(ShortcutComboBox, PreviewShortcutGesture.ControlSpace, "Ctrl + Space");

        AddChoice(ThemeComboBox, AppTheme.System, _localization.GetString("SystemTheme"));
        AddChoice(ThemeComboBox, AppTheme.Light, _localization.GetString("LightTheme"));
        AddChoice(ThemeComboBox, AppTheme.Dark, _localization.GetString("DarkTheme"));

        AddChoice(WindowSizeComboBox, PreviewWindowSize.Compact, _localization.GetString("CompactSize"));
        AddChoice(WindowSizeComboBox, PreviewWindowSize.Medium, _localization.GetString("MediumSize"));
        AddChoice(WindowSizeComboBox, PreviewWindowSize.Large, _localization.GetString("LargeSize"));

        AddChoice(LanguageComboBox, LocalizationService.English, "English");
        AddChoice(LanguageComboBox, LocalizationService.TraditionalChinese, "繁體中文");
    }

    private void LoadValues()
    {
        var settings = _settings.Current;
        EnabledCheckBox.IsChecked = settings.IsEnabled;
        StartupCheckBox.IsChecked = settings.LaunchAtStartup;
        ShowPathCheckBox.IsChecked = settings.ShowFullPath;
        OutsideClickCheckBox.IsChecked = settings.CloseWhenClickingOutside;
        SelectChoice(ShortcutComboBox, settings.Shortcut);
        SelectChoice(ThemeComboBox, settings.Theme);
        SelectChoice(WindowSizeComboBox, settings.WindowSize);
        SelectChoice(
            LanguageComboBox,
            string.IsNullOrEmpty(settings.Language) ? _localization.CurrentCulture.Name : settings.Language);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        _settings.Update(settings => settings with
        {
            IsEnabled = EnabledCheckBox.IsChecked == true,
            LaunchAtStartup = StartupCheckBox.IsChecked == true,
            ShowFullPath = ShowPathCheckBox.IsChecked == true,
            CloseWhenClickingOutside = OutsideClickCheckBox.IsChecked == true,
            Shortcut = GetChoice<PreviewShortcutGesture>(ShortcutComboBox),
            Theme = GetChoice<AppTheme>(ThemeComboBox),
            WindowSize = GetChoice<PreviewWindowSize>(WindowSizeComboBox),
            Language = GetChoice<string>(LanguageComboBox)
        });
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private static void AddChoice(System.Windows.Controls.ComboBox comboBox, object value, string text)
    {
        comboBox.Items.Add(new ComboBoxItem { Tag = value, Content = text });
    }

    private static void SelectChoice(System.Windows.Controls.ComboBox comboBox, object value)
    {
        comboBox.SelectedItem = comboBox.Items
            .OfType<ComboBoxItem>()
            .FirstOrDefault(item => Equals(item.Tag, value));
        comboBox.SelectedIndex = Math.Max(comboBox.SelectedIndex, 0);
    }

    private static T GetChoice<T>(System.Windows.Controls.ComboBox comboBox)
    {
        return (T)((ComboBoxItem)comboBox.SelectedItem).Tag;
    }
}
