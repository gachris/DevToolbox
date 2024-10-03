using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using DevToolbox.Wpf.Interop;
using DevToolbox.Wpf.Utils;
using HANDLE_MESSAGE = System.Collections.Generic.KeyValuePair<DevToolbox.Wpf.Interop.WM, DevToolbox.Wpf.Windows.MessageHandler>;

namespace DevToolbox.Wpf.Windows;

internal delegate IntPtr MessageHandler(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled);

internal class WindowExBehaviour : DependencyObject
{
    #region Fields

    private readonly List<HANDLE_MESSAGE> _messageTable;
    private WindowEx? _WindowEx;
    private IntPtr _hwnd;
    private UIElement? _trackedControl;
    private TRACKMOUSEEVENT _trackMouseEvent;
    private HwndSource? _hwndSource;

    #endregion

    public static readonly DependencyProperty WindowExBehaviourProperty = DependencyProperty.RegisterAttached(
        "WindowExBehaviour",
        typeof(WindowExBehaviour),
        typeof(WindowExBehaviour),
        new PropertyMetadata(null, OnWindowExBehaviourChanged));

    private static void OnWindowExBehaviourChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var w = (WindowEx)d;
        var cw = (WindowExBehaviour)e.NewValue;

        cw.SetWindow(w);
    }

    public static WindowExBehaviour GetWindowExBehaviour(WindowEx window)
    {
        return (WindowExBehaviour)window.GetValue(WindowExBehaviourProperty);
    }

    public static void SetWindowExBehaviour(WindowEx window, WindowExBehaviour chrome)
    {
        window.SetValue(WindowExBehaviourProperty, chrome);
    }

    public WindowExBehaviour()
    {
        _messageTable =
        [
            new HANDLE_MESSAGE(WM.NCMOUSELEAVE,          HandleNCMOUSELEAVE),
            new HANDLE_MESSAGE(WM.MOUSELEAVE,            HandleMOUSELEAVE),
            new HANDLE_MESSAGE(WM.NCMOUSEMOVE,           HandleNCMOUSEMOVE),
            new HANDLE_MESSAGE(WM.MOUSEMOVE,             HandleMOUSEMOVE),
            new HANDLE_MESSAGE(WM.NCHITTEST,             HandleNCHitTest),
            new HANDLE_MESSAGE(WM.NCRBUTTONUP,           HandleNCRButtonUp),
            new HANDLE_MESSAGE(WM.SIZE,                  HandleSize),
            new HANDLE_MESSAGE(WM.NCLBUTTONDOWN,         HandleNCLBUTTONDOWN),
            new HANDLE_MESSAGE(WM.NCLBUTTONDBLCLK,       HandleNCLBUTTONDBLCLK),
            new HANDLE_MESSAGE(WM.NCRBUTTONDOWN,         HandleNCRBUTTONDOWN),
            new HANDLE_MESSAGE(WM.NCRBUTTONDBLCLK,       HandleNCRBUTTONDBLCLK),
        ];
    }

    private void SetWindow(WindowEx WindowEx)
    {
        _WindowEx = WindowEx;

        _hwnd = new WindowInteropHelper(WindowEx).Handle;

        if (IntPtr.Zero != _hwnd)
        {
            // We've seen that the HwndSource can't always be retrieved from the HWND, so cache it early.
            // Specifically it seems to sometimes disappear when the OS theme is changing.
            _hwndSource = HwndSource.FromHwnd(_hwnd);
            ApplyNewCustomChrome();

            _hwndSource.AddHook(WndProc);
        }
        else
        {
            WindowEx.SourceInitialized += (sender, e) =>
            {
                _hwnd = new WindowInteropHelper(WindowEx).Handle;
                _hwndSource = HwndSource.FromHwnd(_hwnd);
                ApplyNewCustomChrome();

                _hwndSource.AddHook(WndProc);
            };
        }
    }

    private void ApplyNewCustomChrome()
    {
        if (_WindowEx is null)
        {
            throw new ArgumentNullException(nameof(_WindowEx));
        }

        // Remove AeroCaptionButtons
        var hwnd_style = (WS)NativeMethods.GetWindowLongPtr(_hwnd, GWL.STYLE);
        var hwnd_new_style = hwnd_style & ~WS.SYSMENU;
        NativeMethods.SetWindowLongPtr(_hwnd, GWL.STYLE, (IntPtr)hwnd_new_style);
    }

    #region WindowProc and Message Handlers

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        var message = (WM)msg;
        foreach (var handlePair in _messageTable)
        {
            if (handlePair.Key == message)
            {
                return handlePair.Value(message, wParam, lParam, out handled);
            }
        }
        return IntPtr.Zero;
    }

    private IntPtr HandleNCHitTest(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
    {
        if (_WindowEx is null)
        {
            throw new ArgumentNullException(nameof(_WindowEx));
        }

        if (GetControlUnderMouse(_WindowEx, out var res) != null && res != HT.CAPTION)
        {
            handled = true;
            return new IntPtr((int)res);
        }

        handled = false;
        return IntPtr.Zero;
    }

    private IntPtr HandleNCRButtonUp(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
    {
        if (_WindowEx is null)
        {
            throw new ArgumentNullException(nameof(_WindowEx));
        }

        // Emulate the system behavior of clicking the right mouse button over the caption area
        // to bring up the system menu.
        if ((HT)wParam.ToInt32() is HT.MAXBUTTON or HT.MINBUTTON or HT.CLOSE or HT.HELP)
        {
            ShowSystemMenuPhysicalCoordinates(_WindowEx, new Point(NativeMethods.GET_X_LPARAM(lParam), NativeMethods.GET_Y_LPARAM(lParam)));
        }

        handled = false;
        return IntPtr.Zero;
    }

    private IntPtr HandleSize(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
    {
        handled = false;
        var visualChildrenCount = VisualTreeHelper.GetChildrenCount(_WindowEx);

        if (visualChildrenCount < 1)
        {
            return IntPtr.Zero;
        }

        if (VisualTreeHelper.GetChild(_WindowEx, 0) is not FrameworkElement child)
        {
            return IntPtr.Zero;
        }

        if (wParam.ToInt32() == (int)WM_SIZE.MAXSHOW)
        {
            var size = SystemParametersEx.WindowResizeBorderThickness;
            child.Margin = new Thickness(size.cx, size.cy, size.cx, size.cy);
        }
        else if (wParam.ToInt32() == (int)WM_SIZE.RESTORED)
        {
            child.Margin = new Thickness(0);
        }

        return IntPtr.Zero;
    }

    private IntPtr HandleNCMOUSEMOVE(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
    {
        _trackMouseEvent = new TRACKMOUSEEVENT
        {
            hWnd = _hwnd,
            cbSize = Marshal.SizeOf(typeof(TRACKMOUSEEVENT)), // make sure it gets correct size in different platforms
            dwFlags = TMEFlags.TME_NONCLIENT | TMEFlags.TME_LEAVE,
            dwHoverTime = 0
        };

        _ = User32.TrackMouseEvent(ref _trackMouseEvent);

        handled = HoverTrackedControl();
        return IntPtr.Zero;
    }

    private IntPtr HandleMOUSEMOVE(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
    {
        ClearTrackedControl();
        handled = false;
        return IntPtr.Zero;
    }

    private IntPtr HandleNCMOUSELEAVE(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
    {
        ClearTrackedControl();
        handled = false;
        return IntPtr.Zero;
    }

    private IntPtr HandleMOUSELEAVE(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
    {
        ClearTrackedControl();
        handled = false;
        return IntPtr.Zero;
    }

    private IntPtr HandleNCLBUTTONDOWN(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
    {
        handled = RaiseMouseMessage(WM.LBUTTONDOWN);
        return IntPtr.Zero;
    }

    private IntPtr HandleNCLBUTTONDBLCLK(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
    {
        handled = RaiseMouseMessage(WM.LBUTTONDBLCLK);
        return IntPtr.Zero;
    }

    private IntPtr HandleNCRBUTTONDOWN(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
    {
        handled = RaiseMouseMessage(WM.RBUTTONDOWN);
        return IntPtr.Zero;
    }

    private IntPtr HandleNCRBUTTONDBLCLK(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
    {
        handled = RaiseMouseMessage(WM.RBUTTONDBLCLK);
        return IntPtr.Zero;
    }

    #endregion

    #region Methods

    private bool RaiseMouseMessage(WM wM)
    {
        if (_trackedControl is null)
            return false;

        NativeMethods.RaiseMouseMessage(_hwnd, wM);

        return true;
    }

    [SecurityCritical]
    internal static void ShowSystemMenuPhysicalCoordinates(Window window, Point physicalScreenLocation)
    {
        IntPtr handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero || !User32.IsWindow(handle))
            return;
        uint num = User32.TrackPopupMenuEx(User32.GetSystemMenu(handle, false), 256U, (int)physicalScreenLocation.X, (int)physicalScreenLocation.Y, handle, IntPtr.Zero);
        if (num == 0U)
            return;
        User32.PostMessage(handle, WM.SYSCOMMAND, new IntPtr(num), IntPtr.Zero);
    }

    private void ClearTrackedControl()
    {
        if (_trackedControl is null || Mouse.LeftButton == MouseButtonState.Pressed)
            return;

        RaiseIsMouseOver(_trackedControl, false);
        _trackedControl = null;
    }

    private bool HoverTrackedControl()
    {
        var controlUnderMouse = _WindowEx is null ? throw new ArgumentNullException(nameof(_WindowEx)) : GetControlUnderMouse(_WindowEx, out _);
        if (controlUnderMouse == _trackedControl)
            return true;

        if (_trackedControl != null)
            RaiseIsMouseOver(_trackedControl, false);

        _trackedControl = controlUnderMouse;

        if (_trackedControl != null)
            RaiseIsMouseOver(_trackedControl, true);

        return true;
    }

    private static void RaiseIsMouseOver(UIElement element, bool isMouseOver)
    {
        if (element == null)
            return;

        var fieldInfo = typeof(UIElement).GetField("IsMouseOverPropertyKey", BindingFlags.Static | BindingFlags.NonPublic);
        if (fieldInfo != null)
        {
            if (fieldInfo.GetValue(element) is DependencyPropertyKey isMouseOverPropertyKey)
                element.SetValue(isMouseOverPropertyKey, isMouseOver);
        }
    }

    private static UIElement? GetControlUnderMouse(Window owner, out HT ht)
    {
        var point = LogicalPointFromLParam(owner);

        var dependencyObject = VisualTreeHelper.HitTest(owner, point)?.VisualHit;
        if (dependencyObject is Visual visualHit)
        {
            DependencyObject? currentControl = visualHit;
            while (currentControl != null)
            {
                var hitTestResult = WindowEx.GetHitTestResult(currentControl);
                if (hitTestResult != HitTestResult.None)
                {
                    ht = GetHitTestResult(hitTestResult);
                    return (UIElement)currentControl;
                }
                currentControl = GetVisualOrLogicalParent(currentControl);
            }
        }

        ht = HT.NOWHERE;
        return null;
    }

    private static HT GetHitTestResult(HitTestResult hitTestResult)
    {
        return hitTestResult == HitTestResult.Help
            ? HT.HELP
            : hitTestResult == HitTestResult.Close
            ? HT.CLOSE
            : hitTestResult == HitTestResult.Min
            ? HT.MINBUTTON
            : hitTestResult == HitTestResult.Max ? HT.MAXBUTTON : hitTestResult == HitTestResult.Restore ? HT.ZOOM : HT.NOWHERE;
    }

    private static Point LogicalPointFromLParam(Window owner)
    {
        var mousePosition = NativeMethods.GetMousePosition();
        return owner.PointFromScreen(new Point(mousePosition.X, mousePosition.Y));
    }

    private static DependencyObject? GetVisualOrLogicalParent(DependencyObject sourceElement) => sourceElement is Visual ? VisualTreeHelper.GetParent(sourceElement) ?? LogicalTreeHelper.GetParent(sourceElement) : null;

    #endregion
}