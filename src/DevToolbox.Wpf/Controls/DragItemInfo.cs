using System.Windows;
using System.Windows.Media;

namespace DevToolbox.Wpf.Controls;

internal class DragItemInfo
{
    public DesignLayer Item { get; }

    public Point StartPosition { get; }

    public Transform Transform { get; }

    public DragItemInfo(DesignLayer item, Point startPosition, Transform rotateTransform)
    {
        Item = item;
        StartPosition = startPosition;
        Transform = rotateTransform;
    }
}