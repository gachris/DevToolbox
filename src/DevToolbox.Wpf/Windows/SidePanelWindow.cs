using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shell;
using DevToolbox.Wpf.Interop;
using DevToolbox.Wpf.Utils;

namespace DevToolbox.Wpf.Windows;

/// <summary>
/// Represents a sliding window control that can expand or collapse, supporting docking and animation.
/// </summary>
public class SidePanelWindow : WindowEx
{
    #region Fields/Consts

    private IntPtr _hwnd = IntPtr.Zero; // Handle to the window
    private HwndSource? _hwndSource; // Source for window interop

    private bool _isHooked;
    private DoubleAnimation? _expandAnimation;
    private DoubleAnimation? _collapseAnimation;
    private ContentControl? _contentWrapper;
    private readonly double _animateSpeedRatio = 3d;

    /// <summary>
    /// Event triggered when the Dock property changes.
    /// </summary>
    public event PropertyChangedCallback? DockChanged;

    /// <summary>
    /// Event triggered when the AutoHide property changes.
    /// </summary>
    public event PropertyChangedCallback? AutoHideChanged;

    /// <summary>
    /// Event triggered when the IsExpanded property changes.
    /// </summary>
    public event PropertyChangedCallback? IsExpandedChanged;

    /// <summary>
    /// Dependency property key for IsAnimationInProcess, which is read-only.
    /// </summary>
    internal static readonly DependencyPropertyKey IsAnimationInProcessPropertyKey =
        DependencyProperty.RegisterReadOnly("IsAnimationInProcess", typeof(bool), typeof(SidePanelWindow),
        new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>
    /// Dependency property for indicating if an animation is currently in process.
    /// </summary>
    public static readonly DependencyProperty IsAnimationInProcessProperty = IsAnimationInProcessPropertyKey.DependencyProperty;

    /// <summary>
    /// Dependency property for the AutoHide feature of the slider control.
    /// </summary>
    public static readonly DependencyProperty AutoHideProperty =
        DependencyProperty.Register("AutoHide", typeof(bool), typeof(SidePanelWindow),
        new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.Inherits, OnAutoHideChanged));

    /// <summary>
    /// Dependency property for the docking position of the slider control.
    /// </summary>
    public static readonly DependencyProperty DockProperty =
        DependencyProperty.Register("Dock", typeof(Dock), typeof(SidePanelWindow),
        new FrameworkPropertyMetadata(default(Dock), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDockChanged, DockCoerceValue));

    /// <summary>
    /// Dependency property for indicating if the slider control is expanded.
    /// </summary>
    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register("IsExpanded", typeof(bool), typeof(SidePanelWindow),
        new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsExpandedChanged, IsExpandedCoerceValue));

    /// <summary>
    /// Dependency property for the size to which the slider control expands.
    /// </summary>
    public static readonly DependencyProperty ExpandSizeProperty =
        DependencyProperty.Register("ExpandSize", typeof(double), typeof(SidePanelWindow),
        new FrameworkPropertyMetadata(320D, OnExpandSizeChanged));

    /// <summary>
    /// Dependency property for the easing function used in animations.
    /// </summary>
    public static readonly DependencyProperty EasingFunctionProperty =
        DependencyProperty.Register("EasingFunction", typeof(IEasingFunction), typeof(SidePanelWindow),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnEasingFunctionChanged));

    /// <summary>
    /// Routed event triggered when a collapse animation completes.
    /// </summary>
    public static readonly RoutedEvent CollapseCompletedEvent =
        EventManager.RegisterRoutedEvent("CollapseCompletedEventHandler", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SidePanelWindow));

    /// <summary>
    /// Routed event triggered when an expand animation completes.
    /// </summary>
    public static readonly RoutedEvent ExpandCompletedEvent =
        EventManager.RegisterRoutedEvent("ExpandCompletedEventHandler", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SidePanelWindow));

    /// <summary>
    /// Routed event triggered when a collapse animation starts.
    /// </summary>
    public static readonly RoutedEvent CollapseStartedEvent =
        EventManager.RegisterRoutedEvent("CollapseStartedEventHandler", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SidePanelWindow));

    /// <summary>
    /// Routed event triggered when an expand animation starts.
    /// </summary>
    public static readonly RoutedEvent ExpandStartedEvent =
        EventManager.RegisterRoutedEvent("ExpandStartedEventHandler", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SidePanelWindow));

    /// <summary>
    /// Routed event triggered when the expand state is invalidated.
    /// </summary>
    public static readonly RoutedEvent ExpandStateInvalidatedEvent =
        EventManager.RegisterRoutedEvent("ExpandStateInvalidatedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SidePanelWindow));

    /// <summary>
    /// Routed event triggered when the collapse state is invalidated.
    /// </summary>
    public static readonly RoutedEvent CollapseStateInvalidatedEvent =
        EventManager.RegisterRoutedEvent("CollapseStateInvalidatedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SidePanelWindow));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets a value indicating whether the window should automatically hide when collapsed.
    /// </summary>
    public bool AutoHide
    {
        get => (bool)GetValue(AutoHideProperty);
        set => SetValue(AutoHideProperty, value);
    }

    /// <summary>
    /// Gets or sets the dock position of the side panel window.
    /// </summary>
    public Dock Dock
    {
        get => (Dock)GetValue(DockProperty);
        set => SetValue(DockProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the window is currently expanded.
    /// </summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>
    /// Gets or sets the size to which the window expands.
    /// </summary>
    public double ExpandSize
    {
        get => (double)GetValue(ExpandSizeProperty);
        set => SetValue(ExpandSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the easing function used for animations.
    /// </summary>
    public IEasingFunction EasingFunction
    {
        get => (IEasingFunction)GetValue(EasingFunctionProperty);
        set => SetValue(EasingFunctionProperty, value);
    }

    /// <summary>
    /// Gets a value indicating whether an animation is currently in process.
    /// </summary>
    public bool IsAnimationInProcess => (bool)GetValue(IsAnimationInProcessProperty);

    #endregion

    #region Routed Events

    /// <summary>
    /// Occurs when the expansion process starts.
    /// </summary>
    public event RoutedEventHandler ExpandStarted
    {
        add => AddHandler(ExpandStartedEvent, value);
        remove => RemoveHandler(ExpandStartedEvent, value);
    }

    /// <summary>
    /// Occurs when the collapse process starts.
    /// </summary>
    public event RoutedEventHandler CollapseStarted
    {
        add => AddHandler(CollapseStartedEvent, value);
        remove => RemoveHandler(CollapseStartedEvent, value);
    }

    /// <summary>
    /// Occurs when the expansion process is completed.
    /// </summary>
    public event RoutedEventHandler ExpandCompleted
    {
        add => AddHandler(ExpandCompletedEvent, value);
        remove => RemoveHandler(ExpandCompletedEvent, value);
    }

    /// <summary>
    /// Occurs when the collapse process is completed.
    /// </summary>
    public event RoutedEventHandler CollapseCompleted
    {
        add => AddHandler(CollapseCompletedEvent, value);
        remove => RemoveHandler(CollapseCompletedEvent, value);
    }

    /// <summary>
    /// Occurs when the state of the expansion is invalidated.
    /// </summary>
    public event RoutedEventHandler ExpandStateInvalidated
    {
        add => AddHandler(ExpandStateInvalidatedEvent, value);
        remove => RemoveHandler(ExpandStateInvalidatedEvent, value);
    }

    /// <summary>
    /// Occurs when the state of the collapse is invalidated.
    /// </summary>
    public event RoutedEventHandler CollapseStateInvalidated
    {
        add => AddHandler(CollapseStateInvalidatedEvent, value);
        remove => RemoveHandler(CollapseStateInvalidatedEvent, value);
    }

    #endregion

    static SidePanelWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SidePanelWindow), new FrameworkPropertyMetadata(typeof(SidePanelWindow)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SidePanelWindow"/> class.
    /// </summary>
    public SidePanelWindow() : base()
    {
        Chrome.CaptionHeight = 0;
        Chrome.GlassFrameThickness = new(0);
        Chrome.ResizeBorderThickness = new(0);
        Chrome.NonClientFrameEdges = NonClientFrameEdges.None;

        AllowsTransparency = true;
        WindowStyle = WindowStyle.None;

        _expandAnimation = new DoubleAnimation();
        _collapseAnimation = new DoubleAnimation();
        _expandAnimation.Completed += OnExpandCompleted;
        _collapseAnimation.Completed += OnCollapseCompleted;
        _expandAnimation.CurrentStateInvalidated += ExpandCurrentStateInvalidated;
        _collapseAnimation.CurrentStateInvalidated += OnCollapseCurrentStateInvalidated;
    }

    #region Methods Override

    /// <inheritdoc/>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        UnhookWindow();
    }

    /// <summary>
    /// Called when the control's template is applied. Initializes the slider control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _contentWrapper = Template.FindName("PART_ContentWrapper", this) as ContentControl;

        if (!IsExpanded)
        {
            _contentWrapper?.SetValue(WidthProperty, 0D);
            _contentWrapper?.SetValue(HeightProperty, 0D);
        }
        else if (Dock is Dock.Left or Dock.Right)
        {
            _contentWrapper?.SetValue(WidthProperty, ExpandSize);
            _contentWrapper?.SetValue(HeightProperty, double.NaN);
        }
        else if (Dock is Dock.Top or Dock.Bottom)
        {
            _contentWrapper?.SetValue(HeightProperty, ExpandSize);
            _contentWrapper?.SetValue(WidthProperty, double.NaN);
        }
    }

    /// <summary>
    /// Called when the window source is initialized. Sets window properties and hooks.
    /// </summary>
    /// <param name="e">Event arguments.</param>
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        _hwnd = new WindowInteropHelper(this).Handle;

        if (IntPtr.Zero != _hwnd)
        {
            _hwndSource = HwndSource.FromHwnd(_hwnd);

            var exStyle = NativeMethods.GetWindowStyleEx(_hwnd);
            exStyle = (exStyle & ~WS_EX.APPWINDOW) | WS_EX.TOOLWINDOW;

            NativeMethods.SetWindowStyleEx(_hwnd, exStyle);

            var style = NativeMethods.GetWindowStyle(_hwnd);
            style = style & ~(WS.MAXIMIZEBOX | WS.CAPTION | WS.SYSMENU);

            NativeMethods.SetWindowStyle(_hwnd, style);

            if (IsExpanded)
            {
                UpdateWindowPos();
            }

            _hwndSource.AddHook(new HwndSourceHook(WndProc));
            _isHooked = true;

            UpdateFrameState();
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Processes window messages for the side panel window.
    /// </summary>
    /// <param name="hwnd">Handle to the window.</param>
    /// <param name="msg">Message ID.</param>
    /// <param name="wParam">Message parameters.</param>
    /// <param name="lParam">Message parameters.</param>
    /// <param name="handled">Indicates whether the message was handled.</param>
    /// <returns>Return value of the processed message.</returns>
    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        var message = (WM)msg;

        return message switch
        {
            WM.SYSCOMMAND => HandleSYSCOMMAND(hwnd, msg, wParam, lParam, ref handled),
            // LBUTTONDOWN
            WM.LBUTTONDOWN => HandleLBUTTONDOWN(hwnd, msg, wParam, lParam, ref handled),
            // KILLFOCUS
            WM.KILLFOCUS => HandleKILLFOCUS(hwnd, msg, wParam, lParam, ref handled),
            _ => IntPtr.Zero,
        };
    }

    /// <summary>
    /// Shows the side panel window and expands it.
    /// </summary>
    public new void Show()
    {
        IsExpanded = true;
    }

    /// <summary>
    /// Hides the side panel window and collapses it.
    /// </summary>
    public new void Hide()
    {
        IsExpanded = false;
    }

    /// <summary>
    /// Sets the window position based on docking and expand size.
    /// </summary>
    private void UpdateWindowPos()
    {
        var left = 0;
        var top = 0;
        var width = 0;
        var height = 0;

        var dpiScale = this.GetDpi();

        if (Dock == Dock.Right)
        {
            width = (int)(ExpandSize * dpiScale.DpiScaleX / 100) + 20;
            height = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Bottom - 20;
            left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Right - width - 10;
            top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Top + 10;
        }
        else if (Dock == Dock.Left)
        {
            width = (int)(ExpandSize * dpiScale.DpiScaleX / 100);
            height = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Bottom;
            left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Left;
            top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Top;
        }
        else if (Dock == Dock.Bottom)
        {
            width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Right;
            height = (int)(ExpandSize * dpiScale.DpiScaleY / 100);
            left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Left;
            top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Bottom - height;
        }
        else if (Dock == Dock.Top)
        {
            width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Right;
            left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Left;
            height = (int)(ExpandSize * dpiScale.DpiScaleY / 100);
            top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Top;
        }

        Left = left;
        Top = top;
        Width = width;
        Height = height;
    }

    private static HorizontalAlignment GetHorizontalAlignment(Dock dock)
    {
        return dock is Dock.Right ? HorizontalAlignment.Right : (dock is Dock.Left ? HorizontalAlignment.Left : HorizontalAlignment.Stretch);
    }

    private static VerticalAlignment GetVerticalAlignment(Dock dock)
    {
        return dock is Dock.Top ? VerticalAlignment.Top : (dock is Dock.Bottom ? VerticalAlignment.Bottom : VerticalAlignment.Stretch);
    }

    private object IsExpandedCoerceValue(bool baseValue)
    {
        return DesignerProperties.GetIsInDesignMode(this) || (!IsAnimationInProcess ? baseValue : IsExpanded);
    }

    private object DockCoerceValue(Dock baseValue)
    {
        return !IsAnimationInProcess ? baseValue : (object)Dock;
    }

    private void OnIsExpandedChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateFrameState();

        if (IsExpanded)
        {
            Expand();
        }
        else
        {
            Collapse();
        }

        OnIsExpandedChanged((bool)e.OldValue, (bool)e.NewValue);
        IsExpandedChanged?.Invoke(this, e);
    }

    private void OnExpandSizeChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateFrameState();
    }

    private void OnEasingFunctionChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateFrameState();
    }

    private void OnAutoHideChanged(DependencyPropertyChangedEventArgs e)
    {
        OnAutoHideChanged((bool)e.OldValue, (bool)e.NewValue);
        AutoHideChanged?.Invoke(this, e);
    }

    private void OnDockChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateFrameState();

        OnDockChanged((Dock)e.OldValue, (Dock)e.NewValue);
        DockChanged?.Invoke(this, e);

        UpdateWindowPos();
    }

    private static object IsExpandedCoerceValue(DependencyObject d, object baseValue)
    {
        var sidePanelWindow = (SidePanelWindow)d;
        return sidePanelWindow.IsExpandedCoerceValue((bool)baseValue);
    }

    private static object DockCoerceValue(DependencyObject d, object baseValue)
    {
        var sidePanelWindow = (SidePanelWindow)d;
        return sidePanelWindow.DockCoerceValue((Dock)baseValue);
    }

    private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var sidePanelWindow = (SidePanelWindow)d;
        sidePanelWindow.OnIsExpandedChanged(e);
    }

    private static void OnExpandSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var sidePanelWindow = (SidePanelWindow)d;
        sidePanelWindow.OnExpandSizeChanged(e);
    }

    private static void OnEasingFunctionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var sidePanelWindow = (SidePanelWindow)d;
        sidePanelWindow.OnEasingFunctionChanged(e);
    }

    private static void OnAutoHideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var sidePanelWindow = (SidePanelWindow)d;
        sidePanelWindow.OnAutoHideChanged(e);
    }

    private static void OnDockChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var sidePanelWindow = (SidePanelWindow)d;
        sidePanelWindow.OnDockChanged(e);
    }

    /// <summary>
    /// Called when the Dock property changes.
    /// </summary>
    /// <param name="oldValue">The old value of the Dock property.</param>
    /// <param name="newValue">The new value of the Dock property.</param>
    protected virtual void OnDockChanged(Dock oldValue, Dock newValue)
    {
    }

    /// <summary>
    /// Called when the AutoHide property changes.
    /// </summary>
    /// <param name="oldValue">The old value of the AutoHide property.</param>
    /// <param name="newValue">The new value of the AutoHide property.</param>
    protected virtual void OnAutoHideChanged(bool oldValue, bool newValue)
    {
    }

    /// <summary>
    /// Called when the IsExpanded property changes.
    /// </summary>
    /// <param name="oldValue">The old value of the IsExpanded property.</param>
    /// <param name="newValue">The new value of the IsExpanded property.</param>
    protected virtual void OnIsExpandedChanged(bool oldValue, bool newValue)
    {
    }

    private void UpdateFrameState()
    {
        var verticalAlignment = GetVerticalAlignment(Dock);
        _contentWrapper?.SetValue(VerticalAlignmentProperty, verticalAlignment);

        var horizontalAlignment = GetHorizontalAlignment(Dock);
        _contentWrapper?.SetValue(HorizontalAlignmentProperty, horizontalAlignment);

        if (IsExpanded)
        {
            if (Dock is Dock.Right or Dock.Left)
            {
                _contentWrapper?.SetValue(WidthProperty, ExpandSize);
                _contentWrapper?.SetValue(HeightProperty, double.NaN);
            }
            else if (Dock is Dock.Bottom or Dock.Top)
            {
                _contentWrapper?.SetValue(HeightProperty, ExpandSize);
                _contentWrapper?.SetValue(WidthProperty, double.NaN);
            }
        }

        SetEasingFunction(EasingFunction);
    }

    /// <summary>
    /// Expands the slider control to its defined size, triggering an animation.
    /// </summary>
    public void Expand()
    {
        if (_expandAnimation is null)
        {
            return;
        }

        _expandAnimation.From = 0;
        _expandAnimation.To = ExpandSize;
        _expandAnimation.SpeedRatio = _animateSpeedRatio;

        UpdateWindowPos();

        base.Show();

        if (Dock is Dock.Right or Dock.Left)
        {
            _contentWrapper?.SetValue(WidthProperty, ExpandSize);
            _contentWrapper?.SetValue(HeightProperty, double.NaN);
            _contentWrapper?.BeginAnimation(WidthProperty, _expandAnimation);
        }
        else if (Dock is Dock.Bottom or Dock.Top)
        {
            _contentWrapper?.SetValue(HeightProperty, ExpandSize);
            _contentWrapper?.SetValue(WidthProperty, double.NaN);
            _contentWrapper?.BeginAnimation(HeightProperty, _expandAnimation);
        }

        var routedEventArgs = new RoutedEventArgs(ExpandStartedEvent, _expandAnimation);
        RaiseEvent(routedEventArgs);
    }

    /// <summary>
    /// Collapses the slider control to its minimum size (zero), triggering an animation.
    /// </summary>
    public void Collapse()
    {
        if (_collapseAnimation is null)
        {
            return;
        }

        _collapseAnimation.From = ExpandSize;
        _collapseAnimation.To = 0;
        _collapseAnimation.SpeedRatio = _animateSpeedRatio;

        if (Dock is Dock.Right or Dock.Left)
        {
            _contentWrapper?.BeginAnimation(WidthProperty, _collapseAnimation);
        }
        else if (Dock is Dock.Bottom or Dock.Top)
        {
            _contentWrapper?.BeginAnimation(HeightProperty, _collapseAnimation);
        }

        var routedEventArgs = new RoutedEventArgs(CollapseStartedEvent, _collapseAnimation);
        RaiseEvent(routedEventArgs);
    }

    /// <summary>
    /// Sets the easing function for the expand and collapse animations.
    /// </summary>
    /// <param name="easingFunction">The easing function to be applied to the animations.</param>
    public void SetEasingFunction(IEasingFunction easingFunction)
    {
        if (_expandAnimation is not null)
        {
            _expandAnimation.EasingFunction = easingFunction;
        }

        if (_collapseAnimation is not null)
        {
            _collapseAnimation.EasingFunction = easingFunction;
        }
    }

    private void ClearAnimation()
    {
        _contentWrapper?.BeginAnimation(WidthProperty, null);
        _contentWrapper?.BeginAnimation(HeightProperty, null);
    }

    private void UnhookWindow()
    {
        if (!_isHooked || _hwndSource is null)
        {
            return;
        }

        if (_expandAnimation is not null)
        {
            _expandAnimation.Completed -= OnExpandCompleted;
            _expandAnimation.CurrentStateInvalidated -= ExpandCurrentStateInvalidated;
            _expandAnimation = null;
        }

        if (_collapseAnimation is not null)
        {
            _collapseAnimation.Completed -= OnCollapseCompleted;
            _collapseAnimation.CurrentStateInvalidated -= OnCollapseCurrentStateInvalidated;
            _collapseAnimation = null;
        }

        _hwndSource.RemoveHook(new HwndSourceHook(WndProc));
        _isHooked = false;
    }

    private IntPtr HandleSYSCOMMAND(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        handled = false;

        var command = (WM)(wParam.ToInt32() & 0xFFF0);
        switch (command)
        {
            case WM.SC_MINIMIZE:
            case WM.SC_RESTORE:
                IsExpanded = !IsExpanded;
                handled = true; // Mark as handled
                break;
        }

        return IntPtr.Zero;
    }

    private IntPtr HandleLBUTTONDOWN(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        handled = false;

        if (AutoHide && IsExpanded)
        {
            var mousePosition = Mouse.GetPosition(this);
            if (VisualTreeHelper.HitTest(this, mousePosition) == null)
            {
                IsExpanded = false;
            }
        }

        return IntPtr.Zero;
    }

    private IntPtr HandleKILLFOCUS(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        handled = false;

        if (AutoHide && IsExpanded)
        {
            IsExpanded = false;
        }

        return IntPtr.Zero;
    }

    #endregion

    #region Events Subscriptions

    private void OnExpandCompleted(object? sender, EventArgs e)
    {
        ClearAnimation();

        if (Dock is Dock.Right or Dock.Left)
        {
            _contentWrapper?.SetValue(WidthProperty, ExpandSize);
            _contentWrapper?.SetValue(HeightProperty, double.NaN);
        }
        else if (Dock is Dock.Bottom or Dock.Top)
        {
            _contentWrapper?.SetValue(HeightProperty, ExpandSize);
            _contentWrapper?.SetValue(WidthProperty, double.NaN);
        }

        Activate(); // Activate the window

        var routedEventArgs = new RoutedEventArgs(ExpandCompletedEvent, sender);
        RaiseEvent(routedEventArgs);
    }

    private void ExpandCurrentStateInvalidated(object? sender, EventArgs e)
    {
        if (sender is not AnimationClock animationClock)
        {
            return;
        }

        var isAnimationInProcess = false;

        if (animationClock.CurrentState == ClockState.Filling)
            isAnimationInProcess = false;
        else if (animationClock.CurrentState == ClockState.Active)
            isAnimationInProcess = true;

        SetValue(IsAnimationInProcessPropertyKey, isAnimationInProcess);

        var routedEventArgs = new RoutedEventArgs(ExpandStateInvalidatedEvent, sender);
        RaiseEvent(routedEventArgs);
    }

    private void OnCollapseCompleted(object? sender, EventArgs e)
    {
        base.Hide();

        ClearAnimation();

        if (Dock is Dock.Right or Dock.Left)
        {
            _contentWrapper?.SetValue(WidthProperty, 0.0D);
            _contentWrapper?.SetValue(HeightProperty, double.NaN);
        }
        else if (Dock is Dock.Bottom or Dock.Top)
        {
            _contentWrapper?.SetValue(HeightProperty, 0.0D);
            _contentWrapper?.SetValue(WidthProperty, double.NaN);
        }

        var routedEventArgs = new RoutedEventArgs(CollapseCompletedEvent, sender);
        RaiseEvent(routedEventArgs);
    }

    private void OnCollapseCurrentStateInvalidated(object? sender, EventArgs e)
    {
        if (sender is not AnimationClock animationClock)
        {
            return;
        }

        var isAnimationInProcess = false;

        if (animationClock.CurrentState == ClockState.Filling)
        {
            isAnimationInProcess = false;
        }
        else if (animationClock.CurrentState == ClockState.Active)
        {
            isAnimationInProcess = true;
        }

        SetValue(IsAnimationInProcessPropertyKey, isAnimationInProcess);

        var routedEventArgs = new RoutedEventArgs(CollapseStateInvalidatedEvent, sender);
        RaiseEvent(routedEventArgs);
    }

    #endregion
}