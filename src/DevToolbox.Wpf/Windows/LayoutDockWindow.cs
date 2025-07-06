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

    /// <summary>
    /// Gets the contained <see cref="LayoutDockItemsControl"/> instance, or null if none.
    /// </summary>
    [Bindable(true)]
    protected internal LayoutDockItemsControl? HostControl => Content as LayoutDockItemsControl;

    #endregion

    static LayoutDockWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(LayoutDockWindow),
            new FrameworkPropertyMetadata(typeof(LayoutDockWindow)));

        WidthProperty.OverrideMetadata(typeof(LayoutDockWindow), new FrameworkPropertyMetadata(300.0));
        HeightProperty.OverrideMetadata(typeof(LayoutDockWindow), new FrameworkPropertyMetadata(300.0));

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

    /// <inheritdoc/>
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        if (oldContent != null)
            throw new InvalidOperationException($"Cannot change Content of {nameof(LayoutDockWindow)}");

        if (newContent is not LayoutDockItemsControl content)
            throw new InvalidOperationException($"Content of {nameof(LayoutDockWindow)} must be {nameof(LayoutDockItemsControl)}");

        // Listen for when the state exits Window so we can close
        content.StateChanged += OnStateChanged;
        content.Closed += DockableControl_Closed;

        base.OnContentChanged(oldContent, newContent);

        // Ensure command bindings update for new content
        CommandManager.InvalidateRequerySuggested();
    }

    /// <inheritdoc/>
    protected override IntPtr WndProc(
        IntPtr hwnd,
        int msg,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled)
    {
        // Let base handle SCROLL/MOVE hooks
        base.WndProc(hwnd, msg, wParam, lParam, ref handled);

        switch (msg)
        {
            case (int)WM.SIZE:
            case (int)WM.MOVE:
                HostControl?.SaveWindowSizeAndPosition(this);
                break;
            case (int)WM.NCLBUTTONDOWN:
                if (wParam.ToInt32() == (int)HT.CAPTION)
                {
                    int x = NativeMethods.GET_X_LPARAM(lParam);
                    int y = NativeMethods.GET_Y_LPARAM(lParam);
                    handled = HostControl?.DockManager?.Drag(
                        this,
                        new Point(x, y),
                        new Point(x - Left, y - Top)) ?? false;
                }
                break;
        }

        return IntPtr.Zero;
    }

    /// <inheritdoc/>
    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        // If the window is being canceled, save its last size/pos
        if (!e.Cancel)
            HostControl?.SaveWindowSizeAndPosition(this);
    }

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected internal override void OnDrop(IDropSurface control, LayoutDockTargetPosition btnDock)
    {
        if (HostControl is null) return;

        if (btnDock == LayoutDockTargetPosition.PaneInto)
        {
            if (control is LayoutDockItemsControl dockableControl)
            {
                HostControl.DockManager?.MoveInto(HostControl, dockableControl);
            }
            else if (control is LayoutItemsControl documentControl)
            {
                HostControl.DockManager?.MoveInto(HostControl, documentControl);
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
                HostControl.DockManager?.MoveTo(HostControl, dockableControl, dock);
            }
            else if (control is LayoutItemsControl documentControl)
            {
                HostControl.DockManager?.MoveTo(HostControl, documentControl, dock);
            }
        }

        HostControl.State = LayoutItemState.Docking;
    }

    /// <summary>
    /// Handles when the hosted control transitions out of Window state,
    /// closing this floating window.
    /// </summary>
    private void OnStateChanged(object? sender, LayoutItemStateChangedEventArgs e)
    {
        if (e.NewValue != LayoutItemState.Window)
            Close();
    }

    private void DockableControl_Closed(object? sender, EventArgs e)
    {
        if (HostControl != null && HostControl.Items.Count == 0)
        {
            var dockManager = HostControl.DockManager!;
            var isReadOnly = ((IList)dockManager.Items).IsReadOnly;

            if (isReadOnly)
            {
                var item = dockManager.ItemFromContainer(HostControl);
                dockManager.Remove(item);
            }
            else
            {
                dockManager.Remove(HostControl);
            }

            Close();
        }
    }

    #endregion
}
