using System;
using System.Runtime.InteropServices;

namespace DevToolbox.Wpf.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct WINDOWPOS
{
    public IntPtr hwnd;
    public IntPtr hwndAfter;
    public int x;
    public int y;
    public int cx;
    public int cy;
    public SWP flags;

    public override string ToString() => x + ":" + y + ":" + cx + ":" + cy + ":" + flags;

    public override bool Equals(object? obj) => base.Equals(obj);

    public override int GetHashCode() => base.GetHashCode();

    public static bool operator ==(WINDOWPOS left, WINDOWPOS right) => left.Equals(right);

    public static bool operator !=(WINDOWPOS left, WINDOWPOS right) => !(left == right);
}
