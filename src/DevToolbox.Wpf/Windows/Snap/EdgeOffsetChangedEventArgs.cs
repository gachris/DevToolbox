using System;

namespace DevToolbox.Wpf.Windows.Snap;

/// <summary>
/// Provides data for the snap result events.
/// </summary>
public class SnapResultEventArgs : EventArgs
{
    /// <summary>
    /// Gets a value indicating whether the window is currently snapped.
    /// </summary>
    public bool IsSnapped { get; }

    /// <summary>
    /// Gets the snapping bounds for the window.
    /// </summary>
    public SnapBounds SnapBounds { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SnapResultEventArgs"/> class.
    /// </summary>
    /// <param name="isSnapped">Indicates whether the window is snapped.</param>
    /// <param name="snapBounds">The bounds to which the window is snapped.</param>
    public SnapResultEventArgs(bool isSnapped, SnapBounds snapBounds) =>
        (IsSnapped, SnapBounds) = (isSnapped, snapBounds);
}