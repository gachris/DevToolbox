using System.Windows;

namespace DevToolbox.Wpf.Controls;

public interface IDropSurface
{
    Rect SurfaceRectangle { get; }

    void OnDragEnter(Point point);

    void OnDragOver(Point point);

    void OnDragLeave(Point point);

    bool OnDrop(Point point);
}
