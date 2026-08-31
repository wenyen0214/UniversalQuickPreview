namespace UniversalQuickPreview.Core.Models;

public sealed record ExplorerSelection(
    string FolderPath,
    IReadOnlyList<string> SelectedFiles,
    int SelectedIndex,
    nint ExplorerWindowHandle);
