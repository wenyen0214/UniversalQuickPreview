using UniversalQuickPreview.Core.Models;

namespace UniversalQuickPreview.Core.Interfaces;

public interface IExplorerSelectionService
{
    ExplorerSelection? GetCurrentSelection();
}
