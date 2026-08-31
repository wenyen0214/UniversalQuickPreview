namespace UniversalQuickPreview.Core.Models;

public enum PreviewShortcut
{
    Space,
    Escape,
    Previous,
    Next
}

public enum PreviewShortcutGesture
{
    Space,
    AltSpace,
    ControlSpace
}
public sealed class PreviewShortcutPressedEventArgs(PreviewShortcut shortcut) : EventArgs
{
    public PreviewShortcut Shortcut { get; } = shortcut;

    public bool Handled { get; set; }
}
