namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents the different states of the capture process in the <see cref="EyeDropper"/> control.
/// </summary>
public enum CaptureState
{
    /// <summary>
    /// Indicates that the capture process has started.
    /// </summary>
    Started,

    /// <summary>
    /// Indicates that the capture process is ongoing.
    /// </summary>
    Continued,

    /// <summary>
    /// Indicates that the capture process has finished.
    /// </summary>
    Finished
}