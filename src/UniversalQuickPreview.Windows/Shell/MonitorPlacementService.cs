using UniversalQuickPreview.Windows.Interop;

namespace UniversalQuickPreview.Windows.Shell;

public static class MonitorPlacementService
{
    public static void CenterAboveOwner(nint previewWindow, nint ownerWindow)
    {
        var monitor = NativeMethods.MonitorFromWindow(ownerWindow, NativeMethods.MonitorDefaultToNearest);
        var monitorInfo = new NativeMethods.MonitorInfo { Size = System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.MonitorInfo>() };

        if (monitor == nint.Zero ||
            !NativeMethods.GetMonitorInfo(monitor, ref monitorInfo) ||
            !NativeMethods.GetWindowRect(previewWindow, out var currentBounds))
        {
            return;
        }

        var workArea = monitorInfo.WorkArea;
        var availableWidth = workArea.Right - workArea.Left;
        var availableHeight = workArea.Bottom - workArea.Top;
        var currentWidth = currentBounds.Right - currentBounds.Left;
        var currentHeight = currentBounds.Bottom - currentBounds.Top;
        var dpi = NativeMethods.GetDpiForWindow(ownerWindow);
        var scale = dpi == 0 ? 1d : dpi / 96d;

        var width = Math.Min((int)(760 * scale), (int)(availableWidth * 0.8));
        var height = Math.Min((int)(320 * scale), (int)(availableHeight * 0.8));
        width = Math.Max(Math.Min(currentWidth, width), Math.Min((int)(560 * scale), availableWidth));
        height = Math.Max(Math.Min(currentHeight, height), Math.Min((int)(260 * scale), availableHeight));

        var left = workArea.Left + (availableWidth - width) / 2;
        var top = workArea.Top + (availableHeight - height) / 2;

        NativeMethods.SetWindowPos(
            previewWindow,
            NativeMethods.HwndTopmost,
            left,
            top,
            width,
            height,
            NativeMethods.SwpNoActivate | NativeMethods.SwpShowWindow);
    }
}
