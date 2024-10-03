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
/// Represents a slider control that can expand and collapse, 
/// providing animation effects and properties for docking and auto-hiding behavior.
/// </summary>
public partial class SliderControl : ContentControl
{
    #region Fields/Consts

    private bool _isHooked;
    private IntPtr _hwnd;
    private Window? _window;
    private HwndSource? _hwndSource;
    private DoubleAnimation? _expandAnimation;
    private DoubleAnimation? _collapseAnimation;
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
        DependencyProperty.RegisterReadOnly("IsAnimationInProcess", typeof(bool), typeof(SliderControl),
        new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>
    /// Dependency property for indicating if an animation is currently in process.
    /// </summary>
    public static readonly DependencyProperty IsAnimationInProcessProperty = IsAnimationInProcessPropertyKey.DependencyProperty;

    /// <summary>
    /// Dependency property for the AutoHide feature of the slider control.
    /// </summary>
    public static readonly DependencyProperty AutoHideProperty =
        DependencyProperty.Register("AutoHide", typeof(bool), typeof(SliderControl),
        new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.Inherits, OnAutoHideChanged));

    /// <summary>
    /// Dependency property for the docking position of the slider control.
    /// </summary>
    public static readonly DependencyProperty DockProperty =
        DependencyProperty.Register("Dock", typeof(Dock), typeof(SliderControl),
        new FrameworkPropertyMetadata(default(Dock), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDockChanged, DockCoerceValue));

    /// <summary>
    /// Dependency property for indicating if the slider control is expanded.
    /// </summary>
    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register("IsExpanded", typeof(bool), typeof(SliderControl),
        new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsExpandedChanged, IsExpandedCoerceValue));

    /// <summary>
    /// Dependency property for the size to which the slider control expands.
    /// </summary>
    public static readonly DependencyProperty ExpandSizeProperty =
        DependencyProperty.Register("ExpandSize", typeof(double), typeof(SliderControl),
        new FrameworkPropertyMetadata(320D, OnExpandSizeChanged));

    /// <summary>
    /// Dependency property for the easing function used in animations.
    /// </summary>
    public static readonly DependencyProperty EasingFunctionProperty =
        DependencyProperty.Register("EasingFunction", typeof(IEasingFunction), typeof(SliderControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnEasingFunctionChanged));

    /// <summary>
    /// Dependency property for enabling or disabling glow effects on the slider control.
    /// </summary>
    public static readonly DependencyProperty IsGlowEnableProperty =
        DependencyProperty.Register("IsGlowEnable", typeof(bool), typeof(SliderControl),
        new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>
    /// Routed event triggered when a collapse animation completes.
    /// </summary>
    public static readonly RoutedEvent CollapseCompletedEvent =
        EventManager.RegisterRoutedEvent("CollapseCompletedEventHandler", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SliderControl));

    /// <summary>
    /// Routed event triggered when an expand animation completes.
    /// </summary>
    public static readonly RoutedEvent ExpandCompletedEvent =
        EventManager.RegisterRoutedEvent("ExpandCompletedEventHandler", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SliderControl));

    /// <summary>
    /// Routed event triggered when a collapse animation starts.
    /// </summary>
    public static readonly RoutedEvent CollapseStartedEvent =
        EventManager.RegisterRoutedEvent("CollapseStartedEventHandler", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SliderControl));

    /// <summary>
    /// Routed event triggered when an expand animation starts.
    /// </summary>
    public static readonly RoutedEvent ExpandStartedEvent =
        EventManager.RegisterRoutedEvent("ExpandStartedEventHandler", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SliderControl));

    /// <summary>
    /// Routed event triggered when the expand state is invalidated.
    /// </summary>
    public static readonly RoutedEvent ExpandStateInvalidatedEvent =
        EventManager.RegisterRoutedEvent("ExpandStateInvalidatedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SliderControl));

    /// <summary>
    /// Routed event triggered when the collapse state is invalidated.
    /// </summary>
    public static readonly RoutedEvent CollapseStateInvalidatedEvent =
        EventManager.RegisterRoutedEvent("CollapseStateInvalidatedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SliderControl));

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
    /// Gets or sets a value indicating whether the control is expanded.
    /// </summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>
    /// Gets or sets the size of the control when expanded.
    /// </summary>
    public double ExpandSize
    {
        get => (double)GetValue(ExpandSizeProperty);
        set => SetValue(ExpandSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the glow effect is enabled for the control.
    /// </summary>
    public bool IsGlowEnable
    {
        get => (bool)GetValue(IsGlowEnableProperty);
        set => SetValue(IsGlowEnableProperty, value);
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

    static SliderControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SliderControl), new FrameworkPropertyMetadata(typeof(SliderControl)));
        HorizontalAlignmentProperty.OverrideMetadata(typeof(SliderControl), new FrameworkPropertyMetadata(null, HorizontalAlignmentCoerceValue));
        VerticalAlignmentProperty.OverrideMetadata(typeof(SliderControl), new FrameworkPropertyMetadata(null, VerticalAlignmentCoerceValue));
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

                ApplyNewSliderControl();
            }
            else
            {
                _window.SourceInitialized += WindowSourceInitialized;
            }
        }

        if (!IsExpanded)
        {
            SetCurrentValue(WidthProperty, 0D);
            SetCurrentValue(HeightProperty, 0D);
        }
        else if (Dock is Dock.Left or Dock.Right)
        {
            SetCurrentValue(WidthProperty, ExpandSize);
            SetCurrentValue(HeightProperty, double.NaN);
        }
        else if (Dock is Dock.Top or Dock.Bottom)
        {
            SetCurrentValue(HeightProperty, ExpandSize);
            SetCurrentValue(WidthProperty, double.NaN);
        }
    }

    #endregion

    #region Methods

    private static bool IsValidHorizontalAlignment(Dock dock, HorizontalAlignment horizontalAlignment)
    {
        return (dock is Dock.Right && horizontalAlignment is HorizontalAlignment.Right)
               || (dock is Dock.Left && horizontalAlignment is HorizontalAlignment.Left)
               || (dock is not Dock.Right && dock is not Dock.Left && horizontalAlignment is HorizontalAlignment.Stretch);
    }

    private static bool IsValidVerticalAlignment(Dock dock, VerticalAlignment verticalAlignment)
    {
        return (dock is Dock.Top && verticalAlignment is VerticalAlignment.Top)
               || (dock is Dock.Bottom && verticalAlignment is VerticalAlignment.Bottom)
               || (dock is not Dock.Top && dock is not Dock.Bottom && verticalAlignment is VerticalAlignment.Stretch);
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

    private object HorizontalAlignmentCoerceValue(HorizontalAlignment baseValue)
    {
        return IsValidHorizontalAlignment(Dock, baseValue) ? baseValue : HorizontalAlignment;
    }

    private object VerticalAlignmentCoerceValue(VerticalAlignment baseValue)
    {
        return IsValidVerticalAlignment(Dock, baseValue) ? baseValue : VerticalAlignment;
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
    }

    private static object IsExpandedCoerceValue(DependencyObject d, object baseValue)
    {
        var sliderControl = (SliderControl)d;
        return sliderControl.IsExpandedCoerceValue((bool)baseValue);
    }

    private static object HorizontalAlignmentCoerceValue(DependencyObject d, object baseValue)
    {
        var sliderControl = (SliderControl)d;
        return sliderControl.HorizontalAlignmentCoerceValue((HorizontalAlignment)baseValue);
    }

    private static object VerticalAlignmentCoerceValue(DependencyObject d, object baseValue)
    {
        var sliderControl = (SliderControl)d;
        return sliderControl.VerticalAlignmentCoerceValue((VerticalAlignment)baseValue);
    }

    private static object DockCoerceValue(DependencyObject d, object baseValue)
    {
        var sliderControl = (SliderControl)d;
        return sliderControl.DockCoerceValue((Dock)baseValue);
    }

    private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var sliderControl = (SliderControl)d;
        sliderControl.OnIsExpandedChanged(e);
    }

    private static void OnExpandSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var sliderControl = (SliderControl)d;
        sliderControl.OnExpandSizeChanged(e);
    }

    private static void OnEasingFunctionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var sliderControl = (SliderControl)d;
        sliderControl.OnEasingFunctionChanged(e);
    }

    private static void OnAutoHideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var sliderControl = (SliderControl)d;
        sliderControl.OnAutoHideChanged(e);
    }

    private static void OnDockChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var sliderControl = (SliderControl)d;
        sliderControl.OnDockChanged(e);
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

    private void ApplyNewSliderControl()
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

            UpdateFrameState();
        }
    }

    private void UpdateFrameState()
    {
        if (IntPtr.Zero == _hwnd || _hwndSource?.IsDisposed is true)
        {
            return;
        }

        var verticalAlignment = GetVerticalAlignment(Dock);
        SetCurrentValue(VerticalAlignmentProperty, verticalAlignment);

        var horizontalAlignment = GetHorizontalAlignment(Dock);
        SetCurrentValue(HorizontalAlignmentProperty, horizontalAlignment);

        if (IsExpanded)
        {
            if (Dock is Dock.Right or Dock.Left)
            {
                SetCurrentValue(WidthProperty, ExpandSize);
                SetCurrentValue(HeightProperty, double.NaN);
            }
            else if (Dock is Dock.Bottom or Dock.Top)
            {
                SetCurrentValue(HeightProperty, ExpandSize);
                SetCurrentValue(WidthProperty, double.NaN);
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

        if (Dock is Dock.Right or Dock.Left)
        {
            SetCurrentValue(WidthProperty, ExpandSize);
            SetCurrentValue(HeightProperty, double.NaN);
            BeginAnimation(WidthProperty, _expandAnimation);
        }
        else if (Dock is Dock.Bottom or Dock.Top)
        {
            SetCurrentValue(HeightProperty, ExpandSize);
            SetCurrentValue(WidthProperty, double.NaN);
            BeginAnimation(HeightProperty, _expandAnimation);
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
            BeginAnimation(WidthProperty, _collapseAnimation);
        }
        else if (Dock is Dock.Bottom or Dock.Top)
        {
            BeginAnimation(HeightProperty, _collapseAnimation);
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
        BeginAnimation(WidthProperty, null);
        BeginAnimation(HeightProperty, null);
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

        if (AutoHide && IsExpanded)
        {
            var mousePosition = Mouse.GetPosition(this);
            if (VisualTreeHelper.HitTest(this, mousePosition) == null)
            {
                SetCurrentValue(IsExpandedProperty, false);
            }
        }

        return IntPtr.Zero;
    }

    private IntPtr HandleKILLFOCUS(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        handled = false;

        if (AutoHide && IsExpanded)
        {
            SetCurrentValue(IsExpandedProperty, false);
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
            SetCurrentValue(WidthProperty, ExpandSize);
            SetCurrentValue(HeightProperty, double.NaN);
        }
        else if (Dock is Dock.Bottom or Dock.Top)
        {
            SetCurrentValue(HeightProperty, ExpandSize);
            SetCurrentValue(WidthProperty, double.NaN);
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
            SetCurrentValue(WidthProperty, 0.0D);
            SetCurrentValue(HeightProperty, double.NaN);
        }
        else if (Dock is Dock.Bottom or Dock.Top)
        {
            SetCurrentValue(HeightProperty, 0.0D);
            SetCurrentValue(WidthProperty, double.NaN);
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

    private void WindowSourceInitialized(object? sender, EventArgs e)
    {
        _hwnd = new WindowInteropHelper(_window).Handle;
        _hwndSource = HwndSource.FromHwnd(_hwnd);

        ApplyNewSliderControl();
    }

    private void WindowClosed(object? sender, EventArgs e)
    {
        UnhookWindow();
    }

    #endregion
}