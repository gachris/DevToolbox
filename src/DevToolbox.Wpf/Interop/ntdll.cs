using System.Runtime.InteropServices;

namespace DevToolbox.Wpf.Interop;

internal static class Ntdll
{
    [DllImport("ntdll.dll", SetLastError = true)]
    public static extern int RtlGetVersion(ref OSVERSIONINFOEX lpVersionInformation);
}