using UniversalQuickPreview.Windows.Interop;

namespace UniversalQuickPreview.Windows.Shell;

public static class MonitorPlacementService
{
    public static void CenterAboveOwner(
        nint previewWindow,
        nint ownerWindow,
        int desiredWidth,
        int desiredHeight)
    {
        var monitor = NativeMethods.MonitorFromWindow(ownerWindow, NativeMethods.MonitorDefaultToNearest);
        var monitorInfo = new NativeMethods.MonitorInfo { Size = System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.MonitorInfo>() };

        if (monitor == nint.Zero ||
            !NativeMethods.GetMonitorInfo(monitor, ref monitorInfo))
        {
            return;
        }

        var workArea = monitorInfo.WorkArea;
        var availableWidth = workArea.Right - workArea.Left;
        var availableHeight = workArea.Bottom - workArea.Top;
        var dpi = NativeMethods.GetDpiForWindow(ownerWindow);
        var scale = dpi == 0 ? 1d : dpi / 96d;

        var maximumWidth = Math.Max(1, (int)(availableWidth * 0.8));
        var maximumHeight = Math.Max(1, (int)(availableHeight * 0.8));
        var minimumWidth = Math.Min((int)(560 * scale), maximumWidth);
        var minimumHeight = Math.Min((int)(360 * scale), maximumHeight);
        var width = Math.Clamp((int)(desiredWidth * scale), minimumWidth, maximumWidth);
        var height = Math.Clamp((int)(desiredHeight * scale), minimumHeight, maximumHeight);

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
