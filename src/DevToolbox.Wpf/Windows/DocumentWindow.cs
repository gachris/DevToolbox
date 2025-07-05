using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using DevToolbox.Wpf.Controls;
using DevToolbox.Wpf.Interop;

namespace DevToolbox.Wpf.Windows;

/// <summary>
/// A specialized window for hosting <see cref="DocumentControl"/> instances within a <see cref="DockManager"/>.
/// Inherits docking behaviors and handles window-specific size, move, and drag operations.
/// </summary>
public class DocumentWindow : DockManagerWindow
{
    #region Properties

    /// <summary>
    /// Gets the hosted <see cref="DocumentControl"/> currently displayed as the window content, or null if none.
    /// </summary>
    protected internal DocumentControl? HostControl => Content as DocumentControl;

    #endregion

    /// <summary>
    /// Static constructor to override the default style for <see cref="DocumentWindow"/>.
    /// </summary>
    static DocumentWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DocumentWindow), new FrameworkPropertyMetadata(typeof(DocumentWindow)));
    }

    #region Methods Overrides

    /// <summary>
    /// Ensures the window content is only set once and is of type <see cref="DocumentControl"/>.
    /// Subscribes to <see cref="DocumentControl.StateChanged"/> events when content is applied.
    /// </summary>
    /// <param name="oldContent">Previous content object.</param>
    /// <param name="newContent">New content object.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if attempting to change content after initial set, or if new content is not <see cref="DocumentControl"/>.
    /// </exception>
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        if (oldContent is not null)
            throw new InvalidOperationException($"Cannot change {nameof(Content)} of {typeof(DockableWindow)}");
        if (newContent is not DocumentControl content)
            throw new InvalidOperationException($"{nameof(Content)} of {typeof(DocumentWindow)} must be {typeof(DocumentControl)} type");

        content.StateChanged += OnStateChanged;

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
    protected internal override void OnDrop(IDropSurface control, DockingPosition btnDock)
    {
        if (HostControl is null)
        {
            return;
        }

        var dockManager = HostControl.DockManager!;  

        if (btnDock == DockingPosition.PaneInto && control is DocumentControl documentControl)
        {
            dockManager.MoveInto(HostControl, documentControl);
            return;
        }
        else if (control is DockManager)
        {
            dockManager.MoveInto(HostControl);
            return;
        }

        var dock = btnDock == DockingPosition.PaneBottom
            ? Dock.Bottom
            : btnDock == DockingPosition.PaneLeft
            ? Dock.Left
            : btnDock == DockingPosition.PaneRight
            ? Dock.Right
            : btnDock == DockingPosition.PaneTop
            ? Dock.Top
            : throw new InvalidOperationException($"{btnDock} DockingPosition not supported for {typeof(DocumentControl)}");

        dockManager.MoveTo(HostControl, (DocumentControl)control, dock);
        HostControl.State = State.Document;
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
    private void OnStateChanged(object? sender, StateChangedEventArgs e)
    {
        if (e.NewValue != State.Window)
        {
            Close();
        }
    }

    #endregion
}
