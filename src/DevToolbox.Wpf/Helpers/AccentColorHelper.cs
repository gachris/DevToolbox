using System.Windows.Media;
using ColorSpace.Net;
using ColorSpace.Net.Colors;
using ColorSpace.Net.Convert;
using DevToolbox.Wpf.Media;
#if NET8_0_OR_GREATER && WINDOWS10_0_17763_0_OR_GREATER
using Windows.UI.ViewManagement;
#endif

namespace DevToolbox.Wpf.Helpers;

internal static class AccentColorHelper
{
    private static readonly Color _defaultAccentColor = Color.FromArgb(0xff, 0x00, 0x78, 0xd4);
    private static readonly IColorConverter<Hsl> _hslConverter;
    private static readonly IColorConverter<Rgb> _rgbConverter;

#if NET8_0_OR_GREATER && WINDOWS10_0_17763_0_OR_GREATER
    private static readonly UISettings _uiSettings = new();
#endif

    static AccentColorHelper()
    {
        _hslConverter = ConverterBuilder.Create(new ColorConverterOptions { Illuminant = Illuminants.D65_2 })
            .ToColor<Hsl>()
            .Build();

        _rgbConverter = ConverterBuilder.Create(new ColorConverterOptions { Illuminant = Illuminants.D65_2 })
            .ToColor<Rgb>()
            .Build();
    }

    public static Color GetColor(UIColorType colorType)
    {
#if NET8_0_OR_GREATER && WINDOWS10_0_17763_0_OR_GREATER
        try
        {
            var systemColor = _uiSettings.GetColorValue((UIColorType)(int)colorType);
            return Color.FromArgb(systemColor.A, systemColor.R, systemColor.G, systemColor.B);
        }
        catch
        {
            // Fallback to custom
        }
#endif
        return GetCustomColor(colorType);
    }

    private static Color GetCustomColor(UIColorType colorType)
    {
        var baseAccent = GetAccentColor();
        var isDarkTheme = IsDarkTheme();

        return colorType switch
        {
            UIColorType.Accent => baseAccent,
            UIColorType.AccentDark1 => ChangeLightness(baseAccent, -4m),
            UIColorType.AccentDark2 => ChangeLightness(baseAccent, -8m),
            UIColorType.AccentDark3 => ChangeLightness(baseAccent, -35m),
            UIColorType.AccentLight1 => ChangeLightness(baseAccent, 4m),
            UIColorType.AccentLight2 => ChangeLightness(baseAccent, 8m),
            UIColorType.AccentLight3 => ChangeLightness(baseAccent, 18m),
            UIColorType.Complement => GetComplement(baseAccent),
            UIColorType.Background => isDarkTheme ? Colors.Black : Colors.White,
            UIColorType.Foreground => isDarkTheme ? Colors.White : Colors.Black,
            _ => baseAccent
        };
    }

    private static Color GetAccentColor()
    {
        if (SystemBrushesHelper.GetAccentBrush() is not SolidColorBrush brush)
            return _defaultAccentColor;

        return brush.Color;
    }

    private static Color ChangeLightness(Color color, decimal lightnessDelta)
    {
        var hsl = _hslConverter.ConvertFrom<Rgb>(Rgb.FromRgb(color.R, color.G, color.B));

        hsl.L = Clamp(hsl.L + lightnessDelta, 0m, 100m);

        var rgb = _rgbConverter.ConvertFrom<Hsl>(hsl);
        return Color.FromArgb(255, rgb.R, rgb.G, rgb.B);
    }

    private static Color GetComplement(Color color)
    {
        var hsl = _hslConverter.ConvertFrom<Rgb>(Rgb.FromRgb(color.R, color.G, color.B));
        hsl.H = (hsl.H + 0.5m) % 1.0m;
        var rgb = _rgbConverter.ConvertFrom<Hsl>(hsl);
        return Color.FromArgb(255, rgb.R, rgb.G, rgb.B);
    }

    private static decimal Clamp(decimal value, decimal min, decimal max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    private static bool IsDarkTheme()
    {
        try
        {
            return ThemeManager.ApplicationTheme is ApplicationTheme.Dark;
        }
        catch
        {
            return false;
        }
    }
}
