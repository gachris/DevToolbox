﻿using System;
using System.Runtime.InteropServices;

namespace DevToolbox.Wpf.Interop;

internal static class Dwmapi
{
    [DllImport("dwmapi.dll", PreserveSig = true)]
    public static extern int DwmGetColorizationColor(out uint colorizationColor, out bool opaqueBlend);

    [DllImport("dwmapi.dll")]
    public static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, ref uint pvAttribute, uint cbAttribute);

    [DllImport("dwmapi.dll", PreserveSig = true)]
    public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);
}