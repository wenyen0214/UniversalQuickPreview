using System.ComponentModel;
using System.Runtime.InteropServices;
using UniversalQuickPreview.Windows.Interop;

namespace UniversalQuickPreview.Windows.Hotkeys;

public sealed class OutsideClickWatcher : IDisposable
{
    private readonly NativeMethods.LowLevelMouseProc _callback;
    private nint _hookHandle;
    private nint _targetWindow;
    private bool _disposed;

    public OutsideClickWatcher()
    {
        _callback = HookCallback;
    }

    public event EventHandler? OutsideClicked;

    public void Start(nint targetWindow)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _targetWindow = targetWindow;
        if (_hookHandle != nint.Zero)
        {
            return;
        }

        _hookHandle = NativeMethods.SetWindowsMouseHookEx(
            NativeMethods.WhMouseLl,
            _callback,
            NativeMethods.GetModuleHandle(null),
            0);
        if (_hookHandle == nint.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to install the mouse hook.");
        }
    }

    public void Stop()
    {
        if (_hookHandle == nint.Zero)
        {
            return;
        }

        NativeMethods.UnhookWindowsHookEx(_hookHandle);
        _hookHandle = nint.Zero;
        _targetWindow = nint.Zero;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Stop();
        _disposed = true;
    }

    private nint HookCallback(int code, nint wParam, nint lParam)
    {
        if (code >= 0 && IsButtonDown(wParam.ToInt32()) && _targetWindow != nint.Zero)
        {
            try
            {
                var data = Marshal.PtrToStructure<NativeMethods.MouseHookData>(lParam);
                var hitWindow = NativeMethods.WindowFromPoint(data.Location);
                var hitRootOwner = hitWindow == nint.Zero
                    ? nint.Zero
                    : NativeMethods.GetAncestor(hitWindow, NativeMethods.GaRootOwner);
                var targetRootOwner = NativeMethods.GetAncestor(_targetWindow, NativeMethods.GaRootOwner);

                if (hitRootOwner == nint.Zero || hitRootOwner != targetRootOwner)
                {
                    OutsideClicked?.Invoke(this, EventArgs.Empty);
                }
            }
            catch
            {
                // Never let an input-hook integration failure terminate the process.
            }
        }

        return NativeMethods.CallNextHookEx(_hookHandle, code, wParam, lParam);
    }

    private static bool IsButtonDown(int message)
    {
        return message is NativeMethods.WmLeftButtonDown or
            NativeMethods.WmRightButtonDown or
            NativeMethods.WmMiddleButtonDown or
            NativeMethods.WmXButtonDown;
    }
}
