using System.Windows;
using System.Windows.Controls;
using DevToolbox.Wpf.Windows;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A transparent overlay window that displays docking drop targets and options when dragging items within a <see cref="DockManager"/>.
/// </summary>
[TemplatePart(Name = PART_DockVectorOuter, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PART_DockVectorSmall, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PART_DockVectorLarge, Type = typeof(FrameworkElement))]
public class LayoutDockTargetControl : Control
{
    #region Fields/Consts

    /// <summary>
    /// Template part name for the options content control.
    /// </summary>
    protected const string PART_DockVectorOuter = nameof(PART_DockVectorOuter);

    /// <summary>
    /// Template part name for the options content control.
    /// </summary>
    protected const string PART_DockVectorSmall = nameof(PART_DockVectorSmall);

    /// <summary>
    /// Template part name for the options content control.
    /// </summary>
    protected const string PART_DockVectorLarge = nameof(PART_DockVectorLarge);

    private readonly LayoutManager _dockManager;
    private IDropSurface? _currentControl;
    private FrameworkElement? _dockVectorOuter;
    private FrameworkElement? _dockVectorSmall;
    private FrameworkElement? _dockVectorLarge;

    private static readonly DependencyPropertyKey DockTargetZonePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(DockTargetZone),
            typeof(LayoutDockTargetZone),
            typeof(LayoutDockTargetControl),
            new UIPropertyMetadata(LayoutDockTargetZone.None));

    private static readonly DependencyPropertyKey DockTargetVisibilityPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(DockTargetVisibility),
            typeof(LayoutDockTargetVisibility),
            typeof(LayoutDockTargetControl),
            new UIPropertyMetadata(LayoutDockTargetVisibility.None));

    /// <summary>
    /// Identifies the <see cref="DockTargetZone"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DockTargetZoneProperty = DockTargetZonePropertyKey.DependencyProperty;

    /// <summary>
    /// Identifies the <see cref="DockTargetVisibility"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DockTargetVisibilityProperty = DockTargetVisibilityPropertyKey.DependencyProperty;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the drop surface currently under the drag cursor, or null if none.
    /// </summary>
    public IDropSurface? HoverControl => _currentControl;

    /// <summary>
    /// Gets the associated <see cref="DockManager"/> instance that this overlay serves.
    /// </summary>
    public LayoutManager DockManager => _dockManager;

    /// <summary>
    /// Gets whether the absolute (global) drop options are visible.
    /// </summary>
    public LayoutDockTargetZone DockTargetZone
    {
        get => (LayoutDockTargetZone)GetValue(DockTargetZoneProperty);
        private set => SetValue(DockTargetZonePropertyKey, value);
    }

    /// <summary>
    /// Gets the dock target visibility.
    /// </summary>
    public LayoutDockTargetVisibility DockTargetVisibility
    {
        get => (LayoutDockTargetVisibility)GetValue(DockTargetVisibilityProperty);
        private set => SetValue(DockTargetVisibilityPropertyKey, value);
    }

    #endregion

    /// <summary>
    /// Static constructor to override the default style key for this control.
    /// </summary>
    static LayoutDockTargetControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(LayoutDockTargetControl),
            new FrameworkPropertyMetadata(typeof(LayoutDockTargetControl)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LayoutDockTargetControl"/> class for the specified <see cref="DockManager"/>.
    /// </summary>
    /// <param name="dockManager">The DockManager that this overlay window will serve.</param>
    public LayoutDockTargetControl(LayoutManager dockManager)
    {
        _dockManager = dockManager;
        SetResourceReference(StyleProperty, DefaultStyleKey);
    }

    #region Overrides

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _dockVectorOuter = Template.FindName(PART_DockVectorOuter, this) as FrameworkElement;
        _dockVectorSmall = Template.FindName(PART_DockVectorSmall, this) as FrameworkElement;
        _dockVectorLarge = Template.FindName(PART_DockVectorLarge, this) as FrameworkElement;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Invokes a drop action on the current drop service with the specified docking position.
    /// </summary>
    /// <param name="dockingPosition">Position where the item should be docked.</param>
    internal void OnDrop(LayoutDockTargetPosition dockingPosition)
    {
        if (_currentControl is null)
        {
            return;
        }

        _dockManager.DragServices.Window?.OnDrop(_currentControl, dockingPosition);
    }

    /// <summary>
    /// Called when a dragged item enters a drop surface; positions and displays overlay options.
    /// </summary>
    /// <param name="control">The drop surface under the pointer.</param>
    /// <param name="point">Pointer location relative to the overlay window.</param>
    internal void OnDragEnter(IDropSurface control, Point point)
    {
        var controlType = control.GetType();
        var windowType = _dockManager.DragServices.Window?.GetType();
        var orientation = _dockManager.LayoutGroupItems.LayoutGroupPanel!.Orientation;
        var rect = control.SurfaceRectangle;
        var myScreenTopLeft = PointToScreen(new Point(0, 0));
        rect.Offset(-myScreenTopLeft.X, -myScreenTopLeft.Y);
        _currentControl = control;

        if (windowType?.Equals(typeof(LayoutWindow)) == true)
        {
            DockTargetZone = controlType.Equals(typeof(LayoutItemsControl)) ? LayoutDockTargetZone.InnerCross : LayoutDockTargetZone.None;
            if (_dockManager.LayoutGroupItems.LayoutGroupPanel.CanAddHorizontal
                && _dockManager.LayoutGroupItems.LayoutGroupPanel.CanAddVertical)
            {
                DockTargetVisibility = LayoutDockTargetVisibility.PaneInto
                    | LayoutDockTargetVisibility.PaneTop
                    | LayoutDockTargetVisibility.PaneBottom
                    | LayoutDockTargetVisibility.PaneLeft
                    | LayoutDockTargetVisibility.PaneRight;
            }
            else if (_dockManager.LayoutGroupItems.LayoutGroupPanel.CanAddHorizontal)
            {
                DockTargetVisibility = LayoutDockTargetVisibility.PaneInto
                    | LayoutDockTargetVisibility.PaneLeft
                    | LayoutDockTargetVisibility.PaneRight;
            }
            else if (_dockManager.LayoutGroupItems.LayoutGroupPanel.CanAddVertical)
            {
                DockTargetVisibility = LayoutDockTargetVisibility.PaneInto
                    | LayoutDockTargetVisibility.PaneTop
                    | LayoutDockTargetVisibility.PaneBottom;
            }
        }
        else if (windowType?.Equals(typeof(LayoutDockWindow)) == true)
        {
            if (controlType.Equals(typeof(LayoutItemsControl)))
            {
                DockTargetZone = LayoutDockTargetZone.FullGridWithOuterEdges;
                if (_dockManager.LayoutGroupItems.LayoutGroupPanel.CanAddHorizontal
                    && _dockManager.LayoutGroupItems.LayoutGroupPanel.CanAddVertical)
                {
                    DockTargetVisibility = LayoutDockTargetVisibility.All;
                }
                else if (_dockManager.LayoutGroupItems.LayoutGroupPanel.CanAddHorizontal)
                {
                    DockTargetVisibility = LayoutDockTargetVisibility.PaneInto
                        | LayoutDockTargetVisibility.PaneLeft
                        | LayoutDockTargetVisibility.PaneRight
                        | LayoutDockTargetVisibility.InnerTop
                        | LayoutDockTargetVisibility.InnerBottom
                        | LayoutDockTargetVisibility.InnerLeft
                        | LayoutDockTargetVisibility.InnerRight
                        | LayoutDockTargetVisibility.Top
                        | LayoutDockTargetVisibility.Bottom
                        | LayoutDockTargetVisibility.Left
                        | LayoutDockTargetVisibility.Right;
                }
                else if (_dockManager.LayoutGroupItems.LayoutGroupPanel.CanAddVertical)
                {
                    DockTargetVisibility = LayoutDockTargetVisibility.PaneInto
                        | LayoutDockTargetVisibility.PaneTop
                        | LayoutDockTargetVisibility.PaneBottom
                        | LayoutDockTargetVisibility.InnerTop
                        | LayoutDockTargetVisibility.InnerBottom
                        | LayoutDockTargetVisibility.InnerLeft
                        | LayoutDockTargetVisibility.InnerRight
                        | LayoutDockTargetVisibility.Top
                        | LayoutDockTargetVisibility.Bottom
                        | LayoutDockTargetVisibility.Left
                        | LayoutDockTargetVisibility.Right;
                }
            }
            else
            {
                DockTargetZone = LayoutDockTargetZone.InnerCrossWithOuterEdges;
                DockTargetVisibility = LayoutDockTargetVisibility.PaneInto
                    | LayoutDockTargetVisibility.PaneTop
                    | LayoutDockTargetVisibility.PaneBottom
                    | LayoutDockTargetVisibility.PaneLeft
                    | LayoutDockTargetVisibility.PaneRight
                    | LayoutDockTargetVisibility.Top
                    | LayoutDockTargetVisibility.Bottom
                    | LayoutDockTargetVisibility.Left
                    | LayoutDockTargetVisibility.Right;
            }
        }

        if (DockTargetZone is LayoutDockTargetZone.InnerCross or LayoutDockTargetZone.InnerCrossWithOuterEdges)
        {
            _dockVectorSmall?.SetValue(Canvas.LeftProperty, rect.Left + rect.Width / 2 - _dockVectorSmall.ActualWidth / 2);
            _dockVectorSmall?.SetValue(Canvas.TopProperty, rect.Top + rect.Height / 2 - _dockVectorSmall.ActualHeight / 2);
        }
        else if (DockTargetZone is LayoutDockTargetZone.FullGridWithOuterEdges)
        {
            _dockVectorLarge?.SetValue(Canvas.LeftProperty, rect.Left + rect.Width / 2 - _dockVectorLarge.ActualWidth / 2);
            _dockVectorLarge?.SetValue(Canvas.TopProperty, rect.Top + rect.Height / 2 - _dockVectorLarge.ActualHeight / 2);
        }
    }

    /// <summary>
    /// Called when a dragged item moves over a drop surface (no-op).
    /// </summary>
    /// <param name="control">The drop surface under the pointer.</param>
    /// <param name="point">Pointer location relative to the overlay window.</param>
    internal void OnDragOver(IDropSurface control, Point point)
    {
    }

    /// <summary>
    /// Called when a dragged item leaves a drop surface; hides overlay options.
    /// </summary>
    /// <param name="control">The drop surface that was under the pointer.</param>
    /// <param name="point">Pointer location relative to the overlay window.</param>
    internal void OnDragLeave(IDropSurface control, Point point)
    {
        DockTargetZone = LayoutDockTargetZone.None;
        DockTargetVisibility = LayoutDockTargetVisibility.None;
    }

    #endregion
}
