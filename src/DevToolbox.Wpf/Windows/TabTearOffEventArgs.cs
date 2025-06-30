using System;

namespace DevToolbox.Wpf.Windows;

/// <summary>
/// Arguments for when a tab is torn off to a new window.
/// </summary>
public class TabTearOffEventArgs : EventArgs
{
    /// <summary>
    /// Gets the item that was torn off.
    /// </summary>
    public object Item { get; }

    /// <summary>
    /// Gets or sets the custom <see cref="TabsWindow"/> to host the torn-off item.
    /// </summary>
    public TabsWindow? NewWindow { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="TabTearOffEventArgs"/>.
    /// </summary>
    /// <param name="item">The item being torn off.</param>
    public TabTearOffEventArgs(object item) => Item = item;
}
