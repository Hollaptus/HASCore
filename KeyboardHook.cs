using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HASCore
{
    public class KeyboardHook : IDisposable
    {
        #region WinAPI Imports
        private const Int32 WH_KEYBOARD_LL = 13;
        private const IntPtr WM_KEYDOWN = 0x0100;
        private const IntPtr WM_KEYUP = 0x0101;
        private const IntPtr WM_SYSKEYDOWN = 0x0104;
        private const IntPtr WM_SYSKEYUP = 0x0105;
        
        private readonly LowLevelKeyboardProc _proc;
        private delegate IntPtr LowLevelKeyboardProc(Int32 nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lpModuleName"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(String lpModuleName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idHook"></param>
        /// <param name="lpfn"></param>
        /// <param name="hMod"></param>
        /// <param name="dwThreadId"></param>
        /// <returns></returns>
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(
            Int32 idHook,
            LowLevelKeyboardProc lpfn,
            IntPtr hMod,
            UInt32 dwThreadId
        );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hhk"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hhk"></param>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(
            IntPtr hhk,
            Int32 nCode,
            IntPtr wParam, 
            IntPtr lParam
        );
        #endregion

        public event EventHandler<Keys>? KeyDown;
        public event EventHandler<Keys>? KeyUp;
        private IntPtr _hookID = IntPtr.Zero;
        private readonly HashSet<Keys> _pressedKeys = [];

        public KeyboardHook()
        {
            _proc = HookCallback;
        }

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
                throw new InvalidOperationException("Failed to establish keyboard hook thorugh WinAPI.");
        }


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

        public void Stop()
        {
            if (_hookID == IntPtr.Zero)
                return;

            UnhookWindowsHookEx(_hookID);
            _hookID = IntPtr.Zero;
            _pressedKeys.Clear();
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
    }
}