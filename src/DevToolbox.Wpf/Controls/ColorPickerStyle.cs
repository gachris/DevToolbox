namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Enum representing different styles for the ColorPicker control, determining
/// the available color and alpha selection options.
/// </summary>
public enum ColorPickerStyle
{
    /// <summary>
    /// Standard style without alpha (transparency) control.
    /// </summary>
    Standard,

    /// <summary>
    /// Full style that provides access to all color components but without alpha control.
    /// </summary>
    Full,

    /// <summary>
    /// Standard style with alpha (transparency) control enabled.
    /// </summary>
    StandardWithAlpha,

    /// <summary>
    /// Full style with alpha (transparency) control enabled.
    /// </summary>
    FullWithAlpha
}
