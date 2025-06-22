using System;
using System.Runtime.InteropServices;

namespace DevToolbox.Wpf.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct NCCALCSIZE_PARAMS
{
    public RECT rgrc0;

    public RECT rgrc1;

    public RECT rgrc2;

    public IntPtr lppos;
}
