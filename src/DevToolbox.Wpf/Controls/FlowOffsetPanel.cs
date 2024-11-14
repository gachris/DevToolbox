using System.Windows;
using System.Windows.Controls;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A custom panel that arranges its child elements with specified horizontal (OffsetX) and vertical (OffsetY) offsets.
/// </summary>
public class FlowOffsetPanel : Panel
{
    #region Fields/Consts

    /// <summary>
    /// Identifies the OffsetX dependency property.
    /// This property specifies the horizontal offset between child elements.
    /// </summary>
    public static readonly DependencyProperty OffsetXProperty =
        DependencyProperty.Register(
            nameof(OffsetX),
            typeof(double),
            typeof(FlowOffsetPanel),
            new FrameworkPropertyMetadata(1.5D, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange));

    /// <summary>
    /// Identifies the OffsetY dependency property.
    /// This property specifies the vertical offset between child elements.
    /// </summary>
    public static readonly DependencyProperty OffsetYProperty =
        DependencyProperty.Register(
            nameof(OffsetY),
            typeof(double),
            typeof(FlowOffsetPanel),
            new FrameworkPropertyMetadata(4.0D, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the horizontal offset between child elements.
    /// </summary>
    public double OffsetX
    {
        get => (double)GetValue(OffsetXProperty);
        set => SetValue(OffsetXProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical offset between child elements.
    /// </summary>
    public double OffsetY
    {
        get => (double)GetValue(OffsetYProperty);
        set => SetValue(OffsetYProperty, value);
    }

    #endregion

    #region Methods Overrides

    /// <summary>
    /// Measures the size required for all child elements and determines the size of the panel.
    /// </summary>
    /// <param name="availableSize">The available size that the panel can use to arrange its children.</param>
    /// <returns>The size required by the panel.</returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        var index = 0;
        var cWidth = 0.0;
        var cHeight = 0.0;

        foreach (UIElement child in Children)
        {
            child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            cWidth += child.DesiredSize.Width;

            if (index == Children.Count - 1)
                cHeight += child.DesiredSize.Height;

            if (Children.Count > 1 && index < Children.Count - 1)
            {
                cWidth += OffsetX;
                cHeight += OffsetY;
            }

            index++;
        }

        return new Size(cWidth, cHeight);
    }

    /// <summary>
    /// Arranges the child elements within the panel based on their measured sizes and the specified offsets.
    /// </summary>
    /// <param name="finalSize">The final area within the parent that the panel should use to arrange itself and its children.</param>
    /// <returns>The actual size used by the panel.</returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var x = 0.0;
        var y = 0.0;

        foreach (UIElement child in Children)
        {
            var rec = new Rect(new Point(x, y), child.DesiredSize);
            child.Arrange(rec);

            if (Children.Count > 1)
            {
                y += OffsetY;
                x += child.DesiredSize.Width + OffsetX;
            }
        }

        return finalSize;
    }

    #endregion
}