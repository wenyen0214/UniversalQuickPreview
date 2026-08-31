namespace UniversalQuickPreview.Core.Models;

public sealed record ExplorerSelection(
    string FolderPath,
    IReadOnlyList<string> SelectedFiles,
    IReadOnlyList<string> FolderFiles,
    int SelectedIndex,
    nint ExplorerWindowHandle);
