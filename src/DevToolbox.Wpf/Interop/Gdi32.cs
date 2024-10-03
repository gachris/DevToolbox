using System;
using System.Runtime.InteropServices;

namespace DevToolbox.Wpf.Interop;

internal static class Gdi32
{
    [DllImport("gdi32.dll")]
    public static extern int GetDeviceCaps(IntPtr hdc, DEVICECAP nIndex);

    [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeleteObject([In] IntPtr hObject);
}
