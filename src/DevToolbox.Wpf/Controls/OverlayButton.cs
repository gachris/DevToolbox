using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using DevToolbox.Wpf.Documents;
using DevToolbox.Wpf.Extensions;
using DevToolbox.Wpf.Windows;

namespace DevToolbox.Wpf.Controls;

internal class OverlayButton : ContentControl, IDropSurface
{
    #region Fields/Consts

    private OverlayWindow? _owner;
    private Canvas? _adornerControl;
    private OverlayButtonAdorner? _adorner;

    public static readonly DependencyProperty AdornerContentTemplateProperty =
        DependencyProperty.Register("AdornerContentTemplate", typeof(DataTemplate), typeof(OverlayButton), new FrameworkPropertyMetadata(default));

    public static readonly DependencyProperty DockingPositionProperty =
        DependencyProperty.Register("DockingPosition", typeof(DockingPosition), typeof(OverlayButton), new PropertyMetadata(default(DockingPosition)));

    #endregion

    #region Properties

    public DockingPosition DockingPosition
    {
        get => (DockingPosition)GetValue(DockingPositionProperty);
        set => SetValue(DockingPositionProperty, value);
    }

    public DataTemplate AdornerContentTemplate
    {
        get => (DataTemplate)GetValue(AdornerContentTemplateProperty);
        set => SetValue(AdornerContentTemplateProperty, value);
    }

    #endregion

    static OverlayButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(OverlayButton), new FrameworkPropertyMetadata(typeof(OverlayButton)));
    }

    #region Methods Overrides

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _owner = (OverlayWindow)Window.GetWindow(this);
        _owner?.DockManager.DragServices.Register(this);
    }

    #endregion

    #region IDropSurface

    public Rect SurfaceRectangle => !IsLoaded || PresentationSource.FromVisual(this) == null
                ? new Rect()
                : new Rect(PointToScreen(new Point(0, 0)), new Size(ActualWidth, ActualHeight));

    public void OnDragEnter(Point point)
    {
        _adornerControl = new Canvas()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
        if (adornerLayer != null)
        {
            _adorner = new OverlayButtonAdorner(_adornerControl);
            adornerLayer.Add(_adorner);
        }
    }

    public void OnDragOver(Point point)
    {
        if (_owner is null || _owner.HoverControl is null || _owner.DockManager.DragServices.Window is null)
        {
            return;
        }

        var adornerContent = new ContentControl() { ContentTemplate = AdornerContentTemplate };

        var top = double.NaN;
        var left = double.NaN;
        var width = double.NaN;
        var height = double.NaN;
        var maxWidth = double.NaN;
        var maxHeight = double.NaN;

        var surfaceRectangle = _owner.HoverControl.SurfaceRectangle;
        var screenTopLeft = _owner.PointToScreen(new Point(0, 0));
        surfaceRectangle.Offset(-screenTopLeft.X, -screenTopLeft.Y);

        if (DockingPosition == DockingPosition.PaneInto)
        {
            width = ((Control)_owner.HoverControl).ActualWidth;
            height = ((Control)_owner.HoverControl).ActualHeight;

            top = surfaceRectangle.Top;
            left = surfaceRectangle.Left;
        }
        else if (DockingPosition is DockingPosition.PaneLeft or DockingPosition.PaneRight)
        {
            width = _owner.DockManager.DragServices.Window.ActualWidth;
            height = ((Control)_owner.HoverControl).ActualHeight;
            maxWidth = ((Control)_owner.HoverControl).ActualWidth / 2;

            top = surfaceRectangle.Top;

            left = DockingPosition == DockingPosition.PaneRight
                ? width > maxWidth ? surfaceRectangle.Left + maxWidth : surfaceRectangle.Left + ((Control)_owner.HoverControl).ActualWidth - width
                : surfaceRectangle.Left;
        }
        else if (DockingPosition is DockingPosition.PaneTop or DockingPosition.PaneBottom)
        {
            var parent = ((Control)_owner.HoverControl).FindVisualAncestor<DocumentList>();

            if (parent != null)
            {
                surfaceRectangle = parent.SurfaceRectangle;
                screenTopLeft = _owner.PointToScreen(new Point(0, 0));
                surfaceRectangle.Offset(-screenTopLeft.X, -screenTopLeft.Y);

                width = parent.ActualWidth;
                height = _owner.DockManager.DragServices.Window.ActualHeight;
                maxHeight = parent.ActualHeight / 2;

                left = surfaceRectangle.Left;

                top = DockingPosition == DockingPosition.PaneBottom
                    ? height > maxHeight ? surfaceRectangle.Top + maxHeight : surfaceRectangle.Top + parent.ActualHeight - height
                    : surfaceRectangle.Top;
            }
            else
            {
                width = ((Control)_owner.HoverControl).ActualWidth;
                height = _owner.DockManager.DragServices.Window.ActualHeight;
                maxHeight = ((Control)_owner.HoverControl).ActualHeight / 2;

                left = surfaceRectangle.Left;

                top = DockingPosition == DockingPosition.PaneBottom
                    ? height > maxHeight ? surfaceRectangle.Top + maxHeight : surfaceRectangle.Top + ((Control)_owner.HoverControl).ActualHeight - height
                    : surfaceRectangle.Top;
            }
        }

        else if (DockingPosition is DockingPosition.Left or DockingPosition.Right)
        {
            width = _owner.DockManager.DragServices.Window.ActualWidth;
            height = _owner.ActualHeight;
            maxWidth = _owner.ActualWidth / 2;

            if (DockingPosition == DockingPosition.Right)
                left = width > maxWidth ? _owner.ActualWidth - maxWidth : _owner.ActualWidth - width;
        }
        else if (DockingPosition is DockingPosition.Top or DockingPosition.Bottom)
        {
            width = _owner.ActualWidth;
            height = _owner.DockManager.DragServices.Window.ActualHeight;
            maxHeight = _owner.ActualHeight / 2;

            if (DockingPosition == DockingPosition.Bottom)
                top = height > maxHeight ? _owner.ActualHeight - maxHeight : _owner.ActualHeight - height;
        }

        adornerContent.Width = width;
        adornerContent.Height = height;

        if (!double.IsNaN(maxWidth))
            adornerContent.MaxWidth = maxWidth;

        if (!double.IsNaN(maxHeight))
            adornerContent.MaxHeight = maxHeight;

        if (!double.IsNaN(top))
            adornerContent.SetValue(Canvas.TopProperty, top);

        if (!double.IsNaN(left))
            adornerContent.SetValue(Canvas.LeftProperty, left);

        if (_adornerControl is not null)
        {
            _adornerControl.Width = _owner.ActualWidth;
            _adornerControl.Height = _owner.ActualHeight;
            _adornerControl.Children.Clear();
            _adornerControl.Children.Add(adornerContent);
        }
    }

    public void OnDragLeave(Point point)
    {
        RemoveAdorner();
    }

    public bool OnDrop(Point point)
    {
        RemoveAdorner();

        if (!IsEnabled)
            return false;

        _owner?.OnDrop(DockingPosition);

        return true;
    }

    private void RemoveAdorner()
    {
        var adornerLayer = AdornerLayer.GetAdornerLayer(this);
        adornerLayer?.Remove(_adorner);
    }

    #endregion
}
