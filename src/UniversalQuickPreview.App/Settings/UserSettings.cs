using UniversalQuickPreview.Core.Models;

namespace UniversalQuickPreview.App.Settings;

public enum AppTheme
{
    System,
    Light,
    Dark
}

public enum PreviewWindowSize
{
    Compact,
    Medium,
    Large
}

public sealed record UserSettings
{
    public string Language { get; init; } = string.Empty;

    public bool IsEnabled { get; init; } = true;

    public bool LaunchAtStartup { get; init; }

    public PreviewShortcutGesture Shortcut { get; init; } = PreviewShortcutGesture.Space;

    public AppTheme Theme { get; init; } = AppTheme.System;

    public PreviewWindowSize WindowSize { get; init; } = PreviewWindowSize.Medium;

    public bool ShowFullPath { get; init; } = true;

    public bool CloseWhenClickingOutside { get; init; } = true;
}
