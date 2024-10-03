using System;
using System.Runtime.InteropServices;

namespace DevToolbox.Wpf.Interop;

internal static class Shell32
{
    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    public static extern uint ExtractIconEx(string szFileName, int nIconIndex, IntPtr[]? phiconLarge, IntPtr[]? phiconSmall, uint nIcons);
}
