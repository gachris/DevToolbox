using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Media;
using ColorSpace.Net;
using ColorSpace.Net.Colors;
using ColorSpace.Net.Convert;
using DevToolbox.Wpf.Controls;

namespace DevToolbox.Wpf.Utils;

internal class EyeDropperHelper
{
    private static readonly IColorConverter<Hsv> _hsvConverter;
    private static readonly IColorConverter<Hsl> _hslConverter;

    static EyeDropperHelper()
    {
        _hsvConverter = ConverterBuilder.Create(new ColorConverterOptions() { Illuminant = Illuminants.D65_2 })
                                        .ToColor<Hsv>()
                                        .Build();

        _hslConverter = ConverterBuilder.Create(new ColorConverterOptions() { Illuminant = Illuminants.D65_2 })
                                        .ToColor<Hsl>()
                                        .Build();
    }

    public static Window CreateVirtualScreenFrame()
    {
        return new Window()
        {
            Topmost = true,
            ShowInTaskbar = false,
            AllowsTransparency = true,
            WindowStyle = WindowStyle.None,
            Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)),
            Top = SystemParameters.VirtualScreenTop,
            Left = SystemParameters.VirtualScreenLeft,
            Width = SystemParameters.VirtualScreenWidth,
            Height = SystemParameters.VirtualScreenHeight
        };
    }

    public static System.Drawing.Bitmap CaptureVirtualScreen()
    {
        var width = (int)SystemParameters.VirtualScreenWidth;
        var height = (int)SystemParameters.VirtualScreenHeight;
        var top = (int)SystemParameters.VirtualScreenTop;
        var left = (int)SystemParameters.VirtualScreenLeft;

        var bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        using (var g = System.Drawing.Graphics.FromImage(bitmap))
        {
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.CopyFromScreen(left, top, 0, 0, new System.Drawing.Size(width, height), System.Drawing.CopyPixelOperation.SourceCopy);
        }

        return bitmap;
    }

    public static System.Drawing.Point GetRelativeCoordinates(System.Windows.Forms.Screen screen, System.Drawing.Point absoluteCoordinates)
    {
        var x = absoluteCoordinates.X + SystemParameters.VirtualScreenWidth - screen.Bounds.Width;
        var y = absoluteCoordinates.Y + SystemParameters.VirtualScreenHeight - screen.Bounds.Height;
        return new System.Drawing.Point((int)x, (int)y);
    }

    public static string GetFormattedColor(SolidColorBrush brush, ColorFormat colorFormat)
    {
        string? formattedColor;

        switch (colorFormat)
        {
            case ColorFormat.DelphiHex:
                formattedColor = string.Format(
                    "${0:X2}{1:X2}{2:X2}{3:X2}",
                    0,
                    brush.Color.B,
                    brush.Color.G,
                    brush.Color.R);
                break;
            case ColorFormat.VBHex:
                formattedColor = string.Format(
                    "&H{0:X2}{1:X2}{2:X2}{3:X2}",
                    0,
                    brush.Color.B,
                    brush.Color.G,
                    brush.Color.R);
                break;
            case ColorFormat.RGB:
                formattedColor = string.Format($"{brush.Color.R}, {brush.Color.G}, {brush.Color.B}");
                break;
            case ColorFormat.RGBFloat:
                formattedColor = string.Format($"{brush.Color.ScR}, {brush.Color.ScG}, {brush.Color.ScB}");
                break;
            case ColorFormat.HSV:
                {
                    var rgb = Rgb.FromRgb(brush.Color.R, brush.Color.G, brush.Color.B);
                    formattedColor = _hsvConverter.ConvertFrom(rgb).ToString();
                    break;
                }
            case ColorFormat.HSL:
                {
                    var rgb = Rgb.FromRgb(brush.Color.R, brush.Color.G, brush.Color.B);
                    formattedColor = _hslConverter.ConvertFrom(rgb).ToString();
                    break;
                }
            case ColorFormat.Long:
                formattedColor = (brush.Color.R << 16 | brush.Color.G << 8 | brush.Color.B).ToString();
                break;
            case ColorFormat.HTML:
            case ColorFormat.Hex:
            default:
                formattedColor = $"#{brush.Color.R:X2}{brush.Color.G:X2}{brush.Color.B:X2}";
                break;
        }

        return formattedColor;
    }
}