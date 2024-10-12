namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Defines how the lasso selection area is arranged in relation to bounds.
/// </summary>
public enum LassoArrangeType
{
    /// <summary>
    /// The selection area is arranged based on the bounds of the lasso itself.
    /// </summary>
    LassoBounds,

    /// <summary>
    /// The selection area is clipped or constrained to a specific boundary.
    /// </summary>
    ClipBounds
}