using System.Diagnostics;
using System.Drawing;
using Forms = System.Windows.Forms;
using UniversalQuickPreview.App.Localization;

namespace UniversalQuickPreview.App.Tray;

public sealed class TrayIconService : IDisposable
{
    private readonly LocalizationService _localization;
    private readonly Forms.NotifyIcon _notifyIcon;
    private Forms.ContextMenuStrip? _menu;
    private bool _enabled;
    private bool _disposed;

    public TrayIconService(LocalizationService localization, bool enabled)
    {
        _localization = localization;
        _enabled = enabled;
        _localization.LanguageChanged += OnLanguageChanged;
        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = LoadApplicationIcon(),
            Visible = true
        };

        RebuildMenu();
    }

    public event Action<bool>? EnabledChanged;

    public event EventHandler? SettingsRequested;

    public event EventHandler? LogsRequested;

    public event EventHandler? ExitRequested;

    public void SetEnabled(bool enabled)
    {
        if (_enabled == enabled)
        {
            return;
        }

        _enabled = enabled;
        RebuildMenu();
    }

    public void ShowAlreadyRunning()
    {
        _notifyIcon.BalloonTipTitle = _localization.GetString("AppName");
        _notifyIcon.BalloonTipText = _localization.GetString("AlreadyRunningMessage");
        _notifyIcon.ShowBalloonTip(2500);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _localization.LanguageChanged -= OnLanguageChanged;
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _menu?.Dispose();
        _disposed = true;
    }

    private void RebuildMenu()
    {
        var oldMenu = _menu;
        var menu = new Forms.ContextMenuStrip();

        var title = new Forms.ToolStripMenuItem(_localization.GetString("AppName"))
        {
            Enabled = false,
            Font = new Font(Forms.Control.DefaultFont, FontStyle.Bold)
        };

        var enabled = new Forms.ToolStripMenuItem(_localization.GetString("EnabledMenu"))
        {
            CheckOnClick = true,
            Checked = _enabled
        };
        enabled.CheckedChanged += (_, _) => EnabledChanged?.Invoke(enabled.Checked);

        var settings = new Forms.ToolStripMenuItem(_localization.GetString("SettingsMenu"));
        settings.Click += (_, _) => SettingsRequested?.Invoke(this, EventArgs.Empty);

        var english = new Forms.ToolStripMenuItem("English")
        {
            Checked = _localization.CurrentCulture.Name == LocalizationService.English
        };
        english.Click += (_, _) => _localization.ApplyLanguage(LocalizationService.English);

        var traditionalChinese = new Forms.ToolStripMenuItem("繁體中文")
        {
            Checked = _localization.CurrentCulture.Name == LocalizationService.TraditionalChinese
        };
        traditionalChinese.Click += (_, _) => _localization.ApplyLanguage(LocalizationService.TraditionalChinese);

        var language = new Forms.ToolStripMenuItem(_localization.GetString("LanguageMenu"));
        language.DropDownItems.AddRange([english, traditionalChinese]);

        var logs = new Forms.ToolStripMenuItem(_localization.GetString("OpenLogsMenu"));
        logs.Click += (_, _) => LogsRequested?.Invoke(this, EventArgs.Empty);

        var about = new Forms.ToolStripMenuItem(_localization.GetString("AboutMenu"));
        about.Click += (_, _) => System.Windows.MessageBox.Show(
            _localization.GetString("AboutMessage"),
            _localization.GetString("AppName"),
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);

        var github = new Forms.ToolStripMenuItem(_localization.GetString("GitHubMenu"));
        github.Click += (_, _) => Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/wenyen0214/UniversalQuickPreview",
            UseShellExecute = true
        });

        var exit = new Forms.ToolStripMenuItem(_localization.GetString("ExitMenu"));
        exit.Click += (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty);

        menu.Items.AddRange([
            title,
            new Forms.ToolStripSeparator(),
            enabled,
            settings,
            language,
            logs,
            github,
            about,
            new Forms.ToolStripSeparator(),
            exit
        ]);

        _menu = menu;
        _notifyIcon.ContextMenuStrip = menu;
        _notifyIcon.Text = _localization.GetString("AppName");
        oldMenu?.Dispose();
    }

    private static Icon LoadApplicationIcon()
    {
        var resource = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Assets/AppIcon.ico"));
        if (resource is null)
        {
            return SystemIcons.Application;
        }

        using var stream = resource.Stream;
        using var icon = new Icon(stream);
        return (Icon)icon.Clone();
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        RebuildMenu();
    }
}
