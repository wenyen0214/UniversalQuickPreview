using System.Diagnostics;
using System.IO;
using System.Windows;
using UniversalQuickPreview.App.Lifecycle;
using UniversalQuickPreview.App.Localization;
using UniversalQuickPreview.App.Logging;
using UniversalQuickPreview.App.Settings;
using UniversalQuickPreview.App.Theming;
using UniversalQuickPreview.App.Tray;
using UniversalQuickPreview.Core.Interfaces;
using UniversalQuickPreview.Core.Models;
using UniversalQuickPreview.Core.Services;
using UniversalQuickPreview.Previewers.Fallback;
using UniversalQuickPreview.Previewers.Image;
using UniversalQuickPreview.Previewers.Text;
using UniversalQuickPreview.Windows.Explorer;
using UniversalQuickPreview.Windows.Hotkeys;
using UniversalQuickPreview.Windows.Shell;

namespace UniversalQuickPreview.App;

public partial class App : System.Windows.Application
{
    private readonly SettingsService _settings = new();
    private readonly ThemeService _theme = new();
    private readonly LocalFileLogger _logger = new();
    private readonly IFileMetadataService _metadataService = new FileMetadataService();
    private readonly IExplorerSelectionService _selectionService = new ExplorerSelectionService();
    private readonly PreviewNavigationService _navigation = new();
    private readonly IPreviewProviderRegistry _previewProviders;
    private readonly StartupRegistrationService _startupRegistration = new();
    private readonly OutsideClickWatcher _outsideClickWatcher = new();
    private readonly LocalizationService _localization;
    private SingleInstanceService? _singleInstance;
    private IGlobalShortcutService? _shortcutService;
    private TrayIconService? _trayIcon;
    private MainWindow? _previewWindow;
    private CancellationTokenSource? _previewCancellation;
    private nint _explorerWindowHandle;
    private bool _isExiting;

    public App()
    {
        _localization = new LocalizationService(_settings);
        _previewProviders = new PreviewProviderRegistry([
            new ImagePreviewProvider(),
            new TextPreviewProvider(),
            new FallbackPreviewProvider()
        ]);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _singleInstance = new SingleInstanceService();
        if (!_singleInstance.IsPrimary)
        {
            _singleInstance.SignalPrimary();
            Shutdown();
            return;
        }

        _localization.Initialize();
        _theme.Apply(_settings.Current.Theme);
        _logger.Info("ApplicationStartup", ("Version", GetType().Assembly.GetName().Version));

        _previewWindow = new MainWindow(_localization);
        _previewWindow.CloseRequested += (_, _) => HidePreview();
        _previewWindow.NavigationRequested += (_, direction) => _ = NavigateAsync(direction);

        _trayIcon = new TrayIconService(_localization, _settings.Current.IsEnabled);
        _trayIcon.EnabledChanged += enabled => _settings.Update(settings => settings with { IsEnabled = enabled });
        _trayIcon.SettingsRequested += (_, _) => ShowSettings();
        _trayIcon.LogsRequested += (_, _) => OpenLogsFolder();
        _trayIcon.ExitRequested += OnExitRequested;

        _outsideClickWatcher.OutsideClicked += (_, _) => Dispatcher.BeginInvoke(HidePreview);
        _singleInstance.ActivationRequested += (_, _) => Dispatcher.BeginInvoke(
            () => _trayIcon?.ShowAlreadyRunning());
        _settings.Changed += OnSettingsChanged;

        _shortcutService = new GlobalKeyboardHookService();
        _shortcutService.ShortcutPressed += OnShortcutPressed;
        ApplySettings();

        try
        {
            _shortcutService.Start();
        }
        catch (Exception exception)
        {
            _logger.Error("ShortcutStartFailed", exception);
            System.Windows.MessageBox.Show(
                string.Format(_localization.GetString("ShortcutStartFailedMessage"), exception.Message),
                _localization.GetString("AppName"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _logger.Info("ApplicationExit", ("ExitCode", e.ApplicationExitCode));
        _previewCancellation?.Cancel();
        _previewCancellation?.Dispose();
        _outsideClickWatcher.Dispose();
        _shortcutService?.Dispose();
        _trayIcon?.Dispose();
        _singleInstance?.Dispose();
        base.OnExit(e);
    }

    private void OnShortcutPressed(object? sender, PreviewShortcutPressedEventArgs e)
    {
        _logger.Info("ShortcutPressed", ("Shortcut", e.Shortcut));

        if (_previewWindow is null)
        {
            return;
        }

        switch (e.Shortcut)
        {
            case PreviewShortcut.Escape when _previewWindow.IsVisible:
                e.Handled = true;
                HidePreview();
                break;
            case PreviewShortcut.Space when _previewWindow.IsVisible:
                e.Handled = true;
                HidePreview();
                break;
            case PreviewShortcut.Space:
                e.Handled = true;
                _ = Dispatcher.InvokeAsync(OpenCurrentExplorerSelectionAsync);
                break;
            case PreviewShortcut.Previous when _previewWindow.IsVisible:
                e.Handled = true;
                _ = NavigateAsync(-1);
                break;
            case PreviewShortcut.Next when _previewWindow.IsVisible:
                e.Handled = true;
                _ = NavigateAsync(1);
                break;
        }
    }

    private async Task OpenCurrentExplorerSelectionAsync()
    {
        ExplorerSelection? selection;
        try
        {
            selection = _selectionService.GetCurrentSelection();
        }
        catch (Exception exception)
        {
            _logger.Error("ExplorerSelectionFailed", exception);
            return;
        }

        var selectedPath = selection?.SelectedFiles.FirstOrDefault();
        if (selection is null || string.IsNullOrWhiteSpace(selectedPath))
        {
            _logger.Info("ExplorerSelectionEmpty");
            return;
        }

        _explorerWindowHandle = selection.ExplorerWindowHandle;
        _navigation.Start(selection.FolderFiles, selection.SelectedIndex);
        await LoadPreviewAsync(selectedPath).ConfigureAwait(true);
    }

    private async Task NavigateAsync(int direction)
    {
        var before = _navigation.Current;
        var target = _navigation.Move(direction);
        if (string.IsNullOrWhiteSpace(target) || string.Equals(before, target, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await LoadPreviewAsync(target).ConfigureAwait(true);
    }

    private async Task LoadPreviewAsync(string filePath)
    {
        if (_previewWindow is null)
        {
            return;
        }

        var previousCancellation = Interlocked.Exchange(ref _previewCancellation, new CancellationTokenSource());
        previousCancellation?.Cancel();
        previousCancellation?.Dispose();
        var cancellation = _previewCancellation;

        try
        {
            var metadata = _metadataService.GetMetadata(filePath);
            _logger.Info("PreviewRequested", ("File", filePath));
            var previewHandle = _previewWindow.ShowLoading(metadata, _explorerWindowHandle, _settings.Current);

            _outsideClickWatcher.Stop();
            if (_settings.Current.CloseWhenClickingOutside)
            {
                _outsideClickWatcher.Start(previewHandle, transparentMargin: 18);
            }

            var result = await _previewProviders.LoadAsync(filePath, cancellation.Token).ConfigureAwait(true);
            cancellation.Token.ThrowIfCancellationRequested();
            _previewWindow.ShowContent(result.Content);
            _logger.Info(
                "PreviewLoaded",
                ("File", filePath),
                ("Provider", result.ProviderName),
                ("DurationMs", Math.Round(result.LoadDuration.TotalMilliseconds, 1)));
        }
        catch (OperationCanceledException)
        {
            _logger.Info("PreviewCancelled", ("File", filePath));
        }
        catch (Exception exception)
        {
            if (!cancellation.IsCancellationRequested)
            {
                _previewWindow.ShowError();
                _logger.Error("PreviewFailed", exception, ("File", filePath));
            }
        }
    }

    private void HidePreview()
    {
        _previewCancellation?.Cancel();
        _outsideClickWatcher.Stop();
        _previewWindow?.HidePreview();
    }

    private void ShowSettings()
    {
        HidePreview();
        var window = new SettingsWindow(_settings, _localization);
        window.ShowDialog();
    }

    private void OpenLogsFolder()
    {
        try
        {
            Directory.CreateDirectory(_logger.LogDirectory);
            Process.Start(new ProcessStartInfo("explorer.exe", $"\"{_logger.LogDirectory}\"")
            {
                UseShellExecute = true
            });
        }
        catch (Exception exception)
        {
            _logger.Error("OpenLogsFailed", exception);
        }
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        ApplySettings();
    }

    private void ApplySettings()
    {
        var settings = _settings.Current;
        if (_shortcutService is not null)
        {
            _shortcutService.IsEnabled = settings.IsEnabled;
            _shortcutService.Gesture = settings.Shortcut;
        }

        _trayIcon?.SetEnabled(settings.IsEnabled);
        _theme.Apply(settings.Theme);
        if (!string.IsNullOrEmpty(settings.Language) &&
            !string.Equals(_localization.CurrentCulture.Name, settings.Language, StringComparison.OrdinalIgnoreCase))
        {
            _localization.SynchronizeLanguage(settings.Language);
        }

        try
        {
            _startupRegistration.SetEnabled(settings.LaunchAtStartup, Environment.ProcessPath!);
        }
        catch (Exception exception)
        {
            _logger.Error("StartupRegistrationFailed", exception);
        }

        if (!settings.IsEnabled)
        {
            HidePreview();
        }

        _logger.Info(
            "SettingsApplied",
            ("Enabled", settings.IsEnabled),
            ("Shortcut", settings.Shortcut),
            ("Theme", settings.Theme));
    }

    private void OnExitRequested(object? sender, EventArgs e)
    {
        if (_isExiting)
        {
            return;
        }

        _isExiting = true;
        HidePreview();
        _previewWindow?.CloseForExit();
        Shutdown();
    }
}
