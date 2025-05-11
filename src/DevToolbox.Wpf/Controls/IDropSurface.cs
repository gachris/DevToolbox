using System.Windows;

namespace DevToolbox.Wpf.Controls;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public interface IDropSurface
{
    Rect SurfaceRectangle { get; }

    void OnDragEnter(Point point);

    void OnDragOver(Point point);

    void OnDragLeave(Point point);

    bool OnDrop(Point point);
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
