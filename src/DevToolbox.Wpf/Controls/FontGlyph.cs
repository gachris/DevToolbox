using System;
using System.Windows;
using System.Windows.Controls;
using DevToolbox.Wpf.Extensions;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A custom control that displays a glyph from a font, with support for rotation and flip orientation.
/// The glyph is represented by a hex code string, which is converted to a character for rendering.
/// </summary>
public class FontGlyph : Control
{
    #region Fields/Consts

    /// <summary>
    /// Read-only dependency property key for storing the formatted code (glyph character).
    /// </summary>
    private static readonly DependencyPropertyKey FormattedCodePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(FormattedCode), typeof(string), typeof(FontGlyph), new PropertyMetadata(default));

    /// <summary>
    /// Dependency property for accessing the formatted code (glyph character).
    /// </summary>
    public static readonly DependencyProperty FormattedCodeProperty = FormattedCodePropertyKey.DependencyProperty;

    /// <summary>
    /// Dependency property for storing the hex code that represents the glyph.
    /// </summary>
    public static readonly DependencyProperty CodeProperty =
        DependencyProperty.Register(nameof(Code), typeof(string), typeof(FontGlyph), new PropertyMetadata(default, OnCodeChanged));

    /// <summary>
    /// Dependency property for specifying the rotation angle of the glyph (0 to 360 degrees).
    /// </summary>
    public static readonly DependencyProperty RotationProperty =
        DependencyProperty.Register(nameof(Rotation), typeof(double), typeof(FontGlyph), new PropertyMetadata(default(double), RotationChanged, RotationCoerceValue));

    /// <summary>
    /// Dependency property for specifying the flip orientation (horizontal, vertical, or none).
    /// </summary>
    public static readonly DependencyProperty FlipOrientationProperty =
        DependencyProperty.Register(nameof(FlipOrientation), typeof(FlipOrientation), typeof(FontGlyph), new PropertyMetadata(default(FlipOrientation), FlipOrientationChanged));

    #endregion

    #region Properties

    /// <summary>
    /// Gets the resource key for the FontFamily associated with the FontGlyph.
    /// </summary>
    public static ComponentResourceKey FontFamilyKey => new(typeof(FontGlyph), nameof(FontFamilyKey));

    /// <summary>
    /// Gets the formatted glyph character derived from the hex code.
    /// </summary>
    public string FormattedCode => (string)GetValue(FormattedCodeProperty);

    /// <summary>
    /// Gets or sets the hex code representing the glyph character to display.
    /// </summary>
    public string Code
    {
        get => (string)GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    /// <summary>
    /// Gets or sets the rotation angle of the glyph, in degrees (0 to 360).
    /// </summary>
    public double Rotation
    {
        get => (double)GetValue(RotationProperty);
        set => SetValue(RotationProperty, value);
    }

    /// <summary>
    /// Gets or sets the orientation used to flip the glyph (horizontal or vertical).
    /// </summary>
    public FlipOrientation FlipOrientation
    {
        get => (FlipOrientation)GetValue(FlipOrientationProperty);
        set => SetValue(FlipOrientationProperty, value);
    }

    #endregion

    /// <summary>
    /// Static constructor to override the default style for the FontGlyph control.
    /// </summary>
    static FontGlyph()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FontGlyph), new FrameworkPropertyMetadata(typeof(FontGlyph)));
    }

    #region Methods

    /// <summary>
    /// Called when the Code property changes. Updates the FormattedCode property with the corresponding glyph character.
    /// </summary>
    private void OnCodeChanged(string oldValue, string newValue)
    {
        if (string.IsNullOrEmpty(newValue))
        {
            ClearValue(FormattedCodePropertyKey);
            return;
        }

        try
        {
            // Convert the hex code to a character
            char hexChar = (char)int.Parse(newValue, System.Globalization.NumberStyles.AllowHexSpecifier);
            SetValue(FormattedCodePropertyKey, char.ToString(hexChar));
        }
        catch (Exception)
        {
            ClearValue(FormattedCodePropertyKey);
        }
    }

    /// <summary>
    /// Called when the Rotation property changes. Updates the rotation of the glyph.
    /// </summary>
    private void RotationChanged(double oldValue, double newValue)
    {
        this.SetRotation();
    }

    /// <summary>
    /// Coerces the value of the Rotation property to be between 0 and 360 degrees.
    /// </summary>
    private static object RotationCoerceValue(double value)
    {
        return value < 0.0 ? 0.0 : value > 360.0 ? 360.0 : value;
    }

    /// <summary>
    /// Called when the FlipOrientation property changes. Updates the flip orientation of the glyph.
    /// </summary>
    private void FlipOrientationChanged(FlipOrientation oldValue, FlipOrientation newValue)
    {
        this.SetFlipOrientation();
    }

    /// <summary>
    /// Handles the Code property change event.
    /// </summary>
    private static void OnCodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var fontGlyph = (FontGlyph)d;
        fontGlyph.OnCodeChanged((string)e.OldValue, (string)e.NewValue);
    }

    /// <summary>
    /// Handles the FlipOrientation property change event.
    /// </summary>
    private static void FlipOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var fontGlyph = (FontGlyph)d;
        fontGlyph.FlipOrientationChanged((FlipOrientation)e.OldValue, (FlipOrientation)e.NewValue);
    }

    /// <summary>
    /// Handles the Rotation property change event.
    /// </summary>
    private static void RotationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var fontGlyph = (FontGlyph)d;
        fontGlyph.RotationChanged((double)e.OldValue, (double)e.NewValue);
    }

    /// <summary>
    /// Coerces the Rotation property value.
    /// </summary>
    private static object RotationCoerceValue(DependencyObject d, object value)
    {
        return RotationCoerceValue((double)value);
    }

    #endregion
}