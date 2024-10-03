namespace DevToolbox.Wpf.Windows;

/// <summary>
/// Specifies the results of hit testing in the custom window.
/// </summary>
public enum HitTestResult
{
    /// <summary>
    /// No hit test result. Indicates that no valid hit test was performed.
    /// </summary>
    None,

    /// <summary>
    /// Hit test result indicating the minimize button.
    /// </summary>
    Min,

    /// <summary>
    /// Hit test result indicating the maximize button.
    /// </summary>
    Max,

    /// <summary>
    /// Hit test result indicating the restore button.
    /// </summary>
    Restore,

    /// <summary>
    /// Hit test result indicating the close button.
    /// </summary>
    Close,

    /// <summary>
    /// Hit test result indicating the help button.
    /// </summary>
    Help
}
