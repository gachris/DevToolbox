using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ColorSpace.Net;
using ColorSpace.Net.Colors;
using ColorSpace.Net.Convert;
using DevToolbox.Wpf.Data;

namespace DevToolbox.Wpf.Controls;

internal class ColorComponents : Control
{
    private enum ColorSourceChange
    {
        FromHsv,
        FromRgb,
        FromLab,
        FromCmyk,
        FromHex,
        FromColor
    }

    #region Fields/Consts

    private readonly IColorConverter<Rgb> _rgbConverter;
    private readonly IColorConverter<Cmyk> _cmykConverter;
    private readonly IColorConverter<Lab> _labConverter;
    private readonly IColorConverter<Hsv> _hsvConverter;

    private Color _color;

    public bool _colorUpdating;

    public event EventHandler? ColorChanged;

    public static readonly DependencyProperty NormalComponentTypeProperty =
        DependencyProperty.Register(nameof(NormalComponentType), typeof(NormalComponentType), typeof(ColorComponents), new FrameworkPropertyMetadata(default(NormalComponentType)));

    #endregion

    #region Properties

    public NormalComponentType NormalComponentType
    {
        get => (NormalComponentType)GetValue(NormalComponentTypeProperty);
        set => SetValue(NormalComponentTypeProperty, value);
    }

    public HsvColor Hsv { get; }

    public RgbColor Rgb { get; }

    public LabColor Lab { get; }

    public CmykColor Cmyk { get; }

    public HexColor Hex { get; }

    public Color Color => _color;

    #endregion

    static ColorComponents()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorComponents), new FrameworkPropertyMetadata(typeof(ColorComponents)));
    }

    public ColorComponents()
    {
        Hsv = new HsvColor();
        Hsv.PropertyChanged += HsvColorChanged;

        Rgb = new RgbColor();
        Rgb.PropertyChanged += RgbColorChanged;

        Lab = new LabColor();
        Lab.PropertyChanged += LabColorChanged;

        Cmyk = new CmykColor();
        Cmyk.PropertyChanged += CmykColorChanged;

        Hex = new HexColor();
        Hex.PropertyChanged += HexColorChanged;

        _hsvConverter = ConverterBuilder.Create(new ColorConverterOptions() { Illuminant = Illuminants.D65_2 })
            .ToColor<Hsv>()
            .Build();

        _rgbConverter = ConverterBuilder.Create(new ColorConverterOptions() { Illuminant = Illuminants.D65_2 })
            .ToColor<Rgb>()
            .Build();

        _labConverter = ConverterBuilder.Create(new ColorConverterOptions() { Illuminant = Illuminants.D65_2 })
            .ToColor<Lab>()
            .Build();

        _cmykConverter = ConverterBuilder.Create(new ColorConverterOptions() { Illuminant = Illuminants.D65_2 })
            .ToColor<Cmyk>()
            .Build();
    }

    #region Methods

    public int GetNormal()
    {
        return NormalComponentType switch
        {
            NormalComponentType.Hsv_H => Hsv.Hue,
            NormalComponentType.Hsv_S => Hsv.Saturation,
            NormalComponentType.Hsv_V => Hsv.Value,
            NormalComponentType.Rgb_R => Rgb.Red,
            NormalComponentType.Rgb_G => Rgb.Green,
            NormalComponentType.Rgb_B => Rgb.Blue,
            NormalComponentType.Lab_L => Lab.Lightness,
            NormalComponentType.Lab_A => Lab.A,
            NormalComponentType.Lab_B => Lab.B,
            _ => throw new ArgumentOutOfRangeException(nameof(NormalComponentType), $"The NormalComponentType value '{NormalComponentType}' is not valid.")
        };
    }

    public void SetNormal(int value)
    {
        switch (NormalComponentType)
        {
            case NormalComponentType.Hsv_H:
                Hsv.Hue = value;
                break;
            case NormalComponentType.Hsv_S:
                Hsv.Saturation = value;
                break;
            case NormalComponentType.Hsv_V:
                Hsv.Value = value;
                break;
            case NormalComponentType.Rgb_R:
                Rgb.Red = (byte)value;
                break;
            case NormalComponentType.Rgb_G:
                Rgb.Green = (byte)value;
                break;
            case NormalComponentType.Rgb_B:
                Rgb.Blue = (byte)value;
                break;
            case NormalComponentType.Lab_L:
                Lab.Lightness = value;
                break;
            case NormalComponentType.Lab_A:
                Lab.A = value;
                break;
            case NormalComponentType.Lab_B:
                Lab.B = value;
                break;
            default:
                break;
        }
    }

    public void UpdateFromColor(Color color)
    {
        UpdateFromColor(color, ColorSourceChange.FromColor);
    }

    private void UpdateFromColor(Color color, ColorSourceChange colorSourceChange)
    {
        if (_colorUpdating)
            return;

        _colorUpdating = true;

        var rgb = ColorSpace.Net.Colors.Rgb.FromRgb(color.R, color.G, color.B);
        var hsv = _hsvConverter.ConvertFrom(rgb);
        var lab = _labConverter.ConvertFrom(rgb);
        var cmyk = _cmykConverter.ConvertFrom(rgb);

        if (colorSourceChange is not ColorSourceChange.FromHsv)
        {
            if (NormalComponentType is not NormalComponentType.Hsv_H)
                Hsv.Hue = (int)Math.Round(hsv.H);
            if (NormalComponentType is not NormalComponentType.Hsv_S)
                Hsv.Saturation = (int)Math.Round(hsv.S * 100m);
            if (NormalComponentType is not NormalComponentType.Hsv_V)
                Hsv.Value = (int)Math.Round(hsv.V * 100m);
        }

        if (colorSourceChange is not ColorSourceChange.FromRgb)
        {
            if (NormalComponentType is not NormalComponentType.Rgb_R)
                Rgb.Red = rgb.R;
            if (NormalComponentType is not NormalComponentType.Rgb_G)
                Rgb.Green = rgb.G;
            if (NormalComponentType is not NormalComponentType.Rgb_B)
                Rgb.Blue = rgb.B;
        }

        if (colorSourceChange is not ColorSourceChange.FromLab)
        {
            if (NormalComponentType is not NormalComponentType.Lab_L)
                Lab.Lightness = (int)Math.Round(lab.L);
            if (NormalComponentType is not NormalComponentType.Lab_A)
                Lab.A = (int)Math.Round(lab.A);
            if (NormalComponentType is not NormalComponentType.Lab_B)
                Lab.B = (int)Math.Round(lab.B);
        }

        if (colorSourceChange is not ColorSourceChange.FromCmyk)
        {
            Cmyk.Cyan = (int)Math.Round(cmyk.C * 100m);
            Cmyk.Magenta = (int)Math.Round(cmyk.M * 100m);
            Cmyk.Yellow = (int)Math.Round(cmyk.Y * 100m);
            Cmyk.Key = (int)Math.Round(cmyk.K * 100m);
        }

        if (colorSourceChange is not ColorSourceChange.FromHex)
        {
            Hex.Value = color.ToString();
        }

        _color = color;

        ColorChanged?.Invoke(this, EventArgs.Empty);

        _colorUpdating = false;
    }

    #endregion

    #region Events Subscription

    private void HsvColorChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_colorUpdating)
            return;

        var hsv = ColorSpace.Net.Colors.Hsv.FromHsv(Hsv.Hue, Hsv.Saturation / 100m, Hsv.Value / 100m);
        var rgb = _rgbConverter.ConvertFrom(hsv);
        var color = Color.FromArgb(Color.A, rgb.R, rgb.G, rgb.B);
        UpdateFromColor(color, ColorSourceChange.FromHsv);
    }

    private void RgbColorChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_colorUpdating)
            return;

        var rgb = ColorSpace.Net.Colors.Rgb.FromRgb(Rgb.Red, Rgb.Green, Rgb.Blue);
        var color = Color.FromArgb(Color.A, rgb.R, rgb.G, rgb.B);
        UpdateFromColor(color, ColorSourceChange.FromRgb);
    }

    private void LabColorChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_colorUpdating)
            return;

        var lab = ColorSpace.Net.Colors.Lab.FromLab(Lab.Lightness, Lab.A, Lab.B);
        var rgb = _rgbConverter.ConvertFrom(lab);
        var color = Color.FromArgb(Color.A, rgb.R, rgb.G, rgb.B);
        UpdateFromColor(color, ColorSourceChange.FromLab);
    }

    private void CmykColorChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_colorUpdating)
            return;

        var cmyk = ColorSpace.Net.Colors.Cmyk.FromCmyk(Cmyk.Cyan / 100m, Cmyk.Magenta / 100m, Cmyk.Yellow / 100m, Cmyk.Key / 100m);
        var rgb = _rgbConverter.ConvertFrom(cmyk);
        var color = Color.FromArgb(Color.A, rgb.R, rgb.G, rgb.B);
        UpdateFromColor(color, ColorSourceChange.FromCmyk);
    }

    private void HexColorChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_colorUpdating)
            return;

        var hex = Hex.Value ?? string.Empty;

        if (hex.StartsWith("#"))
        {
            hex = hex.Replace("#", string.Empty);
        }

        if (hex.Length is < 6 and not 3)
        {
            hex = hex.PadLeft(6, '0');
        }

        if (hex.Length is < 8 and not 3)
        {
            hex = hex.PadLeft(8, 'f');
        }

        var color = (Color)ColorConverter.ConvertFromString("#" + hex);
        UpdateFromColor(color, ColorSourceChange.FromHex);
    }

    #endregion
}
