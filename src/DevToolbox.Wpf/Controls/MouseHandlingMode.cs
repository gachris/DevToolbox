namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Specifies the different modes of mouse handling for the zoom and pan control.
/// </summary>
public enum MouseHandlingMode
{
    /// <summary>
    /// No mouse handling mode is active.
    /// </summary>
    None,

    /// <summary>
    /// The mode for dragging rectangles within the zoom and pan control.
    /// </summary>
    DraggingRectangles,

    /// <summary>
    /// The mode for panning the view when the mouse is dragged.
    /// </summary>
    Panning,

    /// <summary>
    /// The mode for zooming in or out using the mouse.
    /// </summary>
    Zooming,

    /// <summary>
    /// The mode for dragging a zoom rectangle to define a zoom area.
    /// </summary>
    DragZooming,
}