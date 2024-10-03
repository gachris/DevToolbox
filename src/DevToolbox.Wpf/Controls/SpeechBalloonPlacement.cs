namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Specifies the placement options for a speech balloon.
/// </summary>
public enum SpeechBalloonPlacement
{
    /// <summary>
    /// No placement is specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// The speech balloon is placed to the left of the target element.
    /// </summary>
    Left = 1,

    /// <summary>
    /// The speech balloon is placed above the target element.
    /// </summary>
    Top = 2,

    /// <summary>
    /// The speech balloon is placed to the right of the target element.
    /// </summary>
    Right = 3,

    /// <summary>
    /// The speech balloon is placed below the target element.
    /// </summary>
    Bottom = 4,

    /// <summary>
    /// The speech balloon's placement is determined automatically based on available space.
    /// </summary>
    Auto = 5,
}
