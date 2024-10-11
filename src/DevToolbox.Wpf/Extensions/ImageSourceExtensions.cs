using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DevToolbox.Wpf.Interop;

namespace DevToolbox.Wpf.Extensions;

internal static class ImageSourceExtensions
{
    public static ImageSource ToImageSource(this Bitmap bmp)
    {
        var handle = bmp.GetHbitmap();
        try
        {
            return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
        finally
        {
            Gdi32.DeleteObject(handle);
        }
    }
}
