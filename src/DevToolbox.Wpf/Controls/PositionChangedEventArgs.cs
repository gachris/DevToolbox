using System;
using System.Windows;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Provides data for the <see cref="Connector.PositionChanged"/> event.
/// </summary>
public class PositionChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the old position value before the position change.
    /// </summary>
    public Point OldValue { get; }

    /// <summary>
    /// Gets the new position value after the position change.
    /// </summary>
    public Point NewValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PositionChangedEventArgs"/> class.
    /// </summary>
    /// <param name="oldValue">The old position value.</param>
    /// <param name="newValue">The new position value.</param>
    public PositionChangedEventArgs(Point oldValue, Point newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}