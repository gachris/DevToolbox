using System;
using System.Windows.Media;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Provides data for the <see cref="EyeDropper.ColorChanged"/> event.
/// </summary>
public class ColorChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the old color value before the color change.
    /// </summary>
    public Brush OldValue { get; }

    /// <summary>
    /// Gets the new color value after the color change.
    /// </summary>
    public Brush NewValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorChangedEventArgs"/> class.
    /// </summary>
    /// <param name="oldValue">The old color value.</param>
    /// <param name="newValue">The new color value.</param>
    public ColorChangedEventArgs(Brush oldValue, Brush newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}