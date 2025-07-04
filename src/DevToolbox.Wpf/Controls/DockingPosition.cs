namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Specifies possible docking positions for a <see cref="DockableControl"/> relative to its container or pane.
/// </summary>
public enum DockingPosition
{
    /// <summary>
    /// Dock the control to the top edge of the container.
    /// </summary>
    Top,

    /// <summary>
    /// Dock the control to the bottom edge of the container.
    /// </summary>
    Bottom,

    /// <summary>
    /// Dock the control to the left edge of the container.
    /// </summary>
    Left,

    /// <summary>
    /// Dock the control to the right edge of the container.
    /// </summary>
    Right,

    /// <summary>
    /// Dock the control to the top edge of an existing pane.
    /// </summary>
    PaneTop,

    /// <summary>
    /// Dock the control to the bottom edge of an existing pane.
    /// </summary>
    PaneBottom,

    /// <summary>
    /// Dock the control to the left edge of an existing pane.
    /// </summary>
    PaneLeft,

    /// <summary>
    /// Dock the control to the right edge of an existing pane.
    /// </summary>
    PaneRight,

    /// <summary>
    /// Insert the control as a tab inside an existing pane.
    /// </summary>
    PaneInto
}
