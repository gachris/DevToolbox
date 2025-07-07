using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevToolbox.Wpf.Controls;
using DevToolbox.Wpf.Extensions;
using DevToolbox.Wpf.Interop;

namespace DevToolbox.Wpf.Windows;

/// <summary>
/// A floating window that hosts a single <see cref="LayoutDockItemsControl"/>.
/// Handles native window messages for resizing, moving, and dragging,
/// and delegates drop operations back to the host control.
/// </summary>
public class LayoutDockWindow : LayoutBaseWindow
{
    #region Properties

    private bool _allowContentChange;

    /// <summary>
    /// Gets the hosted <see cref="LayoutDockItemsControl"/> currently displayed as the window content, or null if none.
    /// </summary>
    protected internal LayoutDockItemsControl? HostControl { get; private set; }

    #endregion

    /// <summary>
    /// Static constructor overrides default style, size, and taskbar visibility metadata for <see cref="LayoutDockWindow"/>.
    /// </summary>
    static LayoutDockWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(LayoutDockWindow),
            new FrameworkPropertyMetadata(typeof(LayoutDockWindow)));

        WidthProperty.OverrideMetadata(
            typeof(LayoutDockWindow),
            new FrameworkPropertyMetadata(300.0));

        HeightProperty.OverrideMetadata(
            typeof(LayoutDockWindow),
            new FrameworkPropertyMetadata(300.0));

        ShowInTaskbarProperty.OverrideMetadata(
            typeof(LayoutDockWindow),
            new FrameworkPropertyMetadata(false));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LayoutDockWindow"/> class,
    /// setting up default window chrome and behaviors for dockable windows.
    /// </summary>
    public LayoutDockWindow()
    {
        Chrome.CaptionHeight = 22;
    }

    #region Methods Overrides

    /// <summary>
    /// Ensures the window content is only set via <see cref="SetHostContent"/>,
    /// and subscribes to <see cref="LayoutDockItemsControl.StateChanged"/> and Closed events.
    /// </summary>
    /// <param name="oldContent">The previous content object.</param>
    /// <param name="newContent">The new content object.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if attempting to change content after initial set, or if new content is not <see cref="LayoutDockItemsControl"/>.
    /// </exception>
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        if (!_allowContentChange)
            throw new InvalidOperationException($"External code may not change the Content of {nameof(LayoutDockWindow)}");

        if (oldContent is LayoutDockItemsControl oldControl)
        {
            oldControl.StateChanged -= OnStateChanged;
            oldControl.Closed -= LayoutDockItemClosed;
        }

        if (newContent is LayoutDockItemsControl newControl)
        {
            newControl.StateChanged += OnStateChanged;
            newControl.Closed += LayoutDockItemClosed;
            HostControl = newControl;
        }

        base.OnContentChanged(oldContent, newContent);

        // Ensure command bindings update for new content
        CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>
    /// Processes Windows messages for resizing, moving, and caption drag to support docking and size persistence.
    /// </summary>
    /// <param name="hwnd">Window handle.</param>
    /// <param name="msg">Window message ID.</param>
    /// <param name="wParam">Additional message parameter.</param>
    /// <param name="lParam">Additional message parameter.</param>
    /// <param name="handled">Indicates whether the message was handled.</param>
    /// <returns>Always returns <see cref="IntPtr.Zero"/>.</returns>
    protected override IntPtr WndProc(
        IntPtr hwnd,
        int msg,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled)
    {
        base.WndProc(hwnd, msg, wParam, lParam, ref handled);

        if (HostControl == null)
            return IntPtr.Zero;

        var wm = (WM)msg;
        switch (wm)
        {
            case WM.SIZE:
            case WM.MOVE:
                HostControl?.SaveWindowSizeAndPosition(this);
                break;
            case WM.NCLBUTTONDOWN:
                if (wParam.ToInt32() == (int)HT.CAPTION)
                {
                    var x = NativeMethods.GET_X_LPARAM(lParam);
                    var y = NativeMethods.GET_Y_LPARAM(lParam);
                    handled = HostControl?.DockManager?.Drag(
                        this,
                        new Point(x, y),
                        new Point(x - Left, y - Top)) ?? false;
                }
                break;
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// Handles drop operations from the <see cref="IDropSurface"/>,
    /// moving or docking items based on the specified <see cref="LayoutDockTargetPosition"/>.
    /// </summary>
    /// <param name="control">The surface receiving the drop.</param>
    /// <param name="btnDock">The target docking position.</param>
    protected internal override void OnDrop(IDropSurface control, LayoutDockTargetPosition btnDock)
    {
        if (HostControl is null)
        {
            return;
        }

        var dockManager = HostControl.DockManager!;
        SetHostContent(null);

        if (btnDock == LayoutDockTargetPosition.PaneInto)
        {
            if (control is LayoutDockItemsControl dockableControl)
            {
                dockManager.MoveInto(HostControl, dockableControl);
            }
            else if (control is LayoutItemsControl documentControl)
            {
                dockManager.MoveInto(HostControl, documentControl);
            }
            return;
        }

        Dock dock = default;

        if (btnDock is LayoutDockTargetPosition.Bottom or LayoutDockTargetPosition.InnerBottom)
            HostControl.Dock = Dock.Bottom;
        else if (btnDock is LayoutDockTargetPosition.Left or LayoutDockTargetPosition.InnerLeft)
            HostControl.Dock = Dock.Left;
        else if (btnDock is LayoutDockTargetPosition.Right or LayoutDockTargetPosition.InnerRight)
            HostControl.Dock = Dock.Right;
        else if (btnDock is LayoutDockTargetPosition.Top or LayoutDockTargetPosition.InnerTop)
            HostControl.Dock = Dock.Top;
        else
        {
            if (btnDock == LayoutDockTargetPosition.PaneTop)
                dock = Dock.Top;
            else if (btnDock == LayoutDockTargetPosition.PaneBottom)
                dock = Dock.Bottom;
            else if (btnDock == LayoutDockTargetPosition.PaneLeft)
                dock = Dock.Left;
            else if (btnDock == LayoutDockTargetPosition.PaneRight)
                dock = Dock.Right;

            if (control is LayoutDockItemsControl dockableControl)
            {
                dockManager.MoveTo(HostControl, dockableControl, dock);
            }
            else if (control is LayoutItemsControl documentControl)
            {
                dockManager.MoveTo(HostControl, documentControl, dock);
            }
        }

        HostControl.State = LayoutItemState.Docking;
    }

    /// <inheritdoc/>
    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        if (e.Cancel)
            return;

        HostControl?.SaveWindowSizeAndPosition(this);
    }

    /// <inheritdoc/>
    protected override void OnClosed(EventArgs e)
    {
        SetHostContent(null);
        HostControl = null;

        base.OnClosed(e);
    }

    /// <summary>
    /// Sets the hosted content. Must be used by internal code to initialize or clear <see cref="HostControl"/>.
    /// Prevents external code from setting Content directly.
    /// </summary>
    /// <param name="content">The <see cref="LayoutDockItemsControl"/> to host, or null to clear.</param>
    protected internal override void SetHostContent(object? content)
    {
        _allowContentChange = true;
        try
        {
            Content = content;
        }
        finally
        {
            _allowContentChange = false;
        }
    }

    #endregion

    #region Events Subscriptions

    /// <summary>
    /// Handles when the hosted control transitions out of Window state, closing this floating window.
    /// </summary>
    /// <param name="sender">The control raising the state change event.</param>
    /// <param name="e">State change event arguments.</param>
    private void OnStateChanged(object? sender, LayoutItemStateChangedEventArgs e)
    {
        if (e.NewValue != LayoutItemState.Window)
            Close();
    }

    /// <summary>
    /// Handles cleanup when the hosted dock control is closed. Removes it from its manager and closes the window if empty.
    /// </summary>
    /// <param name="sender">The control that was closed.</param>
    /// <param name="e">Event arguments.</param>
    private void LayoutDockItemClosed(object? sender, EventArgs e)
    {
        if (HostControl is null || HostControl.Items.Count > 0)
            return;

        var manager = HostControl.DockManager!;
        manager.RemoveLayoutDockItemsControl(HostControl);
        Close();
    }

    #endregion
}
