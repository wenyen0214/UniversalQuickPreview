using System.ComponentModel;
using System.Runtime.InteropServices;
using UniversalQuickPreview.Core.Interfaces;
using UniversalQuickPreview.Core.Models;
using UniversalQuickPreview.Windows.Interop;

namespace UniversalQuickPreview.Windows.Hotkeys;

public sealed class GlobalKeyboardHookService : IGlobalShortcutService
{
    private readonly NativeMethods.LowLevelKeyboardProc _callback;
    private readonly HashSet<uint> _pressedKeys = [];
    private nint _hookHandle;
    private bool _disposed;

    public GlobalKeyboardHookService()
    {
        _callback = HookCallback;
    }

    public event EventHandler<PreviewShortcutPressedEventArgs>? ShortcutPressed;

    public bool IsEnabled { get; set; } = true;

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_hookHandle != nint.Zero)
        {
            return;
        }

        _hookHandle = NativeMethods.SetWindowsHookEx(
            NativeMethods.WhKeyboardLl,
            _callback,
            NativeMethods.GetModuleHandle(null),
            0);

        if (_hookHandle == nint.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to install the keyboard hook.");
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_hookHandle != nint.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_hookHandle);
            _hookHandle = nint.Zero;
        }

        _pressedKeys.Clear();
        _disposed = true;
    }

    private nint HookCallback(int code, nint wParam, nint lParam)
    {
        if (code < 0 || !IsEnabled)
        {
            return NativeMethods.CallNextHookEx(_hookHandle, code, wParam, lParam);
        }

        try
        {
            var message = wParam.ToInt32();
            var hookData = Marshal.PtrToStructure<NativeMethods.KeyboardHookData>(lParam);

            if (message is NativeMethods.WmKeyUp or NativeMethods.WmSysKeyUp)
            {
                _pressedKeys.Remove(hookData.VirtualKeyCode);
                return NativeMethods.CallNextHookEx(_hookHandle, code, wParam, lParam);
            }

            if (message is not (NativeMethods.WmKeyDown or NativeMethods.WmSysKeyDown) ||
                !_pressedKeys.Add(hookData.VirtualKeyCode))
            {
                return NativeMethods.CallNextHookEx(_hookHandle, code, wParam, lParam);
            }

            PreviewShortcut? shortcut = hookData.VirtualKeyCode switch
            {
                NativeMethods.VkSpace when !AreModifierKeysPressed() && ForegroundWindowInspector.IsExplorerReadyForShortcut() => PreviewShortcut.Space,
                NativeMethods.VkEscape => PreviewShortcut.Escape,
                _ => null
            };

            if (shortcut is null)
            {
                return NativeMethods.CallNextHookEx(_hookHandle, code, wParam, lParam);
            }

            var eventArgs = new PreviewShortcutPressedEventArgs(shortcut.Value);
            ShortcutPressed?.Invoke(this, eventArgs);
            if (eventArgs.Handled)
            {
                return 1;
            }
        }
        catch
        {
            // A low-level hook must never allow an integration failure to crash the process.
        }

        return NativeMethods.CallNextHookEx(_hookHandle, code, wParam, lParam);
    }

    private static bool AreModifierKeysPressed()
    {
        return IsPressed(NativeMethods.VkShift) ||
               IsPressed(NativeMethods.VkControl) ||
               IsPressed(NativeMethods.VkMenu) ||
               IsPressed(NativeMethods.VkLeftWindows) ||
               IsPressed(NativeMethods.VkRightWindows);
    }

    private static bool IsPressed(int virtualKey)
    {
        return (NativeMethods.GetAsyncKeyState(virtualKey) & 0x8000) != 0;
    }
}
