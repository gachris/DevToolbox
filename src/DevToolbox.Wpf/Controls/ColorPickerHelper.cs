using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PointEx = System.Drawing.Point;

namespace DevToolbox.Wpf.Controls;

internal class ColorPickerHelper
{
    public static PointEx ConvertToDrawingPoint(Point point) => new((int)point.X, (int)point.Y);

    public static Color ConvertToMediaColor(System.Drawing.Color color, byte? overrideAlpha = null) => Color.FromArgb(overrideAlpha ?? color.A, color.R, color.G, color.B);

    public static System.Drawing.Color ConvertToDrawingColor(Color color) => System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);

    public static WriteableBitmap CreateWriteableBitmap(byte[] bitmapData, int width, int height, int stride, PixelFormat pixelFormat)
    {
        var writeableBitmap = new WriteableBitmap(width, height, 96, 96, pixelFormat, null);
        writeableBitmap.WritePixels(new(0, 0, width, height), bitmapData, stride, 0);
        return writeableBitmap;
    }
}