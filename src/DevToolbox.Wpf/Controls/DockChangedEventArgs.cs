using System;
using System.Windows.Controls;

namespace DevToolbox.Wpf.Controls;

public class DockChangedEventArgs : EventArgs
{
    public Dock OldValue { get; }

    public Dock NewValue { get; }

    public DockChangedEventArgs(Dock oldValue, Dock newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}