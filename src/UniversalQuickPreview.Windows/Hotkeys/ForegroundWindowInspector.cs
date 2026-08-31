using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;
using UniversalQuickPreview.Windows.Interop;

namespace UniversalQuickPreview.Windows.Hotkeys;

internal static class ForegroundWindowInspector
{
    private static readonly HashSet<string> ExplorerWindowClasses = new(StringComparer.Ordinal)
    {
        "CabinetWClass",
        "ExploreWClass"
    };

    internal static bool IsExplorerReadyForShortcut()
    {
        var foregroundWindow = NativeMethods.GetForegroundWindow();
        if (foregroundWindow == nint.Zero || !ExplorerWindowClasses.Contains(GetClassName(foregroundWindow)))
        {
            return false;
        }

        return !IsTextInputFocused(foregroundWindow);
    }

    private static bool IsTextInputFocused(nint foregroundWindow)
    {
        var threadId = NativeMethods.GetWindowThreadProcessId(foregroundWindow, out _);
        var info = new NativeMethods.GuiThreadInfo { Size = Marshal.SizeOf<NativeMethods.GuiThreadInfo>() };

        if (!NativeMethods.GetGUIThreadInfo(threadId, ref info) || info.FocusedWindow == nint.Zero)
        {
            return false;
        }

        var className = GetClassName(info.FocusedWindow);
        if (className.Contains("Edit", StringComparison.OrdinalIgnoreCase) ||
            className.Contains("RichEdit", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        try
        {
            var focusedElement = AutomationElement.FocusedElement;
            return focusedElement?.Current.ControlType == ControlType.Edit;
        }
        catch (ElementNotAvailableException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (COMException)
        {
            return false;
        }
    }

    private static string GetClassName(nint windowHandle)
    {
        var buffer = new StringBuilder(256);
        return NativeMethods.GetClassName(windowHandle, buffer, buffer.Capacity) > 0
            ? buffer.ToString()
            : string.Empty;
    }
}
