using System;
using System.ComponentModel;
using System.Windows.Interop;
using DevToolbox.Wpf.Controls;

namespace DevToolbox.Wpf.Windows;

/// <summary>
/// Base class for windows that host a <see cref="LayoutManager"/>,
/// managing native window messages and drop operations.
/// </summary>
public abstract class LayoutBaseWindow : WindowEx
{
    #region Fields/Consts

    private HwndSource? _hwndSource;

    #endregion

    #region Methods Overrides

    /// <inheritdoc/>
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var helper = new WindowInteropHelper(this);
        _hwndSource = HwndSource.FromHwnd(helper.Handle);
        _hwndSource?.AddHook(WndProc);
    }

    /// <inheritdoc/>
    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        if (!e.Cancel)
            _hwndSource?.RemoveHook(WndProc);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Processes native window messages before WPF handles them.
    /// Subclasses can override to handle WM_DROP or other messages.
    /// </summary>
    /// <param name="hwnd">Handle of the window receiving the message.</param>
    /// <param name="msg">Message ID.</param>
    /// <param name="wParam">Message wParam.</param>
    /// <param name="lParam">Message lParam.</param>
    /// <param name="handled">Set to true to prevent default processing.</param>
    /// <returns>Resulting LRESULT value.</returns>
    protected virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        handled = false;
        return IntPtr.Zero;
    }

    /// <summary>
    /// Called when a drop occurs on a dockable control.
    /// Implementers should reposition or re-dock as needed.
    /// </summary>
    /// <param name="control">The surface receiving the drop.</param>
    /// <param name="btnDock">The docking position indicated by the drop.</param>
    protected internal abstract void OnDrop(IDropSurface control, LayoutDockTargetPosition btnDock);

    #endregion
}
