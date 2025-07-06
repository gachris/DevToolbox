using System.Collections.Generic;
using System.Windows;
using DevToolbox.Wpf.Controls;
using DevToolbox.Wpf.Windows;

namespace DevToolbox.Wpf.Services;

internal class DockingDragServices
{
    #region Fields/Consts

    private Point _offset;
    private readonly List<IDropSurface> _surfaces = [];
    private readonly List<IDropSurface> _surfacesWithDragOver = [];
    private LayoutBaseWindow? _window;

    #endregion

    #region Properties

    public LayoutBaseWindow? Window => _window;

    #endregion

    #region Methods

    public void Register(IDropSurface surface)
    {
        if (!_surfaces.Contains(surface))
        {
            _surfaces.Add(surface);
        }
    }

    public void Unregister(IDropSurface surface)
    {
        _surfaces.Remove(surface);
    }

    public void StartDrag(LayoutBaseWindow wnd, Point point, Point offset)
    {
        _offset = offset;

        _window = wnd;

        if (_offset.X >= _window.Width)
            _offset.X = _window.Width / 2;

        _window.Left = point.X - _offset.X;
        _window.Top = point.Y - _offset.Y;
        _window.Show();

        foreach (var surface in _surfaces)
        {
            if (surface.SurfaceRectangle.Contains(point))
            {
                _surfacesWithDragOver.Add(surface);
                surface.OnDragEnter(point);
            }
        }
    }

    public void MoveDrag(Point point)
    {
        if (_window == null) return;

        _window.Left = point.X - _offset.X;
        _window.Top = point.Y - _offset.Y;

        var enteringSurfaces = new List<IDropSurface>();

        foreach (var surface in _surfaces)
        {
            if (surface.SurfaceRectangle.Contains(point))
            {
                if (!_surfacesWithDragOver.Contains(surface))
                    enteringSurfaces.Add(surface);
                else
                    surface.OnDragOver(point);
            }
            else if (_surfacesWithDragOver.Contains(surface))
            {
                _surfacesWithDragOver.Remove(surface);
                surface.OnDragLeave(point);
            }
        }

        foreach (var surface in enteringSurfaces)
        {
            _surfacesWithDragOver.Add(surface);
            surface.OnDragEnter(point);
        }
    }

    public void EndDrag(Point point)
    {
        var dropSufrace = default(IDropSurface?);

        foreach (var surface in _surfaces)
        {
            if (surface.SurfaceRectangle.Contains(point))
            {
                if (surface.OnDrop(point))
                {
                    dropSufrace = surface;
                    break;
                }
            }
        }

        foreach (var surface in _surfacesWithDragOver)
        {
            if (surface != dropSufrace) surface.OnDragLeave(point);
        }

        _surfacesWithDragOver.Clear();

        if (dropSufrace != null) _window?.Close();

        _window = null;
    }

    #endregion
}
