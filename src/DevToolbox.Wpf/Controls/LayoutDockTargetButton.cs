using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using DevToolbox.Wpf.Documents;
using DevToolbox.Wpf.Extensions;

namespace DevToolbox.Wpf.Controls;

internal class LayoutDockTargetButton : ContentControl, IDropSurface, IDisposable
{
    #region Fields/Consts

    private Canvas? _adornerSurface;
    private ContentControl? _adornerContent;
    private LayoutDockOverlayButtonAdorner? _adorner;
    private LayoutDockTargetControl? _owner;
    private bool _isRegistered;

    public static readonly DependencyProperty AdornerContentTemplateProperty =
        DependencyProperty.Register(nameof(AdornerContentTemplate), typeof(DataTemplate), typeof(LayoutDockTargetButton), new FrameworkPropertyMetadata(default));

    public static readonly DependencyProperty DockingPositionProperty =
        DependencyProperty.Register(nameof(DockingPosition), typeof(LayoutDockTargetPosition), typeof(LayoutDockTargetButton), new PropertyMetadata(default(LayoutDockTargetPosition)));

    #endregion

    #region Properties

    public LayoutDockTargetPosition DockingPosition
    {
        get => (LayoutDockTargetPosition)GetValue(DockingPositionProperty);
        set => SetValue(DockingPositionProperty, value);
    }

    public DataTemplate AdornerContentTemplate
    {
        get => (DataTemplate)GetValue(AdornerContentTemplateProperty);
        set => SetValue(AdornerContentTemplateProperty, value);
    }

    public Rect SurfaceRectangle => !IsLoaded || PresentationSource.FromVisual(this) == null
                ? new Rect()
                : new Rect(PointToScreen(new Point(0, 0)), new Size(ActualWidth, ActualHeight));

    #endregion

    static LayoutDockTargetButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(LayoutDockTargetButton),
            new FrameworkPropertyMetadata(typeof(LayoutDockTargetButton)));
    }

    public LayoutDockTargetButton()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #region Methods Overrides

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _owner = this.FindVisualAncestor<LayoutDockTargetControl>(); 
        Register();
    }

    #endregion

    #region Methods

    public void OnDragEnter(Point point)
    {
        if (_adorner != null)
            return;

        var layer = AdornerLayer.GetAdornerLayer(this);
        if (layer == null)
            return;

        // Create fresh visual tree for adorner
        _adornerSurface = new Canvas
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        _adornerContent = new ContentControl
        {
            ContentTemplate = AdornerContentTemplate,
            Visibility = Visibility.Collapsed
        };
        _adornerSurface.Children.Add(_adornerContent);

        _adorner = new LayoutDockOverlayButtonAdorner(_adornerSurface);
        layer.Add(_adorner);
    }

    public void OnDragOver(Point point)
    {
        if (_owner is null
            || _adornerContent is null
            || _adornerSurface is null)
            return;

        var (targetRect, maxSize) = CalculateOverlayPosition(DockingPosition);

        _adornerContent.Width = targetRect.Width;
        _adornerContent.Height = targetRect.Height;

        if (!double.IsNaN(maxSize.Width))
            _adornerContent.MaxWidth = maxSize.Width;

        if (!double.IsNaN(maxSize.Height))
            _adornerContent.MaxHeight = maxSize.Height;

        if (!double.IsNaN(targetRect.X))
            _adornerContent.SetValue(Canvas.TopProperty, targetRect.X);

        if (!double.IsNaN(targetRect.Y))
            _adornerContent.SetValue(Canvas.LeftProperty, targetRect.Y);

        _adornerSurface.Width = _owner.ActualWidth;
        _adornerSurface.Height = _owner.ActualHeight;
        _adornerContent.Visibility = Visibility.Visible;
    }

    public void OnDragLeave(Point point)
    {
        RemoveAdorner();
    }

    public bool OnDrop(Point point)
    {
        RemoveAdorner();

        if (!IsEnabled || _owner == null)
            return false;

        _owner.OnDrop(DockingPosition);
        return true;
    }

    private (Rect targetRect, Size maxSize) CalculateOverlayPosition(LayoutDockTargetPosition dockingPosition)
    {
        var hover = _owner!.HoverControl!;
        var win = _owner.DockManager.DragServices.Window!
            ?? throw new InvalidOperationException("DragServices.Window cannot be null.");

        var x = double.NaN;
        var y = double.NaN;
        var width = double.NaN;
        var height = double.NaN;
        var maxWidth = double.NaN;
        var maxHeight = double.NaN;

        var surfaceRectangle = hover.SurfaceRectangle;
        var screenTopLeft = _owner.PointToScreen(new Point(0, 0));
        surfaceRectangle.Offset(-screenTopLeft.X, -screenTopLeft.Y);

        switch (DockingPosition)
        {
            case LayoutDockTargetPosition.Top:
            case LayoutDockTargetPosition.Bottom:
            case LayoutDockTargetPosition.InnerTop:
            case LayoutDockTargetPosition.InnerBottom:
                {
                    width = _owner.ActualWidth;
                    height = _owner.DockManager.DragServices.Window.ActualHeight;
                    maxHeight = _owner.ActualHeight / 2;

                    if (DockingPosition is LayoutDockTargetPosition.Bottom or LayoutDockTargetPosition.InnerBottom)
                        y = height > maxHeight ? _owner.ActualHeight - maxHeight : _owner.ActualHeight - height;
                    break;
                }
            case LayoutDockTargetPosition.Left:
            case LayoutDockTargetPosition.Right:
            case LayoutDockTargetPosition.InnerLeft:
            case LayoutDockTargetPosition.InnerRight:
                {
                    width = _owner.DockManager.DragServices.Window.ActualWidth;
                    height = _owner.ActualHeight;
                    maxWidth = _owner.ActualWidth / 2;

                    if (DockingPosition is LayoutDockTargetPosition.Right or LayoutDockTargetPosition.InnerRight)
                        x = width > maxWidth ? _owner.ActualWidth - maxWidth : _owner.ActualWidth - width;
                    break;
                }
            case LayoutDockTargetPosition.PaneTop:
            case LayoutDockTargetPosition.PaneBottom:
                {
                    var parent = ((Control)hover).FindVisualAncestor<LayoutGroupItemsControl>();

                    if (parent != null)
                    {
                        surfaceRectangle = parent.SurfaceRectangle;
                        screenTopLeft = _owner.PointToScreen(new Point(0, 0));
                        surfaceRectangle.Offset(-screenTopLeft.X, -screenTopLeft.Y);
                        width = parent.ActualWidth;
                        height = _owner.DockManager.DragServices.Window.ActualHeight;
                        maxHeight = parent.ActualHeight / 2;
                        x = surfaceRectangle.Left;
                        y = DockingPosition == LayoutDockTargetPosition.PaneBottom
                            ? height > maxHeight ? surfaceRectangle.Top + maxHeight : surfaceRectangle.Top + parent.ActualHeight - height
                            : surfaceRectangle.Top;
                    }
                    else
                    {
                        width = ((Control)hover).ActualWidth;
                        height = _owner.DockManager.DragServices.Window.ActualHeight;
                        maxHeight = ((Control)hover).ActualHeight / 2;
                        x = surfaceRectangle.Left;
                        y = DockingPosition == LayoutDockTargetPosition.PaneBottom
                            ? height > maxHeight ? surfaceRectangle.Top + maxHeight : surfaceRectangle.Top + ((Control)hover).ActualHeight - height
                            : surfaceRectangle.Top;
                    }
                    break;
                }
            case LayoutDockTargetPosition.PaneLeft:
            case LayoutDockTargetPosition.PaneRight:
                {
                    width = _owner.DockManager.DragServices.Window.ActualWidth;
                    height = ((Control)hover).ActualHeight;
                    maxWidth = ((Control)hover).ActualWidth / 2;
                    x = DockingPosition == LayoutDockTargetPosition.PaneRight
                        ? width > maxWidth ? surfaceRectangle.Left + maxWidth : surfaceRectangle.Left + ((Control)hover).ActualWidth - width
                        : surfaceRectangle.Left;
                    y = surfaceRectangle.Top;
                    break;
                }
            case LayoutDockTargetPosition.PaneInto:
                {
                    x = surfaceRectangle.Left;
                    y = surfaceRectangle.Top;
                    width = ((Control)hover).ActualWidth;
                    height = ((Control)hover).ActualHeight;
                    break;
                }
            default:
                break;
        }

        return (new Rect(y, x, width, height), new Size(maxWidth, maxHeight));
    }

    private void RemoveAdorner()
    {
        if (_adorner == null)
            return;

        var layer = AdornerLayer.GetAdornerLayer(this);
        layer?.Remove(_adorner);
        _adorner = null;
        _adornerContent = null;
        _adornerSurface = null;
    }

    private void Register()
    {
        if (_isRegistered || _owner?.DockManager == null)
            return;

        _owner.DockManager.DragServices.Register(this);
        _isRegistered = true;
    }

    private void Unregister()
    {
        if (!_isRegistered || _owner?.DockManager == null)
            return;

        _owner.DockManager.DragServices.Unregister(this);
        _isRegistered = false;
        RemoveAdorner();
    }

    public void Dispose()
    {
        if (_isRegistered && _owner?.DockManager != null)
        {
            _owner.DockManager.DragServices.Unregister(this);
        }
        RemoveAdorner();
    }

    #endregion

    #region Events Subscriptions

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Register();
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        Unregister();
    }

    #endregion
}
