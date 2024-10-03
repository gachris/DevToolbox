using System.Windows.Forms;

namespace DevToolbox.Wpf.Windows.Snap;

/// <summary>
/// Represents the result of a snap operation for a window.
/// </summary>
internal class SnapResult
{
    /// <summary>
    /// Gets a value indicating whether the window is snapped.
    /// </summary>
    public bool IsSnapped { get; }

    /// <summary>
    /// Gets the snap bounds of the window if it is snapped.
    /// </summary>
    public SnapBounds SnapBounds { get; }

    /// <summary>
    /// Gets the monitor where the window is snapped.
    /// </summary>
    public Screen Monitor { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SnapResult"/> class.
    /// </summary>
    /// <param name="isSnapped">Indicates whether the window is snapped.</param>
    /// <param name="monitor">The monitor where the window is snapped.</param>
    /// <param name="snapBounds">The snap bounds of the window.</param>
    public SnapResult(bool isSnapped, Screen monitor, SnapBounds snapBounds)
        => (IsSnapped, Monitor, SnapBounds) = (isSnapped, monitor, snapBounds);
}
