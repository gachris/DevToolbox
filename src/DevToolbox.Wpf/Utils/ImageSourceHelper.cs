using System;
using System.Drawing;
using System.Linq;
using System.Security;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DevToolbox.Wpf.Interop;

namespace DevToolbox.Wpf.Utils;

internal static class ImageSourceHelper
{
    public static ImageSource GetImageSource(Icon icon)
    {
        return Imaging.CreateBitmapSourceFromHBitmap(icon.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
    }

    [SecuritySafeCritical]
    public static void GetIcons(out ImageSource[] largeIcons, out ImageSource[] smallIcons)
    {
        var buffer = new StringBuilder(byte.MaxValue);
        _ = Kernel32.GetModuleFileName(IntPtr.Zero, buffer, byte.MaxValue);
        var numArray1 = new IntPtr[1] { IntPtr.Zero };
        var numArray2 = new IntPtr[1] { IntPtr.Zero };
        var iconEx1 = Shell32.ExtractIconEx(buffer.ToString(), -1, null, null, 0U);
        _ = (int)Shell32.ExtractIconEx(buffer.ToString(), 0, numArray1, numArray2, iconEx1);
        InitImageSources(numArray1, out largeIcons);
        InitImageSources(numArray2, out smallIcons);
    }

    [SecuritySafeCritical]
    private static void InitImageSources(IntPtr[] pointers, out ImageSource[] resultImages)
    {
        var array = pointers.Where(x => x != IntPtr.Zero).ToArray();
        resultImages = new ImageSource[array.Length];
        var index = 0;
        foreach (var num in array)
        {
            resultImages[index] = Imaging.CreateBitmapSourceFromHIcon(num, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            User32.DestroyIcon(num);
            ++index;
        }
    }
}
