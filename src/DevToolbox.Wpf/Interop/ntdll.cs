using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DevToolbox.Wpf.Interop;

internal static class ntdll
{
    [DllImport("ntdll.dll", SetLastError = true)]
    public static extern int RtlGetVersion(ref OSVERSIONINFOEX lpVersionInformation);
}