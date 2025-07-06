using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using DevToolbox.Wpf.Documents;
using DevToolbox.Wpf.Extensions;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a dock target button control that acts as a drop surface for layout docking operations.
/// Provides visual feedback via adorners during drag-and-drop and handles drop events to trigger docking.
/// </summary>
internal class LayoutDockTargetButton : Control, IDropSurface, IDisposable
{
    #region Fields/Consts

    private Canvas? _adornerSurface;
    private ContentControl? _adornerContent;
    private LayoutDockOverlayButtonAdorner? _adorner;
    private LayoutDockTargetControl? _owner;
    private bool _isRegistered;

    /// <summary>
    /// Identifies the <see cref="AdornerContentTemplate"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty AdornerContentTemplateProperty =
        DependencyProperty.Register(nameof(AdornerContentTemplate), typeof(DataTemplate), typeof(LayoutDockTargetButton), new FrameworkPropertyMetadata(default));

    /// <summary>
    /// Identifies the <see cref="DockingPosition"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DockingPositionProperty =
        DependencyProperty.Register(nameof(DockingPosition), typeof(LayoutDockTargetPosition), typeof(LayoutDockTargetButton), new PropertyMetadata(default(LayoutDockTargetPosition)));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the position where the content will be docked when dropped on this target.
    /// </summary>
    public LayoutDockTargetPosition DockingPosition
    {
        get => (LayoutDockTargetPosition)GetValue(DockingPositionProperty);
        set => SetValue(DockingPositionProperty, value);
    }

    /// <summary>
    /// Gets or sets the template used to visualize the adorner content during drag operations.
    /// </summary>
    public DataTemplate AdornerContentTemplate
    {
        get => (DataTemplate)GetValue(AdornerContentTemplateProperty);
        set => SetValue(AdornerContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets the bounding rectangle of the control in screen coordinates, or an empty rectangle if not loaded.
    /// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="LayoutDockTargetButton"/> class.
    /// Hooks Loaded and Unloaded event handlers for registration.
    /// </summary>
    public LayoutDockTargetButton()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #region Methods Overrides

    /// <summary>
    /// Applies the control template and registers this drop surface with its owner.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _owner = this.FindVisualAncestor<LayoutDockTargetControl>();
        Register();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Called when a drag operation enters the surface. Adds an adorner for visual feedback.
    /// </summary>
    /// <param name="point">The current drag point in surface coordinates.</param>
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

    /// <summary>
    /// Updates the adorner position and visibility during a drag-over event.
    /// </summary>
    /// <param name="point">The current drag point in surface coordinates.</param>
    public void OnDragOver(Point point)
    {
        if (_owner is null || _adornerContent is null || _adornerSurface is null)
            return;

        var (targetRect, maxSize) = CalculateOverlayPosition();

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

    /// <summary>
    /// Called when a drag operation leaves the surface. Removes any existing adorner.
    /// </summary>
    /// <param name="point">The current drag point in surface coordinates.</param>
    public void OnDragLeave(Point point)
    {
        RemoveAdorner();
    }

    /// <summary>
    /// Called when an item is dropped on this surface. Triggers docking on the owner.
    /// </summary>
    /// <param name="point">The drop point in surface coordinates.</param>
    /// <returns>True if the drop was handled and docking triggered; otherwise, false.</returns>
    public bool OnDrop(Point point)
    {
        RemoveAdorner();

        if (!IsEnabled || _owner == null)
            return false;

        _owner.OnDrop(DockingPosition);
        return true;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Calculates the rectangle and maximum size for the adorner overlay based on the docking position.
    /// </summary>
    /// <returns>A tuple containing the target adorner rectangle and maximum allowed size.</returns>
    private (Rect targetRect, Size maxSize) CalculateOverlayPosition()
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

    /// <summary>
    /// Removes the current adorner from the adorner layer if it exists.
    /// </summary>
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

    /// <summary>
    /// Registers this drop surface with the drag services when loaded.
    /// </summary>
    private void Register()
    {
        if (_isRegistered || _owner?.DockManager == null)
            return;

        _owner.DockManager.DragServices.Register(this);
        _isRegistered = true;
    }

    /// <summary>
    /// Unregisters this drop surface from the drag services and removes adorners.
    /// </summary>
    private void Unregister()
    {
        if (!_isRegistered || _owner?.DockManager == null)
            return;

        _owner.DockManager.DragServices.Unregister(this);
        _isRegistered = false;
        RemoveAdorner();
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// Disposes the control by unregistering from drag services and removing adorners.
    /// </summary>
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

    /// <summary>
    /// Handles the Loaded event to register the drop surface.
    /// </summary>
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Register();
    }

    /// <summary>
    /// Handles the Unloaded event to unregister the drop surface.
    /// </summary>
    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        Unregister();
    }

    #endregion
}
