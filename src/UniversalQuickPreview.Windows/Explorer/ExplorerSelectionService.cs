using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using UniversalQuickPreview.Core.Interfaces;
using UniversalQuickPreview.Core.Models;
using UniversalQuickPreview.Windows.Interop;

namespace UniversalQuickPreview.Windows.Explorer;

public sealed class ExplorerSelectionService : IExplorerSelectionService
{
    private static readonly HashSet<string> ExplorerWindowClasses = new(StringComparer.Ordinal)
    {
        "CabinetWClass",
        "ExploreWClass"
    };

    public ExplorerSelection? GetCurrentSelection()
    {
        var foregroundWindow = NativeMethods.GetForegroundWindow();
        if (foregroundWindow == nint.Zero || !ExplorerWindowClasses.Contains(GetClassName(foregroundWindow)))
        {
            return null;
        }

        object? shell = null;
        object? shellWindows = null;

        try
        {
            var shellType = Type.GetTypeFromProgID("Shell.Application");
            if (shellType is null)
            {
                return null;
            }

            shell = Activator.CreateInstance(shellType);
            if (shell is null)
            {
                return null;
            }

            shellWindows = ((dynamic)shell).Windows();
            var windowCount = Convert.ToInt32(((dynamic)shellWindows).Count);

            for (var index = 0; index < windowCount; index++)
            {
                object? browserWindow = null;
                try
                {
                    browserWindow = ((dynamic)shellWindows).Item(index);
                    if (browserWindow is null || Convert.ToInt64(((dynamic)browserWindow).HWND) != foregroundWindow.ToInt64())
                    {
                        continue;
                    }

                    return ReadSelection(browserWindow!, foregroundWindow);
                }
                catch (COMException)
                {
                    // Explorer windows can disappear while ShellWindows is being enumerated.
                }
                finally
                {
                    ReleaseComObject(browserWindow);
                }
            }
        }
        catch (COMException)
        {
            return null;
        }
        finally
        {
            ReleaseComObject(shellWindows);
            ReleaseComObject(shell);
        }

        return null;
    }

    private static ExplorerSelection? ReadSelection(object browserWindow, nint foregroundWindow)
    {
        object? document = null;
        object? folder = null;
        object? folderSelf = null;
        object? selectedItems = null;

        try
        {
            document = ((dynamic)browserWindow).Document;
            folder = ((dynamic)document).Folder;
            folderSelf = ((dynamic)folder).Self;
            selectedItems = ((dynamic)document).SelectedItems();

            var folderPath = Convert.ToString(((dynamic)folderSelf).Path) ?? string.Empty;
            var count = Convert.ToInt32(((dynamic)selectedItems).Count);
            var paths = new List<string>(count);

            for (var index = 0; index < count; index++)
            {
                object? item = null;
                try
                {
                    item = ((dynamic)selectedItems).Item(index);
                    var path = Convert.ToString(((dynamic)item).Path);
                    if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                    {
                        paths.Add(Path.GetFullPath(path));
                    }
                }
                finally
                {
                    ReleaseComObject(item);
                }
            }

            return paths.Count == 0
                ? null
                : new ExplorerSelection(folderPath, paths, 0, foregroundWindow);
        }
        finally
        {
            ReleaseComObject(selectedItems);
            ReleaseComObject(folderSelf);
            ReleaseComObject(folder);
            ReleaseComObject(document);
        }
    }

    private static string GetClassName(nint windowHandle)
    {
        var buffer = new StringBuilder(256);
        return NativeMethods.GetClassName(windowHandle, buffer, buffer.Capacity) > 0
            ? buffer.ToString()
            : string.Empty;
    }

    private static void ReleaseComObject(object? value)
    {
        if (value is not null && Marshal.IsComObject(value))
        {
            Marshal.FinalReleaseComObject(value);
        }
    }
}
