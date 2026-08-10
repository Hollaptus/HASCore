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

    #region Events

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

    #endregion

    #region Private Fields

    private readonly LowLevelKeyboardProc _lowLevelProc;

    /// Description
    /// <summary>
    ///     Handle to the installed Windows hook.
    /// </summary>
    private IntPtr _hookHandle = IntPtr.Zero;

    /// Description
    /// <summary>
    ///     Set of keys currently held down (used to avoid duplicate events).
    /// </summary>
    private readonly HashSet<Keys> _pressedKeys = [];

    /// Description
    /// <summary>
    ///     Holds a deferred <c>LControlKey</c> down event while waiting to see
    ///     if the next event is <c>RMenu</c> (Right Alt).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This field is used by the Chromium-style AltGr filter to distinguish
    ///         a real <c>LControlKey</c> press from a synthetic one generated by
    ///         Windows when the AltGr key is pressed.
    ///     </para>
    ///     <para>
    ///         When a <c>LControlKey</c> down event is received, it is stored here
    ///         instead of being raised immediately. If the very next key event is
    ///         <c>RMenu</c> (Right Alt), the stored key is discarded as synthetic.
    ///         If any other key arrives first, the stored key is raised as a real
    ///         <c>LControlKey</c> press.
    ///     </para>
    ///     <para>
    ///         This field is set to <c>null</c> when no event is pending or when
    ///         the deferred event has been resolved (either sent or discarded).
    ///     </para>
    /// </remarks>
    private Keys? _deferredControlKey = null;

    #endregion

    #region Constructor

    /// Description
    /// <summary>
    ///     Initializes a new instance of the <see cref="KeyboardHook"/> class.
    /// </summary>
    /// <remarks>
    ///     The hook is not started automatically. Call <see cref="Start"/> to begin capturing.
    /// </remarks>
    public KeyboardHook()
    {
        _lowLevelProc = HookCallback;
    }

    #endregion

    #region Public Methods

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
        if (_hookHandle != IntPtr.Zero)
            return;

        using (Process curProcess = Process.GetCurrentProcess())
        if (curProcess.MainModule is not null)
        {
            using ProcessModule curModule = curProcess.MainModule;
            _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _lowLevelProc, GetModuleHandle(curModule.ModuleName), 0);
        }

        if (_hookHandle == IntPtr.Zero)
            throw new InvalidOperationException("Failed to establish keyboard hook through WinAPI.");
    }

    /// Description
    /// <summary>
    ///     Stops the global keyboard hook and releases resources.
    /// </summary>
    /// <remarks>
    ///     After stopping, the hook will no longer capture events. The <see cref="KeyDown"/>
    ///     and <see cref="KeyUp"/> events will no longer be raised. This method also clears
    ///     the internal pressed‑keys set and any deferred state.
    /// </remarks>
    public void Stop()
    {
        if (_hookHandle == IntPtr.Zero)
            return;

        UnhookWindowsHookEx(_hookHandle);
        _hookHandle = IntPtr.Zero;
        _pressedKeys.Clear();
        _deferredControlKey = null;
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

    #endregion

    #region Private Methods

    /// Description
    /// <summary>
    ///     The low-level keyboard hook callback that processes key events.
    /// </summary>
    /// <param name="nCode">
    ///     The hook code. If less than zero, the hook procedure must pass the message
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
    ///     If <paramref name="nCode"/> is less than zero, returns the value from
    ///     <see cref="CallNextHookEx"/>. Otherwise, returns a nonzero value to block
    ///     the event, or zero to allow it.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         This callback implements the Chromium-style AltGr filter to handle the
    ///         synthetic <c>LControlKey</c> event that Windows generates when the
    ///         AltGr (Right Alt on international keyboards) key is pressed.
    ///     </para>
    ///     <para>
    ///         <b>How the filter works:</b>
    ///         <list type="number">
    ///             <item>
    ///                 <description>
    ///                     When <c>LControlKey</c> is pressed, the event is <b>deferred</b>
    ///                     (held back) rather than being sent immediately.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     If the next event is <c>RMenu</c> (Right Alt), the deferred
    ///                     <c>LControlKey</c> is identified as a synthetic AltGr event
    ///                     and is <b>discarded</b>.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     If any other key event arrives first, the deferred
    ///                     <c>LControlKey</c> is identified as a real physical key press
    ///                     and is <b>sent</b> to subscribers.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         <b>References:</b>
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                     Chromium – https://codereview.chromium.org/1416233002
    ///                     (Documents the AltGr keydown/keyup cycle)
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     QEMU – https://github.com/ispras/qemu/commit/2df9f57
    ///                     ("removes the extra left control key up/down input events")
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     SDL – https://github.com/libsdl-org/SDL/commit/f62a1be
    ///                     ("Fix spurious LCtrl on RAlt key pressed")
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     Zed Editor – https://github.com/zed-industries/zed/commit/15580a8
    ///                     ("AltGr is emulated as Right Alt + synthetic Left Ctrl")
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     VirtualBox – https://github.com/VirtualBox/virtualbox/commit/39ea180
    ///                     ("FE/Qt: win: improved AltGr handling.")
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    private IntPtr HookCallback(Int32 nCode, IntPtr wParam, IntPtr lParam)
    {
        // -------------------------------------------------------------------------
        //  REMARK – AltGr Filter Implementation History
        // -------------------------------------------------------------------------
        // This Chromium-style approach was chosen after testing several alternatives.
        //
        // The QEMU and SDL-like approaches were tried but did not reliably filter
        // the synthetic LControlKey event. Sometimes the LControlKey appears when
        // pressing Right Alt + Shift, sometimes it doesn't. Most of the time,
        // pressing Right Alt after other keys (e.g., D1 -> Right Shift -> Right Alt)
        // worked, but not the other way around.
        //
        // After extensive testing, the Chromium-like approach worked best.
        // The LControlKey did not appear even once, so this implementation is
        // kept as the final solution. The next best approach was QEMU's scan code check.
        //
        // For reference, here is the QEMU approach that was tested (rejected):
        //
        //   private IntPtr HookCallback(Int32 nCode, IntPtr wParam, IntPtr lParam)
        //   {
        //       if (nCode >= 0)
        //       {
        //           Int32 scanCode = (Marshal.ReadInt32(lParam) >> 16) & 0xFF;
        //           Int32 vkCode = Marshal.ReadInt32(lParam);
        //           Keys key = (Keys)vkCode;
        //           
        //           // QEMU's approach: check if bit 9 of the scan code is set.
        ///          // This identifies the synthetic LeftControl from AltGr.
        //           if (key == Keys.LControlKey && (scanCode & 0x200) != 0)
        //           {
        //               return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        //           }
        //
        //           if (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN)
        //           {
        //               if (_pressedKeys.Add(key))
        //                   KeyDown?.Invoke(this, key);
        //           }
        //           else if (wParam == WM_KEYUP || wParam == WM_SYSKEYUP)
        //           {
        //               if (_pressedKeys.Remove(key))
        //                   KeyUp?.Invoke(this, key);
        //           }
        //       }
        //
        //       return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        //   }
        // -------------------------------------------------------------------------

        // If the hook code is less than zero, we must pass the message along
        // without any processing.
        if (nCode < 0)
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);

        // Extract the virtual key code from the KBDLLHOOKSTRUCT structure.
        Int32 vkCode = Marshal.ReadInt32(lParam);
        Keys key = (Keys)vkCode;

        // Determine whether this is a key down or key up event.
        Boolean isKeyDown = wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN;
        Boolean isKeyUp   = wParam == WM_KEYUP   || wParam == WM_SYSKEYUP;

        // Handle key down events
        if (isKeyDown)
        {
            // LControlKey down -> defer it.
            // We wait to see if the next event is RMenu (Right Alt).
            if (key == Keys.LControlKey)
            {
                _deferredControlKey = key;
                return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
            }

            // RMenu (Right Alt) down -> if we had a deferred LControlKey,
            // that LControlKey was synthetic (AltGr). Discard it and process RMenu.
            if (key == Keys.RMenu)
            {
                // The deferred LControlKey is synthetic - discard it.
                if (_deferredControlKey == Keys.LControlKey)
                    _deferredControlKey = null;

                // Process RMenu as a genuine key press.
                RaiseKeyEvent(key, true);
                return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
            }

            // Any other key down -> if we have a deferred LControlKey,
            // it was a real key press, send it.
            if (_deferredControlKey == Keys.LControlKey)
            {
                Keys deferredKey = _deferredControlKey.Value;
                _deferredControlKey = null;
                RaiseKeyEvent(deferredKey, true);
            }

            // Process the current key down event.
            RaiseKeyEvent(key, true);
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        // Handle key up events
        if (isKeyUp)
        {
            // LControlKey up -> if we had a deferred LControlKey, this is a
            // synthetic AltGr release, we discard it. Otherwise, it's a real release.
            if (key == Keys.LControlKey)
            {
                if (_deferredControlKey == Keys.LControlKey)
                {
                    // Synthetic AltGr release - just clear the deferred state.
                    _deferredControlKey = null;
                    return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
                }

                // Real LControlKey release.
                RaiseKeyEvent(key, false);
                return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
            }

            // RMenu (Right Alt) up -> process normally.
            if (key == Keys.RMenu)
            {
                RaiseKeyEvent(key, false);
                return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
            }

            // Any other key up -> process normally.
            RaiseKeyEvent(key, false);
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        // This should be unreachable.
        return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    /// Description
    /// <summary>
    ///     Raises the appropriate key event (down or up) if the key state has changed.
    /// </summary>
    /// <param name="key">The key to raise the event for.</param>
    /// <param name="isDown">
    ///     <c>true</c> to raise a <see cref="KeyDown"/> event;
    ///     <c>false</c> to raise a <see cref="KeyUp"/> event.
    /// </param>
    /// <remarks>
    ///     This method checks whether the key was already in the pressed set before
    ///     raising the event, preventing duplicate events and ensuring that only
    ///     state changes trigger notifications.
    /// </remarks>
    private void RaiseKeyEvent(Keys key, Boolean isDown)
    {
        if (isDown)
        {
            // Only raise KeyDown if the key wasn't already pressed.
            if (_pressedKeys.Add(key))
                KeyDown?.Invoke(this, key);
        }
        else
        {
            // Only raise KeyUp if the key was pressed.
            if (_pressedKeys.Remove(key))
                KeyUp?.Invoke(this, key);
        }
    }

    #endregion
}