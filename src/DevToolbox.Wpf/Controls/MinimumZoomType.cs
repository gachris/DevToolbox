namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Specifies the types of minimum zoom levels available for the zoom and pan control.
/// </summary>
public enum MinimumZoomType
{
    /// <summary>
    /// Sets the minimum zoom level to fit the entire content within the available screen area.
    /// </summary>
    FitScreen,

    /// <summary>
    /// Sets the minimum zoom level to fill the entire screen with the content, possibly cropping some parts.
    /// </summary>
    FillScreen,

    /// <summary>
    /// Sets the minimum zoom level to a user-defined minimum zoom value.
    /// </summary>
    MinimumZoom
}