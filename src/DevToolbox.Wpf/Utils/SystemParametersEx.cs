using System;
using System.Reflection;
using System.Windows;
using DevToolbox.Wpf.Interop;

namespace DevToolbox.Wpf.Utils;

internal class SystemParametersEx
{
    private static readonly Version _osVersion = Environment.OSVersion.Version;
    private static readonly Version? _presentationFrameworkVersion = Assembly.GetAssembly(typeof(Window))?.GetName().Version;

    public static bool IsOSVistaOrNewer => SystemParametersEx._osVersion >= new Version(6, 0);

    public static bool IsOSWindows7OrNewer => SystemParametersEx._osVersion >= new Version(6, 1);

    public static bool IsPresentationFrameworkVersionLessThan4 => SystemParametersEx._presentationFrameworkVersion < new Version(4, 0);

    public static SIZE WindowResizeBorderThickness
    {
        get
        {
            float dpix = GetDpi(DEVICECAP.LOGPIXELSX);
            float dpiy = GetDpi(DEVICECAP.LOGPIXELSY);

            int dx = User32.GetSystemMetrics(SM.CXFRAME);
            int dy = User32.GetSystemMetrics(SM.CYFRAME);

            int d = User32.GetSystemMetrics(SM.CXPADDEDBORDER);
            dx += d;
            dy += d;

            var leftBorder = dx / dpix;
            var topBorder = dy / dpiy;

            return new SIZE((int)leftBorder, (int)topBorder);
        }
    }

    private static float GetDpi(DEVICECAP index)
    {
        IntPtr desktopWnd = IntPtr.Zero;
        IntPtr dc = User32.GetDC(desktopWnd);
        float dpi;
        try
        {
            dpi = Gdi32.GetDeviceCaps(dc, index);
        }
        finally
        {
            _ = User32.ReleaseDC(desktopWnd, dc);
        }
        return dpi / 96f;
    }
}
