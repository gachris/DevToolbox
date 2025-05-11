using System;

namespace DevToolbox.Wpf.Controls;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class StateChangedEventArgs : EventArgs
{
    public State OldValue { get; }

    public State NewValue { get; }

    public StateChangedEventArgs(State oldValue, State newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member