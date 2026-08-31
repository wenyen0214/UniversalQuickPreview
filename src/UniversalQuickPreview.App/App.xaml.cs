using System.Windows;
using System.IO;
using UniversalQuickPreview.App.Localization;
using UniversalQuickPreview.App.Tray;
using UniversalQuickPreview.Core.Interfaces;
using UniversalQuickPreview.Core.Models;
using UniversalQuickPreview.Core.Services;
using UniversalQuickPreview.Windows.Explorer;
using UniversalQuickPreview.Windows.Hotkeys;

namespace UniversalQuickPreview.App;

public partial class App : System.Windows.Application
{
    private readonly LocalizationService _localization = new();
    private readonly IFileMetadataService _metadataService = new FileMetadataService();
    private readonly IExplorerSelectionService _selectionService = new ExplorerSelectionService();
    private IGlobalShortcutService? _shortcutService;
    private TrayIconService? _trayIcon;
    private MainWindow? _previewWindow;
    private bool _isExiting;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _localization.Initialize();

        _previewWindow = new MainWindow(_localization);
        _trayIcon = new TrayIconService(_localization);
        _trayIcon.EnabledChanged += OnEnabledChanged;
        _trayIcon.ExitRequested += OnExitRequested;

        _shortcutService = new GlobalKeyboardHookService();
        _shortcutService.ShortcutPressed += OnShortcutPressed;

        try
        {
            _shortcutService.Start();
        }
        catch (Exception exception)
        {
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
        _shortcutService?.Dispose();
        _trayIcon?.Dispose();
        base.OnExit(e);
    }

    private void OnShortcutPressed(object? sender, PreviewShortcutPressedEventArgs e)
    {
        if (_previewWindow is null)
        {
            return;
        }

        if (e.Shortcut == PreviewShortcut.Escape)
        {
            if (_previewWindow.IsVisible)
            {
                e.Handled = true;
                _previewWindow.HidePreview();
            }

            return;
        }

        if (_previewWindow.IsVisible)
        {
            e.Handled = true;
            _previewWindow.HidePreview();
            return;
        }

        e.Handled = true;
        Dispatcher.BeginInvoke(OpenCurrentExplorerSelection);
    }

    private void OpenCurrentExplorerSelection()
    {
        if (_previewWindow is null)
        {
            return;
        }

        ExplorerSelection? selection;
        try
        {
            selection = _selectionService.GetCurrentSelection();
        }
        catch
        {
            // Explorer can close or navigate while its COM automation objects are queried.
            return;
        }

        var selectedPath = selection?.SelectedFiles.FirstOrDefault();
        if (selection is null || string.IsNullOrWhiteSpace(selectedPath))
        {
            return;
        }

        try
        {
            var metadata = _metadataService.GetMetadata(selectedPath);
            _previewWindow.ShowMetadata(metadata, selection.ExplorerWindowHandle);
        }
        catch (IOException)
        {
            _previewWindow.ShowUnavailable(selectedPath, selection.ExplorerWindowHandle);
        }
        catch (UnauthorizedAccessException)
        {
            _previewWindow.ShowUnavailable(selectedPath, selection.ExplorerWindowHandle);
        }
    }

    private void OnEnabledChanged(bool enabled)
    {
        if (_shortcutService is not null)
        {
            _shortcutService.IsEnabled = enabled;
        }

        if (!enabled)
        {
            _previewWindow?.HidePreview();
        }
    }

    private void OnExitRequested(object? sender, EventArgs e)
    {
        if (_isExiting)
        {
            return;
        }

        _isExiting = true;
        _previewWindow?.CloseForExit();
        Shutdown();
    }
}
