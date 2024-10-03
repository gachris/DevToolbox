using System;

namespace DevToolbox.Wpf.Windows.Snap;

/// <summary>
/// Provides data for the <see cref="WindowSnap.EdgeOffsetChanged"/> event.
/// </summary>
public class EdgeOffsetChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the edge offset associated with the event.
    /// </summary>
    public EdgeOffset Offset { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EdgeOffsetChangedEventArgs"/> class.
    /// </summary>
    /// <param name="offset">The edge offset associated with the event.</param>
    public EdgeOffsetChangedEventArgs(EdgeOffset offset) => Offset = offset;
}
