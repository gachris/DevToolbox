using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DevToolbox.Wpf.Interop;

internal static class Kernel32
{
    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern int GetModuleFileName(IntPtr hModule, StringBuilder buffer, int length);
}