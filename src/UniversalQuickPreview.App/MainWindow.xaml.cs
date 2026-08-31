using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using UniversalQuickPreview.App.Localization;
using UniversalQuickPreview.Core.Formatting;
using UniversalQuickPreview.Core.Models;
using UniversalQuickPreview.Windows.Shell;

namespace UniversalQuickPreview.App;

public partial class MainWindow : Window
{
    private readonly LocalizationService _localization;
    private FilePreviewMetadata? _metadata;
    private bool _allowClose;

    public MainWindow(LocalizationService localization)
    {
        _localization = localization;
        _localization.LanguageChanged += OnLanguageChanged;
        InitializeComponent();
    }

    public void ShowMetadata(FilePreviewMetadata metadata, nint explorerWindowHandle)
    {
        _metadata = metadata;
        FileNameText.Text = metadata.Name;
        FullPathText.Text = metadata.FullPath;
        UnavailableText.Visibility = Visibility.Collapsed;
        RefreshLocalizedValues();
        ShowAndPosition(explorerWindowHandle);
    }

    public void ShowUnavailable(string filePath, nint explorerWindowHandle)
    {
        _metadata = null;
        FileNameText.Text = Path.GetFileName(filePath);
        FullPathText.Text = filePath;
        FileSizeText.Text = string.Empty;
        ModifiedText.Text = string.Empty;
        UnavailableText.Visibility = Visibility.Visible;
        ShowAndPosition(explorerWindowHandle);
    }

    public void HidePreview()
    {
        Hide();
    }

    public void CloseForExit()
    {
        _allowClose = true;
        _localization.LanguageChanged -= OnLanguageChanged;
        Close();
    }

    private void ShowAndPosition(nint explorerWindowHandle)
    {
        if (!IsVisible)
        {
            Show();
        }

        var windowHandle = new WindowInteropHelper(this).EnsureHandle();
        MonitorPlacementService.CenterAboveOwner(windowHandle, explorerWindowHandle);
    }

    private void RefreshLocalizedValues()
    {
        if (_metadata is null)
        {
            return;
        }

        FileSizeText.Text = string.Format(
            _localization.CurrentCulture,
            _localization.GetString("FileSizeFormat"),
            FileSizeFormatter.Format(_metadata.SizeBytes, _localization.CurrentCulture));
        ModifiedText.Text = string.Format(
            _localization.CurrentCulture,
            _localization.GetString("ModifiedFormat"),
            _metadata.ModifiedAt);
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        RefreshLocalizedValues();
    }

    private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key is Key.Space or Key.Escape)
        {
            e.Handled = true;
            HidePreview();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        HidePreview();
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (!_allowClose)
        {
            e.Cancel = true;
            HidePreview();
        }
    }
}
