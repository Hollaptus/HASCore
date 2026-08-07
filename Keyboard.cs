using System.Runtime.InteropServices;

namespace HASCore
{
    public class Keyboard
    {
        const int INPUT_KEYBOARD = 1;
        const uint KEYEVENTF_KEYDOWN = 0x0000;
        const uint KEYEVENTF_KEYUP = 0x0002;
        const uint KEYEVENTF_SCANCODE = 0x0008;

        struct INPUT
        {
            public int type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern short GetAsyncKeyState(byte virtualKeyCode);

        internal static bool IsKeyDown(Keys keyCode)
        {
            short keyState = GetAsyncKeyState((byte)keyCode);
            return keyState < 0;
        }
        
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        static extern IntPtr GetMessageExtraInfo();

        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        const uint MAPVK_VK_TO_VSC = 0x00;

        public static bool SendKey(Keys key, bool keyDown)
        {
            INPUT[] inputs =
            [
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    u = new InputUnion
                    {
                        ki = new KEYBDINPUT()
                        {
                            wVk = 0,
                            wScan = (ushort)MapVirtualKey((uint)key, MAPVK_VK_TO_VSC),
                            dwFlags = (keyDown ? KEYEVENTF_KEYDOWN : KEYEVENTF_KEYUP) | KEYEVENTF_SCANCODE,
                            dwExtraInfo = GetMessageExtraInfo(),
                            time = keyDown ? unchecked ((uint)-1) : 0
                        }
                    }
                }
            ];

            return SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT))) > 0;
        }
    }
}
