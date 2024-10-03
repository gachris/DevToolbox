using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using DevToolbox.Wpf.Controls;
using DevToolbox.Wpf.Interop;
using DevToolbox.Wpf.Utils;

namespace DevToolbox.Wpf.Windows;

/// <summary>
/// Represents a sliding window control that can expand or collapse, supporting docking and animation.
/// </summary>
[TemplatePart(Name = "PART_SliderControl", Type = typeof(SliderControl))]
public class SliderWindow : WindowEx
{
    #region Fields/Consts

    private IntPtr _hwnd = IntPtr.Zero; // Handle to the window
    private HwndSource? _hwndSource; // Source for window interop
    private SliderControl? _sliderControl; // Slider control instance

    /// <summary>
    /// Identifies the <see cref="Dock"/> dependency property.
    /// This property defines how the slider window is docked in relation to other UI elements.
    /// </summary>
    public static readonly DependencyProperty DockProperty =
        SliderControl.DockProperty.AddOwner(typeof(SliderWindow));

    /// <summary>
    /// Identifies the <see cref="ExpandSize"/> dependency property.
    /// This property specifies the size to which the slider window expands when opened.
    /// </summary>
    public static readonly DependencyProperty ExpandSizeProperty =
        SliderControl.ExpandSizeProperty.AddOwner(typeof(SliderWindow));

    /// <summary>
    /// Identifies the <see cref="AutoHide"/> dependency property.
    /// This property determines whether the slider window automatically hides when not in use.
    /// </summary>
    public static readonly DependencyProperty AutoHideProperty =
        SliderControl.AutoHideProperty.AddOwner(typeof(SliderWindow));

    /// <summary>
    /// Identifies the <see cref="IsExpanded"/> dependency property.
    /// This property indicates whether the slider window is currently expanded.
    /// </summary>
    public static readonly DependencyProperty IsExpandedProperty =
        SliderControl.IsExpandedProperty.AddOwner(typeof(SliderWindow));

    /// <summary>
    /// Identifies the <see cref="EasingFunction"/> dependency property.
    /// This property specifies the easing function used for animations during expansion and collapse.
    /// </summary>
    public static readonly DependencyProperty EasingFunctionProperty =
        SliderControl.EasingFunctionProperty.AddOwner(typeof(SliderWindow));

    /// <summary>
    /// Identifies the <see cref="ExpandStarted"/> routed event.
    /// This event is raised when the expansion of the slider window begins.
    /// </summary>
    public static readonly RoutedEvent ExpandStartedEvent =
        SliderControl.ExpandStartedEvent.AddOwner(typeof(SliderWindow));

    /// <summary>
    /// Identifies the <see cref="CollapseStarted"/> routed event.
    /// This event is raised when the collapse of the slider window begins.
    /// </summary>
    public static readonly RoutedEvent CollapseStartedEvent =
        SliderControl.CollapseStartedEvent.AddOwner(typeof(SliderWindow));

    /// <summary>
    /// Identifies the <see cref="ExpandCompleted"/> routed event.
    /// This event is raised when the expansion of the slider window is completed.
    /// </summary>
    public static readonly RoutedEvent ExpandCompletedEvent =
        SliderControl.ExpandCompletedEvent.AddOwner(typeof(SliderWindow));

    /// <summary>
    /// Identifies the <see cref="CollapseCompleted"/> routed event.
    /// This event is raised when the collapse of the slider window is completed.
    /// </summary>
    public static readonly RoutedEvent CollapseCompletedEvent =
        SliderControl.CollapseCompletedEvent.AddOwner(typeof(SliderWindow));

    /// <summary>
    /// Identifies the <see cref="CollapseStateInvalidated"/> routed event.
    /// This event is raised when the collapse state of the slider window is invalidated.
    /// </summary>
    public static readonly RoutedEvent CollapseStateInvalidatedEvent =
        SliderControl.CollapseStateInvalidatedEvent.AddOwner(typeof(SliderWindow));

    /// <summary>
    /// Identifies the <see cref="ExpandStateInvalidated"/> routed event.
    /// This event is raised when the expand state of the slider window is invalidated.
    /// </summary>
    public static readonly RoutedEvent ExpandStateInvalidatedEvent =
        SliderControl.ExpandStateInvalidatedEvent.AddOwner(typeof(SliderWindow));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the dock position of the slider window.
    /// </summary>
    public Dock Dock
    {
        get => (Dock)GetValue(DockProperty);
        set => SetValue(DockProperty, value);
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
    /// Gets or sets a value indicating whether the window should automatically hide when collapsed.
    /// </summary>
    public bool AutoHide
    {
        get => (bool)GetValue(AutoHideProperty);
        set => SetValue(AutoHideProperty, value);
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

    static SliderWindow()
    {
        // Override default style key and property metadata for the SliderWindow
        SliderWindow.DefaultStyleKeyProperty.OverrideMetadata(typeof(SliderWindow), new FrameworkPropertyMetadata(typeof(SliderWindow)));
        SliderWindow.WidthProperty.OverrideMetadata(typeof(SliderWindow), new FrameworkPropertyMetadata(null, (d, baseValue) => (d as SliderWindow)?.OnWidthChangedCoerceValueCallback(baseValue)));
        SliderWindow.HeightProperty.OverrideMetadata(typeof(SliderWindow), new FrameworkPropertyMetadata(null, (d, baseValue) => (d as SliderWindow)?.OnHeightChangedCoerceValueCallback(baseValue)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SliderWindow"/> class.
    /// </summary>
    public SliderWindow()
    {
        // Subscribe to expansion and collapse events
        ExpandStarted += OnExpandStarted;
        ExpandCompleted += OnExpandCompleted;
        CollapseStarted += OnCollapseStarted;
        CollapseCompleted += OnCollapseCompleted;

        // Uncomment to enable specific window styles (e.g., glass frame)
        // GlassFrameThickness = new(-1);
        // ResizeBorderThickness = new(0);
        // NonClientFrameEdges = NonClientFrameEdges.None;

        // Mica.Apply(this); // Uncomment to apply Mica effect
    }

    #region Methods Override

    /// <summary>
    /// Called when the control's template is applied. Initializes the slider control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // Unsubscribe from the previous slider control's DockChanged event
        if (_sliderControl != null)
            _sliderControl.DockChanged -= DockChanged;

        // Find the SliderControl in the template
        _sliderControl = Template.FindName("PART_SliderControl", this) as SliderControl;

        // Subscribe to the new slider control's DockChanged event
        if (_sliderControl != null)
            _sliderControl.DockChanged += DockChanged;
    }

    /// <summary>
    /// Called when the window source is initialized. Sets window properties and hooks.
    /// </summary>
    /// <param name="e">Event arguments.</param>
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        _hwnd = new WindowInteropHelper(this).Handle;

        if (_hwnd != IntPtr.Zero)
        {
            _ = User32.ShowWindow(_hwnd, SW.HIDE); // Hide the window initially

            _hwndSource = HwndSource.FromHwnd(_hwnd);

            // Update window styles
            var style = NativeMethods.GetWindowStyle(_hwnd);
            style = NativeMethods.SetWindowStyle(_hwnd, style & ~(WS.MAXIMIZEBOX | WS.CAPTION | WS.SYSMENU));

            var styleEx = NativeMethods.GetWindowStyleEx(_hwnd);
            styleEx = NativeMethods.SetWindowStyleEx(_hwnd, styleEx ^ WS_EX.TRANSPARENT);

            // Set the position if expanded
            if (IsExpanded) SetWindowPos(SWP.SHOWWINDOW, (int)ExpandSize, Dock);

            // Add hook for window messages
            _hwndSource.AddHook(WndProc);
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Handles the left mouse button up event to toggle the expansion state of the window.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">Event arguments.</param>
    private void WindowTrayManagerLeftMouseUp(object? sender, EventArgs e) => SetCurrentValue(IsExpandedProperty, !IsExpanded);

    /// <summary>
    /// Handles changes to the dock property of the slider control.
    /// </summary>
    /// <param name="d">The dependency object.</param>
    /// <param name="e">The property change event args.</param>
    private void DockChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => SetWindowPos(SWP.ASYNCWINDOWPOS, (int)ExpandSize, (Dock)e.NewValue);

    /// <summary>
    /// Processes window messages for the slider window.
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

        switch (message)
        {
            case WM.SYSCOMMAND:
                var command = (WM)(wParam.ToInt32() & 0xFFF0);
                switch (command)
                {
                    case WM.SC_MINIMIZE:
                    case WM.SC_RESTORE:
                        SetCurrentValue(IsExpandedProperty, !IsExpanded);
                        handled = true; // Mark as handled
                        break;
                }
                break;
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// Handles the event when expansion starts. Restores and shows the window.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">Event arguments.</param>
    private void OnExpandStarted(object sender, RoutedEventArgs e)
    {
        _ = User32.ShowWindow(_hwnd, SW.RESTORE); // Restore the window
        SetWindowPos(SWP.SHOWWINDOW, (int)ExpandSize, Dock); // Show the window at the specified position
    }

    /// <summary>
    /// Handles the event when expansion completes. Activates the window.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">Event arguments.</param>
    private void OnExpandCompleted(object sender, EventArgs e) => Activate(); // Activate the window

    /// <summary>
    /// Handles the event when collapse starts. Placeholder for future functionality.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">Event arguments.</param>
    private void OnCollapseStarted(object sender, RoutedEventArgs e)
    {
        // Placeholder for future functionality when collapse starts
    }

    /// <summary>
    /// Handles the event when collapse completes. Minimizes the window.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">Event arguments.</param>
    private void OnCollapseCompleted(object sender, RoutedEventArgs e) => _ = User32.ShowWindow(_hwnd, SW.MINIMIZE); // Minimize the window

    /// <summary>
    /// Shows the slider window and expands it.
    /// </summary>
    public new void Show() => SetCurrentValue(IsExpandedProperty, true);

    /// <summary>
    /// Hides the slider window and collapses it.
    /// </summary>
    public new void Hide() => SetCurrentValue(IsExpandedProperty, false);

    /// <summary>
    /// Sets the window position based on docking and expand size.
    /// </summary>
    /// <param name="setWindowPosFlags">Flags to set window position.</param>
    /// <param name="expandSize">Size to expand to.</param>
    /// <param name="dock">Docking position.</param>
    private void SetWindowPos(SWP setWindowPosFlags, int expandSize, Dock dock)
    {
        var left = 0;
        var top = 0;
        var width = 0;
        var height = 0;

        var dpiScale = this.GetDpi(); // Get DPI scale

        // Set window position based on dock
        if (dock == Dock.Right)
        {
            width = (int)(expandSize * dpiScale.DpiScaleX);
            height = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Bottom;
            left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Right - width;
            top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Top;
        }
        else if (dock == Dock.Left)
        {
            width = (int)(expandSize * dpiScale.DpiScaleX);
            height = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Bottom;
            left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Left;
            top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Top;
        }
        else if (dock == Dock.Bottom)
        {
            width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Right;
            height = (int)(expandSize * dpiScale.DpiScaleY);
            left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Left;
            top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Bottom - height;
        }
        else if (dock == Dock.Top)
        {
            width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Right;
            left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Left;
            height = (int)(expandSize * dpiScale.DpiScaleY);
            top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Top;
        }

        // Set the window position
        User32.SetWindowPos(_hwnd, (IntPtr)SpecialWindowHandles.HWND_TOP, left, top, width, height, setWindowPosFlags);
    }

    /// <summary>
    /// Coerces the height property value based on the current state.
    /// </summary>
    /// <param name="baseValue">The base value of the height property.</param>
    /// <returns>The coerced height value.</returns>
    private object OnHeightChangedCoerceValueCallback(object baseValue) =>
        // Uncomment for design mode adjustments
        //if (DependencyObjectExtensions.IsDesignMode)
        //{
        //    if (Dock == Dock.Left || Dock == Dock.Right)
        //        return SystemParameters.WorkArea.Bottom;
        //    else
        //        return ExpandSize;
        //}
        //else 
        baseValue;

    /// <summary>
    /// Coerces the width property value based on the current state.
    /// </summary>
    /// <param name="baseValue">The base value of the width property.</param>
    /// <returns>The coerced width value.</returns>
    private object OnWidthChangedCoerceValueCallback(object baseValue) =>
        // Uncomment for design mode adjustments
        //if (DependencyObjectExtensions.IsDesignMode)
        //{
        //    if (Dock == Dock.Left || Dock == Dock.Right)
        //        return ExpandSize;
        //    else
        //        return SystemParameters.WorkArea.Right;
        //}
        //else
        baseValue;

    #endregion
}
