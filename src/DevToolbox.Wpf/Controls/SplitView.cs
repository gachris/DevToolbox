using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a split view that can expand and collapse, 
/// providing animation effects and properties for docking and auto-hiding behavior.
/// </summary>
public partial class SplitView : ContentControl
{
    #region Fields/Consts

    private bool _isHooked;
    private IntPtr _hwnd;
    private Window? _window;
    private HwndSource? _hwndSource;
    private DoubleAnimation? _expandAnimation;
    private DoubleAnimation? _collapseAnimation;
    private ContentPresenter? _paneContentControl;
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
        DependencyProperty.RegisterReadOnly(nameof(IsAnimationInProcess), typeof(bool), typeof(SplitView),
        new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>
    /// Dependency property for indicating if an animation is currently in process.
    /// </summary>
    public static readonly DependencyProperty IsAnimationInProcessProperty = IsAnimationInProcessPropertyKey.DependencyProperty;

    /// <summary>
    /// Dependency property for the AutoHide feature of the split view.
    /// </summary>
    public static readonly DependencyProperty AutoHideProperty =
        DependencyProperty.Register(nameof(AutoHide), typeof(bool), typeof(SplitView),
        new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.Inherits, OnAutoHideChanged));

    /// <summary>
    /// Dependency property for the docking position of the split view.
    /// </summary>
    public static readonly DependencyProperty DockProperty =
        DependencyProperty.Register(nameof(Dock), typeof(Dock), typeof(SplitView),
        new FrameworkPropertyMetadata(default(Dock), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDockChanged, DockCoerceValue));

    /// <summary>
    /// Dependency property for indicating if the split view is expanded.
    /// </summary>
    public static readonly DependencyProperty IsPaneOpenProperty =
        DependencyProperty.Register(nameof(IsPaneOpen), typeof(bool), typeof(SplitView),
        new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsExpandedChanged, IsExpandedCoerceValue));

    /// <summary>
    /// Dependency property for the size to which the split view expands.
    /// </summary>
    public static readonly DependencyProperty OpenPaneLengthProperty =
        DependencyProperty.Register(nameof(OpenPaneLength), typeof(double), typeof(SplitView),
        new FrameworkPropertyMetadata(320D, OnExpandSizeChanged));

    /// <summary>
    /// Dependency property for the easing function used in animations.
    /// </summary>
    public static readonly DependencyProperty EasingFunctionProperty =
        DependencyProperty.Register(nameof(EasingFunction), typeof(IEasingFunction), typeof(SplitView),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnEasingFunctionChanged));

    /// <summary>
    /// Dependency property for the pane.
    /// </summary>
    public static readonly DependencyProperty PaneProperty =
        DependencyProperty.Register(nameof(Pane), typeof(object), typeof(SplitView), new FrameworkPropertyMetadata(null));

    /// <summary>
    /// Routed event triggered when a collapse animation completes.
    /// </summary>
    public static readonly RoutedEvent CollapseCompletedEvent =
        EventManager.RegisterRoutedEvent("CollapseCompletedEventHandler", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SplitView));

    /// <summary>
    /// Routed event triggered when an expand animation completes.
    /// </summary>
    public static readonly RoutedEvent ExpandCompletedEvent =
        EventManager.RegisterRoutedEvent("ExpandCompletedEventHandler", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SplitView));

    /// <summary>
    /// Routed event triggered when a collapse animation starts.
    /// </summary>
    public static readonly RoutedEvent CollapseStartedEvent =
        EventManager.RegisterRoutedEvent("CollapseStartedEventHandler", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SplitView));

    /// <summary>
    /// Routed event triggered when an expand animation starts.
    /// </summary>
    public static readonly RoutedEvent ExpandStartedEvent =
        EventManager.RegisterRoutedEvent("ExpandStartedEventHandler", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SplitView));

    /// <summary>
    /// Routed event triggered when the expand state is invalidated.
    /// </summary>
    public static readonly RoutedEvent ExpandStateInvalidatedEvent =
        EventManager.RegisterRoutedEvent("ExpandStateInvalidatedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SplitView));

    /// <summary>
    /// Routed event triggered when the collapse state is invalidated.
    /// </summary>
    public static readonly RoutedEvent CollapseStateInvalidatedEvent =
        EventManager.RegisterRoutedEvent("CollapseStateInvalidatedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SplitView));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets a value indicating whether the control should auto-hide when not in use.
    /// </summary>
    public bool AutoHide
    {
        get => (bool)GetValue(AutoHideProperty);
        set => SetValue(AutoHideProperty, value);
    }

    /// <summary>
    /// Gets or sets the docking position of the control.
    /// </summary>
    public Dock Dock
    {
        get => (Dock)GetValue(DockProperty);
        set => SetValue(DockProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pane is opened.
    /// </summary>
    public bool IsPaneOpen
    {
        get => (bool)GetValue(IsPaneOpenProperty);
        set => SetValue(IsPaneOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets the size of the pane.
    /// </summary>
    public double OpenPaneLength
    {
        get => (double)GetValue(OpenPaneLengthProperty);
        set => SetValue(OpenPaneLengthProperty, value);
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

    /// <summary>
    /// Gets or sets the pane.
    /// </summary>
    public object Pane
    {
        get { return GetValue(PaneProperty); }
        set { SetValue(PaneProperty, value); }
    }

    #endregion

    #region Routed Events

    /// <summary>
    /// Occurs when the expand animation starts.
    /// </summary>
    public event RoutedEventHandler ExpandStarted
    {
        add => AddHandler(ExpandStartedEvent, value);
        remove => RemoveHandler(ExpandStartedEvent, value);
    }

    /// <summary>
    /// Occurs when the collapse animation starts.
    /// </summary>
    public event RoutedEventHandler CollapseStarted
    {
        add => AddHandler(CollapseStartedEvent, value);
        remove => RemoveHandler(CollapseStartedEvent, value);
    }

    /// <summary>
    /// Occurs when the expand animation completes.
    /// </summary>
    public event RoutedEventHandler ExpandCompleted
    {
        add => AddHandler(ExpandCompletedEvent, value);
        remove => RemoveHandler(ExpandCompletedEvent, value);
    }

    /// <summary>
    /// Occurs when the collapse animation completes.
    /// </summary>
    public event RoutedEventHandler CollapseCompleted
    {
        add => AddHandler(CollapseCompletedEvent, value);
        remove => RemoveHandler(CollapseCompletedEvent, value);
    }

    /// <summary>
    /// Occurs when the expand state is invalidated.
    /// </summary>
    public event RoutedEventHandler ExpandStateInvalidated
    {
        add => AddHandler(ExpandStateInvalidatedEvent, value);
        remove => RemoveHandler(ExpandStateInvalidatedEvent, value);
    }

    /// <summary>
    /// Occurs when the collapse state is invalidated.
    /// </summary>
    public event RoutedEventHandler CollapseStateInvalidated
    {
        add => AddHandler(CollapseStateInvalidatedEvent, value);
        remove => RemoveHandler(CollapseStateInvalidatedEvent, value);
    }

    #endregion

    static SplitView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitView), new FrameworkPropertyMetadata(typeof(SplitView)));
    }

    #region Methods Override

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        var window = Window.GetWindow(this);
        if (window != null)
        {
            UnhookWindow();

            _window = window;
            _hwnd = new WindowInteropHelper(_window).Handle;
            _window.Closed += WindowClosed;

            if (IntPtr.Zero != _hwnd)
            {
                _hwndSource = HwndSource.FromHwnd(_hwnd);

                Initialize();
            }
            else
            {
                _window.SourceInitialized += WindowSourceInitialized;
            }
        }

        _paneContentControl = Template.FindName("PART_Pane", this) as ContentPresenter;

        if (!IsPaneOpen)
        {
            _paneContentControl?.SetCurrentValue(WidthProperty, 0D);
            _paneContentControl?.SetCurrentValue(HeightProperty, 0D);
        }
        else if (Dock is Dock.Left or Dock.Right)
        {
            _paneContentControl?.SetCurrentValue(WidthProperty, OpenPaneLength);
            _paneContentControl?.SetCurrentValue(HeightProperty, double.NaN);
        }
        else if (Dock is Dock.Top or Dock.Bottom)
        {
            _paneContentControl?.SetCurrentValue(HeightProperty, OpenPaneLength);
            _paneContentControl?.SetCurrentValue(WidthProperty, double.NaN);
        }
    }

    #endregion

    #region Methods

    private object IsExpandedCoerceValue(bool baseValue)
    {
        return DesignerProperties.GetIsInDesignMode(this) || (!IsAnimationInProcess ? baseValue : IsPaneOpen);
    }

    private object DockCoerceValue(Dock baseValue)
    {
        return !IsAnimationInProcess ? baseValue : (object)Dock;
    }

    private void OnIsExpandedChanged(DependencyPropertyChangedEventArgs e)
    {
        Update();

        if (IsPaneOpen)
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
        Update();
    }

    private void OnEasingFunctionChanged(DependencyPropertyChangedEventArgs e)
    {
        Update();
    }

    private void OnAutoHideChanged(DependencyPropertyChangedEventArgs e)
    {
        OnAutoHideChanged((bool)e.OldValue, (bool)e.NewValue);
        AutoHideChanged?.Invoke(this, e);
    }

    private void OnDockChanged(DependencyPropertyChangedEventArgs e)
    {
        Update();

        OnDockChanged((Dock)e.OldValue, (Dock)e.NewValue);
        DockChanged?.Invoke(this, e);
    }

    private static object IsExpandedCoerceValue(DependencyObject d, object baseValue)
    {
        var splitView = (SplitView)d;
        return splitView.IsExpandedCoerceValue((bool)baseValue);
    }

    private static object DockCoerceValue(DependencyObject d, object baseValue)
    {
        var splitView = (SplitView)d;
        return splitView.DockCoerceValue((Dock)baseValue);
    }

    private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var splitView = (SplitView)d;
        splitView.OnIsExpandedChanged(e);
    }

    private static void OnExpandSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var splitView = (SplitView)d;
        splitView.OnExpandSizeChanged(e);
    }

    private static void OnEasingFunctionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var splitView = (SplitView)d;
        splitView.OnEasingFunctionChanged(e);
    }

    private static void OnAutoHideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var splitView = (SplitView)d;
        splitView.OnAutoHideChanged(e);
    }

    private static void OnDockChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var splitView = (SplitView)d;
        splitView.OnDockChanged(e);
    }

    /// <summary>
    /// Called when the Dock property changes.
    /// </summary>
    /// <param name="oldValue">The old value of the Dock property.</param>
    /// <param name="newValue">The new value of the Dock property.</param>
    protected virtual void OnDockChanged(Dock oldValue, Dock newValue)
    {
        // This method can be overridden in derived classes to handle logic
        // that should occur when the Dock property changes.
    }

    /// <summary>
    /// Called when the AutoHide property changes.
    /// </summary>
    /// <param name="oldValue">The old value of the AutoHide property.</param>
    /// <param name="newValue">The new value of the AutoHide property.</param>
    protected virtual void OnAutoHideChanged(bool oldValue, bool newValue)
    {
        // This method can be overridden in derived classes to implement
        // custom behavior in response to changes in the AutoHide property.
    }

    /// <summary>
    /// Called when the IsExpanded property changes.
    /// </summary>
    /// <param name="oldValue">The old value of the IsExpanded property.</param>
    /// <param name="newValue">The new value of the IsExpanded property.</param>
    protected virtual void OnIsExpandedChanged(bool oldValue, bool newValue)
    {
        // This method can be overridden in derived classes to define behavior
        // that should occur when the IsExpanded property changes.
    }

    private void Initialize()
    {
        if (_hwnd == IntPtr.Zero || _hwndSource?.IsDisposed is true)
        {
            return;
        }
        else
        {
            if (!_isHooked && _hwndSource is not null)
            {
                _hwndSource.AddHook(new HwndSourceHook(WndProc));
                _isHooked = true;
            }

            _expandAnimation = new DoubleAnimation();
            _collapseAnimation = new DoubleAnimation();
            _expandAnimation.Completed += OnExpandCompleted;
            _collapseAnimation.Completed += OnCollapseCompleted;
            _expandAnimation.CurrentStateInvalidated += ExpandCurrentStateInvalidated;
            _collapseAnimation.CurrentStateInvalidated += OnCollapseCurrentStateInvalidated;

            Update();
        }
    }

    private void Update()
    {
        if (IntPtr.Zero == _hwnd || _hwndSource?.IsDisposed is true)
        {
            return;
        }

        if (IsPaneOpen && _paneContentControl is not null)
        {
            if (Dock is Dock.Right or Dock.Left)
            {
                _paneContentControl.SetCurrentValue(WidthProperty, OpenPaneLength);
                _paneContentControl.SetCurrentValue(HeightProperty, double.NaN);
            }
            else if (Dock is Dock.Bottom or Dock.Top)
            {
                _paneContentControl.SetCurrentValue(HeightProperty, OpenPaneLength);
                _paneContentControl.SetCurrentValue(WidthProperty, double.NaN);
            }
        }

        SetEasingFunction(EasingFunction);
    }

    /// <summary>
    /// Expands the split view to its defined size, triggering an animation.
    /// </summary>
    public void Expand()
    {
        if (_expandAnimation is null)
        {
            return;
        }

        _expandAnimation.From = 0;
        _expandAnimation.To = OpenPaneLength;
        _expandAnimation.SpeedRatio = _animateSpeedRatio;

        if (Dock is Dock.Right or Dock.Left)
        {
            _paneContentControl?.SetCurrentValue(WidthProperty, OpenPaneLength);
            _paneContentControl?.SetCurrentValue(HeightProperty, double.NaN);
            _paneContentControl?.BeginAnimation(WidthProperty, _expandAnimation);
        }
        else if (Dock is Dock.Bottom or Dock.Top)
        {
            _paneContentControl?.SetCurrentValue(HeightProperty, OpenPaneLength);
            _paneContentControl?.SetCurrentValue(WidthProperty, double.NaN);
            _paneContentControl?.BeginAnimation(HeightProperty, _expandAnimation);
        }

        var routedEventArgs = new RoutedEventArgs(ExpandStartedEvent, _expandAnimation);
        RaiseEvent(routedEventArgs);
    }

    /// <summary>
    /// Collapses the split view to its minimum size (zero), triggering an animation.
    /// </summary>
    public void Collapse()
    {
        if (_collapseAnimation is null)
        {
            return;
        }

        _collapseAnimation.From = OpenPaneLength;
        _collapseAnimation.To = 0;
        _collapseAnimation.SpeedRatio = _animateSpeedRatio;

        if (Dock is Dock.Right or Dock.Left)
        {
            _paneContentControl?.BeginAnimation(WidthProperty, _collapseAnimation);
        }
        else if (Dock is Dock.Bottom or Dock.Top)
        {
            _paneContentControl?.BeginAnimation(HeightProperty, _collapseAnimation);
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
        _paneContentControl?.BeginAnimation(WidthProperty, null);
        _paneContentControl?.BeginAnimation(HeightProperty, null);
    }

    private void UnhookWindow()
    {
        if (!_isHooked || _hwndSource is null)
        {
            return;
        }

        if (_window is not null)
        {
            _window.SourceInitialized -= WindowSourceInitialized;
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

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        return msg switch
        {
            // LBUTTONDOWN
            0x0201 => HandleLBUTTONDOWN(hwnd, msg, wParam, lParam, ref handled),
            // KILLFOCUS
            0x0008 => HandleKILLFOCUS(hwnd, msg, wParam, lParam, ref handled),
            _ => IntPtr.Zero,
        };
    }

    private IntPtr HandleLBUTTONDOWN(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        handled = false;

        if (AutoHide && IsPaneOpen)
        {
            var mousePosition = Mouse.GetPosition(this);
            if (VisualTreeHelper.HitTest(this, mousePosition) == null)
            {
                SetCurrentValue(IsPaneOpenProperty, false);
            }
        }

        return IntPtr.Zero;
    }

    private IntPtr HandleKILLFOCUS(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        handled = false;

        if (AutoHide && IsPaneOpen)
        {
            SetCurrentValue(IsPaneOpenProperty, false);
        }

        return IntPtr.Zero;
    }

    #endregion

    #region Events Subscriptions

    private void WindowSourceInitialized(object? sender, EventArgs e)
    {
        _hwnd = new WindowInteropHelper(_window).Handle;
        _hwndSource = HwndSource.FromHwnd(_hwnd);

        Initialize();
    }

    private void WindowClosed(object? sender, EventArgs e)
    {
        UnhookWindow();
    }

    private void OnExpandCompleted(object? sender, EventArgs e)
    {
        ClearAnimation();

        if (Dock is Dock.Right or Dock.Left)
        {
            _paneContentControl?.SetCurrentValue(WidthProperty, OpenPaneLength);
            _paneContentControl?.SetCurrentValue(HeightProperty, double.NaN);
        }
        else if (Dock is Dock.Bottom or Dock.Top)
        {
            _paneContentControl?.SetCurrentValue(HeightProperty, OpenPaneLength);
            _paneContentControl?.SetCurrentValue(WidthProperty, double.NaN);
        }

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
        ClearAnimation();

        if (Dock is Dock.Right or Dock.Left)
        {
            _paneContentControl?.SetCurrentValue(WidthProperty, 0.0D);
            _paneContentControl?.SetCurrentValue(HeightProperty, double.NaN);
        }
        else if (Dock is Dock.Bottom or Dock.Top)
        {
            _paneContentControl?.SetCurrentValue(HeightProperty, 0.0D);
            _paneContentControl?.SetCurrentValue(WidthProperty, double.NaN);
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