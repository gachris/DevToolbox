using System;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Provides data for the <see cref="LayoutDockItemsControl.StateChanged"/> event,
/// containing the previous and current <see cref="LayoutItemState"/> of the control.
/// </summary>
public class LayoutItemStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the <see cref="LayoutItemState"/> of the control before the change.
    /// </summary>
    public LayoutItemState OldValue { get; }

    /// <summary>
    /// Gets the <see cref="LayoutItemState"/> of the control after the change.
    /// </summary>
    public LayoutItemState NewValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LayoutItemStateChangedEventArgs"/> class with
    /// the specified old and new state values.
    /// </summary>
    /// <param name="oldValue">The state before the change.</param>
    /// <param name="newValue">The state after the change.</param>
    public LayoutItemStateChangedEventArgs(LayoutItemState oldValue, LayoutItemState newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}
