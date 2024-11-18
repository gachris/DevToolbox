using System;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
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

    private static readonly Regex RgbPattern = new(@"\((R|G|B|r|g|b)\)");
    private static readonly Regex HsvPattern = new(@"\((H|S|V|h|s|v)\)");
    private static readonly Regex HslPattern = new(@"\((H|S|L|h|s|l)\)");
    private static readonly Regex LongPattern = new(@"\((long)\)", RegexOptions.IgnoreCase);

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

    public static System.Drawing.Point GetRelativeCoordinatesToVirtualScreen(System.Drawing.Point absoluteCoordinates)
    {
        var x = absoluteCoordinates.X - (int)SystemParameters.VirtualScreenLeft;
        var y = absoluteCoordinates.Y - (int)SystemParameters.VirtualScreenTop;
        return new System.Drawing.Point(x, y);
    }

    public static string GetFormattedColor(SolidColorBrush brush, ColorFormat colorFormat, string template, int precision)
    {
        if (precision < 1) precision = 1;
        if (precision > 11) precision = 11;

        string ReplaceHexPlaceholders(string input)
        {
            return RgbPattern.Replace(input, match =>
            {
                var component = match.Groups[1].Value[0];
                var value = component switch
                {
                    'R' or 'r' => brush.Color.R,
                    'G' or 'g' => brush.Color.G,
                    'B' or 'b' => brush.Color.B,
                    _ => throw new InvalidOperationException("Invalid color component")
                };
                return char.IsUpper(component) ? value.ToString("X2") : value.ToString("x2");
            });
        }

        string ReplaceRgbPlaceholders(string input)
        {
            return RgbPattern.Replace(input, match =>
            {
                var component = match.Groups[1].Value[0];
                var value = component switch
                {
                    'R' or 'r' => brush.Color.R,
                    'G' or 'g' => brush.Color.G,
                    'B' or 'b' => brush.Color.B,
                    _ => throw new InvalidOperationException("Invalid color component")
                };
                return value.ToString();
            });
        }

        string ReplaceFloatPlaceholders(string input)
        {
            return RgbPattern.Replace(input, match =>
            {
                var component = match.Groups[1].Value;
                var value = component switch
                {
                    "R" or "r" => brush.Color.ScR,
                    "G" or "g" => brush.Color.ScG,
                    "B" or "b" => brush.Color.ScB,
                    _ => throw new InvalidOperationException("Invalid color component")
                };
                return value.ToString($"F{precision}", System.Globalization.CultureInfo.InvariantCulture);
            });
        }

        string ReplaceHsvPlaceholders(string input)
        {
            var rgb = Rgb.FromRgb(brush.Color.R, brush.Color.G, brush.Color.B);
            var hsv = _hsvConverter.ConvertFrom(rgb);
            return HsvPattern.Replace(input, match =>
            {
                var component = match.Groups[1].Value[0];
                var value = component switch
                {
                    'H' or 'h' => hsv.H,
                    'S' or 's' => hsv.S * 100,
                    'V' or 'v' => hsv.V * 100,
                    _ => throw new InvalidOperationException("Invalid color component")
                };
                return value.ToString($"N0", System.Globalization.CultureInfo.InvariantCulture);
            });
        }

        string ReplaceHslPlaceholders(string input)
        {
            var rgb = Rgb.FromRgb(brush.Color.R, brush.Color.G, brush.Color.B);
            var hsl = _hslConverter.ConvertFrom(rgb);
            return HslPattern.Replace(input, match =>
            {
                var component = match.Groups[1].Value[0];
                var value = component switch
                {
                    'H' or 'h' => hsl.H,
                    'S' or 's' => hsl.S,
                    'L' or 'l' => hsl.L,
                    _ => throw new InvalidOperationException("Invalid color component")
                };
                return value.ToString($"N0", System.Globalization.CultureInfo.InvariantCulture);
            });
        }

        string ReplaceLongPlaceholder(string input)
        {
            var colorValue = (brush.Color.R << 16) | (brush.Color.G << 8) | brush.Color.B;
            return LongPattern.Replace(input, _ => colorValue.ToString());
        }

        switch (colorFormat)
        {
            case ColorFormat.DelphiHex:
                if (string.IsNullOrEmpty(template))
                    template = "$00(b)(g)(r)";
                return Regex.Unescape(ReplaceHexPlaceholders(template));

            case ColorFormat.VBHex:
                if (string.IsNullOrEmpty(template))
                    template = "&H00(B)(G)(R)";
                return Regex.Unescape(ReplaceHexPlaceholders(template));

            case ColorFormat.RGB:
                if (string.IsNullOrEmpty(template))
                    template = "(r), (g), (b)";
                return Regex.Unescape(ReplaceRgbPlaceholders(template));

            case ColorFormat.RGBFloat:
                if (string.IsNullOrEmpty(template))
                    template = "(r), (g), (b)";
                return Regex.Unescape(ReplaceFloatPlaceholders(template));

            case ColorFormat.HSV:
                if (string.IsNullOrEmpty(template))
                    template = "(h), (s), (v)";
                return Regex.Unescape(ReplaceHsvPlaceholders(template));

            case ColorFormat.HSL:
                if (string.IsNullOrEmpty(template))
                    template = "(h), (s), (l)";
                return Regex.Unescape(ReplaceHslPlaceholders(template));

            case ColorFormat.Long:
                if (string.IsNullOrEmpty(template))
                    template = "(long)";
                return Regex.Unescape(ReplaceLongPlaceholder(template));

            case ColorFormat.HTML:
                if (string.IsNullOrEmpty(template))
                    template = "#(R)(G)(B)";
                return Regex.Unescape(ReplaceHexPlaceholders(template));
            case ColorFormat.Hex:
            default:
                if (string.IsNullOrEmpty(template))
                    template = "0x(B)(G)(R)";
                return Regex.Unescape(ReplaceHexPlaceholders(template));
        }
    }
}