using System;
using System.Collections.Generic;
using System.Security;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using DevToolbox.Wpf.Interop;
using Microsoft.Win32;
using HANDLE_MESSAGE = System.Collections.Generic.KeyValuePair<DevToolbox.Wpf.Interop.WM, DevToolbox.Wpf.Windows.MessageHandler>;

namespace DevToolbox.Wpf.Windows;

internal class SidePanelWindowBehavior : DependencyObject
{
    #region Fields

    private readonly List<HANDLE_MESSAGE> _messageTable;

    private SidePanelWindow? _sidePanelWindow;
    private IntPtr _hwnd;
    private HwndSource? _hwndSource;

    public static readonly DependencyProperty SidePanelWindowBehaviorProperty = DependencyProperty.RegisterAttached(
        nameof(SidePanelWindowBehavior),
        typeof(SidePanelWindowBehavior),
        typeof(SidePanelWindowBehavior),
        new PropertyMetadata(null, OnSidePanelWindowBehaviorChanged));

    #endregion

    static SidePanelWindowBehavior()
    {
    }

    public SidePanelWindowBehavior()
    {
        _messageTable =
        [
            new(WM.SYSCOMMAND,  HandleSYSCOMMAND),
            new(WM.LBUTTONDOWN, HandleLBUTTONDOWN),
            new(WM.NCHITTEST,   HandleNCHitTest),
            new(WM.NCCALCSIZE, HandleNCCalcSize),
            new(WM.NCPAINT, HandleNCPaint),
            new(WM.KILLFOCUS,   HandleKILLFOCUS),
            new(WM.DPICHANGED,   HandleDPICHANGED)
        ];
    }

    #region Attached Property Accessors

    public static SidePanelWindowBehavior GetSidePanelWindowBehavior(SidePanelWindow window) =>
        (SidePanelWindowBehavior)window.GetValue(SidePanelWindowBehaviorProperty);

    public static void SetSidePanelWindowBehavior(SidePanelWindow window, SidePanelWindowBehavior behavior) =>
        window.SetValue(SidePanelWindowBehaviorProperty, behavior);

    private static void OnSidePanelWindowBehaviorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SidePanelWindow window && e.NewValue is SidePanelWindowBehavior behavior)
        {
            behavior.SetWindow(window);
        }
    }

    #endregion

    #region WndProc

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        var message = (WM)msg;
        foreach (var pair in _messageTable)
        {
            if (pair.Key == message)
                return pair.Value(message, wParam, lParam, out handled);
        }
        return IntPtr.Zero;
    }

    [SecurityCritical]
    private IntPtr HandleNCCalcSize(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
    {
        // Tells Windows “treat the *entire* window as client”
        handled = true;
        return IntPtr.Zero;
    }

    [SecurityCritical]
    private IntPtr HandleNCPaint(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
    {
        // Suppress all non-client painting (no border ever drawn)
        handled = true;
        return IntPtr.Zero;
    }

    private IntPtr HandleNCHitTest(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
    {
        // otherwise it's inside our window: treat it as a normal client hit
        handled = true;
        return new IntPtr((int)HT.CLIENT);
    }

    private IntPtr HandleSYSCOMMAND(WM msg, IntPtr wParam, IntPtr lParam, out bool handled)
    {
        handled = true;
        return IntPtr.Zero;
    }

    private IntPtr HandleLBUTTONDOWN(WM msg, IntPtr wParam, IntPtr lParam, out bool handled)
    {
        handled = false;
        if (_sidePanelWindow?.AutoHide == true && _sidePanelWindow.IsExpanded)
        {
            var pos = Mouse.GetPosition(_sidePanelWindow);
            if (VisualTreeHelper.HitTest(_sidePanelWindow, pos) == null)
            {
                _sidePanelWindow.IsExpanded = false;
            }
        }
        return IntPtr.Zero;
    }

    private IntPtr HandleKILLFOCUS(WM msg, IntPtr wParam, IntPtr lParam, out bool handled)
    {
        handled = false;
        if (_sidePanelWindow?.AutoHide == true && _sidePanelWindow.IsExpanded)
        {
            _sidePanelWindow.IsExpanded = false;
        }
        return IntPtr.Zero;
    }

    private IntPtr HandleDPICHANGED(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
    {
        handled = false;
        if (_sidePanelWindow == null)
            return IntPtr.Zero;
        _sidePanelWindow.UpdateWindowPos();
        return IntPtr.Zero;
    }

    #endregion

    #region Initialization

    private void SetWindow(SidePanelWindow window)
    {
        _sidePanelWindow = window;
        _sidePanelWindow.Closed += SidePanelWindow_Closed;

        _hwnd = new WindowInteropHelper(window).Handle;

        if (_hwnd != IntPtr.Zero)
        {
            _hwndSource = HwndSource.FromHwnd(_hwnd);
            ApplyCustomChrome();
            _hwndSource.AddHook(WndProc);
        }
        else
        {
            window.SourceInitialized += (_, _) =>
            {
                _hwnd = new WindowInteropHelper(window).Handle;
                _hwndSource = HwndSource.FromHwnd(_hwnd);
                ApplyCustomChrome();
                _hwndSource.AddHook(WndProc);
            };
        }
    }

    #endregion

    #region Custom Chrome & Appearance

    [SecurityCritical]
    private void ApplyCustomChrome()
    {
        if (_sidePanelWindow == null)
            throw new InvalidOperationException("SidePanelWindow is not initialized.");

        RefreshFrame();

        SystemEvents.UserPreferenceChanged += OnUserPrefChanged;
    }

    private void OnUserPrefChanged(object? sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category is UserPreferenceCategory.Color or
            UserPreferenceCategory.General)
        {
            RefreshFrame();
        }
    }

    private void RefreshFrame()
    {
        if (_hwndSource is null || _sidePanelWindow is null)
            return;

        var style = NativeMethods.GetWindowStyle(_hwnd);
        style &= ~(WS.SYSMENU | WS.CAPTION | WS.MINIMIZEBOX | WS.MAXIMIZEBOX | WS.SIZEBOX);
        style |= (WS.POPUP | WS.VISIBLE);
        NativeMethods.SetWindowStyle(_hwnd, style);

        _hwndSource.CompositionTarget.BackgroundColor = Colors.Transparent;

        var margins = new MARGINS { cxLeftWidth = -1, cxRightWidth = -1, cyTopHeight = -1, cyBottomHeight = -1 };
        _ = Dwmapi.DwmExtendFrameIntoClientArea(_hwndSource.Handle, ref margins);

        var systemBackdropType = (uint)DWM_SYSTEMBACKDROP_TYPE.DWMSBT_NONE;
        _ = Dwmapi.DwmSetWindowAttribute(_hwnd, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, ref systemBackdropType, sizeof(uint));

        _sidePanelWindow.UpdateWindowPos();
        _sidePanelWindow.InvalidateMeasure();
    }

    #endregion

    #region Cleanup

    private void SidePanelWindow_Closed(object? sender, EventArgs e) => Dispose();

    private void Dispose()
    {
        if (_sidePanelWindow != null)
        {
            SystemEvents.UserPreferenceChanged -= OnUserPrefChanged;

            _sidePanelWindow.Closed -= SidePanelWindow_Closed;

            if (_hwndSource != null)
            {
                _hwndSource.RemoveHook(WndProc);
                _hwndSource = null;
            }

            _sidePanelWindow = null;
        }
    }

    #endregion
}
