using System.Windows;
using System.Windows.Media;

namespace DevToolbox.Wpf.Data;

internal class DragInfo
{
    public UIElement Element { get; }

    public Point StartPosition { get; }

    public Transform Transform { get; }

    public DragInfo(UIElement element, Point startPosition, Transform rotateTransform)
    {
        Element = element;
        StartPosition = startPosition;
        Transform = rotateTransform;
    }
}