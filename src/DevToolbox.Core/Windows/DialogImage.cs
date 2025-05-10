namespace DevToolbox.Core.Windows;

/// <summary>
/// Specifies the icon to display in a dialog window.
/// </summary>
public enum DialogImage
{
    /// <summary>
    /// No icon is displayed.
    /// </summary>
    None,

    /// <summary>
    /// Displays an informational icon (typically an 'i' in a circle).
    /// </summary>
    Info,

    /// <summary>
    /// Displays a warning icon (typically an exclamation mark in a triangle).
    /// </summary>
    Warning,

    /// <summary>
    /// Displays an error icon (typically a red 'X' or stop sign).
    /// </summary>
    Error
}