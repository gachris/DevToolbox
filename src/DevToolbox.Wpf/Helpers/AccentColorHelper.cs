using System;
using System.Windows.Media;
using DevToolbox.Wpf.Interop;
using DevToolbox.Wpf.Media;
using Microsoft.Win32;

namespace DevToolbox.Wpf.Helpers;

internal static class AccentColorHelper
{
    // Private backing fields for accent colors
    private static Color _immersiveSystemAccent = default!;
    private static Color _immersiveSystemAccentDark1 = default!;
    private static Color _immersiveSystemAccentDark2 = default!;
    private static Color _immersiveSystemAccentDark3 = default!;
    private static Color _immersiveSystemAccentLight1 = default!;
    private static Color _immersiveSystemAccentLight2 = default!;
    private static Color _immersiveSystemAccentLight3 = default!;

    // Private backing fields for frozen brushes
    private static SolidColorBrush _immersiveSystemAccentBrush = default!;
    private static SolidColorBrush _immersiveSystemAccentDark1Brush = default!;
    private static SolidColorBrush _immersiveSystemAccentDark2Brush = default!;
    private static SolidColorBrush _immersiveSystemAccentDark3Brush = default!;
    private static SolidColorBrush _immersiveSystemAccentLight1Brush = default!;
    private static SolidColorBrush _immersiveSystemAccentLight2Brush = default!;
    private static SolidColorBrush _immersiveSystemAccentLight3Brush = default!;

    private static readonly Color _defaultAccentColor = Color.FromArgb(0xFF, 0x00, 0x78, 0xD4);

    static AccentColorHelper()
    {
        Initialize();
        SystemEvents.UserPreferenceChanged += (_, _) => Initialize();
    }

    /// <summary>
    /// Gets the accent-based color for the given type, using only the immersive APIs.
    /// Falls back to a hard-coded default if anything fails.
    /// </summary>
    public static Color GetColor(UIColorType colorType)
    {
        try
        {
            return colorType switch
            {
                UIColorType.Accent => _immersiveSystemAccent,
                UIColorType.AccentDark1 => _immersiveSystemAccentDark1,
                UIColorType.AccentDark2 => _immersiveSystemAccentDark2,
                UIColorType.AccentDark3 => _immersiveSystemAccentDark3,
                UIColorType.AccentLight1 => _immersiveSystemAccentLight1,
                UIColorType.AccentLight2 => _immersiveSystemAccentLight2,
                UIColorType.AccentLight3 => _immersiveSystemAccentLight3,
                UIColorType.Complement => GetComplement(_immersiveSystemAccent),
                UIColorType.Background => IsDarkTheme() ? Colors.Black : Colors.White,
                UIColorType.Foreground => IsDarkTheme() ? Colors.White : Colors.Black,
                _ => _immersiveSystemAccent,
            };
        }
        catch
        {
            return _defaultAccentColor;
        }
    }

    /// <summary>
    /// Creates a frozen SolidColorBrush for the given accent type.
    /// </summary>
    public static SolidColorBrush GetBrush(UIColorType colorType)
    {
        SolidColorBrush brush = colorType switch
        {
            UIColorType.Accent => _immersiveSystemAccentBrush,
            UIColorType.AccentDark1 => _immersiveSystemAccentDark1Brush,
            UIColorType.AccentDark2 => _immersiveSystemAccentDark2Brush,
            UIColorType.AccentDark3 => _immersiveSystemAccentDark3Brush,
            UIColorType.AccentLight1 => _immersiveSystemAccentLight1Brush,
            UIColorType.AccentLight2 => _immersiveSystemAccentLight2Brush,
            UIColorType.AccentLight3 => _immersiveSystemAccentLight3Brush,
            _ => _immersiveSystemAccentBrush,
        };
        return brush;
    }

    internal static void Initialize()
    {
        if (!SystemInfo.IsWin7())
        {
            _immersiveSystemAccent = GetColorByTypeName("ImmersiveSystemAccent");
            _immersiveSystemAccentDark1 = GetColorByTypeName("ImmersiveSystemAccentDark1");
            _immersiveSystemAccentDark2 = GetColorByTypeName("ImmersiveSystemAccentDark2");
            _immersiveSystemAccentDark3 = GetColorByTypeName("ImmersiveSystemAccentDark3");
            _immersiveSystemAccentLight1 = GetColorByTypeName("ImmersiveSystemAccentLight1");
            _immersiveSystemAccentLight2 = GetColorByTypeName("ImmersiveSystemAccentLight2");
            _immersiveSystemAccentLight3 = GetColorByTypeName("ImmersiveSystemAccentLight3");
        }
        else
        {
            _immersiveSystemAccent = (Color)ColorConverter.ConvertFromString("#FF2990CC");
            _immersiveSystemAccentDark1 = (Color)ColorConverter.ConvertFromString("#FF2481B6");
            _immersiveSystemAccentDark2 = (Color)ColorConverter.ConvertFromString("#FF2071A1");
            _immersiveSystemAccentDark3 = (Color)ColorConverter.ConvertFromString("#FF205B7E");
            _immersiveSystemAccentLight1 = (Color)ColorConverter.ConvertFromString("#FF2D9FE1");
            _immersiveSystemAccentLight2 = (Color)ColorConverter.ConvertFromString("#FF51A5D6");
            _immersiveSystemAccentLight3 = (Color)ColorConverter.ConvertFromString("#FF7BB1D0");
        }

        _immersiveSystemAccentBrush = CreateBrush(_immersiveSystemAccent);
        _immersiveSystemAccentDark1Brush = CreateBrush(_immersiveSystemAccentDark1);
        _immersiveSystemAccentDark2Brush = CreateBrush(_immersiveSystemAccentDark2);
        _immersiveSystemAccentDark3Brush = CreateBrush(_immersiveSystemAccentDark3);
        _immersiveSystemAccentLight1Brush = CreateBrush(_immersiveSystemAccentLight1);
        _immersiveSystemAccentLight2Brush = CreateBrush(_immersiveSystemAccentLight2);
        _immersiveSystemAccentLight3Brush = CreateBrush(_immersiveSystemAccentLight3);
    }

    public static Color GetColorByTypeName(string name)
    {
        var colorSet = UXTheme.GetImmersiveUserColorSetPreference(false, false);
        var colorType = UXTheme.GetImmersiveColorTypeFromName(name);
        var rawColor = UXTheme.GetImmersiveColorFromColorSetEx(colorSet, colorType, false, 0);

        var bytes = BitConverter.GetBytes(rawColor);
        return Color.FromArgb(bytes[3], bytes[0], bytes[1], bytes[2]);
    }

    /// <summary>
    /// HSL‐based 180° hue shift for the “Complement” variant.
    /// </summary>
    private static Color GetComplement(Color baseColor)
    {
        RgbToHsl(baseColor, out double h, out double s, out double l);
        h = (h + 0.5) % 1.0;
        return HslToRgb(h, s, l);
    }

    private static void RgbToHsl(Color c, out double h, out double s, out double l)
    {
        var r = c.R / 255.0;
        var g = c.G / 255.0;
        var b = c.B / 255.0;
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        l = (max + min) / 2.0;
        if (Math.Abs(max - min) < 0.0001)
        {
            h = s = 0;
        }
        else
        {
            var d = max - min;
            s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);
            if (max == r)
                h = (g - b) / d + (g < b ? 6 : 0);
            else if (max == g)
                h = (b - r) / d + 2;
            else
                h = (r - g) / d + 4;
            h /= 6.0;
        }
    }

    private static Color HslToRgb(double h, double s, double l)
    {
        double r, g, b;
        if (s == 0) r = g = b = l;
        else
        {
            var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            var p = 2 * l - q;
            static double Hue2Rgb(double p, double q, double t)
            {
                if (t < 0) t += 1;
                if (t > 1) t -= 1;
                if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
                if (t < 0.5) return q;
                if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
                return p;
            }
            r = Hue2Rgb(p, q, h + 1.0 / 3.0);
            g = Hue2Rgb(p, q, h);
            b = Hue2Rgb(p, q, h - 1.0 / 3.0);
        }
        return Color.FromArgb(255, (byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
    }

    private static SolidColorBrush CreateBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static bool IsDarkTheme()
    {
        return ThemeManager.ApplicationTheme == ApplicationTheme.Dark;
    }
}
