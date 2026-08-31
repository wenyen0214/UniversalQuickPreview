using UniversalQuickPreview.Core.Models;

namespace UniversalQuickPreview.Core.Interfaces;

public interface IGlobalShortcutService : IDisposable
{
    event EventHandler<PreviewShortcutPressedEventArgs>? ShortcutPressed;

    bool IsEnabled { get; set; }

    PreviewShortcutGesture Gesture { get; set; }

    void Start();
}
