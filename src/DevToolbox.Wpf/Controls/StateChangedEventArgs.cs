using System;

namespace DevToolbox.Wpf.Controls;

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
