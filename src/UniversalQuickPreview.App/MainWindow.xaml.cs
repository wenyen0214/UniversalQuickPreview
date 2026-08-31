using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using UniversalQuickPreview.App.Localization;
using UniversalQuickPreview.App.Settings;
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

    public event EventHandler? CloseRequested;

    public event EventHandler<int>? NavigationRequested;

    public nint ShowLoading(
        FilePreviewMetadata metadata,
        nint explorerWindowHandle,
        UserSettings settings)
    {
        _metadata = metadata;
        FileNameText.Text = metadata.Name;
        FullPathText.Text = metadata.FullPath;
        PathPanel.Visibility = settings.ShowFullPath ? Visibility.Visible : Visibility.Collapsed;
        DimensionsText.Text = string.Empty;
        ResetPanels();
        LoadingPanel.Visibility = Visibility.Visible;
        RefreshLocalizedValues();

        if (!IsVisible)
        {
            Show();
        }

        var windowHandle = new WindowInteropHelper(this).EnsureHandle();
        var (width, height) = settings.WindowSize switch
        {
            PreviewWindowSize.Compact => (680, 420),
            PreviewWindowSize.Large => (1040, 680),
            _ => (860, 540)
        };
        MonitorPlacementService.CenterAboveOwner(windowHandle, explorerWindowHandle, width, height);
        return windowHandle;
    }

    public void ShowContent(PreviewContent content)
    {
        ResetPanels();

        switch (content.Kind)
        {
            case PreviewContentKind.Text:
                TextPreview.Text = content.Text ?? string.Empty;
                TextPreview.Visibility = Visibility.Visible;
                DimensionsText.Text = content.IsTruncated
                    ? _localization.GetString("PreviewTruncated")
                    : string.Empty;
                break;
            case PreviewContentKind.Image:
                ImagePreview.Source = LoadImage(content.SourcePath!);
                ImagePanel.Visibility = Visibility.Visible;
                DimensionsText.Text = content.PixelWidth is not null && content.PixelHeight is not null
                    ? $"{content.PixelWidth} × {content.PixelHeight}"
                    : string.Empty;
                break;
            default:
                FallbackTypeText.Text = string.Format(
                    _localization.CurrentCulture,
                    _localization.GetString("UnsupportedTypeFormat"),
                    content.Detail);
                FallbackPanel.Visibility = Visibility.Visible;
                break;
        }
    }

    public void ShowError()
    {
        ResetPanels();
        ErrorPanel.Visibility = Visibility.Visible;
    }

    public void SetNavigationAvailability(bool canMovePrevious, bool canMoveNext)
    {
        PreviousButton.IsEnabled = canMovePrevious;
        NextButton.IsEnabled = canMoveNext;
    }

    public void HidePreview()
    {
        ImagePreview.Source = null;
        TextPreview.Text = string.Empty;
        Hide();
    }

    public void CloseForExit()
    {
        _allowClose = true;
        _localization.LanguageChanged -= OnLanguageChanged;
        Close();
    }

    private static BitmapImage LoadImage(string filePath)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
        image.DecodePixelWidth = 1800;
        image.UriSource = new Uri(filePath, UriKind.Absolute);
        image.EndInit();
        image.Freeze();
        return image;
    }

    private void ResetPanels()
    {
        LoadingPanel.Visibility = Visibility.Collapsed;
        TextPreview.Visibility = Visibility.Collapsed;
        ImagePanel.Visibility = Visibility.Collapsed;
        FallbackPanel.Visibility = Visibility.Collapsed;
        ErrorPanel.Visibility = Visibility.Collapsed;
        ImagePreview.Source = null;
    }

    private void RefreshLocalizedValues()
    {
        if (_metadata is null)
        {
            return;
        }

        FileSizeText.Text = FileSizeFormatter.Format(_metadata.SizeBytes, _localization.CurrentCulture);
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
        switch (e.Key)
        {
            case Key.Space:
            case Key.Escape:
                e.Handled = true;
                CloseRequested?.Invoke(this, EventArgs.Empty);
                break;
            case Key.Left:
                e.Handled = true;
                NavigationRequested?.Invoke(this, -1);
                break;
            case Key.Right:
                e.Handled = true;
                NavigationRequested?.Invoke(this, 1);
                break;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void PreviousButton_Click(object sender, RoutedEventArgs e)
    {
        NavigationRequested?.Invoke(this, -1);
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        NavigationRequested?.Invoke(this, 1);
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (!_allowClose)
        {
            e.Cancel = true;
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
