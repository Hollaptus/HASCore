using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HASCore.Keyboard;

/// Description
/// <summary>
///     Provides a low‑level global keyboard hook that captures key press and release events.
/// </summary>
/// <remarks>
///     This class wraps the Windows <c>SetWindowsHookEx</c> API to install a
///     <c>WH_KEYBOARD_LL</c> hook. It raises <see cref="KeyDown"/> and <see cref="KeyUp"/>
///     events for each physical key event, and maintains a set of currently pressed keys
///     to avoid duplicate events.
/// </remarks>
public class KeyboardHook : IDisposable
{
    #region WinAPI Constants and Imports

    // Hook type for low-level keyboard events.
    private const Int32 WH_KEYBOARD_LL  = 13;
    // Windows message constants.
    private const IntPtr WM_KEYDOWN     = 0x0100;
    private const IntPtr WM_KEYUP       = 0x0101;
    private const IntPtr WM_SYSKEYDOWN  = 0x0104;
    private const IntPtr WM_SYSKEYUP    = 0x0105;

    /// Description
    /// <summary>
    ///     Delegate for the low-level keyboard hook callback.
    /// </summary>
    /// <param name="nCode">
    ///     A hook code. If less than zero, the hook procedure must pass the message
    ///     to <see cref="CallNextHookEx"/> without further processing.
    /// </param>
    /// <param name="wParam">
    ///     The identifier of the keyboard message (WM_KEYDOWN, WM_KEYUP, etc.).
    /// </param>
    /// <param name="lParam">
    ///     A pointer to a <c>KBDLLHOOKSTRUCT</c> structure that contains details
    ///     about the key event.
    /// </param>
    /// <returns>
    ///     If <paramref name="nCode"/> is less than zero, must return the value
    ///     from <see cref="CallNextHookEx"/>. Otherwise, can return a nonzero value
    ///     to prevent the system from passing the message to the target window.
    /// </returns>
    private delegate IntPtr LowLevelKeyboardProc(Int32 nCode, IntPtr wParam, IntPtr lParam);

    private readonly LowLevelKeyboardProc _proc;

    /// Description
    /// <summary>
    ///     Retrieves a module handle for the specified module name.
    /// </summary>
    /// <remarks>
    ///     This function is used to obtain the base address of the current process's
    ///     main module, which is required when installing a low‑level hook with
    ///     <see cref="SetWindowsHookEx"/>.
    /// </remarks>
    /// <param name="lpModuleName">
    ///     The name of the module (e.g., "HASCore.exe"). Passing <see cref="null"/>
    ///     returns the handle of the calling process.
    /// </param>
    /// <returns>
    ///     A handle to the specified module, or <see cref="IntPtr.Zero"/> on failure.
    /// </returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(String lpModuleName);

    /// Description
    /// <summary>
    ///     Installs a hook procedure into the Windows hook chain.
    /// </summary>
    /// <remarks>
    ///     This function sets a low‑level keyboard hook (<c>WH_KEYBOARD_LL</c>)
    ///     that monitors keyboard input globally.
    /// </remarks>
    /// <param name="idHook">
    ///     The type of hook to install (e.g., <c>WH_KEYBOARD_LL</c>).
    /// </param>
    /// <param name="lpfn">
    ///     A pointer to the hook procedure (<see cref="LowLevelKeyboardProc"/>).
    /// </param>
    /// <param name="hMod">
    ///     A handle to the module containing the hook procedure.
    /// </param>
    /// <param name="dwThreadId">
    ///     The identifier of the thread with which the hook is to be associated.
    ///     For low‑level global hooks, this must be <c>0</c>.
    /// </param>
    /// <returns>
    ///     A handle to the hook if successful; otherwise, <see cref="IntPtr.Zero"/>.
    /// </returns>
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(
        Int32 idHook,
        LowLevelKeyboardProc lpfn,
        IntPtr hMod,
        UInt32 dwThreadId
    );

    /// Description
    /// <summary>
    ///     Removes a hook installed by <see cref="SetWindowsHookEx"/>.
    /// </summary>
    /// <param name="hhk">The handle to the hook to remove.</param>
    /// <returns>
    ///     <c>true</c> if the hook was successfully removed; otherwise, <c>false</c>.
    /// </returns>
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    /// Description
    /// <summary>
    ///     Passes the hook information to the next hook procedure in the chain.
    /// </summary>
    /// <remarks>
    ///     This function must be called by the hook procedure for each message,
    ///     unless the hook procedure has handled the message and wants to block it.
    /// </remarks>
    /// <param name="hhk">The handle to the current hook.</param>
    /// <param name="nCode">The hook code passed to the hook procedure.</param>
    /// <param name="wParam">The <c>wParam</c> value passed to the hook procedure.</param>
    /// <param name="lParam">The <c>lParam</c> value passed to the hook procedure.</param>
    /// <returns>
    ///     The return value from the next hook procedure in the chain.
    /// </returns>
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(
        IntPtr hhk,
        Int32 nCode,
        IntPtr wParam,
        IntPtr lParam
    );

    #endregion

    /// Description
    /// <summary>
    ///     Occurs when a key is pressed (down event).
    /// </summary>
    /// <remarks>
    ///     The event provides the virtual key code as a <see cref="Keys"/> value.
    ///     The event is raised only once per physical key down, even if the key
    ///     is held down (auto‑repeat is filtered).
    /// </remarks>
    public event EventHandler<Keys>? KeyDown;

    /// Description
    /// <summary>
    ///     Occurs when a key is released (up event).
    /// </summary>
    /// <remarks>
    ///     The event provides the virtual key code as a <see cref="Keys"/> value.
    ///     It is raised exactly once for each corresponding <see cref="KeyDown"/> event.
    /// </remarks>
    public event EventHandler<Keys>? KeyUp;

    private IntPtr _hookID = IntPtr.Zero;
    private readonly HashSet<Keys> _pressedKeys = [];

    /// Description
    /// <summary>
    ///     Initializes a new instance of the <see cref="KeyboardHook"/> class.
    /// </summary>
    /// <remarks>
    ///     The hook is not started automatically. Call <see cref="Start"/> to begin capturing.
    /// </remarks>
    public KeyboardHook()
    {
        _proc = HookCallback;
    }

    /// Description
    /// <summary>
    ///     Starts the global keyboard hook.
    /// </summary>
    /// <remarks>
    ///     This method installs the low‑level hook using <see cref="SetWindowsHookEx"/>.
    ///     If the hook is already running, the call does nothing.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the hook could not be installed (e.g., insufficient permissions).
    /// </exception>
    public void Start()
    {
        if (_hookID != IntPtr.Zero)
            return;

        using (Process curProcess = Process.GetCurrentProcess())
        if (curProcess.MainModule is not null)
        {
            using ProcessModule curModule = curProcess.MainModule;
            _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(curModule.ModuleName), 0);
        }

        if (_hookID == IntPtr.Zero)
            throw new InvalidOperationException("Failed to establish keyboard hook through WinAPI.");
    }

    // The low-level keyboard hook callback.
    private IntPtr HookCallback(Int32 nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            Int32 vkCode = Marshal.ReadInt32(lParam);
            Keys key = (Keys)vkCode;

            if (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN)
            {
                if (_pressedKeys.Add(key))
                    KeyDown?.Invoke(this, key);
            }
            else if (wParam == WM_KEYUP || wParam == WM_SYSKEYUP)
            {
                if (_pressedKeys.Remove(key))
                    KeyUp?.Invoke(this, key);
            }
        }

        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    /// Description
    /// <summary>
    ///     Stops the global keyboard hook and releases resources.
    /// </summary>
    /// <remarks>
    ///     After stopping, the hook will no longer capture events. The <see cref="KeyDown"/>
    ///     and <see cref="KeyUp"/> events will no longer be raised. This method also clears
    ///     the internal pressed‑keys set.
    /// </remarks>
    public void Stop()
    {
        if (_hookID == IntPtr.Zero)
            return;

        UnhookWindowsHookEx(_hookID);
        _hookID = IntPtr.Zero;
        _pressedKeys.Clear();
    }

    /// Description
    /// <summary>
    ///     Implements <see cref="IDisposable"/> to release the hook resources.
    /// </summary>
    /// <remarks>
    ///     Calls <see cref="Stop"/> and suppresses finalization.
    /// </remarks>
    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}