namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Defines the different types of selection modes available in a user interface.
/// </summary>
public enum SelectionType
{
    /// <summary>
    /// A single item can be selected at a time.
    /// </summary>
    Single,

    /// <summary>
    /// Multiple items can be selected at once, typically by using a modifier key (e.g., Ctrl).
    /// </summary>
    Multiple,

    /// <summary>
    /// An extended selection mode where multiple contiguous or non-contiguous items can be selected.
    /// </summary>
    Extended,

    /// <summary>
    /// Selection using a lasso tool, where a custom shape is drawn to select items.
    /// </summary>
    Lasso,

    /// <summary>
    /// Selection using a rubberband, which typically involves dragging a rectangular area to select multiple items.
    /// </summary>
    Rubberband
}