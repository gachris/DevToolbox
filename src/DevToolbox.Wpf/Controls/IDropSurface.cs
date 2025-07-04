using System.Windows;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Defines a surface that can accept drag-and-drop operations within a <see cref="DockManager"/>.
/// </summary>
public interface IDropSurface
{
    /// <summary>
    /// Gets the screen area that represents the drop target surface.
    /// </summary>
    Rect SurfaceRectangle { get; }

    /// <summary>
    /// Called when a drag enters the surface area.
    /// </summary>
    /// <param name="point">The screen coordinates of the drag event.</param>
    void OnDragEnter(Point point);

    /// <summary>
    /// Called repeatedly while an object is dragged over the surface.
    /// </summary>
    /// <param name="point">The screen coordinates of the drag event.</param>
    void OnDragOver(Point point);

    /// <summary>
    /// Called when a drag leaves the surface area.
    /// </summary>
    /// <param name="point">The screen coordinates of the drag event.</param>
    void OnDragLeave(Point point);

    /// <summary>
    /// Called when a drop occurs on the surface.
    /// </summary>
    /// <param name="point">The screen coordinates of the drop event.</param>
    /// <returns><c>true</c> if the drop was successfully handled; otherwise, <c>false</c>.</returns>
    bool OnDrop(Point point);
}