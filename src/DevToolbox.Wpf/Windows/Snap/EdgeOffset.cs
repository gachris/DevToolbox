using System.Windows;

namespace DevToolbox.Wpf.Windows.Snap;

/// <summary>
/// Represents the offsets for different types of window edges.
/// </summary>
public class EdgeOffset
{
    /// <summary>
    /// Gets or sets the size of the fixed frame offset.
    /// </summary>
    public Size FixedFrame { get; set; }

    /// <summary>
    /// Gets or sets the size of the resize frame offset.
    /// </summary>
    public Size ResizeFrame { get; set; }

    /// <summary>
    /// Gets or sets the size of the thick border offset.
    /// </summary>
    public Size ThickBorder { get; set; }

    /// <summary>
    /// Gets or sets the size of the thin border offset.
    /// </summary>
    public Size ThinBorder { get; set; }

    /// <summary>
    /// Gets the total offset size, calculated as the sum of all frame and border sizes.
    /// </summary>
    public Size Value => new Size
    {
        Width = FixedFrame.Width + ResizeFrame.Width +
                ThickBorder.Width + ThinBorder.Width,

        Height = FixedFrame.Height + ResizeFrame.Height +
                ThickBorder.Height + ThinBorder.Height
    };
}
