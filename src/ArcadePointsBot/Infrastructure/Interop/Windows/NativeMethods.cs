//https://gist.github.com/DrustZ/640912b9d5cb745a3a56971c9bd58ac7
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Avalonia.Input;
using Avalonia.Win32.Input;

namespace ArcadePointsBot.Interop.Windows;

internal static class NativeMethods
{
    //User32 wrappers cover API's used for Mouse input
    #region User32
    // Two special bitmasks we define to be able to grab
    // shift and character information out of a VKey.
    internal const int VKeyShiftMask = 0x0100;
    internal const int VKeyCharMask = 0x00FF;

    // Various Win32 constants
    internal const int KeyeventfExtendedkey = 0x0001;
    internal const int KeyeventfKeyup = 0x0002;
    internal const int KeyeventfScancode = 0x0008;

    internal const int MouseeventfVirtualdesk = 0x4000;

    internal const int SMXvirtualscreen = 76;
    internal const int SMYvirtualscreen = 77;
    internal const int SMCxvirtualscreen = 78;
    internal const int SMCyvirtualscreen = 79;

    internal const int XButton1 = 0x0001;
    internal const int XButton2 = 0x0002;
    internal const int WheelDelta = 120;

    internal const int InputMouse = 0;
    internal const int InputKeyboard = 1;

    // Various Win32 data structures
    [StructLayout(LayoutKind.Sequential)]
    internal struct INPUT
    {
        internal int type;
        internal INPUTUNION union;
    };

    [StructLayout(LayoutKind.Explicit)]
    internal struct INPUTUNION
    {
        [FieldOffset(0)]
        internal MOUSEINPUT mouseInput;

        [FieldOffset(0)]
        internal KEYBDINPUT keyboardInput;
    };

    [StructLayout(LayoutKind.Sequential)]
    internal struct MOUSEINPUT
    {
        internal int dx;
        internal int dy;
        internal int mouseData;
        internal int dwFlags;
        internal int time;
        internal IntPtr dwExtraInfo;
    };

    [StructLayout(LayoutKind.Sequential)]
    internal struct KEYBDINPUT
    {
        internal short wVk;
        internal short wScan;
        internal int dwFlags;
        internal int time;
        internal IntPtr dwExtraInfo;
    };

    [Flags]
    internal enum SendMouseInputFlags
    {
        Move = 0x0001,
        LeftDown = 0x0002,
        LeftUp = 0x0004,
        RightDown = 0x0008,
        RightUp = 0x0010,
        MiddleDown = 0x0020,
        MiddleUp = 0x0040,
        XDown = 0x0080,
        XUp = 0x0100,
        Wheel = 0x0800,
        Absolute = 0x8000,
    };

    // Importing various Win32 APIs that we need for input
    [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
    internal static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    internal static extern int MapVirtualKey(int nVirtKey, int nMapType);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern int SendInput(int nInputs, ref INPUT mi, int cbSize);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    internal static extern short VkKeyScan(char ch);

    #endregion
}

[SupportedOSPlatform("windows")]
public static class Keyboard
{
    #region Public Members

    /// <summary>
    /// Presses down a key.
    /// </summary>
    /// <param name="key">The key to press.</param>
    public static void Press(Key key)
    {
        SendKeyboardInput(key, true);
    }

    /// <summary>
    /// Releases a key.
    /// </summary>
    /// <param name="key">The key to release.</param>
    public static void Release(Key key)
    {
        SendKeyboardInput(key, false);
    }

    /// <summary>
    /// Performs a press-and-release operation for the specified key, which is effectively equivallent to typing.
    /// </summary>
    /// <param name="key">The key to press.</param>
    public static void Type(Key key)
    {
        Press(key);
        Release(key);
    }

    #endregion

    #region Private Members

    /// <summary>
    /// Injects keyboard input into the system.
    /// </summary>
    /// <param name="key">Indicates the key pressed or released. Can be one of the constants defined in the Key enum.</param>
    /// <param name="press">True to inject a key press, false to inject a key release.</param>
    private static void SendKeyboardInput(Key key, bool press)
    {
        NativeMethods.INPUT ki = new NativeMethods.INPUT();
        ki.type = NativeMethods.InputKeyboard;
        ki.union.keyboardInput.wVk = (short)KeyInterop.VirtualKeyFromKey(key);
        ki.union.keyboardInput.wScan = (short)
            NativeMethods.MapVirtualKey(ki.union.keyboardInput.wVk, 0);

        int dwFlags = 0;

        if (ki.union.keyboardInput.wScan > 0)
        {
            dwFlags |= NativeMethods.KeyeventfScancode;
        }

        if (!press)
        {
            dwFlags |= NativeMethods.KeyeventfKeyup;
        }

        ki.union.keyboardInput.dwFlags = dwFlags;

        if (ExtendedKeys.Contains(key))
        {
            ki.union.keyboardInput.dwFlags |= NativeMethods.KeyeventfExtendedkey;
        }

        ki.union.keyboardInput.time = 0;
        ki.union.keyboardInput.dwExtraInfo = new IntPtr(0);

        if (NativeMethods.SendInput(1, ref ki, Marshal.SizeOf(ki)) == 0)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    // From the SDK:
    // The extended-key flag indicates whether the keystroke message originated from one of
    // the additional keys on the enhanced keyboard. The extended keys consist of the ALT and
    // CTRL keys on the right-hand side of the keyboard; the INS, DEL, HOME, END, PAGE UP,
    // PAGE DOWN, and arrow keys in the clusters to the left of the numeric keypad; the NUM LOCK
    // key; the BREAK (CTRL+PAUSE) key; the PRINT SCRN key; and the divide (/) and ENTER keys in
    // the numeric keypad. The extended-key flag is set if the key is an extended key.
    //
    // - docs appear to be incorrect. Use of Spy++ indicates that break is not an extended key.
    // Also, menu key and windows keys also appear to be extended.
    private static readonly Key[] ExtendedKeys = new Key[]
    {
        Key.RightAlt,
        Key.RightCtrl,
        Key.NumLock,
        Key.Insert,
        Key.Delete,
        Key.Home,
        Key.End,
        Key.Prior,
        Key.Next,
        Key.Up,
        Key.Down,
        Key.Left,
        Key.Right,
        Key.Apps,
        Key.RWin,
        Key.LWin
    };
    // Note that there are no distinct values for the following keys:
    // numpad divide
    // numpad enter

    #endregion
}

[SupportedOSPlatform("windows")]
public static class Mouse
{
    public static void Click(MouseButton mouseButton)
    {
        Down(mouseButton);
        Up(mouseButton);
    }

    public static void DoubleClick(MouseButton mouseButton)
    {
        Click(mouseButton);
        Click(mouseButton);
    }

    public static void Down(MouseButton mouseButton)
    {
        switch (mouseButton)
        {
            case MouseButton.Left:
                SendMouseButtonInput(NativeMethods.SendMouseInputFlags.LeftDown);
                break;
            case MouseButton.Right:
                SendMouseButtonInput(NativeMethods.SendMouseInputFlags.RightDown);
                break;
            case MouseButton.Middle:
                SendMouseButtonInput(NativeMethods.SendMouseInputFlags.MiddleDown);
                break;
            case MouseButton.XButton1:
                SendMouseButtonInput(
                    NativeMethods.SendMouseInputFlags.XDown,
                    NativeMethods.XButton1
                );
                break;
            case MouseButton.XButton2:
                SendMouseButtonInput(
                    NativeMethods.SendMouseInputFlags.XDown,
                    NativeMethods.XButton2
                );
                break;
            default:
                throw new InvalidOperationException("Unsupported MouseButton input.");
        }
    }

    public static void Up(MouseButton mouseButton)
    {
        switch (mouseButton)
        {
            case MouseButton.Left:
                SendMouseButtonInput(NativeMethods.SendMouseInputFlags.LeftUp);
                break;
            case MouseButton.Right:
                SendMouseButtonInput(NativeMethods.SendMouseInputFlags.RightUp);
                break;
            case MouseButton.Middle:
                SendMouseButtonInput(NativeMethods.SendMouseInputFlags.MiddleUp);
                break;
            case MouseButton.XButton1:
                SendMouseButtonInput(NativeMethods.SendMouseInputFlags.XUp, NativeMethods.XButton1);
                break;
            case MouseButton.XButton2:
                SendMouseButtonInput(NativeMethods.SendMouseInputFlags.XUp, NativeMethods.XButton2);
                break;
            default:
                throw new InvalidOperationException("Unsupported MouseButton input.");
        }
    }

    private static void SendMouseButtonInput(NativeMethods.SendMouseInputFlags flags, int data = 0)
    {
        var mouseInput = CreateMouseInput(flags, data);

        if (NativeMethods.SendInput(1, ref mouseInput, Marshal.SizeOf(mouseInput)) == 0)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    private static NativeMethods.INPUT CreateMouseInput(
        NativeMethods.SendMouseInputFlags flags,
        int data = 0
    )
    {
        int intflags = (int)flags;

        if ((intflags & (int)NativeMethods.SendMouseInputFlags.Absolute) != 0)
        {
            intflags |= NativeMethods.MouseeventfVirtualdesk;
        }
        NativeMethods.INPUT mi = new() { type = NativeMethods.InputMouse };
        mi.union.mouseInput.dx = 0;
        mi.union.mouseInput.dy = 0;
        mi.union.mouseInput.mouseData = data;
        mi.union.mouseInput.dwFlags = intflags;
        mi.union.mouseInput.time = 0;
        mi.union.mouseInput.dwExtraInfo = new IntPtr(0);
        return mi;
    }
}
