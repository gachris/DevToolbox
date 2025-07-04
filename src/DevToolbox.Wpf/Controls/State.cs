namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Defines the possible layout states for controls managed by a <see cref="DockManager"/>.
/// </summary>
public enum State
{
    /// <summary>
    /// The control is displayed in its own floating window.
    /// </summary>
    Window,

    /// <summary>
    /// The control is docked within the <see cref="DockManager"/> layout.
    /// </summary>
    Docking,

    /// <summary>
    /// The control's content is shown as a tabbed document in the document area.
    /// </summary>
    Document,

    /// <summary>
    /// The control is in auto-hide mode (hidden at the edge until hovered).
    /// </summary>
    AutoHide,

    /// <summary>
    /// The control is hidden and not visible until reactivated.
    /// </summary>
    Hidden
}
