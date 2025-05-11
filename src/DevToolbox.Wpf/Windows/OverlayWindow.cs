using System.Windows;
using System.Windows.Controls;
using DevToolbox.Wpf.Controls;

namespace DevToolbox.Wpf.Windows;

[TemplatePart(Name = PART_Options, Type = typeof(ContentControl))]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class OverlayWindow : Window
{
    #region Fields/Consts

    protected const string PART_Options = nameof(PART_Options);

    private readonly DockManager _dockManager;
    private IDropSurface? _currentControl;
    private ContentControl? _options;

    private static readonly DependencyPropertyKey IsAbsolutleOptionsVisiblePropertyKey = 
        DependencyProperty.RegisterReadOnly(nameof(IsAbsolutleOptionsVisible), typeof(bool), typeof(OverlayWindow), new UIPropertyMetadata(false));

    private static readonly DependencyPropertyKey IsInnerOptionsVisiblePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsInnerOptionsVisible), typeof(bool), typeof(OverlayWindow), new UIPropertyMetadata(default));

    public static readonly DependencyProperty IsAbsolutleOptionsVisibleProperty = IsAbsolutleOptionsVisiblePropertyKey.DependencyProperty;
    public static readonly DependencyProperty IsInnerOptionsVisibleProperty = IsInnerOptionsVisiblePropertyKey.DependencyProperty;

    #endregion

    #region Properties

    public IDropSurface? HoverControl => _currentControl;

    public DockManager DockManager => _dockManager;

    public bool IsAbsolutleOptionsVisible
    {
        get => (bool)GetValue(IsAbsolutleOptionsVisibleProperty);
        private set => SetValue(IsAbsolutleOptionsVisiblePropertyKey, value);
    }

    public bool IsInnerOptionsVisible
    {
        get => (bool)GetValue(IsInnerOptionsVisibleProperty);
        private set => SetValue(IsInnerOptionsVisiblePropertyKey, value);
    }

    #endregion

    static OverlayWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(OverlayWindow), new FrameworkPropertyMetadata(typeof(OverlayWindow)));
    }

    public OverlayWindow(DockManager dockManager)
    {
        ShowActivated = false;
        AllowsTransparency = true;
        WindowStyle = WindowStyle.None;

        _dockManager = dockManager;
    }

    #region Overrides

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _options = Template.FindName(PART_Options, this) as ContentControl;
    }

    #endregion

    #region Methods

    internal void OnDrop(DockingPosition dockingPosition)
    {
        if (_currentControl is null)
        {
            return;
        }

        _dockManager.DragServices.Window?.OnDrop(_currentControl, dockingPosition);
    }

    internal void OnDragEnter(IDropSurface control, Point point)
    {
        var controlType = control.GetType();
        var windowType = _dockManager.DragServices.Window?.GetType();

        if (windowType?.Equals(typeof(DocumentWindow)) == true && controlType.Equals(typeof(DockableControl)))
        {
            IsAbsolutleOptionsVisible = false;
            return;
        }
        else
        {
            IsAbsolutleOptionsVisible = windowType?.Equals(typeof(DocumentWindow)) == false;
        }

        _currentControl = control;

        var rect = control.SurfaceRectangle;

        var myScreenTopLeft = PointToScreen(new Point(0, 0));
        rect.Offset(-myScreenTopLeft.X, -myScreenTopLeft.Y);

        _options?.SetValue(Canvas.LeftProperty, rect.Left + rect.Width / 2 - _options.ActualWidth / 2);
        _options?.SetValue(Canvas.TopProperty, rect.Top + rect.Height / 2 - _options.ActualHeight / 2);

        IsInnerOptionsVisible = true;
    }

    internal void OnDragOver(IDropSurface control, Point point)
    {
    }

    internal void OnDragLeave(IDropSurface control, Point point)
    {
        IsInnerOptionsVisible = false;
        IsAbsolutleOptionsVisible = false;
    }

    #endregion
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member