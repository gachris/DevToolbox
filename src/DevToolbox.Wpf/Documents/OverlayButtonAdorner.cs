using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace DevToolbox.Wpf.Documents;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class OverlayButtonAdorner : Adorner
{
    private UIElement _child = null!;

    protected override int VisualChildrenCount => 1;

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

    public OverlayButtonAdorner(UIElement adornedElement) : base(adornedElement)
    {
        Child = adornedElement;
    }

    protected override Visual GetVisualChild(int index)
    {
        return index != 0 ? throw new ArgumentOutOfRangeException() : (Visual)_child;
    }

    protected override Size MeasureOverride(Size constraint)
    {
        _child.Measure(constraint);
        return _child.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _child.Arrange(new Rect(new Point(0, 0), finalSize));
        return _child.DesiredSize;
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member