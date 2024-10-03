using System;

namespace DevToolbox.Wpf.Windows.Snap;

/// <summary>
/// Specifies the bounds for snapping a window.
/// </summary>
[Flags]
public enum SnapBounds
{
    /// <summary>
    /// No snapping bounds.
    /// </summary>
    None = 0,

    /// <summary>
    /// Snap to the left side of the screen.
    /// </summary>
    Left = 1,

    /// <summary>
    /// Snap to the right side of the screen.
    /// </summary>
    Right = 2,

    /// <summary>
    /// Snap to the top side of the screen.
    /// </summary>
    Top = 4,

    /// <summary>
    /// Snap to the bottom side of the screen.
    /// </summary>
    Bottom = 8
}
