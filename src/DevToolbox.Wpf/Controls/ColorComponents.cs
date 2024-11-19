using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ColorSpace.Net;
using ColorSpace.Net.Colors;
using ColorSpace.Net.Convert;
using DevToolbox.Wpf.Data;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// This class provides functionality for representing and updating different color models (HSV, RGB, Lab, CMYK, and Hex) in a WPF control.
/// </summary>
internal class ColorComponents : Control
{
    /// <summary>
    /// Indicates the source of a color change, used to avoid redundant or recursive updates.
    /// </summary>
    private enum ColorSourceChange
    {
        FromHsv,
        FromRgb,
        FromLab,
        FromCmyk,
        FromHex
    }

    #region Fields/Consts

    private readonly IColorConverter<Rgb> _rgbConverter;
    private readonly IColorConverter<Cmyk> _cmykConverter;
    private readonly IColorConverter<Lab> _labConverter;
    private readonly IColorConverter<Hsv> _hsvConverter;
    private readonly DispatcherTimer _throttleTimer;

    private Color _color;
    private Color? _pendingNewColor;
    private Color? _pendingFullUpdateColor;
    private ColorSourceChange? _pendingFullUpdateSourceChange = null;
    private bool _isThrottling;
    private bool _colorUpdating;

    public event EventHandler? ColorChanged;

    public static readonly DependencyProperty NormalComponentTypeProperty =
        DependencyProperty.Register(nameof(NormalComponentType), typeof(NormalComponentType), typeof(ColorComponents), new FrameworkPropertyMetadata(default(NormalComponentType)));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the component type that is treated as "normal" (editable via UI).
    /// The "normal" component type refers to the primary adjustable component of the color model.
    /// </summary>
    public NormalComponentType NormalComponentType
    {
        get => (NormalComponentType)GetValue(NormalComponentTypeProperty);
        set => SetValue(NormalComponentTypeProperty, value);
    }

    /// <summary>
    /// Gets the HSV (Hue, Saturation, Value) color representation.
    /// Represents the color in HSV color space, allowing manipulation of hue, saturation, and value.
    /// </summary>
    public HsvColor Hsv { get; }

    /// <summary>
    /// Gets the RGB (Red, Green, Blue) color representation.
    /// Represents the color in RGB color space, allowing manipulation of red, green, and blue channels.
    /// </summary>
    public RgbColor Rgb { get; }

    /// <summary>
    /// Gets the Lab color representation.
    /// Represents the color in CIE-Lab color space, useful for perceptual color adjustments.
    /// </summary>
    public LabColor Lab { get; }

    /// <summary>
    /// Gets the CMYK (Cyan, Magenta, Yellow, Key) color representation.
    /// Represents the color in CMYK color space, often used in printing.
    /// </summary>
    public CmykColor Cmyk { get; }

    /// <summary>
    /// Gets the Hexadecimal color representation.
    /// Represents the color in hexadecimal notation, commonly used in web development.
    /// </summary>
    public HexColor Hex { get; }

    /// <summary>
    /// Gets the current color value.
    /// This property represents the color in the <see cref="System.Windows.Media.Color"/> format.
    /// </summary>
    public Color Color => _color;

    /// <summary>
    /// Gets or sets the "normal" color component value based on the selected <see cref="NormalComponentType"/>.
    /// Allows getting or setting the value of the currently selected color component.
    /// </summary>
    public int Normal
    {
        get
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
        set
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
                    throw new ArgumentOutOfRangeException(nameof(NormalComponentType), $"The NormalComponentType value '{NormalComponentType}' is not valid.");
            }
        }
    }

    #endregion

    static ColorComponents()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorComponents), new FrameworkPropertyMetadata(typeof(ColorComponents)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorComponents"/> control.
    /// This constructor sets up color representations, initializes the necessary color converters, and subscribes to property change events.
    /// </summary>
    public ColorComponents()
    {
        // Initialize color representations
        Hsv = new HsvColor();
        Rgb = new RgbColor();
        Lab = new LabColor();
        Cmyk = new CmykColor();
        Hex = new HexColor();

        // Subscribe to property change events for each color model
        Hsv.PropertyChanged += HsvColorChanged;
        Rgb.PropertyChanged += RgbColorChanged;
        Lab.PropertyChanged += LabColorChanged;
        Cmyk.PropertyChanged += CmykColorChanged;
        Hex.PropertyChanged += HexColorChanged;

        // Initialize color converters for various color models
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

        // Initialize throttling timer to reduce frequent updates
        _throttleTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(160) // Set throttle interval to 160ms
        };
        _throttleTimer.Tick += OnThrottleTimerTick;
    }

    #region Methods

    /// <summary>
    /// Updates the current color representation and throttles frequent updates to improve performance.
    /// This method is useful for limiting the rate at which color changes are propagated to avoid performance issues.
    /// </summary>
    /// <param name="color">The new color value to update to.</param>
    public void UpdateFromColor(Color color)
    {
        UpdateFromColor(color, null);
    }

    /// <summary>
    /// Core method for updating the color, considering throttling and the source of the color change.
    /// This method allows more granular control over the origin of the color change, useful for avoiding redundant updates.
    /// </summary>
    /// <param name="color">The new color to apply.</param>
    /// <param name="colorSourceChange">Indicates the source of the color change, to prevent recursive updates.</param>
    private void UpdateFromColor(Color color, ColorSourceChange? colorSourceChange)
    {
        if (!_isThrottling)
        {
            // If not throttling, apply the update immediately
            PerformUpdateFromColor(color, colorSourceChange);
            _isThrottling = true; // Enable throttling
            _throttleTimer.Start(); // Start the throttling timer
        }
        else
        {
            // Queue the color update for the next throttling cycle
            _pendingFullUpdateColor = color;
            _pendingFullUpdateSourceChange = colorSourceChange;
        }
    }

    /// <summary>
    /// Updates the color representation without affecting the "normal" value, throttled for performance.
    /// This method is useful when the normal component value should not change, but other components must be updated.
    /// </summary>
    /// <param name="color">The new color value to update to.</param>
    public void UpdateFromColorWithoutChangeNormalValue(Color color)
    {
        if (!_isThrottling)
        {
            // If not throttling, apply the update immediately
            PerformUpdateFromColorWithoutChangeNormalValue(color);
            _isThrottling = true; // Enable throttling
            _throttleTimer.Start(); // Start the throttling timer
        }
        else
        {
            // Queue the new color for the next throttling cycle
            _pendingNewColor = color;
        }
    }

    /// <summary>
    /// Performs the actual update of all color properties based on the provided color.
    /// This method handles conversion to different color models and updates each respective property accordingly.
    /// </summary>
    /// <param name="color">The new color value to update.</param>
    /// <param name="colorSourceChange">Indicates the source of the color change to avoid circular updates.</param>
    private void PerformUpdateFromColor(Color color, ColorSourceChange? colorSourceChange)
    {
        if (_colorUpdating)
            return; // Prevent recursive updates

        _colorUpdating = true;

        // Convert the input color to different color spaces
        var rgb = ColorSpace.Net.Colors.Rgb.FromRgb(color.R, color.G, color.B);
        var hsv = _hsvConverter.ConvertFrom(rgb);
        var lab = _labConverter.ConvertFrom(rgb);
        var cmyk = _cmykConverter.ConvertFrom(rgb);

        // Update HSV properties if the change did not originate from HSV
        if (colorSourceChange is not ColorSourceChange.FromHsv or null)
        {
            Hsv.Hue = (int)Math.Round(hsv.H);
            Hsv.Saturation = (int)Math.Round(hsv.S * 100m);
            Hsv.Value = (int)Math.Round(hsv.V * 100m);
        }

        // Update RGB properties if the change did not originate from RGB
        if (colorSourceChange is not ColorSourceChange.FromRgb or null)
        {
            Rgb.Red = rgb.R;
            Rgb.Green = rgb.G;
            Rgb.Blue = rgb.B;
        }

        // Update Lab properties if the change did not originate from Lab
        if (colorSourceChange is not ColorSourceChange.FromLab or null)
        {
            Lab.Lightness = (int)Math.Round(lab.L);
            Lab.A = (int)Math.Round(lab.A);
            Lab.B = (int)Math.Round(lab.B);
        }

        // Update CMYK properties if the change did not originate from CMYK
        if (colorSourceChange is not ColorSourceChange.FromCmyk or null)
        {
            Cmyk.Cyan = (int)Math.Round(cmyk.C * 100m);
            Cmyk.Magenta = (int)Math.Round(cmyk.M * 100m);
            Cmyk.Yellow = (int)Math.Round(cmyk.Y * 100m);
            Cmyk.Key = (int)Math.Round(cmyk.K * 100m);
        }

        // Update Hex value if the change did not originate from Hex
        if (colorSourceChange is not ColorSourceChange.FromHex or null)
        {
            Hex.Value = color.ToString();
        }

        // Update the current color
        _color = color;

        // Notify listeners about the color change
        ColorChanged?.Invoke(this, EventArgs.Empty);

        _colorUpdating = false;
    }

    /// <summary>
    /// Performs the actual update of all color properties based on the provided color, without changing the "normal" component value.
    /// This method handles conversion to different color models and updates each respective property accordingly, ensuring the "normal" component remains unchanged.
    /// </summary>
    /// <param name="color">The new color value to update.</param>
    private void PerformUpdateFromColorWithoutChangeNormalValue(Color color)
    {
        if (_colorUpdating)
            return;

        _colorUpdating = true;

        // Convert the input color to different color spaces
        var rgb = ColorSpace.Net.Colors.Rgb.FromRgb(color.R, color.G, color.B);
        var hsv = _hsvConverter.ConvertFrom(rgb);
        var lab = _labConverter.ConvertFrom(rgb);
        var cmyk = _cmykConverter.ConvertFrom(rgb);

        // Update HSV properties unless the normal component type is one of the HSV components
        if (NormalComponentType is not NormalComponentType.Hsv_H)
            Hsv.Hue = (int)Math.Round(hsv.H);
        if (NormalComponentType is not NormalComponentType.Hsv_S)
            Hsv.Saturation = (int)Math.Round(hsv.S * 100m);
        if (NormalComponentType is not NormalComponentType.Hsv_V)
            Hsv.Value = (int)Math.Round(hsv.V * 100m);

        // Update RGB properties unless the normal component type is one of the RGB components
        if (NormalComponentType is not NormalComponentType.Rgb_R)
            Rgb.Red = rgb.R;
        if (NormalComponentType is not NormalComponentType.Rgb_G)
            Rgb.Green = rgb.G;
        if (NormalComponentType is not NormalComponentType.Rgb_B)
            Rgb.Blue = rgb.B;

        // Update Lab properties unless the normal component type is one of the Lab components
        if (NormalComponentType is not NormalComponentType.Lab_L)
            Lab.Lightness = (int)Math.Round(lab.L);
        if (NormalComponentType is not NormalComponentType.Lab_A)
            Lab.A = (int)Math.Round(lab.A);
        if (NormalComponentType is not NormalComponentType.Lab_B)
            Lab.B = (int)Math.Round(lab.B);

        // Update CMYK properties
        Cmyk.Cyan = (int)Math.Round(cmyk.C * 100m);
        Cmyk.Magenta = (int)Math.Round(cmyk.M * 100m);
        Cmyk.Yellow = (int)Math.Round(cmyk.Y * 100m);
        Cmyk.Key = (int)Math.Round(cmyk.K * 100m);

        // Update Hex value
        Hex.Value = color.ToString();

        // Update the current color
        _color = color;

        // Notify listeners about the color change
        ColorChanged?.Invoke(this, EventArgs.Empty);

        _colorUpdating = false;
    }

    #endregion

    #region Events Subscription

    /// <summary>
    /// Handles the throttled timer tick event to process any pending updates.
    /// This helps reduce frequent property updates by applying batched changes.
    /// </summary>
    private void OnThrottleTimerTick(object? sender, EventArgs e)
    {
        // Stop the timer and reset the throttling flag
        _throttleTimer.Stop();
        _isThrottling = false;

        // Apply the pending updates if there are queued color changes
        if (_pendingNewColor.HasValue)
        {
            PerformUpdateFromColorWithoutChangeNormalValue(_pendingNewColor.Value);
            _pendingNewColor = null; // Clear the pending color
        }

        if (_pendingFullUpdateColor.HasValue)
        {
            PerformUpdateFromColor(_pendingFullUpdateColor.Value, _pendingFullUpdateSourceChange);
            _pendingFullUpdateColor = null; // Clear the pending color
            _pendingFullUpdateSourceChange = null;
        }
    }

    /// <summary>
    /// Handles property changes for the HSV color model.
    /// Updates other color models to reflect changes originating from HSV.
    /// </summary>
    private void HsvColorChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_colorUpdating)
            return;

        var hsv = ColorSpace.Net.Colors.Hsv.FromHsv(Hsv.Hue, Hsv.Saturation / 100m, Hsv.Value / 100m);
        var rgb = _rgbConverter.ConvertFrom(hsv);
        var color = Color.FromArgb(Color.A, rgb.R, rgb.G, rgb.B);
        UpdateFromColor(color, ColorSourceChange.FromHsv);
    }

    /// <summary>
    /// Handles property changes for the RGB color model.
    /// Updates other color models to reflect changes originating from RGB.
    /// </summary>
    private void RgbColorChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_colorUpdating)
            return;

        var rgb = ColorSpace.Net.Colors.Rgb.FromRgb(Rgb.Red, Rgb.Green, Rgb.Blue);
        var color = Color.FromArgb(Color.A, rgb.R, rgb.G, rgb.B);
        UpdateFromColor(color, ColorSourceChange.FromRgb);
    }

    /// <summary>
    /// Handles property changes for the Lab color model.
    /// Updates other color models to reflect changes originating from Lab.
    /// </summary>
    private void LabColorChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_colorUpdating)
            return;

        var lab = ColorSpace.Net.Colors.Lab.FromLab(Lab.Lightness, Lab.A, Lab.B);
        var rgb = _rgbConverter.ConvertFrom(lab);
        var color = Color.FromArgb(Color.A, rgb.R, rgb.G, rgb.B);
        UpdateFromColor(color, ColorSourceChange.FromLab);
    }

    /// <summary>
    /// Handles property changes for the CMYK color model.
    /// Updates other color models to reflect changes originating from CMYK.
    /// </summary>
    private void CmykColorChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_colorUpdating)
            return;

        var cmyk = ColorSpace.Net.Colors.Cmyk.FromCmyk(Cmyk.Cyan / 100m, Cmyk.Magenta / 100m, Cmyk.Yellow / 100m, Cmyk.Key / 100m);
        var rgb = _rgbConverter.ConvertFrom(cmyk);
        var color = Color.FromArgb(Color.A, rgb.R, rgb.G, rgb.B);
        UpdateFromColor(color, ColorSourceChange.FromCmyk);
    }

    /// <summary>
    /// Handles property changes for the Hex color representation.
    /// Updates other color models to reflect changes originating from Hex.
    /// </summary>
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