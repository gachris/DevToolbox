using System;
using System.Runtime.InteropServices;

namespace DevToolbox.Wpf.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct TRACKMOUSEEVENT
{
    public int cbSize;    // using Int32 instead of UInt32 is safe here, and this avoids casting the result  of Marshal.SizeOf()
    [MarshalAs(UnmanagedType.U4)]
    public TMEFlags dwFlags;
    public IntPtr hWnd;
    public uint dwHoverTime;

    public TRACKMOUSEEVENT(TMEFlags dwFlags, IntPtr hWnd, uint dwHoverTime)
    {
        cbSize = Marshal.SizeOf(typeof(TRACKMOUSEEVENT));
        this.dwFlags = dwFlags;
        this.hWnd = hWnd;
        this.dwHoverTime = dwHoverTime;
    }
}
