using System.Windows;
using System.Windows.Controls;
using DevToolbox.Wpf.Controls;

namespace DevToolbox.Wpf.Windows;

/// <summary>
/// A transparent overlay window that displays docking drop targets and options when dragging items within a <see cref="DockManager"/>.
/// </summary>
[TemplatePart(Name = PART_Options, Type = typeof(ContentControl))]
public class OverlayWindow : Window
{
    #region Fields/Consts

    /// <summary>
    /// Template part name for the options content control.
    /// </summary>
    protected const string PART_Options = nameof(PART_Options);

    private readonly DockManager _dockManager;
    private IDropSurface? _currentControl;
    private ContentControl? _options;

    private static readonly DependencyPropertyKey IsAbsoluteOptionsVisiblePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsAbsoluteOptionsVisible),
            typeof(bool),
            typeof(OverlayWindow),
            new UIPropertyMetadata(false));

    private static readonly DependencyPropertyKey IsInnerOptionsVisiblePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsInnerOptionsVisible),
            typeof(bool),
            typeof(OverlayWindow),
            new UIPropertyMetadata(default));

    /// <summary>
    /// Identifies the <see cref="IsAbsoluteOptionsVisible"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsAbsoluteOptionsVisibleProperty = IsAbsoluteOptionsVisiblePropertyKey.DependencyProperty;

    /// <summary>
    /// Identifies the <see cref="IsInnerOptionsVisible"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsInnerOptionsVisibleProperty = IsInnerOptionsVisiblePropertyKey.DependencyProperty;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the drop surface currently under the drag cursor, or null if none.
    /// </summary>
    public IDropSurface? HoverControl => _currentControl;

    /// <summary>
    /// Gets the associated <see cref="DockManager"/> instance that this overlay serves.
    /// </summary>
    public DockManager DockManager => _dockManager;

    /// <summary>
    /// Gets whether the absolute (global) drop options are visible.
    /// </summary>
    public bool IsAbsoluteOptionsVisible
    {
        get => (bool)GetValue(IsAbsoluteOptionsVisibleProperty);
        private set => SetValue(IsAbsoluteOptionsVisiblePropertyKey, value);
    }

    /// <summary>
    /// Gets whether the inner (surface-specific) drop options are visible.
    /// </summary>
    public bool IsInnerOptionsVisible
    {
        get => (bool)GetValue(IsInnerOptionsVisibleProperty);
        private set => SetValue(IsInnerOptionsVisiblePropertyKey, value);
    }

    #endregion

    /// <summary>
    /// Static constructor to override the default style key for this control.
    /// </summary>
    static OverlayWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(OverlayWindow), new FrameworkPropertyMetadata(typeof(OverlayWindow)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OverlayWindow"/> class for the specified <see cref="DockManager"/>.
    /// </summary>
    /// <param name="dockManager">The DockManager that this overlay window will serve.</param>
    public OverlayWindow(DockManager dockManager)
    {
        ShowActivated = false;
        AllowsTransparency = true;
        WindowStyle = WindowStyle.None;
        ShowInTaskbar = false;

        _dockManager = dockManager;

        SetResourceReference(StyleProperty, DefaultStyleKey);
    }

    #region Overrides

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _options = Template.FindName(PART_Options, this) as ContentControl;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Invokes a drop action on the current drop service with the specified docking position.
    /// </summary>
    /// <param name="dockingPosition">Position where the item should be docked.</param>
    internal void OnDrop(DockingPosition dockingPosition)
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

        if (windowType?.Equals(typeof(DocumentWindow)) == true && controlType.Equals(typeof(DockableControl)))
        {
            IsAbsoluteOptionsVisible = false;
            return;
        }
        else
        {
            IsAbsoluteOptionsVisible = windowType?.Equals(typeof(DocumentWindow)) == false;
        }

        _currentControl = control;

        var rect = control.SurfaceRectangle;

        var myScreenTopLeft = PointToScreen(new Point(0, 0));
        rect.Offset(-myScreenTopLeft.X, -myScreenTopLeft.Y);

        _options?.SetValue(Canvas.LeftProperty, rect.Left + rect.Width / 2 - _options.ActualWidth / 2);
        _options?.SetValue(Canvas.TopProperty, rect.Top + rect.Height / 2 - _options.ActualHeight / 2);

        IsInnerOptionsVisible = true;
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
        IsInnerOptionsVisible = false;
        IsAbsoluteOptionsVisible = false;
    }

    #endregion
}
