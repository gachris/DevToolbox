namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Specifies the initial positioning types for the zoom and pan control.
/// </summary>
public enum ZoomAndPanInitialPositionType
{
    /// <summary>
    /// The default initial position, typically centered or set by default settings.
    /// </summary>
    Default,

    /// <summary>
    /// The initial position set to fit the content within the available screen space.
    /// </summary>
    FitScreen,

    /// <summary>
    /// The initial position set to fill the entire screen with the content.
    /// </summary>
    FillScreen,

    /// <summary>
    /// The initial position set to center the content at 100% zoom level.
    /// </summary>
    OneHundredPercentCentered,
}