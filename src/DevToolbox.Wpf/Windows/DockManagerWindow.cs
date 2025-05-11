using System;
using System.ComponentModel;
using System.Windows.Interop;
using DevToolbox.Wpf.Controls;

namespace DevToolbox.Wpf.Windows;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public abstract class DockManagerWindow : WindowEx
{
    #region Fields/Consts

    private HwndSource? _hwndSource;

    #endregion

    #region Methods Override

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        WindowInteropHelper helper = new(this);
        _hwndSource = HwndSource.FromHwnd(helper.Handle);
        _hwndSource.AddHook(WndProc);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        if (e.Cancel)
        {
            _hwndSource?.RemoveHook(WndProc);
        }
    }

    #endregion

    #region Methods

    protected virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        handled = false;
        return IntPtr.Zero;
    }

    protected internal abstract void OnDrop(IDropSurface control, DockingPosition btnDock);

    #endregion
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member