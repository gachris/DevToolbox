using System;
using System.Windows.Controls;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Provides data for the <see cref="LayoutDockItemsControl.DockChanged"/> event,
/// containing the previous and current <see cref="Dock"/> values of the control's docking position.
/// </summary>
public class LayoutDockChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the <see cref="Dock"/> value before the change.
    /// </summary>
    public Dock OldValue { get; }

    /// <summary>
    /// Gets the <see cref="Dock"/> value after the change.
    /// </summary>
    public Dock NewValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LayoutDockChangedEventArgs"/> class with
    /// the specified old and new docking values.
    /// </summary>
    /// <param name="oldValue">The docking position before the change.</param>
    /// <param name="newValue">The docking position after the change.</param>
    public LayoutDockChangedEventArgs(Dock oldValue, Dock newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}
