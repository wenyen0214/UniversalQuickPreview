namespace UniversalQuickPreview.Core.Services;

public sealed class PreviewNavigationService
{
    private IReadOnlyList<string> _files = [];
    private int _index = -1;

    public string? Current => _index >= 0 && _index < _files.Count ? _files[_index] : null;

    public bool CanMovePrevious => _index > 0;

    public bool CanMoveNext => _index >= 0 && _index < _files.Count - 1;

    public void Start(IReadOnlyList<string> files, int selectedIndex)
    {
        ArgumentNullException.ThrowIfNull(files);
        _files = files;
        _index = files.Count == 0 ? -1 : Math.Clamp(selectedIndex, 0, files.Count - 1);
    }

    public string? Move(int offset)
    {
        if (_index < 0 || offset == 0)
        {
            return Current;
        }

        var target = _index + offset;
        if (target < 0 || target >= _files.Count)
        {
            return Current;
        }

        _index = target;
        return Current;
    }
}
