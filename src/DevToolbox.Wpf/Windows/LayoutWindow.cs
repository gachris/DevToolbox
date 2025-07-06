using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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

    /// <summary>
    /// Gets the hosted <see cref="LayoutItemsControl"/> currently displayed as the window content, or null if none.
    /// </summary>
    protected internal LayoutItemsControl? HostControl => Content as LayoutItemsControl;

    #endregion

    /// <summary>
    /// Static constructor to override the default style for <see cref="LayoutWindow"/>.
    /// </summary>
    static LayoutWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutWindow), new FrameworkPropertyMetadata(typeof(LayoutWindow)));
    }

    #region Methods Overrides

    /// <summary>
    /// Ensures the window content is only set once and is of type <see cref="LayoutItemsControl"/>.
    /// Subscribes to <see cref="LayoutItemsControl.StateChanged"/> events when content is applied.
    /// </summary>
    /// <param name="oldContent">Previous content object.</param>
    /// <param name="newContent">New content object.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if attempting to change content after initial set, or if new content is not <see cref="LayoutItemsControl"/>.
    /// </exception>
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        if (oldContent is not null)
            throw new InvalidOperationException($"Cannot change {nameof(Content)} of {typeof(LayoutDockWindow)}");
        if (newContent is not LayoutItemsControl content)
            throw new InvalidOperationException($"{nameof(Content)} of {typeof(LayoutWindow)} must be {typeof(LayoutItemsControl)} type");

        content.StateChanged += OnStateChanged;
        content.Closed += DocumentControl_Closed;

        base.OnContentChanged(oldContent, newContent);
    }

    /// <inheritdoc/>
    protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
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
                    var x = NativeMethods.GET_X_LPARAM(lParam);
                    var y = NativeMethods.GET_Y_LPARAM(lParam);

                    handled = HostControl?.DockManager?.Drag(this, new Point(x, y), new Point(x - Left, y - Top)) ?? false;
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
    /// <param name="btnDock">The specified docking position.</param>
    protected internal override void OnDrop(IDropSurface control, LayoutDockTargetPosition btnDock)
    {
        if (HostControl is null)
        {
            return;
        }

        var dockManager = HostControl.DockManager!;

        if (btnDock == LayoutDockTargetPosition.PaneInto && control is LayoutItemsControl documentControl)
        {
            dockManager.MoveInto(HostControl, documentControl);
            return;
        }
        else if (control is LayoutManager)
        {
            dockManager.MoveInto(HostControl);
            return;
        }

        var dock = btnDock == LayoutDockTargetPosition.PaneBottom
            ? Dock.Bottom
            : btnDock == LayoutDockTargetPosition.PaneLeft
            ? Dock.Left
            : btnDock == LayoutDockTargetPosition.PaneRight
            ? Dock.Right
            : btnDock == LayoutDockTargetPosition.PaneTop
            ? Dock.Top
            : throw new InvalidOperationException($"{btnDock} DockingPosition not supported for {typeof(LayoutItemsControl)}");

        dockManager.MoveTo(HostControl, (LayoutItemsControl)control, dock);
        HostControl.State = LayoutItemState.Document;
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

    /// <summary>
    /// Closes the window when its associated document control exits the Window state.
    /// </summary>
    /// <param name="sender">The document control raising the event.</param>
    /// <param name="e">State change arguments.</param>
    private void OnStateChanged(object? sender, LayoutItemStateChangedEventArgs e)
    {
        if (e.NewValue != LayoutItemState.Window)
        {
            Close();
        }
    }

    private void DocumentControl_Closed(object? sender, EventArgs e)
    {
        if (HostControl != null && HostControl.Items.Count == 0)
        {
            var dockManager = HostControl.DockManager!;
            var isReadOnly = ((IList)dockManager.LayoutGroupItems.Items).IsReadOnly;

            if (isReadOnly)
            {
                var item = dockManager.LayoutGroupItems.ItemFromContainer(HostControl);
                dockManager.LayoutGroupItems.Remove(item);
            }
            else
            {
                dockManager.LayoutGroupItems.Remove(HostControl);
            }
            
            Close();
        }
    }

    #endregion
}
