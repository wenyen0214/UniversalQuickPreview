using UniversalQuickPreview.Core.Models;

namespace UniversalQuickPreview.Core.Interfaces;

public interface IGlobalShortcutService : IDisposable
{
    event EventHandler<PreviewShortcutPressedEventArgs>? ShortcutPressed;

    bool IsEnabled { get; set; }

    void Start();
}
