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
/// A specialized window for hosting <see cref="LayoutItemsControl"/> instances within a <see cref="LayoutManager"/>.
/// Inherits docking behaviors and handles window-specific size, move, and drag operations.
/// </summary>
public class LayoutWindow : LayoutBaseWindow
{
    #region Properties

    private bool _allowContentChange;

    /// <summary>
    /// Gets the hosted <see cref="LayoutItemsControl"/> currently displayed as the window content, or null if none.
    /// </summary>
    protected internal LayoutItemsControl? HostControl { get; private set; }

    #endregion

    /// <summary>
    /// Static constructor to override the default style for <see cref="LayoutWindow"/>.
    /// </summary>
    static LayoutWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(LayoutWindow), 
            new FrameworkPropertyMetadata(typeof(LayoutWindow)));
    }

    #region Methods Overrides

    /// <summary>
    /// Ensures the window content is only set once and is of type <see cref="LayoutItemsControl"/>.
    /// Subscribes to <see cref="LayoutItemsControl.StateChanged"/> and <see cref="TabControlEdit.Closed"/> events when content is applied.
    /// </summary>
    /// <param name="oldContent">Previous content object.</param>
    /// <param name="newContent">New content object.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if attempting to change content after initial set, or if new content is not <see cref="LayoutItemsControl"/>.
    /// </exception>
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        if (!_allowContentChange)
            throw new InvalidOperationException($"External code may not change the Content of {nameof(LayoutWindow)}");

        if (oldContent is LayoutItemsControl oldControl)
        {
            oldControl.StateChanged -= OnStateChanged;
            oldControl.Closed -= LayoutItemClosed;
        }

        if (newContent is LayoutItemsControl newControl)
        {
            newControl.StateChanged += OnStateChanged;
            newControl.Closed += LayoutItemClosed;
            HostControl = newControl;
        }

        base.OnContentChanged(oldContent, newContent);

        // Ensure command bindings update for new content
        CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>
    /// Processes Windows messages for size, move, and non-client left button down events to support docking and size persistence.
    /// </summary>
    /// <param name="hwnd">Window handle.</param>
    /// <param name="msg">Windows message identifier.</param>
    /// <param name="wParam">Additional message parameter.</param>
    /// <param name="lParam">Additional message parameter.</param>
    /// <param name="handled">Set to true if the message is handled by this method.</param>
    /// <returns>Always returns IntPtr.Zero.</returns>
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
                HostControl.SaveWindowSizeAndPosition(this);
                break;
            case WM.NCLBUTTONDOWN:
                if (wParam.ToInt32() == (int)HT.CAPTION)
                {
                    var x = NativeMethods.GET_X_LPARAM(lParam);
                    var y = NativeMethods.GET_Y_LPARAM(lParam);
                    handled = HostControl.DockManager?.Drag(
                        this,
                        new Point(x, y),
                        new Point(x - Left, y - Top)) ?? false;
                }
                break;
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// Handles docking when an item is dropped onto a drop surface.
    /// Supports pane-into, pane docking, and document docking positions.
    /// </summary>
    /// <param name="control">The drop surface receiving the item.</param>
    /// <param name="dockPosition">The specified docking position.</param>
    protected internal override void OnDrop(IDropSurface control, LayoutDockTargetPosition dockPosition)
    {
        if (HostControl is null)
        {
            return;
        }

        var dockManager = HostControl.DockManager!;
        SetHostContent(null);

        if (dockPosition == LayoutDockTargetPosition.PaneInto && control is LayoutItemsControl documentControl)
        {
            dockManager.MoveInto(HostControl, documentControl);
            return;
        }
        else if (control is LayoutManager)
        {
            dockManager.MoveInto(HostControl);
            return;
        }

        var dock = dockPosition switch
        {
            LayoutDockTargetPosition.PaneLeft => Dock.Left,
            LayoutDockTargetPosition.PaneRight => Dock.Right,
            LayoutDockTargetPosition.PaneTop => Dock.Top,
            LayoutDockTargetPosition.PaneBottom => Dock.Bottom,
            _ => throw new InvalidOperationException(
                $"{dockPosition} not supported for {nameof(LayoutItemsControl)}.")
        };

        dockManager.MoveTo(HostControl, (LayoutItemsControl)control, dock);
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

    #endregion

    #region Methods

    /// <summary>
    /// Sets the hosted content. Must be used by internal code to initialize or clear the <see cref="HostControl"/>.
    /// Prevents external code from setting Content directly.
    /// </summary>
    /// <param name="content">The <see cref="LayoutItemsControl"/> to host, or null to clear.</param>
    protected internal void SetHostContent(LayoutItemsControl? content)
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
    /// Closes the window when its associated document control exits the Window state.
    /// </summary>
    /// <param name="sender">The document control raising the event.</param>
    /// <param name="e">State change arguments.</param>
    private void OnStateChanged(object? sender, LayoutItemStateChangedEventArgs e)
    {
        if (e.NewValue != LayoutItemState.Window)
            Close();
    }

    /// <summary>
    /// Handles cleanup when a hosted layout item is closed. Removes the control from its group and closes the host if no items remain.
    /// </summary>
    /// <param name="sender">The document control that was closed.</param>
    /// <param name="e">Event arguments.</param>
    private void LayoutItemClosed(object? sender, EventArgs e)
    {
        if (HostControl is null || HostControl.Items.Count > 0)
            return;

        var manager = HostControl.DockManager!;
        var items = (IList)manager.LayoutGroupItems.Items;
        if (items.IsReadOnly)
        {
            var item = manager.LayoutGroupItems.ItemFromContainer(HostControl);
            manager.LayoutGroupItems.Remove(item);
        }
        else
        {
            manager.LayoutGroupItems.Remove(HostControl);
        }

        Close();
    }

    #endregion
}
