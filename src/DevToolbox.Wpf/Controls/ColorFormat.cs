namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Defines different formats for representing colors.
/// </summary>
public enum ColorFormat
{
    /// <summary>
    /// HTML color format (e.g., "#FF5733" or "rgba(255, 87, 51, 1)").
    /// Commonly used in web development.
    /// </summary>
    HTML,

    /// <summary>
    /// Standard hexadecimal color format (e.g., "#FF5733").
    /// A common format for specifying colors in various systems.
    /// </summary>
    Hex,

    /// <summary>
    /// Delphi-style hexadecimal color format.
    /// Used in Delphi programming (e.g., "$00FF5733").
    /// </summary>
    DelphiHex,

    /// <summary>
    /// Visual Basic-style hexadecimal color format.
    /// Used in VB programming (e.g., <c>&amp;H00FF5733</c>).
    /// </summary>
    VBHex,

    /// <summary>
    /// RGB color format (Red, Green, Blue) as integers (e.g., "255, 87, 51").
    /// Represents the intensity of each color channel.
    /// </summary>
    RGB,

    /// <summary>
    /// RGB color format as floating-point numbers between 0 and 1 (e.g., "1.0, 0.34, 0.2").
    /// Used in contexts where normalized values are preferred.
    /// </summary>
    RGBFloat,

    /// <summary>
    /// HSV color format (Hue, Saturation, Value).
    /// Used for color manipulation in terms of hue, saturation, and brightness.
    /// </summary>
    HSV,

    /// <summary>
    /// HSL color format (Hue, Saturation, Lightness).
    /// Used to represent colors in terms of hue, saturation, and lightness.
    /// </summary>
    HSL,

    /// <summary>
    /// Long integer color format.
    /// Represents a color as a single integer value.
    /// </summary>
    Long
}