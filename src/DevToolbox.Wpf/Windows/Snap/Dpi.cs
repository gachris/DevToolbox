using System;
using System.Windows;
using System.Windows.Media;
using DevToolbox.Wpf.Interop;

namespace DevToolbox.Wpf.Windows.Snap;

/// <summary>
/// Provides methods for converting device coordinates to logical coordinates based on the system DPI.
/// </summary>
internal static class Dpi
{
    private static Matrix _transformToDevice;
    private static Matrix _transformToDip;
    private static Point _systemDpi;
    private static Point _ratio;

    static Dpi()
    {
        _systemDpi = GetDotsPerInch();

        _transformToDip = Matrix.Identity;
        _transformToDip.Scale(96d / _systemDpi.X, 96d / _systemDpi.Y);

        _transformToDevice = Matrix.Identity;
        _transformToDevice.Scale(_systemDpi.X / 96d, _systemDpi.Y / 96d);

        _ratio = new Point(_transformToDevice.M11, _transformToDevice.M22);
    }

    /// <summary>
    /// Converts a device X coordinate to a logical X coordinate based on the system DPI.
    /// </summary>
    /// <param name="deviceX">The X coordinate in device units.</param>
    /// <returns>The corresponding logical X coordinate.</returns>
    public static double ToLogicalX(double deviceX) => deviceX / _ratio.X;

    /// <summary>
    /// Converts a device Y coordinate to a logical Y coordinate based on the system DPI.
    /// </summary>
    /// <param name="deviceY">The Y coordinate in device units.</param>
    /// <returns>The corresponding logical Y coordinate.</returns>
    public static double ToLogicalY(double deviceY) => deviceY / _ratio.Y;

    /// <summary>
    /// Retrieves the system DPI (dots per inch) for both X and Y axes.
    /// </summary>
    /// <returns>A <see cref="Point"/> representing the system DPI in the X and Y directions.</returns>
    internal static Point GetDotsPerInch()
    {
        var dpi = new Point(96d, 96d);
        var hDC = IntPtr.Zero;

        try
        {
            hDC = User32.GetDC(IntPtr.Zero);

            if (hDC != IntPtr.Zero)
            {
                var dpiX = (uint)Gdi32.GetDeviceCaps(hDC, DEVICECAP.LOGPIXELSX);
                var dpiY = (uint)Gdi32.GetDeviceCaps(hDC, DEVICECAP.LOGPIXELSY);

                dpi = new Point(dpiX, dpiY);
            }
        }
        finally
        {
            if (hDC != IntPtr.Zero)
            {
                _ = User32.ReleaseDC(IntPtr.Zero, hDC);
            }
        }

        return dpi;
    }
}
