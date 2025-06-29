using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DevToolbox.Wpf.Windows;

/// <summary>
/// Represents a sliding window control that can expand or collapse, supporting docking and animation.
/// </summary>
public class SidePanelWindow : WindowEx
{
    #region Fields/Consts

    private const double DefaultAnimationSpeedRatio = 5d;
    private TranslateTransform _contentTransform;
    private FrameworkElement? _container;

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
        DependencyProperty.RegisterReadOnly(nameof(IsAnimationInProcess), typeof(bool), typeof(SidePanelWindow), new FrameworkPropertyMetadata(default(bool)));

    /// <summary>
    /// Dependency property for indicating if an animation is currently in process.
    /// </summary>
    public static readonly DependencyProperty IsAnimationInProcessProperty = IsAnimationInProcessPropertyKey.DependencyProperty;

    /// <summary>
    /// Dependency property for the AutoHide feature of the slider control.
    /// </summary>
    public static readonly DependencyProperty AutoHideProperty =
        DependencyProperty.Register(nameof(AutoHide), typeof(bool), typeof(SidePanelWindow),
        new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.Inherits, OnAutoHideChanged));

    /// <summary>
    /// Dependency property for the docking position of the slider control.
    /// </summary>
    public static readonly DependencyProperty DockProperty =
        DependencyProperty.Register(nameof(Dock), typeof(Dock), typeof(SidePanelWindow),
        new FrameworkPropertyMetadata(Dock.Right, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDockChanged, DockCoerceValue));

    /// <summary>
    /// Dependency property for indicating if the slider control is expanded.
    /// </summary>
    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(SidePanelWindow),
        new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsExpandedChanged, IsExpandedCoerceValue));

    /// <summary>
    /// Dependency property for the easing function used in animations.
    /// </summary>
    public static readonly DependencyProperty EasingFunctionProperty =
        DependencyProperty.Register(nameof(EasingFunction), typeof(IEasingFunction), typeof(SidePanelWindow),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>
    /// Dependency property for the corner radius used in window.
    /// </summary>
    public static readonly DependencyProperty AnimationSpeedRatioProperty =
        DependencyProperty.Register(nameof(AnimationSpeedRatio), typeof(double), typeof(SidePanelWindow), new PropertyMetadata(DefaultAnimationSpeedRatio));

    /// <summary>
    /// Dependency property for the corner radius used in window.
    /// </summary>
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(SidePanelWindow), new PropertyMetadata(default(CornerRadius)));

    /// <summary>
    /// Routed event triggered when a collapse animation completes.
    /// </summary>
    public static readonly RoutedEvent CollapseCompletedEvent =
        EventManager.RegisterRoutedEvent(nameof(CollapseCompleted), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SidePanelWindow));

    /// <summary>
    /// Routed event triggered when an expand animation completes.
    /// </summary>
    public static readonly RoutedEvent ExpandCompletedEvent =
        EventManager.RegisterRoutedEvent(nameof(ExpandCompleted), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SidePanelWindow));

    /// <summary>
    /// Routed event triggered when a collapse animation starts.
    /// </summary>
    public static readonly RoutedEvent CollapseStartedEvent =
        EventManager.RegisterRoutedEvent(nameof(CollapseStarted), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SidePanelWindow));

    /// <summary>
    /// Routed event triggered when an expand animation starts.
    /// </summary>
    public static readonly RoutedEvent ExpandStartedEvent =
        EventManager.RegisterRoutedEvent(nameof(ExpandStarted), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SidePanelWindow));

    /// <summary>
    /// Routed event triggered when the expand state is invalidated.
    /// </summary>
    public static readonly RoutedEvent ExpandStateInvalidatedEvent =
        EventManager.RegisterRoutedEvent(nameof(ExpandStateInvalidated), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SidePanelWindow));

    /// <summary>
    /// Routed event triggered when the collapse state is invalidated.
    /// </summary>
    public static readonly RoutedEvent CollapseStateInvalidatedEvent =
        EventManager.RegisterRoutedEvent(nameof(CollapseStateInvalidated), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SidePanelWindow));

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
    /// Gets or sets the easing function used for animations.
    /// </summary>
    public IEasingFunction EasingFunction
    {
        get => (IEasingFunction)GetValue(EasingFunctionProperty);
        set => SetValue(EasingFunctionProperty, value);
    }

    /// <summary>
    /// Gets or sets the corner radius the window.
    /// </summary>
    public double AnimationSpeedRatio
    {
        get => (double)GetValue(AnimationSpeedRatioProperty);
        set => SetValue(AnimationSpeedRatioProperty, value);
    }

    /// <summary>
    /// Gets or sets the corner radius the window.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
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

        MinWidthProperty.OverrideMetadata(
            typeof(SidePanelWindow),
            new FrameworkPropertyMetadata(0.0));

        MinHeightProperty.OverrideMetadata(
            typeof(SidePanelWindow),
            new FrameworkPropertyMetadata(0.0));

        WidthProperty.OverrideMetadata(
            typeof(SidePanelWindow),
            new FrameworkPropertyMetadata(320D, OnLayoutPropertyChanged));

        HeightProperty.OverrideMetadata(
            typeof(SidePanelWindow),
            new FrameworkPropertyMetadata(400.0, OnLayoutPropertyChanged));

        VerticalAlignmentProperty.OverrideMetadata(
            typeof(SidePanelWindow),
            new FrameworkPropertyMetadata(VerticalAlignment.Top, OnLayoutPropertyChanged));

        HorizontalAlignmentProperty.OverrideMetadata(
            typeof(SidePanelWindow),
            new FrameworkPropertyMetadata(HorizontalAlignment.Left, OnLayoutPropertyChanged));

        SizeToContentProperty.OverrideMetadata(
            typeof(SidePanelWindow),
            new FrameworkPropertyMetadata(SizeToContent.Manual, OnLayoutPropertyChanged));

        WindowStateProperty.OverrideMetadata(
            typeof(SidePanelWindow),
            new FrameworkPropertyMetadata(
            WindowState.Normal,
            null,
            CoerceWindowState
        ));

        WindowStyleProperty.OverrideMetadata(
            typeof(SidePanelWindow),
            new FrameworkPropertyMetadata(
            WindowStyle.None,
            null,
            CoerceWindowStyle
        ));

        TopmostProperty.OverrideMetadata(
            typeof(SidePanelWindow),
            new FrameworkPropertyMetadata(
            true
        ));

        ResizeModeProperty.OverrideMetadata(
            typeof(SidePanelWindow),
            new FrameworkPropertyMetadata(
            ResizeMode.NoResize,
            null,
            CoerceResizeMode
        ));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SidePanelWindow"/> class.
    /// </summary>
    public SidePanelWindow() : base()
    {
        Chrome.CaptionHeight = 0;
        Chrome.ResizeBorderThickness = new Thickness(0);

        var sidePanelWindowBehavior = new SidePanelWindowBehavior();
        SidePanelWindowBehavior.SetSidePanelWindowBehavior(this, sidePanelWindowBehavior);

        _contentTransform = new TranslateTransform();
    }

    #region Methods Override

    /// <summary>
    /// Called when the control's template is applied. Initializes the slider control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_container is not null)
            _container.RenderTransform = null;

        _container = GetTemplateChild("PART_ContentWrapper") as FrameworkElement;

        if (_container is not null)
        {
            _contentTransform = new TranslateTransform();
            _container.RenderTransform = _contentTransform;
            _container.RenderTransformOrigin = new Point(0, 0);
        }
    }

    /// <inheritdoc/>
    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        WindowState = WindowState.Normal;
    }

    #endregion

    #region Methods

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

    internal void UpdateWindowPos()
    {
        var screen = System.Windows.Forms.Screen.AllScreens
                       .FirstOrDefault(s => s.Primary)
                     ?? System.Windows.Forms.Screen.AllScreens[0];
        var wa = screen.WorkingArea;

        var panelW = Width;
        if (SizeToContent is SizeToContent.Manual && HorizontalAlignment == HorizontalAlignment.Stretch)
            panelW = wa.Width;

        var panelH = Height;
        if (SizeToContent is SizeToContent.Manual && VerticalAlignment == VerticalAlignment.Stretch)
            panelH = wa.Height;

        double x = 0, y = 0;

        if (Dock is Dock.Left or Dock.Right)
        {
            y = VerticalAlignment switch
            {
                VerticalAlignment.Center => wa.Top + (wa.Height - panelH) / 2.0,
                VerticalAlignment.Bottom => wa.Bottom - panelH,
                _ => wa.Top
            };

            x = (Dock == Dock.Left)
                ? wa.Left
                : (wa.Right - panelW);
        }
        else
        {
            x = HorizontalAlignment switch
            {
                HorizontalAlignment.Center => wa.Left + (wa.Width - panelW) / 2.0,
                HorizontalAlignment.Right => wa.Right - panelW,
                _ => wa.Left
            };

            y = (Dock == Dock.Top)
                ? wa.Top
                : (wa.Bottom - panelH);
        }

        Left = x;
        Top = y;
        Width = panelW;
        Height = panelH;
    }

    private object IsExpandedCoerceValue(bool baseValue)
    {
        return DesignerProperties.GetIsInDesignMode(this) || (!IsAnimationInProcess ? baseValue : IsExpanded);
    }

    private object DockCoerceValue(Dock baseValue)
    {
        return !IsAnimationInProcess ? baseValue : Dock;
    }

    private void OnIsExpandedChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateWindowPos();

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

    private void OnLayoutPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateWindowPos();
    }

    private void OnAutoHideChanged(DependencyPropertyChangedEventArgs e)
    {
        OnAutoHideChanged((bool)e.OldValue, (bool)e.NewValue);
        AutoHideChanged?.Invoke(this, e);
    }

    private void OnDockChanged(DependencyPropertyChangedEventArgs e)
    {
        var newValue = (Dock)e.NewValue;

        _contentTransform.BeginAnimation(TranslateTransform.XProperty, null);
        _contentTransform.BeginAnimation(TranslateTransform.YProperty, null);

        var offset = CalculateOffset(newValue);

        if (!IsExpanded)
        {
            if (newValue is Dock.Left or Dock.Right)
            {
                _contentTransform.X = offset;
                _contentTransform.Y = 0;
            }
            else
            {
                _contentTransform.Y = offset;
                _contentTransform.X = 0;
            }
        }
        else
        {
            _contentTransform.X = 0;
            _contentTransform.Y = 0;
        }

        UpdateWindowPos();

        OnDockChanged((Dock)e.OldValue, (Dock)e.NewValue);
        DockChanged?.Invoke(this, e);
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

    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var sidePanelWindow = (SidePanelWindow)d;
        sidePanelWindow.OnLayoutPropertyChanged(e);
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
    /// Coerce callback that forces the window state to always be Normal.
    /// </summary>
    private static object CoerceWindowState(DependencyObject d, object baseValue)
    {
        return WindowState.Normal;
    }

    /// <summary>
    /// Coerce callback that forces the window style to always be None.
    /// </summary>
    private static object CoerceWindowStyle(DependencyObject d, object baseValue)
    {
        return WindowStyle.None;
    }

    /// <summary>
    /// Coerce callback that forces the resize mode to always be NoResize.
    /// </summary>
    private static object CoerceResizeMode(DependencyObject d, object baseValue)
    {
        return ResizeMode.NoResize;
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

    /// <summary>
    /// Expands the slider control to its defined size, triggering an animation.
    /// </summary>
    public void Expand()
    {
        base.Show();

        var offset = CalculateOffset(Dock);
        var animation = new DoubleAnimation()
        {
            From = offset,
            To = 0,
            EasingFunction = EasingFunction,
            SpeedRatio = AnimationSpeedRatio
        };
        animation.Completed += OnExpandCompleted;
        animation.CurrentStateInvalidated += ExpandCurrentStateInvalidated;

        var animationProp = GetTranslateProperty(Dock);
        _contentTransform.BeginAnimation(animationProp, animation);

        RaiseEvent(new RoutedEventArgs(ExpandStartedEvent, animation));
    }

    /// <summary>
    /// Collapses the slider control to its minimum size (zero), triggering an animation.
    /// </summary>
    public void Collapse()
    {
        var offset = CalculateOffset(Dock);
        var animation = new DoubleAnimation
        {
            From = 0,
            To = offset,
            EasingFunction = EasingFunction,
            SpeedRatio = AnimationSpeedRatio
        };
        animation.Completed += OnCollapseCompleted;
        animation.CurrentStateInvalidated += OnCollapseCurrentStateInvalidated;

        var animationProp = GetTranslateProperty(Dock);
        _contentTransform.BeginAnimation(animationProp, animation);

        RaiseEvent(new RoutedEventArgs(CollapseStartedEvent, animation));
    }

    private void OnAnimationCompleted()
    {
        BeginAnimation(TranslateTransform.XProperty, null);
        BeginAnimation(TranslateTransform.YProperty, null);
    }

    private double CalculateOffset(Dock dock)
    {
        var size = GetExpandSize();
        return dock switch
        {
            Dock.Left => -size,
            Dock.Top => -size,
            Dock.Right => size,
            Dock.Bottom => size,
            _ => 0
        };
    }

    private static DependencyProperty GetTranslateProperty(Dock dock) =>
        (dock is Dock.Left or Dock.Right)
            ? TranslateTransform.XProperty
            : TranslateTransform.YProperty;

    internal double GetExpandSize() => (Dock is Dock.Left or Dock.Right) ? Width : Height;

    #endregion

    #region Events Subscriptions

    private void OnExpandCompleted(object? sender, EventArgs e)
    {
        OnAnimationCompleted();
        RaiseEvent(new RoutedEventArgs(ExpandCompletedEvent, sender));
    }

    private void OnCollapseCompleted(object? sender, EventArgs e)
    {
        base.Hide();
        OnAnimationCompleted();
        RaiseEvent(new RoutedEventArgs(CollapseCompletedEvent, sender));
    }

    private void ExpandCurrentStateInvalidated(object? sender, EventArgs e)
    {
        if (sender is not AnimationClock animationClock)
        {
            return;
        }

        var isAnimationInProcess = animationClock.CurrentState switch
        {
            ClockState.Filling => false,
            ClockState.Active => true,
            _ => false
        };

        SetValue(IsAnimationInProcessPropertyKey, isAnimationInProcess);
        RaiseEvent(new RoutedEventArgs(ExpandStateInvalidatedEvent, sender));
    }

    private void OnCollapseCurrentStateInvalidated(object? sender, EventArgs e)
    {
        if (sender is not AnimationClock animationClock)
        {
            return;
        }

        var isAnimationInProcess = animationClock.CurrentState switch
        {
            ClockState.Filling => false,
            ClockState.Active => true,
            _ => false
        };

        SetValue(IsAnimationInProcessPropertyKey, isAnimationInProcess);
        RaiseEvent(new RoutedEventArgs(CollapseStateInvalidatedEvent, sender));
    }

    #endregion
}