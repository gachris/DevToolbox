namespace DevToolbox.Wpf.Markup;

/// <summary>
/// Specifies logical names for button chrome visual state brushes used in styling buttons.
/// These keys are typically used in conjunction with resource dictionaries and component resource keys.
/// </summary>
public enum ButtonChromeBrushKey
{
    /// <summary>
    /// The default background brush used when the button is in its normal state.
    /// </summary>
    Background,

    /// <summary>
    /// The default foreground brush (typically for text) when the button is in its normal state.
    /// </summary>
    Foreground,

    /// <summary>
    /// The default border brush when the button is in its normal state.
    /// </summary>
    BorderBrush,

    /// <summary>
    /// The background brush used when the mouse is hovering over the button.
    /// </summary>
    MouseOverBackground,

    /// <summary>
    /// The foreground brush used when the mouse is hovering over the button.
    /// </summary>
    MouseOverForeground,

    /// <summary>
    /// The border brush used when the mouse is hovering over the button.
    /// </summary>
    MouseOverBorderBrush,

    /// <summary>
    /// The background brush used when the button is pressed (active click).
    /// </summary>
    PressedBackground,

    /// <summary>
    /// The foreground brush used when the button is pressed.
    /// </summary>
    PressedForeground,

    /// <summary>
    /// The border brush used when the button is pressed.
    /// </summary>
    PressedBorderBrush,

    /// <summary>
    /// The background brush used when the button is disabled.
    /// </summary>
    DisabledBackground,

    /// <summary>
    /// The foreground brush used when the button is disabled.
    /// </summary>
    DisabledForeground,

    /// <summary>
    /// The border brush used when the button is disabled.
    /// </summary>
    DisabledBorderBrush,

    /// <summary>
    /// The background brush used when the window is inactive.
    /// </summary>
    InactiveBackground,

    /// <summary>
    /// The foreground brush used when the window is inactive.
    /// </summary>
    InactiveForeground,

    /// <summary>
    /// The border brush used when the window is inactive.
    /// </summary>
    InactiveBorderBrush
}
