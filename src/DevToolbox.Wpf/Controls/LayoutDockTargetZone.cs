namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Specifies which dock-overlay is shown when the user drags a pane.
/// </summary>
public enum LayoutDockTargetZone
{
    /// <summary>
    /// No overlay is displayed.
    /// </summary>
    None,

    /// <summary>
    /// Shows only the inner-pane "cross" targets (5 buttons: top, bottom, left, right, center).
    /// </summary>
    InnerCross,

    /// <summary>
    /// Shows the inner-pane cross (5 buttons) plus the four outer-edge arrows.
    /// </summary>
    InnerCrossWithOuterEdges,

    /// <summary>
    /// Shows the full inner grid (9 buttons: corners, inner edges, center) plus the four outer-edge arrows.
    /// </summary>
    FullGridWithOuterEdges
}
