using System;
using System.Windows.Controls;

namespace DevToolbox.Wpf.Controls;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member