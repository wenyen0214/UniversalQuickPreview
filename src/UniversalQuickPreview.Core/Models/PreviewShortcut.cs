namespace UniversalQuickPreview.Core.Models;

public enum PreviewShortcut
{
    Space,
    Escape
}
public sealed class PreviewShortcutPressedEventArgs(PreviewShortcut shortcut) : EventArgs
{
    public PreviewShortcut Shortcut { get; } = shortcut;

    public bool Handled { get; set; }
}
