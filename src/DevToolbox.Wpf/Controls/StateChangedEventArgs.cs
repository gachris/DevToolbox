using System;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Provides data for the <see cref="DockableControl.StateChanged"/> event,
/// containing the previous and current <see cref="State"/> of the control.
/// </summary>
public class StateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the <see cref="State"/> of the control before the change.
    /// </summary>
    public State OldValue { get; }

    /// <summary>
    /// Gets the <see cref="State"/> of the control after the change.
    /// </summary>
    public State NewValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StateChangedEventArgs"/> class with
    /// the specified old and new state values.
    /// </summary>
    /// <param name="oldValue">The state before the change.</param>
    /// <param name="newValue">The state after the change.</param>
    public StateChangedEventArgs(State oldValue, State newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}
