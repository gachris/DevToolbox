using System.Windows;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Defines the orientation for flipping content in a control.
/// This enum can be used to specify whether the content should remain in its normal orientation, 
/// or be flipped horizontally or vertically.
/// </summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public enum FlipOrientation
{
    /// <summary>
    /// No flipping, content remains in its normal orientation.
    /// </summary>
    Normal,

    /// <summary>
    /// Flips the content horizontally (along the vertical axis).
    /// </summary>
    Horizontal,

    /// <summary>
    /// Flips the content vertically (along the horizontal axis).
    /// </summary>
    Vertical
}