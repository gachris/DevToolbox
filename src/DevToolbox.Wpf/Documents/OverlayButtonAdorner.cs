using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace DevToolbox.Wpf.Documents;

/// <summary>
/// An <see cref="Adorner"/> that displays a single overlay element
/// (typically a button) positioned over its adorned UI element.
/// </summary>
public class OverlayButtonAdorner : Adorner
{
    private UIElement _child = null!;

    /// <summary>
    /// Gets the number of visual children for this adorner (always 1).
    /// </summary>
    protected override int VisualChildrenCount => 1;

    /// <summary>
    /// Gets or sets the child <see cref="UIElement"/> that is rendered as the overlay.
    /// </summary>
    public UIElement Child
    {
        get => _child;
        set
        {
            if (_child != null)
            {
                RemoveVisualChild(_child);
            }

            _child = value;

            if (_child != null)
            {
                AddVisualChild(_child);
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OverlayButtonAdorner"/> class,
    /// adorning the specified UI element.
    /// </summary>
    /// <param name="adornedElement">The element to adorn with the overlay.</param>
    public OverlayButtonAdorner(UIElement adornedElement) : base(adornedElement)
    {
        Child = adornedElement;
    }

    /// <summary>
    /// Returns the specified visual child of this adorner.
    /// </summary>
    /// <param name="index">The zero-based index of the visual child.
    /// Must be 0 to retrieve the <see cref="Child"/> element.</param>
    /// <returns>The visual child at the specified index.</returns>
    protected override Visual GetVisualChild(int index)
    {
        return index != 0
            ? throw new ArgumentOutOfRangeException(nameof(index))
            : (Visual)_child;
    }

    /// <summary>
    /// Measures the overlay element with the given constraint.
    /// </summary>
    /// <param name="constraint">The maximum size available.</param>
    /// <returns>The desired size of the overlay element.</returns>
    protected override Size MeasureOverride(Size constraint)
    {
        _child.Measure(constraint);
        return _child.DesiredSize;
    }

    /// <summary>
    /// Arranges the overlay element to fill the adorned element's bounds.
    /// </summary>
    /// <param name="finalSize">The final size allocated by the layout system.</param>
    /// <returns>The actual size used by the overlay element.</returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        _child.Arrange(new Rect(new Point(0, 0), finalSize));
        return _child.DesiredSize;
    }
}
