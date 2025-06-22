using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace DevToolbox.Wpf.Interop;

internal class NativeMethods
{
    public static Version GetTrueOSVersion()
    {
        var osvi = new OSVERSIONINFOEX { dwOSVersionInfoSize = Marshal.SizeOf<OSVERSIONINFOEX>() };
        if (Ntdll.RtlGetVersion(ref osvi) == 0)
            return new Version(osvi.dwMajorVersion, osvi.dwMinorVersion, osvi.dwBuildNumber);
        return Environment.OSVersion.Version;
    }

    public static System.Drawing.Point GetCursorPosition()
    {
        var point = new POINT();
        User32.GetCursorPos(ref point);

        return new System.Drawing.Point(point.X, point.Y);
    }

    public static int GetDWORD(Color color) => color.R + (color.G << 8) + (color.B << 16);

    public static void RaiseMouseMessage(IntPtr hWnd, WM msg)
    {
        int num = 0;

        if (IsKeyPressed(1))
            num |= 1;

        if (IsKeyPressed(2))
            num |= 2;

        if (IsKeyPressed(4))
            num |= 0x10;

        if (IsKeyPressed(5))
            num |= 0x20;

        if (IsKeyPressed(6))
            num |= 0x40;

        var point = GetMousePosition();
        User32.ScreenToClient(hWnd, ref point);
        User32.SendMessage(hWnd, msg, new IntPtr(num), MakeParam(point.X, point.Y));
    }

    public static bool IsKeyPressed(int vKey) => User32.GetKeyState(vKey) < 0;

    public static IntPtr MakeParam(int lowWord, int highWord) => new IntPtr((lowWord & 0xFFFF) | (highWord << 16));

    public static POINT GetMousePosition()
    {
        var point = new POINT();
        User32.GetCursorPos(ref point);
        return point;
    }

    public static int GET_X_LPARAM(IntPtr lParam) => LOWORD(lParam.ToInt32());

    public static int GET_Y_LPARAM(IntPtr lParam) => HIWORD(lParam.ToInt32());

    public static int HIWORD(int i) => (short)(i >> 16);

    public static int LOWORD(int i) => (short)(i & ushort.MaxValue);

    public static WS_EX GetWindowStyleEx(IntPtr hWnd) => (WS_EX)GetWindowLongPtr(hWnd, GWL.EXSTYLE);

    public static WS GetWindowStyle(IntPtr hWnd) => (WS)GetWindowLongPtr(hWnd, GWL.STYLE);

    public static WS_EX SetWindowStyleEx(IntPtr hWnd, WS_EX dwNewLong) => (WS_EX)SetWindowLongPtr(hWnd, GWL.EXSTYLE, new IntPtr((int)dwNewLong));

    public static WS SetWindowStyle(IntPtr hWnd, WS dwNewLong) => (WS)SetWindowLongPtr(hWnd, GWL.STYLE, new IntPtr((int)dwNewLong));

    // This is aliased as a macro in 32bit Windows.
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static IntPtr GetWindowLongPtr(IntPtr hwnd, GWL nIndex)
    {
        var ret = 8 == IntPtr.Size
            ? User32.GetWindowLongPtr64(hwnd, nIndex)
            : (IntPtr)User32.GetWindowLong32(hwnd, nIndex);

        return IntPtr.Zero == ret ? throw new Win32Exception() : ret;
    }

    // This is aliased as a macro in 32bit Windows.
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static IntPtr SetWindowLongPtr(IntPtr hwnd, GWL nIndex, IntPtr dwNewLong) => 8 == IntPtr.Size
            ? User32.SetWindowLongPtr64(hwnd, nIndex, dwNewLong)
            : new IntPtr(User32.SetWindowLongPtr32(hwnd, nIndex, (uint)dwNewLong));
}
